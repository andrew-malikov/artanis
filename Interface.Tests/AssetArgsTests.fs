namespace Interface.Tests

open Microsoft.FSharp.Reflection
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

open Domain.Assets.AssetFilters
open Domain.Assets.AssetEntity
open Application.FilterOptions
open Interface.FilterOptionsFactory
open Interface.Assets.AssetArgs

module AssetArgsTests =
    type TestData() =
        static member ValidQueries =
            [ ("size=1920:1080 type=cover",
               [ { arg =
                       Some
                           { name = "comparator"
                             value = Equal { width = 1920; height = 1080 } }
                       |> Option
                   category = "assets"
                   name = "bySize" }
                 { arg = Some { name = "type"; value = Cover } |> Option
                   category = "assets"
                   name = "byType" } ])
              ("size>=1920:1080   orientation=portrait   type=image",
               [ { arg =
                       Some
                           { name = "comparator"
                             value = GreaterOrEqual { width = 1920; height = 1080 } }
                       |> Option
                   category = "assets"
                   name = "bySize" }
                 { arg =
                       Some
                           { name = "orientation"
                             value = Portrait }
                       |> Option
                   category = "assets"
                   name = "byOrientation" }
                 { arg = Some { name = "type"; value = Image } |> Option
                   category = "assets"
                   name = "byType" } ])
              ("", [])
              ("    ", [])
              (null, []) ]
            |> Seq.map FSharpValue.GetTupleFields

        static member InvalidQueries =
            [ ("size==3280:440", "No filter options found in the query.")
              ("size>>222121", "No filter options found in the query.")
              ("size>=455:555 orientation=circle", incorrectAssetOrientationQuery)
              ("orientation=portrait    type=animation size<=200:200", incorrectAssetTypeQuery)
              ("orientation=portrait    type=image size<=200200", incorrectAssetSizeQuery)
              ("type", "No filter options found in the query.")
              ("scale=75", "Unknown filter option scale found.") ]
            |> Seq.map FSharpValue.GetTupleFields

    [<Theory; MemberData("ValidQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN valid query WHEN getAssetFilterOptions THEN returns Ok list<FilterOption>``
        query
        expectedFilterOptions
        =
        // Act
        let actualFilterOptions = getAssetFilterOptions query

        // Assert
        match actualFilterOptions with
        | Ok filterOptions ->
            filterOptions
            |> should equal expectedFilterOptions
        | _ -> failwith "Expected Ok list<FilterOption>"

    [<Theory; MemberData("InvalidQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN invalid query WHEN getAssetFilterOption THEN returns Error string`` invalidQuery errorMessage =
        // Act
        let actualFilterOptions = getAssetFilterOptions invalidQuery

        // Assert
        match actualFilterOptions with
        | Error message -> message |> should equal errorMessage
        | _ -> failwith "Expected Error string"
