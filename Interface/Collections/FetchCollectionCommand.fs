namespace Interface.Collections

open System.ComponentModel
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Spectre.Console
open Spectre.Console.Cli

open Domain.Assets.AssetEntity
open Domain.Collections.CollectionEntity
open Application.Collections.CollectionService
open Artstation.Collections.CollectionApi
open Interface.FilterOptionsFactory
open Interface.Cli.Formatters
open Interface.Assets.AssetArgs

module FetchCollectionCommand =
    type private Args =
        { collectionId: int
          username: string
          orientation: Orientation option }

    type Settings(collectionId, username, orientation) =
        inherit CommandSettings()

        [<Description("Collection id")>]
        [<CommandArgument(0, "<collectionId>")>]
        member val collectionId: int = collectionId

        [<Description("Collection account username")>]
        [<CommandArgument(1, "<username>")>]
        member val username: string = username

        [<Description("Assets orientation like 'landscape' 'portrait' or 'square'")>]
        [<CommandOption("-o|--orientation")>]
        member val orientation: string = orientation

    let private parseArgs (settings: Settings) =
        result {
            let! orientation = parseOrientation settings.orientation

            return
                { collectionId = settings.collectionId
                  username = settings.username
                  orientation = orientation }
        }

    type Command() =
        inherit AsyncCommand<Settings>()

        override this.ExecuteAsync(context, settings) =
            let fetchingCollectionResult =
                result {
                    let! { collectionId = collectionId
                           username = username
                           orientation = orientation } = parseArgs settings

                    let collectionId: UserCollectionId =
                        { collectionId = collectionId
                          username = username }

                    return!
                        getFilteredCollection
                            (getCollection getCollectionMetadata getAllCollectionProjects)
                            (getFilterOptions [ getOrientationFilterOption orientation ])
                            collectionId
                }

            match fetchingCollectionResult with
            | Ok fetchingCollection ->
                fetchingCollection
                |> Async.map (fun collection -> AnsiConsole.WriteLine(collection.ToString()))
                |> Async.map (fun _ -> 0)
                |> Async.StartAsTask
            | Error message ->
                AnsiConsole.Markup(formatError message)
                Task.FromResult -1
