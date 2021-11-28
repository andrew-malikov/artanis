namespace Domain.Projects

open Domain.Filters
open Domain.Projects.ProjectEntity

module ProjectFilters =
    let filterProject assetFilters project =
        { project with
              assets = filterMany assetFilters project.assets }

    let filterNotEmptyProject =
        function
        | { assets = [] } -> None
        | project -> Some project
