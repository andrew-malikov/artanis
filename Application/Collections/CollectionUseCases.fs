namespace Application.Collections

open FsToolkit.ErrorHandling

open Domain.Collections.CollectionEntity
open Domain.Collections.CollectionFilters

module CollectionUseCases =
    let getMetadata fetchCollection mapCollection collectionId username : Async<CollectionMetadata> =
        async {
            let! collectionMetadata = fetchCollection collectionId username

            return mapCollection collectionMetadata
        }

    let getCollection getMetadata getProjects collectionId username =
        async {
            let! metadata = getMetadata collectionId username
            let! projects = getProjects collectionId

            return
                { metadata = metadata
                  projects = projects }
        }
        
    let getCollectionFilters getProjectFilters filterOptions =
        result {
            let! projectFilters = getProjectFilters filterOptions
            
            return filterCollection projectFilters
        }

    let getFilteredCollection getCollection filterCollection collectionId username : Async<Collection option> =
        async {
            let! collection = getCollection collectionId username

            return filterCollection collection
        }
