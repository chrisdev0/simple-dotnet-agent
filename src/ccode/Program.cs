using Spectre.Console;

while (true)
{
    var input = AnsiConsole.Prompt(
        new TextPrompt<string>("[green]You[/]:")
            .AllowEmpty());

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    AnsiConsole.MarkupLine("[blue]Agent:[/] No functionality has been implemented yet.");
    AnsiConsole.WriteLine();
}
