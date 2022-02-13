namespace Interface.Assets

open Domain.Assets.AssetEntity
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
                  |> Option.bind
                      (fun assetType ->
                          Some
                              { name = "type"
                                value = assetType })
              )
          category = "assets"
          name = "byType" }