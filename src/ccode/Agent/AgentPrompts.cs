namespace ccode.Agent;

public static class AgentPrompts
{
    public static string Decide(string goal, AgentState state, Memory memory) =>
        $"""
         Memory:
         {memory.GetContext()}

         {state.GetContext()}
         Goal: {goal}

         What should you do next?
         - UseTool: use one of the available tools to make progress
         - Respond: you have enough information to respond to the user
         - Done: the task is fully complete and no response is needed
         """;

    public static string SelectTool(string goal, AgentState state, Memory memory) =>
        $"""
         Memory:
         {memory.GetContext()}

         {state.GetContext()}
         Goal: {goal}

         Which tool should you call next to make progress towards the goal?
         """;

    public static string Respond(string goal, AgentState state, Memory memory) =>
        $"""
         Memory:
         {memory.GetContext()}

         {state.GetContext()}
         Goal: {goal}

         Provide your reply to the user. If the user shared any information worth remembering
         (e.g. their name, preferences, project details), include it in save_to_memory. Otherwise set it to null.
         """;
}
