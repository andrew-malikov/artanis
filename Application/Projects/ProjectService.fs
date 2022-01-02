namespace Application.Projects

open FsToolkit.ErrorHandling

open Domain.Projects.ProjectEntity

module ProjectService =
    let getProjects fetchProjects mapProject collectionId : Async<Project list> =
        fetchProjects collectionId
        |> Async.map (List.map mapProject)
