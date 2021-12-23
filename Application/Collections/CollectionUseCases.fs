namespace Application.Collections

open FsToolkit.ErrorHandling

open Domain.Collections.CollectionEntity
open Domain.Collections.CollectionFilters

module CollectionUseCases =
    type GetCollectionRequest = { collectionId: int; username: string }

    let getMetadata fetchCollection mapCollection request : Async<CollectionMetadata> =
        fetchCollection request.collectionId request.username
        |> Async.map mapCollection

    let getCollection getMetadata getProjects request =
        async {
            let! metadata = getMetadata request
            let! projects = getProjects request.collectionId

            return
                { metadata = metadata
                  projects = projects }
        }

    let getCollectionFilters getProjectFilters filterOptions =
        getProjectFilters filterOptions
        |> Result.map filterCollection

    let getFilteredCollection getCollection filterCollection request : Async<Collection option> =
        getCollection request
        |> Async.map filterCollection
