namespace Interface.Commands.FetchCollectionAssets

open System.ComponentModel

open Domain.Assets.AssetFilters
open FsToolkit.ErrorHandling

open Spectre.Console
open Spectre.Console.Cli

open Domain.Assets.AssetEntity
open Interface.Assets.AssetArgs
open Interface.Cli.Formatters
open Interface.Commands.FetchCollectionAssets.Actors

module Command =
    type private Args =
        { collectionId: int
          username: string
          output: string
          assetType: AssetType option
          orientation: Orientation option
          sizeComparator: AssetSizeComparator option }

    type Settings(collectionId, username, output, orientation, assetType, sizeQuery) =
        inherit CommandSettings()

        [<Description("Collection id")>]
        [<CommandArgument(0, "<collectionId>")>]
        member val collectionId: int = collectionId

        [<Description("Collection account username")>]
        [<CommandArgument(1, "<username>")>]
        member val username: string = username

        [<Description("Output directory")>]
        [<CommandArgument(2, "<output>")>]
        member val output: string = output

        [<Description("Assets orientation like 'landscape' 'portrait' or 'square'")>]
        [<CommandOption("-o|--orientation")>]
        member val orientation: string = orientation

        [<Description("Assets type like 'image', 'video' or 'cover'")>]
        [<CommandOption("-t|--type")>]
        member val assetType: string = assetType

        // TODO: improve the documentation
        [<Description("Size query like 'size=3280:2160'")>]
        [<CommandOption("-s|--size")>]
        member val sizeQuery: string = sizeQuery

    // TODO: adjust the model due to the new output argument
    let private parseArgs (settings: Settings) =
        result {
            let! orientation = parseOrientation settings.orientation
            let! assetType = parseAssetType settings.assetType
            let! assetSizeComparator = parseAssetSizeComparator settings.sizeQuery

            return
                { collectionId = settings.collectionId
                  username = settings.username
                  output = settings.output
                  assetType = assetType
                  orientation = orientation
                  sizeComparator = assetSizeComparator }
        }

    type Handler() =
        inherit Command<Settings>()

        override this.Execute(context, settings) =
            parseArgs settings
            |> Result.map
                (fun args ->
                    { collectionId = args.collectionId
                      username = args.username
                      assetType = args.assetType
                      orientation = args.orientation
                      outputDirectory = args.output })
            |> Result.eitherMap
                (fun request ->
                    AnsiConsole
                        .Status()
                        .Start("Initializing", (fun context -> runCollectionActorSystem context request)))
                (fun message -> AnsiConsole.Markup(formatError message))
            |> Result.fold (fun _ -> 0) (fun _ -> 1)
