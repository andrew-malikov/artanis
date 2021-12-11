namespace Interface.Collections

open System.ComponentModel

open Spectre.Console.Cli

module CollectionCommands =
    type FetchCollectionRequest() =
        inherit CommandSettings()

        [<Description("Artstation account username")>]
        [<CommandArgument(0, "<username>")>]
        member val username: string = null with get, set

        [<Description("Artstation collection id")>]
        [<CommandArgument(1, "<collectionId>")>]
        member val collectionId: string = null with get, set

    type FetchCollectionCommand() =
        inherit Command<FetchCollectionRequest>()

        override this.Execute(context, request) = failwith "not implemented"
