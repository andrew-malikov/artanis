namespace Application.Assets

open Application.Assets.AssetEntity
open Application.Assets.AssetQueries

module AssetFilters =
    let filterAssetByType (filterType: AssetType) asset =
        match asset.assetType with
        | actualType when actualType = filterType -> Some asset
        | _ -> None

    let filterAssetByAspectRation filterRatio asset =
        match getAssetAspectRatio asset with
        | Some assetAspectRation when filterRatio = assetAspectRation -> Some asset
        | _ -> None

    let filterAssetBySize (compare: Size -> Size -> bool) size asset =
        match getAssetSize asset |> compare size with
        | true -> Some asset
        | _ -> None

    let filterAssetByEqualSize size asset =
        filterAssetBySize (fun filterSize assetSize -> filterSize = assetSize) size asset
