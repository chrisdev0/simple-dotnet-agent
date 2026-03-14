namespace ccode.Agent.Tools;

public class WriteFileTool : ITool
{
    public string Name => "write_file";
    public string Description => "Write content to a file at the given path, creating it if it does not exist.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("path", "The path to the file to write."),
        new("content", "The content to write to the file."),
    ];

    public async Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("path", out var path))
            return "Error: missing argument 'path'.";

        if (!arguments.TryGetValue("content", out var content))
            return "Error: missing argument 'content'.";

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, content);
        return $"Successfully wrote to {path}.";
    }
}
