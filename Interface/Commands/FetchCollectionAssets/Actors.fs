namespace Interface.Commands.FetchCollectionAssets

open Akka.Routing
open Domain.Assets.AssetFilters
open FsToolkit.ErrorHandling

open Microsoft.FSharp.Control
open Spectre.Console

open Akka.FSharp

open Domain.Assets.AssetEntity
open Domain.Collections.CollectionEntity
open Application.Collections.CollectionService
open Artstation.Collections.CollectionApi
open Artstation.Assets
open Persistence.StatefulCollectionEntity
open Persistence.LocalAssetPersistenceService
open Interface.FilterOptionsFactory
open Interface.Assets.AssetArgs
open Interface.Projects.ProjectArgs

// TODO: export only a function to run the system
//       and what about to pass dependencies as a parameter?
//       it definitely shall help to test the actor system
module Actors =
    let actorSystem =
        System.create "FetchCollectionAssets" (Configuration.load ())

    type FetchCollectionAssetsRequest =
        { collectionId: int
          username: string
          orientation: Orientation option
          assetType: AssetType option
          sizeComparator: AssetSizeComparator option
          outputDirectory: string }

    type CollectionAsset =
        { asset: StatefulAsset
          collectionId: int
          projectId: int }

    // TODO: adjust events to have error cases
    //       as well as events related to fetching collection metadata
    type Event =
        | PersistedAsset of CollectionAsset
        | PersistAsset of CollectionAsset
        | PersistingAsset of CollectionAsset
        | PersistCollection of Collection
        | PersistedCollection of StatefulCollection
        | FetchingCollection of UserCollectionId
        | FetchCollection of FetchCollectionAssetsRequest

    let displayActor (displayContext: StatusContext) (mailbox: Actor<Event>) =
        let rec loop () =
            actor {
                let! event = mailbox.Receive()

                match event with
                | FetchingCollection userCollectionId ->
                    StatusContextExtensions.Status(
                        displayContext,
                        $"Fetching collection: {userCollectionId.collectionId}"
                    )
                    |> ignore

                    AnsiConsole.MarkupLine $"Fetching collection: {userCollectionId.collectionId}"
                | PersistCollection collection ->
                    StatusContextExtensions.Status(displayContext, $"Persisting collection: {collection.metadata.id}")
                    |> ignore
                | PersistedCollection statefulCollection ->
                    StatusContextExtensions.Status(
                        displayContext,
                        $"Persisted collection: {statefulCollection.metadata.id}"
                    )
                    |> ignore

                    AnsiConsole.MarkupLine $"Persisted collection: {statefulCollection.metadata.id}"
                | PersistingAsset collectionAsset ->
                    AnsiConsole.MarkupLine $"Persisting asset: {collectionAsset.asset.id}"
                | PersistedAsset collectionAsset ->
                    AnsiConsole.MarkupLine $"Persisted asset: {collectionAsset.asset.id}"
                | _ -> ()

                return! loop ()
            }

        loop ()

    let assetActor output (mailbox: Actor<Event>) =
        let rec loop () =
            actor {
                let! event = mailbox.Receive()

                match event with
                | PersistAsset collectionAsset ->
                    mailbox.Sender()
                    <! PersistingAsset collectionAsset

                    let fulfilledAsset =
                        toAsset collectionAsset.asset
                        |> AssetApi.fetchAsset
                        |> Async.RunSynchronously

                    // TODO: check the result and if it is None then raise the event
                    persistAsset fulfilledAsset output |> ignore

                    mailbox.Sender() <! PersistedAsset collectionAsset
                | _ -> ()

                return! loop ()
            }

        loop ()

    type CollectionActorState = { collection: StatefulCollection }

    // TODO: move out inner functions to the higher level as state functions
    let collectionAssetsActor displayActor assetActor (mailbox: Actor<Event>) =
        let rec loop (state: CollectionActorState option) =
            actor {
                let! event = mailbox.Receive()

                let changedState =
                    match event with
                    | PersistCollection collection ->
                        displayActor <! PersistCollection collection

                        let initialState =
                            { collection = toStatefulCollection collection }

                        // TODO: catch a case when there are zero assets
                        //       to terminate the system properly
                        //       and prevent the execution of the rest code
                        match initialState.collection.status with
                        | Persisted ->
                            displayActor
                            <! PersistedCollection initialState.collection

                            mailbox
                                .Context
                                .System
                                .Terminate()
                                .RunSynchronously()
                        | _ -> ()

                        initialState.collection.projects
                        |> List.iter
                            (fun project ->
                                project.assets
                                |> List.iter
                                    (fun asset ->
                                        assetActor
                                        <! PersistAsset
                                            { asset = asset
                                              collectionId = collection.metadata.id
                                              projectId = project.id }))

                        Some initialState
                    | PersistedAsset collectionAsset ->
                        displayActor <! PersistedAsset collectionAsset

                        state
                        |> Option.bind
                            (fun state ->
                                markStatefulCollectionAsset
                                    state.collection
                                    collectionAsset.projectId
                                    collectionAsset.asset.id
                                    Persisted
                                |> Some)
                        |> Option.bind (fun statefulCollection -> Some { collection = statefulCollection })
                        |> Option.bind
                            (fun markedState ->
                                match markedState.collection.status with
                                | Persisted ->
                                    displayActor
                                    <! PersistedCollection markedState.collection

                                    mailbox
                                        .Context
                                        .System
                                        .Terminate()
                                        .RunSynchronously()
                                | _ -> ()

                                Some markedState)
                    | PersistingAsset collectionAsset ->
                        displayActor <! PersistingAsset collectionAsset

                        state
                        |> Option.bind
                            (fun state ->
                                markStatefulCollectionAsset
                                    state.collection
                                    collectionAsset.projectId
                                    collectionAsset.asset.id
                                    Persisting
                                |> Some)
                        |> Option.bind (fun statefulCollection -> Some { collection = statefulCollection })
                    | _ -> state

                return! loop changedState
            }

        loop None

    let collectionActor displayActor collectionAssetsActor (mailbox: Actor<Event>) =
        let rec loop () =
            actor {
                let! event = mailbox.Receive()

                match event with
                | FetchCollection { collectionId = collectionId
                                    username = username
                                    orientation = orientation
                                    assetType = assetType
                                    sizeComparator = sizeComparator } ->

                    let collectionId: UserCollectionId =
                        { collectionId = collectionId
                          username = username }

                    displayActor <! FetchingCollection collectionId

                    let fetchingCollectionResult =
                        getFilteredCollection
                            (getCollection getCollectionMetadata getAllCollectionProjects)
                            (getFilterOptions [ getOrientationFilterOption orientation
                                                getAssetTypeFilterOption assetType
                                                getAssetSizeComparatorFilterOption sizeComparator
                                                Some true |> getNotEmptyProjectFilterOption
                                                Some true |> getFirstProjectAssetFilterOption ])
                            collectionId

                    // TODO: handle the error branch
                    match fetchingCollectionResult with
                    | Ok collection ->
                        collectionAssetsActor
                        <! PersistCollection(collection |> Async.RunSynchronously)
                    | _ -> ()
                | _ -> ()

                return! loop ()
            }

        loop ()

    let runCollectionActorSystem (displayContext: StatusContext) request =
        let displayActorRef =
            spawn actorSystem "Display" (displayActor displayContext)

        // TODO: provide the count as a parameter
        let assetActorRef =
            spawnOpt actorSystem "Asset" (assetActor request.outputDirectory) [ SpawnOption.Router(RoundRobinPool(3)) ]

        let collectionAssetsActorRef =
            spawn actorSystem "CollectionAssets" (collectionAssetsActor displayActorRef assetActorRef)

        let collectionActorRef =
            spawn actorSystem "Collection" (collectionActor displayActorRef collectionAssetsActorRef)

        collectionActorRef <! FetchCollection request

        actorSystem.WhenTerminated
        |> Async.AwaitTask
        |> Async.RunSynchronously
