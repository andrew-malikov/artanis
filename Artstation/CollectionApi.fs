namespace Artstation

open Flurl.Http

open Artstation.ProjectApi

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

    let getCollection (id: int) (username: string) =
        Api
            .BaseUrl
            .AppendPathSegments("collections", $"{id}.json")
            .SetQueryParam("username", username)
            .GetJsonAsync<CollectionResponse>()
        |> Async.AwaitTask

    let getCollectionProjects (id: int) (page: int) =
        Api
            .BaseUrl
            .AppendPathSegments("collections", id, "projects.json")
            .SetQueryParam("collection_id", id)
            .SetQueryParam("page", page)
            .GetJsonAsync<CollectionProjectPageResponse>()
        |> Async.AwaitTask

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
