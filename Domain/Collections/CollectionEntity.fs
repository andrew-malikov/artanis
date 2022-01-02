namespace Domain.Collections

open Domain.Projects.ProjectEntity

module CollectionEntity =
    type UserCollectionId = { collectionId: int; username: string }

    type CollectionMetadata =
        { id: int
          name: string
          projectsCount: int
          userId: int }

    type Collection =
        { metadata: CollectionMetadata
          projects: Project list }
