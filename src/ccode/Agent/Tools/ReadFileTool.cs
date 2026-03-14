namespace ccode.Agent.Tools;

public class ReadFileTool : ITool
{
    public string Name => "read_file";
    public string Description => "Read the contents of a file at the given path.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("path", "The path to the file to read."),
    ];

    public async Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("path", out var path))
            return "Error: missing argument 'path'.";

        if (!File.Exists(path))
            return $"Error: file not found: {path}";

        return await File.ReadAllTextAsync(path);
    }
}
