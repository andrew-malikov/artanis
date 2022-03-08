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

module AssetTypeTests =
    type TestData() =
        static member ValidAssetTypeQueries =
            [ ("type=cover ", Cover)
              ("type=image", Image)
              (" type=video", Video) ]
            |> Seq.map FSharpValue.GetTupleFields

        static member InvalidAssetTypeQueries =
            [ "type=picture"
              "type=other"
              "t=cover"
              "=image"
              "t"
              "type"
              "type = other"
              "type= animation"
              "type =video "
              "orientation=image"
              "video" ]
            |> Seq.map (fun invalidQuery -> [ invalidQuery :> Object ] |> Seq.toArray)

    [<Theory; MemberData("ValidAssetTypeQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN valid assetType query WHEN getAssetFilterOptions THEN returns Ok [ AssetTypeFilterOption ]``
        query
        expectedAssetType
        =
        // Act
        let actualFilterOptions = getAssetFilterOptions query

        // Assert
        match actualFilterOptions with
        | Ok [ assetTypeFilterOption ] ->
            assetTypeFilterOption
            |> should
                equal
                { arg =
                      Some
                          { name = "type"
                            value = expectedAssetType }
                      |> Option
                  category = "assets"
                  name = "byType" }
        | _ -> failwith "Expected Ok [ AssetTypeFilterOption ]"

    [<Theory; MemberData("InvalidAssetTypeQueries", MemberType = typeof<TestData>)>]
    let ``GIVEN invalid assetType query WHEN getAssetFilterOption THEN returns Error string`` invalidQuery =
        // Act
        let actualFilterOptions = getAssetFilterOptions invalidQuery

        // Assert
        match actualFilterOptions with
        | Error message -> message |> should not' EmptyString
        | _ -> failwith "Expected Error string"
