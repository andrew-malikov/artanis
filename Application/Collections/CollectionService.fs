namespace Application.Collections

open FsToolkit.ErrorHandling

open Domain.Collections.CollectionEntity
open Domain.Collections.CollectionUseCases
open Application.Assets.AssetFilters
open Application.Projects.ProjectFilters

module CollectionService =
    let getMetadata fetchCollection mapCollection request : Async<CollectionMetadata> =
        fetchCollection request |> Async.map mapCollection

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
            let! assetFilters = buildAssetFilters filterOptions
            let projectFilters = buildProjectFilters filterOptions

            return
                getFilteredCollection
                    getCollection
                    { userCollectionId = collectionId
                      filters =
                          { assetFilters = assetFilters
                            projectFilters = projectFilters } }
        }
