namespace Artstation.Collections

open Flurl.Http

open Domain.Collections.CollectionEntity
open Artstation.Api
open Artstation.Projects.ProjectApi

module CollectionApi =
    type CollectionResponse =
        { activeProjectsCount: int
          id: int
          isPrivate: bool
          microSquareImageUrl: string
          name: string
          projectsCount: int
          smallSquareImageUrl: string
          userId: int }

    type CollectionProjectPageResponse =
        { data: EmptyProjectResponse List
          totalCount: int }

    let getCollection userCollectionId : Async<CollectionResponse> =
        async {
            let! rawCollection =
                BaseUrl()
                    .AppendPathSegments("collections", $"{userCollectionId.collectionId}.json")
                    .SetQueryParam("username", userCollectionId.username)
                    .GetStringAsync()
                |> Async.AwaitTask

            return parseJson rawCollection
        }

    let getCollectionProjects (id: int) (page: int) : Async<CollectionProjectPageResponse> =
        async {
            let! rawCollectionProjects =
                BaseUrl()
                    .AppendPathSegments("collections", id, "projects.json")
                    .SetQueryParam("collection_id", id)
                    .SetQueryParam("page", page)
                    .GetStringAsync()
                |> Async.AwaitTask

            return parseJson rawCollectionProjects
        }

    let getAllCollectionProjects id =
        let rec fetchProjects page =
            async {
                let! currentPage = getCollectionProjects id page

                return!
                    match currentPage with
                    | { data = [] } -> async.Return []
                    | { data = currentPageEmptyProjects } ->
                        async {
                            let! currentPageProjects =
                                currentPageEmptyProjects
                                |> List.map (fun emptyProject -> emptyProject.id)
                                |> getProjects

                            let! nextPageProjects = fetchProjects (page + 1)

                            return currentPageProjects @ nextPageProjects
                        }
            }

        fetchProjects 1
