namespace Application.Projects

open Application.Filters
open Application.Projects.ProjectEntity

module ProjectFilters =
    let filterProject assetFilters project =
        { project with
              assets = filterMany assetFilters project.assets }

    let filterNotEmptyProject =
        function
        | { assets = [] } -> None
        | project -> Some project
