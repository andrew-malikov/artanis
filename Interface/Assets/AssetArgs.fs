namespace Interface.Assets

open System
open System.Text.RegularExpressions

open FsToolkit.ErrorHandling

open Application.FilterOptions
open Domain.Assets.AssetEntity
open Domain.Assets.AssetFilters
open Interface.FilterOptionsFactory
open Interface.Cli.Formatters

module AssetArgs =
    let incorrectAssetOrientationQuery =
        "Option "
        + formatOption "'orientation'"
        + " is defined but doesn't match to the available values."

    let parseOrientation =
        function
        | null -> Ok None
        | (query: string) ->
            match query.Split("orientation=") |> Array.toList with
            | [ _; "landscape" ] -> Some Orientation.Landscape |> Ok
            | [ _; "portrait" ] -> Some Orientation.Portrait |> Ok
            | [ _; "square" ] -> Some Orientation.Square |> Ok
            | _ -> Error incorrectAssetOrientationQuery

    let getOrientationFilterOption (orientation: Orientation option) =
        { arg =
              Option(
                  orientation
                  |> Option.bind
                      (fun orientation ->
                          Some
                              { name = "orientation"
                                value = orientation })
              )
          category = "assets"
          name = "byOrientation" }

    let incorrectAssetTypeQuery =
        "Option "
        + formatOption "'type'"
        + " is defined but doesn't match to the available values."

    let parseAssetType =
        function
        | null -> Ok None
        | (query: string) ->
            match query.Split("type=") |> Array.toList with
            | [ _; "image" ] -> Some AssetType.Image |> Ok
            | [ _; "video" ] -> Some AssetType.Video |> Ok
            | [ _; "cover" ] -> Some AssetType.Cover |> Ok
            | _ -> Error incorrectAssetTypeQuery

    let getAssetTypeFilterOption (assetType: AssetType option) =
        { arg =
              Option(
                  assetType
                  |> Option.bind (fun assetType -> Some { name = "type"; value = assetType })
              )
          category = "assets"
          name = "byType" }

    let private extractQueryGroups (expression: Regex) query groupCount =
        expression.Matches query
        |> Seq.tryHead
        |> Option.bind
            (fun firstMatch ->
                match firstMatch.Groups.Count with
                | groupCount -> Some firstMatch.Groups
                | _ -> None)
        |> Option.map
            (fun groups ->
                groups
                // the groups contain the whole match, so we need to skip it
                |> Seq.skip 1
                |> Seq.map (fun group -> group.Value))
        |> Option.map Seq.toList

    type RawSizeComparator =
        { compare: string
          width: string
          height: string }

    let private toSizeComparator rawSizeComparator =
        try
            let size =
                { width = Int32.Parse(rawSizeComparator.width)
                  height = Int32.Parse(rawSizeComparator.height) }

            match rawSizeComparator.compare with
            | "=" -> Equal size |> Some
            | ">" -> Greater size |> Some
            | "<" -> Less size |> Some
            | ">=" -> GreaterOrEqual size |> Some
            | "<=" -> LessOrEqual size |> Some
            | _ -> None
        with
        | _ -> None

    let private sizeComparatorExpression = Regex @"size(>=|<=|=|>|<)(\d+):(\d+)"

    let incorrectAssetSizeQuery =
        "Option "
        + formatOption "'size'"
        + " is defined but doesn't match to the pattern."

    let parseAssetSizeComparator query =
        match query with
        | null -> Ok None
        | _ ->
            extractQueryGroups sizeComparatorExpression query 4
            |> Option.map
                (fun groups ->
                    { compare = groups.Item 0
                      width = groups.Item 1
                      height = groups.Item 2 })
            |> Option.bind toSizeComparator
            |> Option.map (fun sizeComparator -> Some sizeComparator |> Ok)
            |> Option.defaultValue (Error incorrectAssetSizeQuery)

    let getAssetSizeComparatorFilterOption (comparator: AssetSizeComparator option) =
        { arg =
              comparator
              |> Option.map
                  (fun comparator ->
                      { name = "comparator"
                        value = comparator })
              |> Option
          category = "assets"
          name = "bySize" }


    // TODO: the name doesn't really fit to the behavior
    let private unpackMatchedValue (matchedQuery: Match) (index: int) = (matchedQuery.Groups.Item index).Value

    let private AssetQueryExpression = Regex "(\w+)(=|<=|>=|>|<)[\d\w:]+"

    let getAssetFilterOptions query =
        match query with
        | null -> Ok []
        | _ ->
            let filterOptions =
                AssetQueryExpression.Matches query
                |> Seq.map
                    (fun (entry: Match) ->
                        match unpackMatchedValue entry 1 with
                        | "orientation" ->
                            unpackMatchedValue entry 0
                            |> parseOrientation
                            |> Result.map getOrientationFilterOption
                        | "type" ->
                            unpackMatchedValue entry 0
                            |> parseAssetType
                            |> Result.map getAssetTypeFilterOption
                        | "size" ->
                            unpackMatchedValue entry 0
                            |> parseAssetSizeComparator
                            |> Result.map getAssetSizeComparatorFilterOption
                        // TODO: format the option name
                        | unknownOption -> Error $"Unknown filter option {unknownOption} found.")
                |> Seq.toList

            match filterOptions, String.IsNullOrWhiteSpace(query) with
            | [], true -> Ok []
            | [], false -> Error "No filter options found in the query."
            | _ -> List.sequenceResultM filterOptions
