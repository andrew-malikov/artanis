namespace Persistence

open System.IO
open System.Text.RegularExpressions

open Domain.Assets.AssetEntity

module LocalAssetPersistenceService =
    let private recreateDirectory path =
        try
            match Directory.Exists path with
            | true -> path |> Some
            | false -> Directory.CreateDirectory path |> string |> Some
        with
        | error ->
            // TODO: turn to a new type
            printfn $"Could not create directory %s{path} %s{error.ToString()}"
            None

    let private writeFile path content =
        try
            File.WriteAllBytes(path, content) |> Some
        with
        | error ->
            // TODO: turn to a new type
            printfn $"Failed to write file %s{path}: %s{error.ToString()}"
            None

    let private assetExtensionsExpression = Regex @"\.(\w+)\?\d+"

    let private getAssetExtension asset =
        let matches =
            assetExtensionsExpression.Matches asset.imageUrl

        matches
        |> Seq.filter (fun (entry: Match) -> entry.Groups.Count = 2)
        |> Seq.tryHead
        |> Option.bind (fun entry -> entry.Groups.Item(1).Value |> Some)

    let private concatPaths (path: string) appended =
        match path.EndsWith("/") with
        | true -> $"{path}{appended}"
        | false -> $"{path}/{appended}"

    let persistAsset asset basePath =
        recreateDirectory basePath
        |> Option.bind
            (fun basePath ->
                match getAssetExtension asset with
                | Some extension ->
                    concatPaths basePath $"{asset.id}.{extension}"
                    |> Some
                | None -> concatPaths basePath asset.id |> Some)
        |> Option.bind (fun path -> writeFile path asset.content)
        |> Option.bind (fun _ -> Some asset)
