using Microsoft.Extensions.AI;

namespace ccode.Shared;

public class LlmClient(IChatClient chatClient)
{
    public async Task<string> GenerateAsync(string prompt)
    {
        var response = await chatClient.GetResponseAsync(prompt);
        return response.Text;
    }
}
