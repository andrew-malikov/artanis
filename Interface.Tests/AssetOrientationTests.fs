namespace Interface.Tests

open System
open Microsoft.FSharp.Reflection
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

open Domain.Assets.AssetEntity
open Application.FilterOptions
open Interface.FilterOptionsFactory
open Interface.Assets.AssetArgs

module AssetOrientationTests =
    type TestData() =
        static member ValidOrientationQueries =
            [ ("orientation=square ", Square)
              ("orientation=landscape", Landscape)
              (" orientation=portrait", Portrait) ]
            |> Seq.map FSharpValue.GetTupleFields

        static member InvalidOrientationQueries =
            [ "orientation=squarish"
              "orient=landscape"
              "o=portrait"
              "o"
              "orientation"
              "orientation = square"
              "orientation= portrait"
              "orientation =portrait "
              "size=landscape"
              "square" ]
            |> Seq.map (fun invalidQuery -> [ invalidQuery :> Object ] |> Seq.toArray)

    [<Theory; MemberData("ValidOrientationQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN valid orientation query WHEN getAssetFilterOptions THEN returns Ok [ OrientationFilterOption ]``
        query
        expectedOrientation
        =
        // Act
        let actualFilterOptions = getAssetFilterOptions query

        // Assert
        match actualFilterOptions with
        | Ok [ orientationFilterOption ] ->
            orientationFilterOption
            |> should
                equal
                { arg =
                      Some
                          { name = "orientation"
                            value = expectedOrientation }
                      |> Option
                  category = "assets"
                  name = "byOrientation" }
        | _ -> failwith "Expected Ok [ OrientationFilterOption ]"

    [<Theory; MemberData("InvalidOrientationQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN invalid orientation query WHEN getAssetFilterOption THEN returns Error string`` invalidQuery =
        // Act
        let actualFilterOptions = getAssetFilterOptions invalidQuery

        // Assert
        match actualFilterOptions with
        | Error message -> message |> should not' EmptyString
        | _ -> failwith "Expected Error string"
