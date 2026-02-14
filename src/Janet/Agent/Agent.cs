using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Janet.Configuration;

namespace Janet.Agent;

public class Agent
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<Agent> _logger;
    private readonly string _systemPrompt;
    private readonly List<ChatMessage> _conversationHistory = new();

    public Agent(IChatClient chatClient, ILogger<Agent> logger, Config config)
    {
        _chatClient = chatClient;
        _logger = logger;
        _systemPrompt = config.SystemPrompt;

        _conversationHistory.Add(new ChatMessage(ChatRole.System, _systemPrompt));
    }

    public async Task<string> RunAsync(
        string userInput,
        IList<AITool> tools,
        CancellationToken ct = default)
    {
        _conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));

        _logger.LogInformation("Sending request to LLM with {ToolCount} tools", tools.Count);

        var options = new ChatOptions
        {
            Tools = tools
        };

        var response = await _chatClient.GetResponseAsync(
            _conversationHistory,
            options,
            ct);

        _conversationHistory.AddMessages(response);

        var content = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<TextContent>()
            .LastOrDefault()?.Text ?? "";

        return content;
    }

    public void ClearHistory()
    {
        _conversationHistory.Clear();
        _conversationHistory.Add(new ChatMessage(ChatRole.System, _systemPrompt));
    }
}
