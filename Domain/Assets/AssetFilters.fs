namespace Domain.Assets

open Domain.Assets.AssetEntity
open Domain.Assets.AssetQueries

module AssetFilters =
    let filterAssetByType (filterType: AssetType) (asset: Asset) =
        match asset.assetType with
        | actualType when actualType = filterType -> Some asset
        | _ -> None

    let filterAssetByAspectRation (filterRatio: AspectRatio) (asset: Asset) =
        match getAssetAspectRatio asset with
        | Some assetAspectRation when filterRatio = assetAspectRation -> Some asset
        | _ -> None

    let filterAssetBySize (compare: Size -> Size -> bool) size (asset: Asset) =
        match getAssetSize asset |> compare size with
        | true -> Some asset
        | _ -> None

    type AssetSizeComparator =
        | Equal
        | Less
        | Greater
        | LessOrEqual
        | GreaterOrEqual
        
    let filterAssetBySizeComparator comparator size (asset: Asset) =
        match comparator with
        | Equal -> filterAssetBySize (fun filterSize assetSize -> filterSize = assetSize) size asset
        | Less -> filterAssetBySize (fun filterSize assetSize -> filterSize > assetSize) size asset
        | Greater -> filterAssetBySize (fun filterSize assetSize -> filterSize < assetSize) size asset
        | LessOrEqual -> filterAssetBySize (fun filterSize assetSize -> filterSize >= assetSize) size asset
        | GreaterOrEqual -> filterAssetBySize (fun filterSize assetSize -> filterSize <= assetSize) size asset
    
    let filterAssetByOrientation filterOrientation (asset: Asset) =
        getAssetOrientation asset
        |> Option.bind
            (function
            | assetOrientation when assetOrientation = filterOrientation -> Some asset
            | _ -> None)
