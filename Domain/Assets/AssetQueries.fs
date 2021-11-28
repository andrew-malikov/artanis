namespace Domain.Assets

open Domain.Assets.AssetEntity

module AssetQueries =
    let getAssetSize asset =
        { width = asset.width
          height = asset.height }

    let rec gcd =
        function
        | a, b when a = b -> Some a
        | a, b when a > b -> gcd (a - b, b)
        | _ -> None

    let getAssetAspectRatio asset =
        gcd (asset.width, asset.height)
        |> Option.bind
            (fun divisor ->
                Some
                    { width = asset.width / divisor
                      height = asset.height / divisor })

    let getAssetOrientation asset =
        getAssetAspectRatio asset
        |> Option.bind
            (function
            | { width = width; height = height } when width > height -> Some Landscape
            | { width = width; height = height } when width < height -> Some Portrait
            | _ -> Some Square)
