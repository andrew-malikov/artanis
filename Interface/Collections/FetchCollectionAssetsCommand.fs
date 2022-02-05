namespace Interface.Collections

open System.ComponentModel
open System.Threading.Tasks

open Akka.Routing
open Artstation.Assets
open FsToolkit.ErrorHandling

open Microsoft.FSharp.Control
open Spectre.Console
open Spectre.Console.Cli

open Akka.FSharp

open Domain.Assets.AssetEntity
open Domain.Collections.CollectionEntity
open Application.Collections.CollectionService
open Artstation.Collections.CollectionApi
open Persistence.StatefulCollectionEntity
open Persistence.LocalAssetPersistenceService
open Interface.Collections.FetchCollectionArgs
open Interface.FilterOptionsFactory
open Interface.Cli.Formatters

// TODO: Separate actors by different modules
//       move out the logic to fetch collection to an actor
//       keep only the code to run the actor system within Spectre CLI
module FetchCollectionAssetsCommand =
    let actorSystem =
        System.create "FetchCollectionAsset" (Configuration.load ())

    type CollectionAsset =
        { asset: StatefulAsset
          collectionId: int
          projectId: int }
    
    // TODO: adjust events to have error cases 
    type Event =
        | PersistedAsset of CollectionAsset
        | PersistAsset of CollectionAsset
        | PersistingAsset of CollectionAsset
        | PersistCollection of Collection
        | PersistedCollection of StatefulCollection

    let displayActor (displayContext: StatusContext) (mailbox: Actor<Event>) =
        let rec loop () =
            actor {
                let! event = mailbox.Receive()

                match event with
                | PersistCollection collection ->
                    displayContext.Status = $"Persisting collection: {collection.metadata.id}"
                    |> ignore
                | PersistedCollection statefulCollection ->
                    displayContext.Status = $"Persisted collection: {statefulCollection.metadata.id}"
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

    let persistActor output (mailbox: Actor<Event>) =
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

    let collectionActor displayActor persistActor (mailbox: Actor<Event>) =
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
                                        persistActor
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

    let runCollectionActorSystem output collection =
        AnsiConsole
            .Status()
            .Start(
                "Fetching collection",
                fun context ->
                    let displayActorRef =
                        spawn actorSystem "Display" (displayActor context)

                    let persistActorRef =
                        spawnOpt actorSystem "Persist" (persistActor output) [ SpawnOption.Router(RoundRobinPool(3)) ]

                    let collectionActorRef =
                        spawn actorSystem "Collection" (collectionActor displayActorRef persistActorRef)

                    collectionActorRef <! PersistCollection collection

                    actorSystem.WhenTerminated
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
            )

    type private Args =
        { collectionId: int
          username: string
          output: string
          orientation: Orientation option }

    type Settings(collectionId, username, output, orientation) =
        inherit CommandSettings()

        [<Description("Collection id")>]
        [<CommandArgument(0, "<collectionId>")>]
        member val collectionId: int = collectionId

        [<Description("Collection account username")>]
        [<CommandArgument(1, "<username>")>]
        member val username: string = username

        [<Description("Output directory")>]
        [<CommandArgument(2, "<output>")>]
        member val output: string = output

        [<Description("Assets orientation like 'landscape' 'portrait' or 'square'")>]
        [<CommandOption("-o|--orientation")>]
        member val orientation: string = orientation

    // TODO: adjust the model due to the new output argument
    let private parseArgs (settings: Settings) =
        result {
            let! orientation = parseOrientation settings.orientation

            return
                { collectionId = settings.collectionId
                  username = settings.username
                  output = settings.output
                  orientation = orientation }
        }

    type Command() =
        inherit AsyncCommand<Settings>()

        override this.ExecuteAsync(context, settings) =
            let fetchingCollectionResult =
                result {
                    let! { collectionId = collectionId
                           username = username
                           orientation = orientation } = parseArgs settings

                    let collectionId: UserCollectionId =
                        { collectionId = collectionId
                          username = username }

                    return!
                        getFilteredCollection
                            (getCollection getCollectionMetadata getAllCollectionProjects)
                            (getFilterOptions [ getOrientationFilterOption orientation ])
                            collectionId
                }
            
            // TODO: restructure the interaction with console via Spectre
            //       it is possible to have a deadlock based on the Status widget
            match fetchingCollectionResult with
            | Ok fetchingCollection ->
                fetchingCollection
                |> Async.map (runCollectionActorSystem settings.output)
                |> Async.map (fun _ -> 0)
                |> Async.StartAsTask
            | Error message ->
                AnsiConsole.Markup(formatError message)
                Task.FromResult -1
