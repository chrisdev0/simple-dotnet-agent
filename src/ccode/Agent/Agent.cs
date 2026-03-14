using ccode.Shared;
using ccode.Shared.Logging;

namespace ccode.Agent;

public enum AgentAction { UseTool, Respond, Done }

public class Agent(LlmClient llm, IReadOnlyList<ITool> tools, Memory memory, AgentLogger logger)
{
    private readonly AgentState _state = new();
    private readonly Planner _planner = new(llm);

    public async Task RunAsync(string goal, Action<AgentEvent> onEvent, int maxSteps = 10)
    {
        _state.Reset();

        var plan = await _planner.CreatePlanAsync(goal, memory);

        if (plan is not null && plan.Steps.Count > 0)
        {
            var validation = await _planner.ValidatePlanAsync(plan, tools);

            if (!validation.IsValid)
            {
                onEvent(AgentEvent.Error($"Plan validation failed:\n{string.Join("\n", validation.Issues.Select(i => $"- {i}"))}"));
                memory.Save();
                return;
            }

            onEvent(AgentEvent.Plan(string.Join("\n", plan.Steps.Select((s, i) => $"{i + 1}. {s}"))));

            foreach (var step in plan.Steps)
                await ExecuteStepAsync(step, onEvent, maxSteps);
        }
        else
        {
            await ExecuteStepAsync(goal, onEvent, maxSteps);
        }

        memory.Save();
    }

    private async Task ExecuteStepAsync(string goal, Action<AgentEvent> onEvent, int maxSteps)
    {
        _state.ResetForNextStep();
        string? previousResult = null;
        var repeatedCount = 0;

        while (!_state.Done && _state.Steps < maxSteps)
        {
            _state.IncrementStep();

            if (_state.LastResult == previousResult && previousResult is not null)
            {
                repeatedCount++;
                if (repeatedCount >= 2)
                {
                    onEvent(AgentEvent.Error($"Stuck in loop on step: {goal}. Aborting."));
                    _state.MarkDone();
                    break;
                }
            }
            else
            {
                repeatedCount = 0;
            }

            previousResult = _state.LastResult;

            var action = await llm.DecideAsync<AgentAction>(AgentPrompts.Decide(goal, _state, memory));

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

        var summary = $"{tool.Name}({argsDisplay}) → {result}";
        _state.SetLastResult(summary);
        memory.Add(summary.Length > 200 ? summary[..200] + "…" : summary);
        logger.LogMemory("add", summary);

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
