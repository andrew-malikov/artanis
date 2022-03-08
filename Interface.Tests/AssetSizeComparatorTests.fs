namespace Interface.Tests

open System
open Microsoft.FSharp.Reflection
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

open Domain.Assets.AssetFilters
open Application.FilterOptions
open Interface.FilterOptionsFactory
open Interface.Assets.AssetArgs

module AssetSizeComparatorTests =
    type TestData() =
        static member ValidSizeQueries =
            [ ("size>=1920:1080", GreaterOrEqual { width = 1920; height = 1080 })
              ("size=3280:2200", Equal { width = 3280; height = 2200 })
              ("size<=400:400", LessOrEqual { width = 400; height = 400 })
              ("size>0:0", Greater { width = 0; height = 0 })
              ("size<2000:4000", Less { width = 2000; height = 4000 }) ]
            |> Seq.map FSharpValue.GetTupleFields

        static member InvalidSizeQueries =
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

    [<Theory; MemberData("ValidSizeQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN valid size query WHEN getAssetFilterOption THEN returns Ok [ SizeFilterOption ]``
        query
        expectedComparator
        =
        // Act
        let actualFilterOptions = getAssetFilterOptions query

        // Assert
        match actualFilterOptions with
        | Ok [ sizeComparator ] ->
            sizeComparator
            |> should
                equal
                { arg =
                      Some
                          { name = "comparator"
                            value = expectedComparator }
                      |> Option
                  category = "assets"
                  name = "bySize" }
        | _ -> failwith "Expected Ok [ SizeFilterOption ]"

    [<Theory; MemberData("InvalidSizeQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN invalid size query WHEN getAssetFilterOption THEN returns Error string`` invalidQuery =
        // Act
        let actualFilterOptions = getAssetFilterOptions invalidQuery

        // Assert
        match actualFilterOptions with
        | Error message -> message |> should not' EmptyString
        | _ -> failwith "Expected Error string"
