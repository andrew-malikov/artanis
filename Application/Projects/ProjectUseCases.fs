namespace Application.Projects

open FsToolkit.ErrorHandling

open Domain.Assets.AssetFiltersBuilder
open Domain.Projects.ProjectEntity
open Domain.Projects.ProjectFilters
open Domain.Projects.ProjectFiltersBuilder

module ProjectUseCases =
    let getProjects fetchProjects mapProject collectionId : Async<Project list> =
        fetchProjects collectionId
        |> Async.map (List.map mapProject)

    let getProjectFilters filterOptions =
        result {
            let! assetFilters = buildAssetFilters filterOptions

            return
                buildProjectFilters filterOptions
                @ [ fun project -> filterProject assetFilters project |> Some ]
        }
