namespace Interface.Tests

open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

open Domain.Assets.AssetFilters
open Interface.Assets.AssetArgs

module AssetSizeComparatorTests =
    [<Fact>]
    let ``GIVEN valid greater query WHEN parseAssetSizeComparator THEN returns Ok Some Comparator`` () =
        //  Arrange
        let validGreaterQuery = "size>=1920:1080"

        // Act
        let actualComparator =
            parseAssetSizeComparator validGreaterQuery

        // Assert
        match actualComparator with
        | Ok comparator ->
            match comparator with
            | Some assetSizeComparator ->
                assetSizeComparator
                |> should be (ofCase <@ GreaterOrEqual { width = 1920; height = 1080 } @>)
            | _ -> failwith "Expected Some Comparator"
        | _ -> failwith "Expected Ok Some Comparator"
