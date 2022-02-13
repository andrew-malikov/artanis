namespace Domain.Assets

module AssetEntity =
    type AssetType =
        | Image
        | Cover
        | Video
        | Other

    type AspectRatio = { width: int; height: int }

    type Size = { width: int; height: int }

    type Orientation =
        | Landscape
        | Portrait
        | Square

    type UnverifiedAsset =
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

    type Asset =
        { assetType: AssetType
          extension: string
          hasImage: bool
          height: int
          id: int
          imageUrl: string
          position: int
          title: string option
          titleFormatted: string
          viewportConstraintType: string
          width: int }

    let verifyAsset (unverifiedAsset: UnverifiedAsset) extension : Asset option =
        let actualType =
            match unverifiedAsset.assetType with
            | Image ->
                match extension with
                | "jpg"
                | "jpeg"
                | "png" -> Image
                | _ -> Other
            | Cover -> Cover
            | Video -> Video
            | Other -> Other

        Some
            { assetType = actualType
              extension = extension
              hasImage = unverifiedAsset.hasImage
              height = unverifiedAsset.height
              id = unverifiedAsset.id
              imageUrl = unverifiedAsset.imageUrl
              position = unverifiedAsset.position
              title = unverifiedAsset.title
              titleFormatted = unverifiedAsset.titleFormatted
              viewportConstraintType = unverifiedAsset.viewportConstraintType
              width = unverifiedAsset.width }

    type FulfilledAsset =
        { assetType: AssetType
          content: byte []
          extension: string
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
          extension = asset.extension
          hasImage = asset.hasImage
          height = asset.height
          id = asset.id
          imageUrl = asset.imageUrl
          position = asset.position
          title = asset.title
          titleFormatted = asset.titleFormatted
          viewportConstraintType = asset.viewportConstraintType
          width = asset.width }
