namespace Artstation.Collections

open Artstation.Collections.CollectionApi
open Artstation.Projects.ProjectFactory
open Domain.Collections.CollectionEntity

module CollectionFactory =
    let getCollectionMetadata (collectionResponse: CollectionResponse) =
        { id = collectionResponse.id
          name = collectionResponse.name
          projectsCount = collectionResponse.projectsCount
          userId = collectionResponse.userId }

    let getCollection collectionResponse projectResponses =
        { metadata = getCollectionMetadata collectionResponse
          projects = List.map getProject projectResponses }
