namespace Application.Projects

open FsToolkit.ErrorHandling

open Domain.Filters
open Domain.Projects.ProjectFilters
open Application.FilterOptions

module ProjectFilters =
    let private mapOptionToFilter option =
        match option.name with
        | "notEmptyProject" ->
            Some
                { selector = filterNotEmptyProject
                  name = "notEmptyProject" }
        | "firstProjectAsset" ->
            Some
                { selector = filterFirstProjectAsset
                  name = "firstProjectAsset" }
        | _ -> None

    let buildProjectFilters options =
        options
        |> List.filter (fun option -> option.category = "projects")
        |> List.map mapOptionToFilter
        |> List.choose id
