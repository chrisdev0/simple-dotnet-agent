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
        _state.Reset(goal);

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
                await ExecutePlanStepAsync(step, onEvent);
        }
        else
        {
            await ExecuteDecideLoopAsync(goal, onEvent, maxSteps);
        }

        memory.Save();
    }

    // For plan steps: convert to a single atomic action and execute directly
    private async Task ExecutePlanStepAsync(string step, Action<AgentEvent> onEvent)
    {
        _state.ResetForNextStep();

        var action = await _planner.CreateAtomicActionAsync(step, _state.Goal ?? step, tools, memory);

        if (action is null)
        {
            onEvent(AgentEvent.Error($"Could not resolve step to a tool call: {step}"));
            return;
        }

        var tool = tools.FirstOrDefault(t => t.Name.Equals(action.Tool, StringComparison.OrdinalIgnoreCase));
        if (tool is null)
        {
            onEvent(AgentEvent.Error($"Unknown tool '{action.Tool}' for step: {step}"));
            return;
        }

        var missingArgs = tool.Parameters
            .Where(p => p.Required && !action.Arguments.ContainsKey(p.Name))
            .Select(p => p.Name)
            .ToList();

        if (missingArgs.Count > 0)
        {
            onEvent(AgentEvent.Error($"Missing required arguments for {tool.Name}: {string.Join(", ", missingArgs)}"));
            return;
        }

        await ExecuteToolAsync(tool, action.Arguments, onEvent);
    }

    // For unplanned goals: full decide loop
    private async Task ExecuteDecideLoopAsync(string goal, Action<AgentEvent> onEvent, int maxSteps)
    {
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
                    onEvent(AgentEvent.Error($"Stuck in loop on: {goal}. Aborting."));
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
                await ExecuteToolDecisionAsync(goal, onEvent);
            else if (action is null or AgentAction.Respond)
                await RespondAsync(goal, onEvent);
            else if (action == AgentAction.Done)
                _state.MarkDone();
        }
    }

    private async Task ExecuteToolDecisionAsync(string goal, Action<AgentEvent> onEvent)
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

        await ExecuteToolAsync(tool, toolCall.Arguments, onEvent);
    }

    private async Task ExecuteToolAsync(ITool tool, Dictionary<string, string> arguments, Action<AgentEvent> onEvent)
    {
        var argsDisplay = string.Join(", ", arguments.Select(a => $"{a.Key}: {a.Value}"));
        onEvent(AgentEvent.ToolCall($"{tool.Name}({argsDisplay})"));

        var timer = AgentLogger.StartTimer();
        var result = await tool.ExecuteAsync(arguments);
        logger.LogToolCall(tool.Name, arguments, result, timer.Elapsed.TotalMilliseconds);

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
