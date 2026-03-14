using ccode.Shared;

namespace ccode.Agent;

public enum AgentAction { UseTool, Respond, Done }

public class Agent(LlmClient llm, IReadOnlyList<ITool> tools)
{
    private readonly AgentState _state = new();

    public async Task RunAsync(string goal, Action<AgentEvent> onEvent, int maxSteps = 10)
    {
        _state.Reset();

        while (!_state.Done && _state.Steps < maxSteps)
        {
            _state.IncrementStep();

            var action = await llm.DecideAsync<AgentAction>(BuildDecidePrompt(goal));

            if (action == AgentAction.UseTool)
                await ExecuteToolStepAsync(goal, onEvent);
            else if (action is null or AgentAction.Respond)
                await RespondAsync(goal, onEvent);
            else if (action == AgentAction.Done)
                _state.MarkDone();
        }
    }

    private async Task ExecuteToolStepAsync(string goal, Action<AgentEvent> onEvent)
    {
        var toolCall = await llm.RequestToolAsync(BuildToolPrompt(goal), tools);

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

        var result = await tool.ExecuteAsync(toolCall.Arguments);
        _state.SetLastResult($"{tool.Name} → {result}");
        onEvent(AgentEvent.ToolResult(result));
    }

    private async Task RespondAsync(string goal, Action<AgentEvent> onEvent)
    {
        var respondPrompt = BuildRespondPrompt(goal);
        var response = await llm.GenerateAsync(respondPrompt);
        _state.MarkDone();
        onEvent(AgentEvent.Response(response));
    }

    private string BuildDecidePrompt(string goal) =>
        $"""
         Goal: {goal}
         {_state}

         What should you do next?
         - UseTool: use one of the available tools to make progress
         - Respond: you have enough information to respond to the user
         - Done: the task is fully complete and no response is needed
         """;

    private string BuildToolPrompt(string goal) =>
        $"""
         Goal: {goal}
         {_state}

         Which tool should you call next to make progress towards the goal?
         """;

    private string BuildRespondPrompt(string goal) =>
        $"""
         Goal: {goal}
         {_state}

         Based on the work done above, provide your final response to the user.
         """;
}
