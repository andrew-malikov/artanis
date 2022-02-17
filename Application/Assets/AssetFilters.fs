namespace Application.Assets

open FsToolkit.ErrorHandling

open Domain.Filters
open Domain.Assets.AssetEntity
open Domain.Assets.AssetFilters
open Application.FilterOptions

module AssetFilters =
    let private mapOptionToFilter option =
        match option.name with
        | "byType" ->
            result {
                let! filterAssetType = getFilterArgumentValue<AssetType> "type" option.args

                return
                    Some
                        { name = "byType"
                          selector = filterAssetByType filterAssetType }
            }
        | "byAspectRatio" ->
            result {
                let! filterAspectRatio = getFilterArgumentValue<AspectRatio> "aspectRatio" option.args

                return
                    Some
                        { name = "byAspectRatio"
                          selector = filterAssetByAspectRation filterAspectRatio }
            }
        | "bySize" ->
            result {
                let! filterComparator = getFilterArgumentValue<Size -> Size -> bool> "comparator" option.args
                let! filterSize = getFilterArgumentValue<Size> "size" option.args

                return
                    Some
                        { name = "bySize"
                          selector = filterAssetBySizeComparator filterComparator filterSize }
            }
        | "byEqualSize" ->
            result {
                let! filterSize = getFilterArgumentValue<Size> "size" option.args

                return
                    Some
                        { selector = filterAssetByEqualSize filterSize
                          name = "byEqualSize" }
            }
        | "byOrientation" ->
            result {
                let! filterOrientation = getFilterArgumentValue<Orientation> "orientation" option.args

                return
                    Some
                        { selector = filterAssetByOrientation filterOrientation
                          name = "byOrientation" }
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
