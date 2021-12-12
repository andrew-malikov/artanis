namespace Interface.Collections

open System.ComponentModel

open Spectre.Console
open Spectre.Console.Cli

open Application.Collections
open Application.Projects
open Artstation.Collections
open Artstation.Projects

module CollectionCommands =
    type FetchCollectionRequest(collectionId, username) =
        inherit CommandSettings()

        [<Description("Collection id")>]
        [<CommandArgument(0, "<collectionId>")>]
        member val collectionId: int = collectionId

        [<Description("Collection account username")>]
        [<CommandArgument(1, "<username>")>]
        member val username: string = username


    type FetchCollectionCommand() =
        inherit AsyncCommand<FetchCollectionRequest>()

        override this.ExecuteAsync(context, request) =
            async {
                let! collection =
                    CollectionUseCases.getCollection
                        (CollectionUseCases.getMetadata
                            CollectionApi.getCollection
                            CollectionFactory.getCollectionMetadata)
                        (ProjectUseCases.getProjects
                            CollectionApi.getAllCollectionProjects
                            ProjectFactory.getProject)
                        request.collectionId
                        request.username

                AnsiConsole.WriteLine(collection.ToString())
                
                return 0
            }
            |> Async.StartAsTask
