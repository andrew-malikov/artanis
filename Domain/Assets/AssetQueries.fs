namespace Domain.Assets

open Domain.Assets.AssetEntity

module AssetQueries =
    let getAssetSize (asset: Asset) =
        { width = asset.width
          height = asset.height }

    let rec gcd =
        function
        | a, b when a = b -> Some a
        | a, b when a > b -> gcd (a - b, b)
        | a, b when b > a -> gcd (b - a, a)
        | _ -> None

    let getAssetAspectRatio (asset: Asset) : AspectRatio Option =
        gcd (asset.width, asset.height)
        |> Option.bind
            (fun divisor ->
                Some
                    { width = asset.width / divisor
                      height = asset.height / divisor })

    let getAssetOrientation (asset: Asset) =
        getAssetAspectRatio asset
        |> Option.bind
            (function
            | { width = width; height = height } when width > height -> Some Landscape
            | { width = width; height = height } when width < height -> Some Portrait
            | _ -> Some Square)
