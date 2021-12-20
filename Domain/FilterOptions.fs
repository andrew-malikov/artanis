namespace Domain

module FilterOptions =
    type FilterArgument = { name: string; value: obj }

    let getFilterArgumentValue<'TValue> argName arguments =
        match arguments
              |> List.filter (fun argument -> argument.name = argName) with
        | [ argument ] ->
            match argument.value with
            | :? 'TValue as value -> Ok value
            | _ -> Error $"The argument with name {argName} has incorrect type"
        | [] -> Error $"The argument with name {argName} is not found"
        | _ -> Error $"Found many arguments with name {argName}"

    type FilterOptions =
        { name: string
          category: string
          args: FilterArgument list }
