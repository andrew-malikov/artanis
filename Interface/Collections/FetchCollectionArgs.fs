namespace Interface.Collections

open Domain.Assets.AssetEntity
open Interface.FilterOptionsFactory
open Interface.Cli.Formatters

module FetchCollectionArgs =
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
