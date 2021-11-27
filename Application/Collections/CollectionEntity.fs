namespace Application.Collections

open Application.Projects.ProjectEntity

module CollectionEntity =
    type CollectionMetadata =
        { id: int
          name: string
          projectsCount: int
          userId: int }

    type Collection =
        { metadata: CollectionMetadata
          projects: Project list }
