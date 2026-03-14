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
}
