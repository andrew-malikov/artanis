namespace Interface.Tests

open System
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

        static member InvalidComparatorQueries =
            [ "comparator>=1920:1080"
              "size=-3280:2200"
              "size<=400:-400"
              "size>-0:-0"
              "size==2000:4000"
              "size=480.5:330"
              "size<=445:-400.33"
              ">0:0"
              "size>>2035:00" ]
            |> Seq.map (fun invalidQuery -> [ invalidQuery :> Object ] |> Seq.toArray)

    [<Theory; MemberData("ValidComparatorQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN valid comparator query WHEN parseAssetSizeComparator THEN returns Ok Some Comparator``
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

    [<Theory; MemberData("InvalidComparatorQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN invalid comparator query WHEN parseAssetSizeComparator THEN returns Error string`` invalidQuery =
        // Act
        let actualComparator = parseAssetSizeComparator invalidQuery

        // Assert
        match actualComparator with
        | Error message -> message |> should not' EmptyString
        | _ -> failwith "Expected Error string"
