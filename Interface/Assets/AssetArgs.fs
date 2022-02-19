namespace Interface.Assets

open System
open System.Text.RegularExpressions

open Domain.Assets.AssetEntity
open Domain.Assets.AssetFilters
open Interface.FilterOptionsFactory
open Interface.Cli.Formatters

module AssetArgs =
    let parseOrientation =
        function
        | "landscape" -> Some Orientation.Landscape |> Ok
        | "portrait" -> Some Orientation.Portrait |> Ok
        | "square" -> Some Orientation.Square |> Ok
        | null -> Ok None
        | _ ->
            "Option "
            + formatOption "'orientation'"
            + " is defined but doesn't match to the available values."
            |> Error

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

    let parseAssetType =
        function
        | "image" -> Some AssetType.Image |> Ok
        | "video" -> Some AssetType.Video |> Ok
        | "cover" -> Some AssetType.Cover |> Ok
        | null -> Ok None
        | _ ->
            "Option "
            + formatOption "'type'"
            + " is defined but doesn't match to the available values."
            |> Error

    let getAssetTypeFilterOption (assetType: AssetType option) =
        { arg =
              Option(
                  assetType
                  |> Option.bind (fun assetType -> Some { name = "type"; value = assetType })
              )
          category = "assets"
          name = "byType" }

    let private sizeComparatorExpression = Regex @"size(>=|<=|=|>|<)(\d+):(\d+)"

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

    let parseAssetSizeComparator query =
        match query with
        | null -> Ok None
        | _ ->
            sizeComparatorExpression.Matches query
            |> Seq.filter (fun (entry: Match) -> entry.Groups.Count = 4)
            |> Seq.tryHead
            |> Option.map
                (fun entry ->
                    entry.Groups
                    |> Seq.skip 1
                    |> Seq.map (fun (group: Group) -> group.Value)
                    |> Seq.toList)
            |> Option.map
                (fun groups ->
                    { compare = groups.Item 0
                      width = groups.Item 1
                      height = groups.Item 2 })
            |> Option.bind toSizeComparator
            |> Option.map (fun sizeComparator -> Some sizeComparator |> Ok)
            |> Option.defaultValue (
                "Option "
                + formatOption "'size'"
                + " is defined but doesn't match to the pattern."
                |> Error
            )

    let getAssetSizeComparatorFilterOption (comparator: AssetSizeComparator option) =
        { arg =
              Option(
                  comparator
                  |> Option.bind
                      (fun comparator ->
                          Some
                              { name = "comparator"
                                value = comparator })
              )
          category = "assets"
          name = "bySize" }
