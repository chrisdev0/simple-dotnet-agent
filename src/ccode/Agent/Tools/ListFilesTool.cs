namespace ccode.Agent.Tools;

public class ListFilesTool : ITool
{
    public string Name => "list_files";
    public string Description => "List files and directories at the given path.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("path", "The directory path to list.", Required: false),
        new("pattern", "Optional glob pattern to filter results, e.g. *.cs", Required: false),
    ];

    public Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        arguments.TryGetValue("path", out var path);
        arguments.TryGetValue("pattern", out var pattern);

        path = string.IsNullOrWhiteSpace(path) ? "." : path;

        if (!Directory.Exists(path))
            return Task.FromResult($"Error: directory not found: {path}");

        var entries = string.IsNullOrWhiteSpace(pattern)
            ? Directory.GetFileSystemEntries(path)
            : Directory.GetFileSystemEntries(path, pattern);

        return Task.FromResult(string.Join("\n", entries.Select(Path.GetFileName)));
    }
}
