using Spectre.Console;

namespace ccode.Shared.Logging;

public class TuiLogSink : ILogSink
{
    public void Write(LogEntry entry)
    {
        var duration = $"[grey]{entry.DurationMs:F0}ms[/]";
        var error = entry.Error is not null ? $" [red]ERROR: {Markup.Escape(entry.Error)}[/]" : "";

        switch (entry.EventType)
        {
            case LogEventType.LlmCall:
                AnsiConsole.MarkupLine($"[dim]▸ LLM[/] {duration}{error}");
                if (entry.Data.TryGetValue("prompt", out var prompt) && prompt is string p)
                    AnsiConsole.MarkupLine($"[dim]  prompt:[/] [grey]{Markup.Escape(Truncate(p, 120))}[/]");
                if (entry.Data.TryGetValue("response", out var response) && response is string r)
                    AnsiConsole.MarkupLine($"[dim]  response:[/] [grey]{Markup.Escape(Truncate(r, 120))}[/]");
                break;

            case LogEventType.ToolCall:
                var toolName = entry.Data.GetValueOrDefault("tool")?.ToString() ?? "?";
                AnsiConsole.MarkupLine($"[dim]▸ Tool[/] [grey]{Markup.Escape(toolName)}[/] {duration}{error}");
                break;

            case LogEventType.Decision:
                var decision = entry.Data.GetValueOrDefault("selected")?.ToString() ?? "?";
                AnsiConsole.MarkupLine($"[dim]▸ Decision[/] [grey]{Markup.Escape(decision)}[/] {duration}{error}");
                break;

            case LogEventType.Memory:
                var op = entry.Data.GetValueOrDefault("operation")?.ToString() ?? "?";
                var value = entry.Data.GetValueOrDefault("value")?.ToString() ?? "";
                AnsiConsole.MarkupLine($"[dim]▸ Memory[/] [grey]{Markup.Escape(op)}: {Markup.Escape(Truncate(value, 80))}[/]");
                break;
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
