namespace Application.Collections

open Domain.Collections.CollectionEntity

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
