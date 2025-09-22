using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Octokit;
using OpenAI;
using OpenAI.Chat;
using PRReviewerBot.Core.Interfaces;
using PRReviewerBot.Infrastructure.HealthChecks;
using PRReviewerBot.Infrastructure.Services;

namespace PRReviewerBot.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // GitHub client
        services.AddSingleton<IGitHubClient>(provider =>
        {
            var token = configuration["GitHub:Token"] ?? throw new InvalidOperationException("GitHub token not configured");
            return new GitHubClient(new ProductHeaderValue("PRReviewerBot", "1.0.0"))
            {
                Credentials = new Credentials(token)
            };
        });

        // OpenAI client
        services.AddSingleton<OpenAIClient>(provider =>
        {
            var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
            return new OpenAIClient(apiKey);
        });

        services.AddSingleton<ChatClient>(provider =>
        {
            var openAIClient = provider.GetRequiredService<OpenAIClient>();
            var model = configuration["OpenAI:Model"] ?? "gpt-4";
            return openAIClient.GetChatClient(model);
        });

        // Application services
        services.AddScoped<IPullRequestReviewService, PullRequestReviewService>();
        services.AddScoped<ICodeAnalysisService, OpenAICodeAnalysisService>();

        // Health checks
        services.AddSingleton<GitHubHealthCheck>();
        services.AddSingleton<OpenAIHealthCheck>();

        // HTTP client for external calls
        services.AddHttpClient();

        return services;
    }
}
