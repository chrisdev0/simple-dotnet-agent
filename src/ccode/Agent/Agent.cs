using ccode.Shared;
using ccode.Shared.Logging;

namespace ccode.Agent;

public enum AgentAction { UseTool, Respond, Done }

public class Agent(LlmClient llm, IReadOnlyList<ITool> tools, Memory memory, AgentLogger logger)
{
    private readonly AgentState _state = new();

    public async Task RunAsync(string goal, Action<AgentEvent> onEvent, int maxSteps = 10)
    {
        _state.Reset();

        while (!_state.Done && _state.Steps < maxSteps)
        {
            _state.IncrementStep();

            var action = await llm.DecideAsync<AgentAction>(AgentPrompts.Decide(goal, _state, memory));

            if (action == AgentAction.UseTool)
                await ExecuteToolStepAsync(goal, onEvent);
            else if (action is null or AgentAction.Respond)
                await RespondAsync(goal, onEvent);
            else if (action == AgentAction.Done)
                _state.MarkDone();
        }

        memory.Save();
    }

    private async Task ExecuteToolStepAsync(string goal, Action<AgentEvent> onEvent)
    {
        var toolCall = await llm.RequestToolAsync(AgentPrompts.SelectTool(goal, _state, memory), tools);

        if (toolCall is null)
        {
            onEvent(AgentEvent.Error("Failed to determine tool call."));
            return;
        }

        var tool = tools.FirstOrDefault(t => t.Name.Equals(toolCall.Tool, StringComparison.OrdinalIgnoreCase));
        if (tool is null)
        {
            onEvent(AgentEvent.Error($"Unknown tool: {toolCall.Tool}"));
            return;
        }

        var argsDisplay = string.Join(", ", toolCall.Arguments.Select(a => $"{a.Key}: {a.Value}"));
        onEvent(AgentEvent.ToolCall($"{tool.Name}({argsDisplay})"));

        var timer = AgentLogger.StartTimer();
        var result = await tool.ExecuteAsync(toolCall.Arguments);
        logger.LogToolCall(tool.Name, toolCall.Arguments, result, timer.Elapsed.TotalMilliseconds);
        _state.SetLastResult($"{tool.Name} → {result}");
        onEvent(AgentEvent.ToolResult(result));
    }

    private async Task RespondAsync(string goal, Action<AgentEvent> onEvent)
    {
        var result = await llm.GenerateStructuredAsync<AgentResponse>(AgentPrompts.Respond(goal, _state, memory));

        if (result is null)
        {
            onEvent(AgentEvent.Error("Failed to generate response."));
            _state.MarkDone();
            return;
        }

        if (result.SaveToMemory is not null)
        {
            memory.Add(result.SaveToMemory);
            logger.LogMemory("add", result.SaveToMemory);
        }

        _state.MarkDone();
        onEvent(AgentEvent.Response(result.Reply));
    }

    private record AgentResponse(string Reply, string? SaveToMemory);
}
