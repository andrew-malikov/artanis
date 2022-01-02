namespace Domain.Collections

open FsToolkit.ErrorHandling

open Domain.Collections.CollectionEntity
open Domain.Collections.CollectionFilters

module CollectionUseCases =
    type GetFilteredCollectionRequest =
        { userCollectionId: UserCollectionId
          filters: CollectionFilters }

    type GetCollection = UserCollectionId -> Async<Collection>

    let getFilteredCollection (getCollection: GetCollection) request =
        async {
            let! collection = getCollection request.userCollectionId

            return filterCollection request.filters collection
        }
