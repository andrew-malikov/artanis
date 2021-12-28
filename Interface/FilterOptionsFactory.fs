namespace Interface

open Domain.FilterOptions

module FilterOptionsFactory =
    type FilterOptionType =
        | Flag of bool
        | Option of FilterArgument option

    type FilterOptionEntry =
        { arg: FilterOptionType
          category: string
          name: string }

    let getFilterOption =
        function
        | { arg = Option (Some arg)
            category = category
            name = name } ->
            Some
                { args = [ arg ]
                  category = category
                  name = name }
        | { arg = Flag true
            category = category
            name = name } ->
            Some
                { args = []
                  category = category
                  name = name }
        | _ -> None

    let getFilterOptions entries =
        entries
        |> List.map getFilterOption
        |> List.choose id
