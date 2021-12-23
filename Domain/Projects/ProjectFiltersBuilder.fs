namespace Domain.Projects

open FsToolkit.ErrorHandling

open Domain.FilterOptions
open Domain.Projects.ProjectFilters

module ProjectFiltersBuilder =
    let private mapOptionToFilter option =
        match option.name with
        | "notEmptyProject" -> Some filterNotEmptyProject
        | "firstProjectAsset" -> Some filterFirstProjectAsset
        | _ -> None

    let buildProjectFilters options =
        options
        |> List.filter (fun option -> option.category = "projects")
        |> List.map mapOptionToFilter
        |> List.choose id
