using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace PRReviewerBot.Infrastructure.HealthChecks;

/// <summary>
/// Health check for OpenAI API connectivity
/// </summary>
public class OpenAIHealthCheck : IHealthCheck
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAIHealthCheck> _logger;

    public OpenAIHealthCheck(ChatClient chatClient, ILogger<OpenAIHealthCheck> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test OpenAI API connectivity with a simple request
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant."),
                new UserChatMessage("Say 'OK' if you can hear me.")
            };

            var response = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                MaxOutputTokenCount = 5,
                Temperature = 0
            });

            var responseText = response.Value.Content[0].Text?.Trim().ToUpperInvariant();
            
            var data = new Dictionary<string, object>
            {
                ["model"] = _chatClient.ToString() ?? "unknown",
                ["response"] = responseText ?? "no response",
                ["test_successful"] = responseText?.Contains("OK") == true
            };

            if (responseText?.Contains("OK") == true)
            {
                return HealthCheckResult.Healthy("OpenAI API is accessible", data);
            }
            else
            {
                _logger.LogWarning("OpenAI API responded but with unexpected content: {Response}", responseText);
                return HealthCheckResult.Degraded("OpenAI API responded with unexpected content", data: data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI health check failed");
            return HealthCheckResult.Unhealthy("OpenAI API is not accessible", ex);
        }
    }
}
