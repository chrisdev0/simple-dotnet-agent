using System.Diagnostics;

namespace ccode.Agent.Tools;

public class RunCommandTool : ITool
{
    public string Name => "run_command";
    public string Description => "Run a shell command and return its output.";
    public IReadOnlyList<ToolParameter> Parameters =>
    [
        new("command", "The command to run."),
    ];

    public async Task<string> ExecuteAsync(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("command", out var command))
            return "Error: missing argument 'command'.";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return string.IsNullOrWhiteSpace(error)
            ? output
            : $"{output}\nError: {error}";
    }
}
