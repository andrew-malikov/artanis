namespace Artstation.Projects

open System

open FsToolkit.ErrorHandling

open Flurl.Http

open Domain.Projects.ProjectEntity
open Artstation.Api
open Artstation.Assets.AssetApi

module ProjectApi =
    type SoftwareItemResponse = { iconUrl: string; name: string }

    type UserResponse =
        { blocked: bool
          followed: bool
          followingBack: bool
          fullName: string
          headline: string
          id: int
          isPlusMember: bool
          isStaff: bool
          largeAvatarUrl: string
          mediumAvatarUrl: string
          permalink: string
          proMember: bool
          smallCoverUrl: string
          username: string }

    type EmptyProjectResponse =
        { adminAdultContent: bool
          adultContent: bool
          assetsCount: int
          coverAssetId: int
          createdAt: DateTime
          description: string
          hashId: string
          hideAsAdult: bool
          id: int
          likesCount: int
          permalink: string
          publishedAt: DateTime
          slug: string
          title: string
          updatedAt: DateTime
          userId: int }

    type Medium = { name: string; id: int }

    type CategoryResponse = { name: string; id: int }

    type ProjectResponse =
        { adminAdultContent: bool
          adultContent: bool
          assets: AssetResponse list option
          categories: CategoryResponse list option
          commentsCount: int 
          coverUrl: string
          createdAt: DateTime
          description: string
          descriptionHtml: string
          editorPick: bool
          hashId: string
          hideAsAdult: bool
          id: int
          likesCount: int
          medium: Medium option
          mediums: Medium list option
          permalink: string
          publishedAt: DateTime
          slug: string
          softwareItems: SoftwareItemResponse List option
          suppressed: bool
          tags: string list
          title: string
          updatedAt: DateTime
          user: UserResponse
          userId: int
          viewsCount: int
          visible: bool
          visibleOnArtstation: bool }

    let private toProject (projectResponse: ProjectResponse) =
        { assets =
              projectResponse.assets
              |> Option.defaultValue []
              |> List.map toAsset
              |> List.choose id
          categories =
              projectResponse.categories
              |> Option.defaultValue []
              |> List.map
                  (fun category ->
                      { Category.id = category.id
                        name = category.name })
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

    let getProject (id: int) =
        BaseUrl()
            .AppendPathSegments("projects", $"{id}.json")
            .GetStringAsync()
        |> Async.AwaitTask
        |> Async.map (parseJson >> toProject)

    let rec getProjects (ids: int list) =
        match ids with
        | id :: restIds ->
            async {
                let! project = getProject id
                let! restProjects = getProjects restIds

                return project :: restProjects
            }
        | [] -> async.Return []
