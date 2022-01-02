namespace Interface.Collections

open System.ComponentModel
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Spectre.Console
open Spectre.Console.Cli

open Domain.Assets.AssetEntity
open Application.Collections
open Application.Projects
open Artstation.Collections
open Artstation.Projects
open Interface.FilterOptionsFactory
open Interface.Cli.Formatters

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

    let private parseOrientation =
        function
        | "landscape" -> Some Orientation.Landscape |> Ok
        | "portrait" -> Some Orientation.Portrait |> Ok
        | "square" -> Some Orientation.Square |> Ok
        | null -> Ok None
        | _ ->
            "Option "
            + formatOption "'orientation'"
            + " is defined but doesn't match to the available values."
            |> Error

    let private parseArgs (settings: Settings) =
        result {
            let! orientation = parseOrientation settings.orientation

            return
                { collectionId = settings.collectionId
                  username = settings.username
                  orientation = orientation }
        }

    let private getOrientationFilterOption orientation =
        { arg =
              Option(
                  orientation
                  |> Option.bind
                      (fun orientation ->
                          Some
                              { name = "orientation"
                                value = orientation })
              )
          category = "assets"
          name = "byOrientation" }


    type Command() =
        inherit AsyncCommand<Settings>()

        override this.ExecuteAsync(context, settings) =
            let getCollection =
                CollectionService.getCollection
                    (CollectionService.getMetadata CollectionApi.getCollection CollectionFactory.getCollectionMetadata)
                    (ProjectService.getProjects CollectionApi.getAllCollectionProjects ProjectFactory.getProject)

            let fetchingCollectionResult =
                result {
                    let! { collectionId = collectionId
                           username = username
                           orientation = orientation } = parseArgs settings

                    return!
                        CollectionService.getFilteredCollection
                            getCollection
                            (getFilterOptions [ getOrientationFilterOption orientation ])
                        <| { collectionId = collectionId
                             username = username }
                }

            match fetchingCollectionResult with
            | Ok fetchingCollection ->
                async {
                    let! collection = fetchingCollection
                    AnsiConsole.WriteLine(collection.ToString())

                    return 0
                }
                |> Async.StartAsTask
            | Error message ->
                AnsiConsole.Markup(formatError message)
                Task.FromResult -1
