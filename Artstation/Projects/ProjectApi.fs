namespace Artstation.Projects

open System

open Flurl.Http

open Artstation.Api

module ProjectApi =
    type AssetResponse =
        { assetType: string
          hasEmbeddedPlayer: bool
          hasImage: bool
          height: int
          id: int
          imageUrl: string
          position: int
          title: string option
          titleFormatted: string
          viewportConstraintType: string
          width: int }

    type SoftwareItemResponse = { iconUrl: string; name: string }

    type UserResponse =
        { blocked: bool
          followed: bool
          followingBack: bool
          fullName: string
          headline: string
          id: int
          isOrganizationOwner: bool
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

    type ProjectResponse =
        { adminAdultContent: bool
          adultContent: bool
          assets: AssetResponse List
          categories: {| name: string; id: int |} List
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
          medium: {| name: string; id: int |}
          mediums: {| name: string; id: int |} List
          permalink: string
          publishedAt: DateTime
          slug: string
          softwareItems: SoftwareItemResponse List
          suppressed: bool
          tags: string List
          title: string
          updatedAt: DateTime
          user: UserResponse
          userId: int
          viewsCount: int
          visible: bool
          visibleOnArtstation: bool }

    let getProject (id: int) =
        BaseUrl
            .AppendPathSegments("project", $"{id}.json")
            .GetJsonAsync<ProjectResponse>()
        |> Async.AwaitTask

    let rec getProjects (ids: int list) =
        match ids with
        | id :: restIds ->
            async {
                let! project = getProject id
                let! restProjects = getProjects restIds

                return project :: restProjects
            }
        | [] -> async.Return []
