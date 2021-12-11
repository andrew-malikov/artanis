namespace Application.Projects

open Domain.Projects.ProjectEntity

module ProjectUseCases =
    let getProjects fetchProjects mapProject collectionId : Async<Project list> =
        async {
            let! projects = fetchProjects collectionId
            
            return projects |> List.map mapProject
        }

