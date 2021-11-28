namespace Artstation.Projects

open Artstation.Projects.ProjectApi
open Artstation.Assets.AssetFactory
open Domain.Projects.ProjectEntity

module ProjectFactory =
    let getProject (projectResponse: ProjectResponse) =
        { assets =
              projectResponse.assets
              |> List.map getAsset
              |> List.choose id
          categories =
              projectResponse.categories
              |> List.map
                  (fun category ->
                      {| id = category.id
                         name = category.name |})
          createdAt = projectResponse.createdAt
          description = projectResponse.description
          hashId = projectResponse.hashId
          id = projectResponse.id
          permalink = projectResponse.permalink
          publishedAt = projectResponse.publishedAt
          tags = projectResponse.tags
          title = projectResponse.title
          updatedAt = projectResponse.updatedAt
          userId = projectResponse.userId }
