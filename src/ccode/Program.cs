using ccode.Agent;
using ccode.Agent.Tools;
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

IReadOnlyList<ITool> tools =
[
    new ReadFileTool(),
    new WriteFileTool(),
    new ListFilesTool(),
    new RunCommandTool(),
    new MoveFileTool(),
    new CreateDirectoryTool(),
    new GetWorkingDirectoryTool(),
    new SearchFilesTool(),
    new SearchInFilesTool(),
];

var memory = new Memory();
memory.Load();

var agent = new Agent(llm, tools, memory);

while (true)
{
    var input = AnsiConsole.Prompt(
        new TextPrompt<string>("[green]You[/]:")
            .AllowEmpty());

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    AnsiConsole.WriteLine();
    await agent.RunAsync(input, RenderEvent);
    AnsiConsole.WriteLine();
}

void RenderEvent(AgentEvent e)
{
    switch (e.Type)
    {
        case AgentEventType.ToolCall:
            AnsiConsole.MarkupLine($"[grey]⚙ {Markup.Escape(e.Content)}[/]");
            break;
        case AgentEventType.ToolResult:
            AnsiConsole.MarkupLine($"[grey]  → {Markup.Escape(e.Content.ReplaceLineEndings(" "))}[/]");
            break;
        case AgentEventType.Response:
            AnsiConsole.MarkupLine("[blue]Agent:[/]");
            MarkdownRenderer.Render(e.Content);
            break;
        case AgentEventType.Error:
            AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(e.Content)}[/]");
            break;
    }
}
