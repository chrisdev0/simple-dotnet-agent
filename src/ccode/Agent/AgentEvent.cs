namespace ccode.Agent;

public record AgentEvent(AgentEventType Type, string Content)
{
    public static AgentEvent ToolCall(string content) => new(AgentEventType.ToolCall, content);
    public static AgentEvent ToolResult(string content) => new(AgentEventType.ToolResult, content);
    public static AgentEvent Response(string content) => new(AgentEventType.Response, content);
    public static AgentEvent Error(string content) => new(AgentEventType.Error, content);
}

public enum AgentEventType { ToolCall, ToolResult, Response, Error }
