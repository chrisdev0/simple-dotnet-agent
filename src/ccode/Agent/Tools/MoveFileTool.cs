namespace ccode.Agent.Tools;

public class MoveFileTool : ITool
{
    public string Name => "move_file";
    public string Description => "Move or rename a file from one path to another.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("source", "The path of the file to move."),
        new("destination", "The destination path."),
    ];

    public Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("source", out var source))
            return Task.FromResult("Error: missing argument 'source'.");

        if (!arguments.TryGetValue("destination", out var destination))
            return Task.FromResult("Error: missing argument 'destination'.");

        if (!File.Exists(source))
            return Task.FromResult($"Error: file not found: {source}");

        var directory = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.Move(source, destination, overwrite: true);
        return Task.FromResult($"Successfully moved {source} to {destination}.");
    }
}
