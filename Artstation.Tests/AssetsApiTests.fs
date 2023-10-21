namespace Artstation.Tests

open System
open Microsoft.FSharp.Reflection
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

module AssetsApiTests =
    type TestData() =
            static member ValidUnverifiedAssets =
                [ ({ assetType = AssetType.Image,
                     hasImage = true 
                     height = 720  
                     id = 99
                     imageUrl = "string"
                     position = Some 1 
                     title = Some "The First" 
                     titleFormatted = "string"
                     viewportConstraintType = "string"
                     width = 1280 }, )
                  ]
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

