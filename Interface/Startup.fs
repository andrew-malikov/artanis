namespace Interface

open Spectre.Console.Cli

open Interface.Collections.CollectionCommands

module Startup =
    let application =
        let cli = CommandApp()

        cli.Configure
            (fun config ->
                config
                    .AddCommand<FetchCollectionCommand>("collection")
                    .WithDescription("fetch collection data")
                |> ignore)

        cli

    [<EntryPoint>]
    let main argv = application.Run argv
