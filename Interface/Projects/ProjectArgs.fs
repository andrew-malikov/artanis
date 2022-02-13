namespace Interface.Projects

open Interface.FilterOptionsFactory

module ProjectArgs =
    let getNotEmptyProjectFilterOption isActive =
        { arg = isActive |> Option.defaultValue false |> Flag
          category = "projects"
          name = "notEmptyProject" }

    let getFirstProjectAssetFilterOption isActive =
        { arg = isActive |> Option.defaultValue false |> Flag
          category = "projects"
          name = "firstProjectAsset" }
