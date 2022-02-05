namespace Domain.Assets

module AssetEntity =
    type AssetType =
        | Image
        | Cover
        | Video

    type AspectRatio = { width: int; height: int }

    type Size = { width: int; height: int }

    type Orientation =
        | Landscape
        | Portrait
        | Square

    type Asset =
        { assetType: AssetType
          hasImage: bool
          height: int
          id: int
          imageUrl: string
          position: int
          title: string option
          titleFormatted: string
          viewportConstraintType: string
          width: int }

    type FulfilledAsset =
        { assetType: AssetType
          content: byte []
          hasImage: bool
          height: int
          id: int
          imageUrl: string
          position: int
          title: string option
          titleFormatted: string
          viewportConstraintType: string
          width: int }

    let toFulfilledAsset (asset: Asset) content =
        { assetType = asset.assetType
          content = content
          hasImage = asset.hasImage
          height = asset.height
          id = asset.id
          imageUrl = asset.imageUrl
          position = asset.position
          title = asset.title
          titleFormatted = asset.titleFormatted
          viewportConstraintType = asset.viewportConstraintType
          width = asset.width }
