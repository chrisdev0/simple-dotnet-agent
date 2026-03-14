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
}
