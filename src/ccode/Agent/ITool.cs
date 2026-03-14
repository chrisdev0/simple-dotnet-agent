namespace ccode.Agent;

public record ToolParameter(string Name, string Description, bool Required = true);

public record ToolCall(string Tool, Dictionary<string, string> Arguments);

public interface ITool
{
    string Name { get; }
    string Description { get; }
    IReadOnlyList<ToolParameter> Parameters { get; }
    Task<string> ExecuteAsync(Dictionary<string, string> arguments);
}
