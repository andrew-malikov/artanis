namespace Artstation.Assets

open System.Text.RegularExpressions
open FsToolkit.ErrorHandling

open Flurl.Http

open Domain.Assets.AssetEntity

module AssetApi =
    type AssetResponse =
        { assetType: string
          hasEmbeddedPlayer: bool
          hasImage: bool
          height: int
          id: int
          imageUrl: string
          position: int
          title: string option
          titleFormatted: string
          viewportConstraintType: string
          width: int }

    // TODO: map the else to Other
    let getAssetType =
        function
        | "image" -> Some Image
        | "video" -> Some Video
        | "cover" -> Some Cover
        | _ -> None

    let private assetExtensionExpression = Regex @"\.(\w+)\?\d+"

    let private getAssetExtension (asset: UnverifiedAsset) =
        assetExtensionExpression.Matches asset.imageUrl
        |> Seq.filter (fun (entry: Match) -> entry.Groups.Count = 2)
        |> Seq.tryHead
        |> Option.bind (fun entry -> entry.Groups.Item(1).Value |> Some)

    let toAsset assetResponse =
        let unverifiedAsset =
            getAssetType assetResponse.assetType
            |> Option.map
                (fun assetType ->
                    { assetType = assetType
                      hasImage = assetResponse.hasImage
                      height = assetResponse.height
                      id = assetResponse.id
                      imageUrl = assetResponse.imageUrl
                      position = assetResponse.position
                      title = assetResponse.title
                      titleFormatted = assetResponse.titleFormatted
                      viewportConstraintType = assetResponse.viewportConstraintType
                      width = assetResponse.width })

        let extension =
            unverifiedAsset |> Option.bind getAssetExtension

        Option.map2 verifyAsset unverifiedAsset extension
        |> Option.flatten

    let fetchAsset (asset: Asset) =
        asset.imageUrl.GetBytesAsync()
        |> Async.AwaitTask
        |> Async.map (fun content -> toFulfilledAsset asset content)
