namespace Application.Assets

module AssetEntity =
    type AssetType =
        | Image
        | Cover
        | Video

    type AspectRatio = { width: int; height: int }

    type Size = { width: int; height: int }

    type Orientation =
        | Horizontal
        | Vertical

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
