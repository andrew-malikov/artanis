namespace Persistence

open System

open Domain.Assets.AssetEntity
open Domain.Collections.CollectionEntity
open Domain.Projects.ProjectEntity

module StatefulCollectionEntity =
    type PersistingStatus =
        | Unprocessed
        | Persisting
        | Persisted

    type StatefulAsset =
        { assetType: AssetType
          status: PersistingStatus
          hasImage: bool
          height: int
          id: int
          imageUrl: string
          position: int
          title: string option
          titleFormatted: string
          viewportConstraintType: string
          width: int }

    let private toStatefulAsset (asset: Asset) =
        { assetType = asset.assetType
          hasImage = asset.hasImage
          height = asset.height
          id = asset.id
          imageUrl = asset.imageUrl
          position = asset.position
          title = asset.title
          titleFormatted = asset.titleFormatted
          viewportConstraintType = asset.viewportConstraintType
          width = asset.width
          status = Unprocessed }

    let private markStatefulAsset status asset = { asset with status = status }

    let private getAssetsStatus assets =
        let allPersisted =
            List.forall (fun (asset: StatefulAsset) -> asset.status = Persisted) assets

        let allUnprocessed =
            List.forall (fun (asset: StatefulAsset) -> asset.status = Unprocessed) assets

        let somePersisting =
            List.exists (fun (asset: StatefulAsset) -> asset.status = Persisting) assets

        match (allPersisted, allUnprocessed, somePersisting) with
        | true, _, _ -> Persisted
        | _, true, _ -> Unprocessed
        | _, _, true -> Persisting
        | _ -> failwith "Invalid state"

    type StatefulProject =
        { assets: StatefulAsset list
          categories: {| name: string; id: int |} list
          createdAt: DateTime
          description: string
          status: PersistingStatus
          hashId: string
          id: int
          permalink: string
          publishedAt: DateTime
          tags: string list
          title: string
          updatedAt: DateTime
          userId: int }

    let private toStatefulProject (project: Project) =
        let categories =
            project.categories
            |> List.map
                (fun category ->
                    {| name = category.name
                       id = category.id |})

        { assets = List.map toStatefulAsset project.assets
          categories = categories
          createdAt = project.createdAt
          description = project.description
          status = Unprocessed
          hashId = project.hashId
          id = project.id
          permalink = project.permalink
          publishedAt = project.publishedAt
          tags = project.tags
          title = project.title
          updatedAt = project.updatedAt
          userId = project.userId }

    let private markStatefulProjectAsset status assetId project =
        let unchangedAssets =
            project.assets
            |> List.filter (fun asset -> asset.id <> assetId)

        let markedAsset =
            project.assets
            |> List.tryFind (fun asset -> asset.id = assetId)
            |> Option.map (markStatefulAsset status)

        match markedAsset with
        | Some asset ->
            let assets = asset :: unchangedAssets

            match getAssetsStatus assets with
            | Persisted ->
                { project with
                      assets = assets
                      status = Persisted }
            | Unprocessed ->
                { project with
                      assets = assets
                      status = Unprocessed }
            | Persisting ->
                { project with
                      assets = assets
                      status = Persisting }
        | None -> project

    let private getProjectsStatus projects =
        let allPersisted =
            List.forall (fun project -> project.status = Persisted) projects

        let allUnprocessed =
            List.forall (fun project -> project.status = Unprocessed) projects

        let somePersisting =
            List.exists (fun project -> project.status = Persisting) projects

        match (allPersisted, allUnprocessed, somePersisting) with
        | true, _, _ -> Persisted
        | _, true, _ -> Unprocessed
        | _, _, true -> Persisting
        | _ -> failwith "Invalid state"

    type StatefulCollection =
        { metadata: CollectionMetadata
          projects: StatefulProject list
          status: PersistingStatus }

    let toStatefulCollection (collection: Collection) =
        { metadata = collection.metadata
          projects = List.map toStatefulProject collection.projects
          status = Unprocessed }

    let markStatefulCollectionAsset collection projectId assetId status =
        let unchangedProjects =
            collection.projects
            |> List.filter (fun project -> project.id <> projectId)

        let markedProject =
            collection.projects
            |> List.tryFind (fun project -> project.id = projectId)
            |> Option.map (markStatefulProjectAsset status assetId)

        match markedProject with
        | Some project ->
            let projects = project :: unchangedProjects

            match getProjectsStatus projects with
            | Persisted ->
                { collection with
                      projects = projects
                      status = Persisted }
            | Unprocessed ->
                { collection with
                      projects = projects
                      status = Unprocessed }
            | Persisting ->
                { collection with
                      projects = projects
                      status = Persisting }
        | None -> collection
