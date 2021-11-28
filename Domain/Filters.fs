namespace Domain

module Filters =
    let rec filterOne filters item =
        match filters with
        | [] -> Some item
        | filter :: restFilters ->
            match filter item with
            | Some filteredItem -> filterOne restFilters filteredItem
            | None -> None

    let rec filterMany filters items =
        match items with
        | [] -> []
        | item :: restItems ->
            match filterOne filters item with
            | Some filteredItem -> filteredItem :: filterMany filters restItems
            | None -> filterMany filters restItems
