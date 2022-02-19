namespace Persistence

open System.IO

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

    let private concatPaths (path: string) appended =
        match path.EndsWith("/") with
        | true -> $"{path}{appended}"
        | false -> $"{path}/{appended}"

    let persistAsset asset basePath =
        recreateDirectory basePath
        |> Option.map (fun basePath -> concatPaths basePath $"{asset.id}.{asset.extension}")
        |> Option.bind (fun path -> writeFile path asset.content)
        |> Option.bind (fun _ -> Some asset)
