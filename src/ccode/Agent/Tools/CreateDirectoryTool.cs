namespace ccode.Agent.Tools;

public class CreateDirectoryTool : ITool
{
    public string Name => "create_directory";
    public string Description => "Create a directory at the given path, including any parent directories.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("path", "The directory path to create."),
    ];

    public Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("path", out var path))
            return Task.FromResult("Error: missing argument 'path'.");

        Directory.CreateDirectory(path);
        return Task.FromResult($"Successfully created directory: {path}");
    }
}
