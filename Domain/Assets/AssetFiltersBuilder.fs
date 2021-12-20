namespace Domain.Assets

open FsToolkit.ErrorHandling

open Domain.FilterOptions
open Domain.Assets.AssetEntity
open Domain.Assets.AssetFilters

module AssetFiltersBuilder =
    let private mapOptionToFilter option =
        match option.name with
        | "byType" ->
            result {
                let! filterAssetType = getFilterArgumentValue<AssetType> "type" option.args

                return Some(filterAssetByType filterAssetType)
            }
        | "byAspectRatio" ->
            result {
                let! filterAspectRatio = getFilterArgumentValue<AspectRatio> "aspectRatio" option.args

                return Some(filterAssetByAspectRation filterAspectRatio)
            }
        | "bySize" ->
            result {
                let! filterComparator = getFilterArgumentValue<Size -> Size -> bool> "comparator" option.args
                let! filterSize = getFilterArgumentValue<Size> "size" option.args

                return Some(filterAssetBySize filterComparator filterSize)
            }
        | "byEqualSize" ->
            result {
                let! filterSize = getFilterArgumentValue<Size> "size" option.args

                return Some(filterAssetByEqualSize filterSize)
            }
        | "byOrientation" ->
            result {
                let! filterOrientation = getFilterArgumentValue<Orientation> "orientation" option.args

                return Some(filterAssetByOrientation filterOrientation)
            }
        | _ -> Ok None

    let buildAssetFilters options =
        result {
            let! validHandlers =
                options
                |> List.filter (fun option -> option.category = "assets")
                |> List.traverseResultM mapOptionToFilter

            return validHandlers |> List.choose id
        }