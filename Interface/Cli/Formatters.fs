namespace Interface.Cli

module Formatters =
    let formatError message = $"[red]Error:[/] {message} \n"

    let formatOption optionName = $"[yellow]{optionName}[/]"
