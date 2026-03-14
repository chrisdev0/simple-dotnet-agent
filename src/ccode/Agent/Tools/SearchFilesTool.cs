namespace ccode.Agent.Tools;

public class SearchFilesTool : ITool
{
    public string Name => "search_files";
    public string Description => "Recursively search for files matching a name pattern.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("pattern", "The filename pattern to search for, e.g. *.cs"),
        new("path", "The directory to search in. Defaults to current directory.", Required: false),
    ];

    public Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("pattern", out var pattern))
            return Task.FromResult("Error: missing argument 'pattern'.");

        arguments.TryGetValue("path", out var path);
        path = string.IsNullOrWhiteSpace(path) ? "." : path;

        if (!Directory.Exists(path))
            return Task.FromResult($"Error: directory not found: {path}");

        var matches = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);

        return Task.FromResult(matches.Length == 0
            ? $"No files found matching '{pattern}' in {path}."
            : string.Join("\n", matches));
    }
}
