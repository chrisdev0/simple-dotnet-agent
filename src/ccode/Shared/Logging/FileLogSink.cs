using System.Text.Json;

namespace ccode.Shared.Logging;

public class FileLogSink(string path = "agent.log.jsonl") : ILogSink
{
    public void Write(LogEntry entry)
    {
        var line = JsonSerializer.Serialize(entry);
        File.AppendAllText(path, line + Environment.NewLine);
    }
}
