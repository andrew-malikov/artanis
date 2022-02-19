namespace Interface.Tests

open Microsoft.FSharp.Reflection
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

open Domain.Assets.AssetFilters
open Interface.Assets.AssetArgs

module AssetSizeComparatorTests =
    type TestData() =
        static member ValidComparatorQueries =
            [ ("size>=1920:1080", GreaterOrEqual { width = 1920; height = 1080 })
              ("size=3280:2200", Equal { width = 3280; height = 2200 })
              ("size<=400:400", LessOrEqual { width = 400; height = 400 })
              ("size>0:0", Greater { width = 0; height = 0 })
              ("size<2000:4000", Less { width = 2000; height = 4000 }) ]
            |> Seq.map FSharpValue.GetTupleFields

    [<Theory; MemberData("ValidComparatorQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN valid greater query WHEN parseAssetSizeComparator THEN returns Ok Some Comparator``
        query
        expectedComparator
        =
        // Act
        let actualComparator = parseAssetSizeComparator query

        // Assert
        match actualComparator with
        | Ok comparator ->
            match comparator with
            | Some assetSizeComparator ->
                assetSizeComparator
                |> should be (equal expectedComparator)
            | _ -> failwith "Expected Some Comparator"
        | _ -> failwith "Expected Ok Some Comparator"
