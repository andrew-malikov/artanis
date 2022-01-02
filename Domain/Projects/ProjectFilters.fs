namespace Domain.Projects

open Domain.Filters
open Domain.Projects.ProjectEntity

module ProjectFilters =

    let filterNotEmptyProject =
        function
        | { assets = [] } -> None
        | project -> Some project

    let filterFirstProjectAsset =
        function
        | { assets = [] } -> None
        | project ->
            Some
                { project with
                      assets =
                          [ project.assets
                            |> List.maxBy (fun asset -> asset.position) ] }

    let filterProject projectFilters assetFilters project =
        let filteredProject =
            { project with
                  assets =
                      project.assets
                      |> filterMany (
                          assetFilters
                          |> List.map (fun filter -> filter.selector)
                      ) }

        let projectFilters =
            filterNotEmptyProject
            :: (projectFilters
                |> List.map (fun filter -> filter.selector))

        filterOne projectFilters filteredProject
