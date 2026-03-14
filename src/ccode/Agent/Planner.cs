using ccode.Shared;

namespace ccode.Agent;

public record Plan(string Goal, List<string> Steps);

public record PlanValidationResult(bool IsValid, List<string> Issues);

public class Planner(LlmClient llm)
{
    private const int MinSteps = 1;
    private const int MaxSteps = 10;

    public async Task<Plan?> CreatePlanAsync(string goal, Memory memory)
    {
        var result = await llm.GenerateStructuredAsync<PlanResult>(
            $"""
             Memory:
             {memory.GetContext()}

             Goal: {goal}

             Break this goal down into a concise, ordered list of steps to complete it.
             Each step should be a single, concrete action.
             Keep the plan short — only include steps that are strictly necessary.
             """);

        return result is null ? null : new Plan(goal, result.Steps);
    }

    public async Task<PlanValidationResult> ValidatePlanAsync(Plan plan, IReadOnlyList<ITool> availableTools)
    {
        var issues = new List<string>();

        // Structural checks
        if (plan.Steps.Count < MinSteps)
            issues.Add("Plan has no steps.");

        if (plan.Steps.Count > MaxSteps)
            issues.Add($"Plan has too many steps ({plan.Steps.Count}). Maximum is {MaxSteps}.");

        var emptySteps = plan.Steps.Where(string.IsNullOrWhiteSpace).ToList();
        if (emptySteps.Count > 0)
            issues.Add($"Plan contains {emptySteps.Count} empty step(s).");

        var duplicates = plan.Steps.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            issues.Add($"Plan contains duplicate steps: {string.Join(", ", duplicates)}");

        if (issues.Count > 0)
            return new PlanValidationResult(false, issues);

        // LLM sanity check
        var toolNames = string.Join(", ", availableTools.Select(t => t.Name));
        var steps = string.Join("\n", plan.Steps.Select((s, i) => $"{i + 1}. {s}"));

        var result = await llm.GenerateStructuredAsync<ValidationResult>(
            $"""
             Goal: {plan.Goal}

             Proposed plan:
             {steps}

             Available tools: {toolNames}

             Is this plan valid? Check:
             - Are the steps concrete and achievable with the available tools?
             - Are the steps in a logical order?
             - Is anything critically missing or redundant?

             Return isValid=true only if the plan is sound. Otherwise describe the issues.
             """);

        if (result is null)
            return new PlanValidationResult(true, []);

        if (!result.IsValid)
            issues.AddRange(result.Issues);

        return new PlanValidationResult(result.IsValid, issues);
    }

    private record PlanResult(List<string> Steps);
    private record ValidationResult(bool IsValid, List<string> Issues);
}
