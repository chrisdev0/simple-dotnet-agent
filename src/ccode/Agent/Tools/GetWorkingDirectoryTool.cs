namespace ccode.Agent.Tools;

public class GetWorkingDirectoryTool : ITool
{
    public string Name => "get_working_directory";
    public string Description => "Returns the current working directory.";
    public IReadOnlyList<ToolParameter> Parameters => [];

    public Task<string> ExecuteAsync(Dictionary<string, string> arguments) =>
        Task.FromResult(Directory.GetCurrentDirectory());
}
