namespace Interface

open Spectre.Console.Cli

open Interface.Collections

module Startup =
    let application =
        let cli = CommandApp()

        cli.Configure
            (fun config ->
                config
                    .AddCommand<FetchCollectionCommand.Command>("collection")
                    .WithDescription("fetch the collection data")
                |> ignore)

        cli

    [<EntryPoint>]
    let main argv = application.Run argv
