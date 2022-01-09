namespace Application.Collections

open FsToolkit.ErrorHandling

open Domain.Collections.CollectionEntity
open Domain.Collections.CollectionFilters
open Application.Assets.AssetFilters
open Application.Projects.ProjectFilters

module CollectionService =
    let getCollection getMetadata getProjects request =
        async {
            let! metadata = getMetadata request
            let! projects = getProjects request.collectionId

            return
                { metadata = metadata
                  projects = projects }
        }

    let getFilteredCollection getCollection filterOptions collectionId =
        result {
            let! filters =
                buildAssetFilters filterOptions
                |> Result.map
                    (fun assetFilters ->
                        { assetFilters = assetFilters
                          projectFilters = buildProjectFilters filterOptions })

            return
                getCollection collectionId
                |> Async.map (applyFilters filters)
        }
