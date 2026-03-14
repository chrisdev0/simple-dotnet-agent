using System.Diagnostics;

namespace ccode.Shared.Logging;

public class AgentLogger(IEnumerable<ILogSink> sinks)
{
    private readonly List<ILogSink> _sinks = sinks.ToList();

    public void LogLlmCall(string prompt, string response, double durationMs, string? error = null) =>
        Write(new LogEntry(LogEventType.LlmCall, DateTime.UtcNow, durationMs, new()
        {
            ["prompt"] = prompt,
            ["response"] = response,
            ["promptLength"] = prompt.Length,
            ["responseLength"] = response.Length,
        }, error));

    public void LogToolCall(string tool, Dictionary<string, string> arguments, string result, double durationMs, string? error = null) =>
        Write(new LogEntry(LogEventType.ToolCall, DateTime.UtcNow, durationMs, new()
        {
            ["tool"] = tool,
            ["arguments"] = arguments,
            ["result"] = result,
        }, error));

    public void LogDecision(string[] choices, string? selected, double durationMs, string? error = null) =>
        Write(new LogEntry(LogEventType.Decision, DateTime.UtcNow, durationMs, new()
        {
            ["choices"] = choices,
            ["selected"] = selected,
        }, error));

    public void LogMemory(string operation, string value) =>
        Write(new LogEntry(LogEventType.Memory, DateTime.UtcNow, 0, new()
        {
            ["operation"] = operation,
            ["value"] = value,
        }));

    public static Stopwatch StartTimer() => Stopwatch.StartNew();

    private void Write(LogEntry entry)
    {
        foreach (var sink in _sinks)
            sink.Write(entry);
    }
}
