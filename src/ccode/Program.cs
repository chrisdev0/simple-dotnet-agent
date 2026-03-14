using ccode.Shared;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Spectre.Console;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var systemPrompt = File.ReadAllText("system_prompt.txt");

var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
IChatClient chatClient = openAiKey is not null
    ? new OpenAI.Chat.ChatClient("gpt-4o-mini", openAiKey).AsIChatClient()
    : new OllamaApiClient(new Uri("http://localhost:11434"), "qwen3.5:9b");
var llm = new LlmClient(chatClient, systemPrompt);

while (true)
{
    var input = AnsiConsole.Prompt(
        new TextPrompt<string>("[green]You[/]:")
            .AllowEmpty());

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    var response = await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots8)
        .StartAsync("Thinking...", _ => llm.GenerateAsync(input));

    AnsiConsole.MarkupLine("[blue]Agent:[/]");
    MarkdownRenderer.Render(response);
    AnsiConsole.WriteLine();
}
