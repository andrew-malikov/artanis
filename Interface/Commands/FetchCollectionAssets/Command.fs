namespace Interface.Commands.FetchCollectionAssets

open System.ComponentModel

open FsToolkit.ErrorHandling

open Interface.FilterOptionsFactory
open Spectre.Console
open Spectre.Console.Cli

open Interface.Assets.AssetArgs
open Interface.Cli.Formatters
open Interface.Commands.FetchCollectionAssets.Actors

module Command =
    type private Args =
        { collectionId: int
          username: string
          output: string
          assetFilterOptions: FilterOptionEntry list }


    type Settings(collectionId, username, output, assetsQuery) =
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

        [<Description("Assets query")>]
        [<CommandOption("-q|--query")>]
        member val assetsQuery: string = assetsQuery

    // TODO: adjust the model due to the new output argument
    let private parseArgs (settings: Settings) =
        result {
            let! assetFilterOptions = getAssetFilterOptions settings.assetsQuery

            return
                { collectionId = settings.collectionId
                  username = settings.username
                  output = settings.output
                  assetFilterOptions = assetFilterOptions }
        }

    type Handler() =
        inherit Command<Settings>()

        override this.Execute(_, settings) =
            parseArgs settings
            |> Result.map
                (fun args ->
                    { collectionId = args.collectionId
                      username = args.username
                      filterOptions = args.assetFilterOptions
                      outputDirectory = args.output })
            |> Result.eitherMap
                (fun request ->
                    AnsiConsole
                        .Status()
                        .Start("Initializing", (fun context -> runCollectionActorSystem context request)))
                (fun message -> AnsiConsole.Markup(formatError message))
            |> Result.fold (fun _ -> 0) (fun _ -> 1)
