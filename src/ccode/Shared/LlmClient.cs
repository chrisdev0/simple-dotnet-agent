using ccode.Agent;
using Microsoft.Extensions.AI;

namespace ccode.Shared;

public class LlmClient(IChatClient chatClient, string systemPrompt)
{
    public async Task<string> GenerateAsync(string prompt)
    {
        List<ChatMessage> messages =
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, prompt),
        ];

        var response = await chatClient.GetResponseAsync(messages);
        return response.Text;
    }

    public async Task<T?> GenerateStructuredAsync<T>(string prompt)
    {
        var schema = JsonHelper.GetSchema<T>();
        var structuredPrompt =
            $"""
             {prompt}

             Respond with valid JSON only. No explanation, no markdown, no code fences.
             Your response must match this JSON schema:
             {schema}
             """;

        List<ChatMessage> messages =
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, structuredPrompt),
        ];

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var response = await chatClient.GetResponseAsync(messages);
            var result = JsonHelper.TryDeserialize<T>(response.Text);
            if (result is not null)
                return result;
        }

        return default;
    }

    public async Task<TAction?> DecideAsync<TAction>(string prompt) where TAction : struct, Enum
    {
        var choices = Enum.GetNames<TAction>();
        var decision = await GenerateStructuredAsync<Decision>(
            $"""
             {prompt}

             You must choose exactly one of the following options: {string.Join(", ", choices.Select(c => $"\"{c}\""))}
             """);

        if (decision is null) return null;
        if (Enum.TryParse<TAction>(decision.Choice, ignoreCase: true, out var result))
            return result;

        return null;
    }

    public async Task<ToolCall?> RequestToolAsync(string prompt, IEnumerable<ITool> tools)
    {
        var toolList = tools.ToList();
        var toolDescriptions = string.Join("\n", toolList.Select(t =>
            $"- {t.Name}: {t.Description}\n" +
            string.Join("\n", t.Parameters.Select(p =>
                $"    {p.Name} ({(p.Required ? "required" : "optional")}): {p.Description}"))));

        return await GenerateStructuredAsync<ToolCall>(
            $"""
             {prompt}

             Available tools:
             {toolDescriptions}

             Respond with the name of the tool to call and its arguments.
             """);
    }

    private record Decision(string Choice);
}
