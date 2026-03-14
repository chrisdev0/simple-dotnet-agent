namespace ccode.Agent.Tools;

public class SearchInFilesTool : ITool
{
    public string Name => "search_in_files";
    public string Description => "Search for a text pattern within files, returning matching lines with file and line number.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("pattern", "The text to search for."),
        new("path", "The directory to search in. Defaults to current directory.", Required: false),
        new("file_pattern", "Optional filename filter, e.g. *.cs. Defaults to all files.", Required: false),
    ];

    public async Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("pattern", out var pattern))
            return "Error: missing argument 'pattern'.";

        arguments.TryGetValue("path", out var path);
        arguments.TryGetValue("file_pattern", out var filePattern);

        path = string.IsNullOrWhiteSpace(path) ? "." : path;
        filePattern = string.IsNullOrWhiteSpace(filePattern) ? "*" : filePattern;

        if (!Directory.Exists(path))
            return $"Error: directory not found: {path}";

        var files = Directory.GetFiles(path, filePattern, SearchOption.AllDirectories);
        var results = new List<string>();

        foreach (var file in files)
        {
            var lines = await File.ReadAllLinesAsync(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    results.Add($"{file}:{i + 1}: {lines[i].Trim()}");
            }
        }

        return results.Count == 0
            ? $"No matches found for '{pattern}'."
            : string.Join("\n", results);
    }
}
