using ccode.Shared;

namespace ccode.Agent;

public record Plan(string Goal, List<string> Steps);

public record PlanValidationResult(bool IsValid, List<string> Issues);

public record AotNode(string Id, string Action, List<string> DependsOn);

public record AotGraph(string Goal, List<AotNode> Nodes);

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

    public async Task<AtomicAction?> CreateAtomicActionAsync(string step, string originalGoal, IReadOnlyList<ITool> availableTools, Memory memory)
    {
        var toolDescriptions = string.Join("\n", availableTools.Select(t =>
            $"- {t.Name}: {t.Description}\n" +
            string.Join("\n", t.Parameters.Select(p =>
                $"    {p.Name} ({(p.Required ? "required" : "optional")}): {p.Description}"))));

        return await llm.GenerateStructuredAsync<AtomicAction>(
            $"""
             Memory:
             {memory.GetContext()}

             Original goal: {originalGoal}
             Step to execute: {step}

             Available tools:
             {toolDescriptions}

             Convert this step into a single tool call with all required arguments.
             All content must be relevant to the original goal — do not invent unrelated content.
             Choose the most appropriate tool and provide all necessary arguments.
             """);
    }

    public async Task<AotGraph?> CreateAotGraphAsync(string goal, Memory memory)
    {
        var result = await llm.GenerateStructuredAsync<AotGraphResult>(
            $"""
             Memory:
             {memory.GetContext()}

             Goal: {goal}

             Create an execution graph to achieve this goal.
             Each node is a single concrete action. Nodes that depend on other nodes must list their dependencies.
             Nodes with no dependencies can run immediately (and in parallel with each other).

             Rules:
             - Each node needs a unique id (use "1", "2", etc.)
             - depends_on lists the ids of nodes that must complete before this one
             - Keep the graph small — only include necessary actions
             """);

        return result is null ? null : new AotGraph(goal, result.Nodes);
    }

    public PlanValidationResult ValidateAotGraph(AotGraph graph)
    {
        var issues = new List<string>();
        var nodeIds = graph.Nodes.Select(n => n.Id).ToHashSet();

        // Check for duplicate IDs
        var duplicates = graph.Nodes.GroupBy(n => n.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            issues.Add($"Duplicate node IDs: {string.Join(", ", duplicates)}");

        // Check all dependencies reference existing nodes
        foreach (var node in graph.Nodes)
        {
            var missing = node.DependsOn.Where(d => !nodeIds.Contains(d)).ToList();
            if (missing.Count > 0)
                issues.Add($"Node '{node.Id}' depends on non-existent nodes: {string.Join(", ", missing)}");
        }

        if (issues.Count > 0)
            return new PlanValidationResult(false, issues);

        // Cycle detection via Kahn's algorithm (topological sort)
        var inDegree = graph.Nodes.ToDictionary(n => n.Id, n => n.DependsOn.Count);
        var queue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var visited = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visited++;

            foreach (var node in graph.Nodes.Where(n => n.DependsOn.Contains(current)))
            {
                inDegree[node.Id]--;
                if (inDegree[node.Id] == 0)
                    queue.Enqueue(node.Id);
            }
        }

        if (visited != graph.Nodes.Count)
            issues.Add("Graph contains circular dependencies.");

        return new PlanValidationResult(issues.Count == 0, issues);
    }

    private record PlanResult(List<string> Steps);
    private record ValidationResult(bool IsValid, List<string> Issues);
    private record AotGraphResult(List<AotNode> Nodes);
}
