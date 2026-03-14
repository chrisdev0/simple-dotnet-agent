namespace ccode.Shared.Logging;

public enum LogEventType { LlmCall, ToolCall, Decision, Memory }

public record LogEntry(
    LogEventType EventType,
    DateTime Timestamp,
    double DurationMs,
    Dictionary<string, object?> Data,
    string? Error = null
);
