using Microsoft.AspNetCore.Mvc;
using PRReviewerBot.Core.Interfaces;
using PRReviewerBot.Core.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PRReviewerBot.Api.Controllers;

/// <summary>
/// Handles GitHub webhook events for pull request reviews
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/webhook")]
[ApiVersion("1.0")]
public class WebhookController : ControllerBase
{
    private readonly IPullRequestReviewService _reviewService;
    private readonly ILogger<WebhookController> _logger;
    private readonly IConfiguration _configuration;

    public WebhookController(
        IPullRequestReviewService reviewService,
        ILogger<WebhookController> logger,
        IConfiguration configuration)
    {
        _reviewService = reviewService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Handles GitHub webhook events for pull requests
    /// </summary>
    /// <param name="payload">GitHub webhook payload</param>
    /// <returns>Response indicating processing status</returns>
    [HttpPost("github")]
    public async Task<IActionResult> HandleGitHubWebhook([FromBody] JsonElement payload)
    {
        try
        {
            // Verify webhook signature
            if (!await VerifyWebhookSignature())
            {
                _logger.LogWarning("Invalid webhook signature received");
                return Unauthorized("Invalid signature");
            }

            // Extract event type
            var eventType = Request.Headers["X-GitHub-Event"].FirstOrDefault();
            if (string.IsNullOrEmpty(eventType))
            {
                _logger.LogWarning("Missing X-GitHub-Event header");
                return BadRequest("Missing event type");
            }

            _logger.LogInformation("Received GitHub webhook event: {EventType}", eventType);

            // Only process pull request events
            if (eventType != "pull_request")
            {
                _logger.LogInformation("Ignoring non-pull request event: {EventType}", eventType);
                return Ok(new { message = "Event type not supported", eventType });
            }

            // Parse webhook payload
            var webhookEvent = ParseWebhookPayload(payload);
            if (webhookEvent == null)
            {
                _logger.LogWarning("Failed to parse webhook payload");
                return BadRequest("Invalid payload format");
            }

            // Check if we should process this event
            if (!ShouldProcessEvent(webhookEvent))
            {
                _logger.LogInformation("Skipping event - Action: {Action}, Target Branch: {Branch}", 
                    webhookEvent.Action, webhookEvent.PullRequest?.Base?.Ref);
                return Ok(new { message = "Event skipped", reason = "Action or branch not configured for processing" });
            }

            // Process the pull request review
            var result = await _reviewService.ReviewPullRequestAsync(webhookEvent);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully processed PR review for #{PrNumber} in {Repository}", 
                    webhookEvent.PullRequest?.Number, webhookEvent.Repository?.FullName);
                
                return Ok(new 
                { 
                    message = "Review completed successfully",
                    prNumber = webhookEvent.PullRequest?.Number,
                    repository = webhookEvent.Repository?.FullName,
                    commentsAdded = result.CommentsAdded
                });
            }
            else
            {
                _logger.LogError("Failed to process PR review: {Error}", result.ErrorMessage);
                return StatusCode(500, new { message = "Review processing failed", error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check endpoint for webhook
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "webhook-handler"
        });
    }

    private async Task<bool> VerifyWebhookSignature()
    {
        var secret = _configuration["GitHub:WebhookSecret"];
        if (string.IsNullOrEmpty(secret))
        {
            _logger.LogWarning("GitHub webhook secret not configured");
            return false;
        }

        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            return false;
        }

        Request.EnableBuffering();
        Request.Body.Position = 0;
        
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var expectedSignature = "sha256=" + ComputeHmacSha256(body, secret);
        
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signature),
            Encoding.UTF8.GetBytes(expectedSignature)
        );
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private GitHubWebhookEvent? ParseWebhookPayload(JsonElement payload)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<GitHubWebhookEvent>(payload.GetRawText(), options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse webhook payload");
            return null;
        }
    }

    private bool ShouldProcessEvent(GitHubWebhookEvent webhookEvent)
    {
        // Only process 'opened' and 'synchronize' actions
        var supportedActions = new[] { "opened", "synchronize" };
        if (!supportedActions.Contains(webhookEvent.Action, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check target branch
        var targetBranch = _configuration["GitHub:TargetBranch"] ?? "beta";
        if (!string.Equals(webhookEvent.PullRequest?.Base?.Ref, targetBranch, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
