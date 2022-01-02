namespace Domain

module Filters =
    [<CustomEquality; NoComparison>]
    type 'item Filter =
        { name: string
          selector: 'item -> 'item option }

        override this.Equals other =
            match other with
            | :? Filter<'item> as filter -> filter.name.Equals this.name
            | _ -> false

        override this.GetHashCode() = this.name.GetHashCode()

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
