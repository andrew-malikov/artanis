namespace Artstation.Assets

open Domain.Assets.AssetEntity

module AssetFactory =
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

    let getAssetType =
        function
        | "image" -> Some Image
        | "video" -> Some Video
        | "Cover" -> Some Cover
        | _ -> None
    
    let getAsset assetResponse =
        getAssetType assetResponse.assetType
        |> Option.bind
            (fun assetType ->
                Some
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
