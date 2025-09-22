using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Octokit;

namespace PRReviewerBot.Infrastructure.HealthChecks;

/// <summary>
/// Health check for GitHub API connectivity
/// </summary>
public class GitHubHealthCheck : IHealthCheck
{
    private readonly IGitHubClient _gitHubClient;
    private readonly ILogger<GitHubHealthCheck> _logger;

    public GitHubHealthCheck(IGitHubClient gitHubClient, ILogger<GitHubHealthCheck> logger)
    {
        _gitHubClient = gitHubClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test GitHub API connectivity by getting rate limit info
            var rateLimit = await _gitHubClient.Miscellaneous.GetRateLimits();
            
            var data = new Dictionary<string, object>
            {
                ["remaining_requests"] = rateLimit.Resources.Core.Remaining,
                ["rate_limit_reset"] = rateLimit.Resources.Core.Reset.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["authenticated"] = true // Assume authenticated if client is configured
            };

            if (rateLimit.Resources.Core.Remaining < 100)
            {
                _logger.LogWarning("GitHub API rate limit is low: {Remaining} requests remaining", 
                    rateLimit.Resources.Core.Remaining);
                
                return HealthCheckResult.Degraded("GitHub API rate limit is low", data: data);
            }

            return HealthCheckResult.Healthy("GitHub API is accessible", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub health check failed");
            return HealthCheckResult.Unhealthy("GitHub API is not accessible", ex);
        }
    }
}
