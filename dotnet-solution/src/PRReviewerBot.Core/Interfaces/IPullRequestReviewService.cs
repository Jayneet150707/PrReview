using PRReviewerBot.Core.Models;

namespace PRReviewerBot.Core.Interfaces;

/// <summary>
/// Service for reviewing pull requests and posting comments
/// </summary>
public interface IPullRequestReviewService
{
    /// <summary>
    /// Reviews a pull request and posts comments (does NOT merge)
    /// </summary>
    /// <param name="webhookEvent">GitHub webhook event containing PR information</param>
    /// <returns>Result of the review operation</returns>
    Task<ReviewResult> ReviewPullRequestAsync(GitHubWebhookEvent webhookEvent);

    /// <summary>
    /// Gets the files changed in a pull request
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="pullNumber">Pull request number</param>
    /// <returns>List of changed files</returns>
    Task<List<GitHubFile>> GetPullRequestFilesAsync(string owner, string repo, int pullNumber);

    /// <summary>
    /// Posts a review comment on a pull request
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="pullNumber">Pull request number</param>
    /// <param name="comment">Review comment to post</param>
    /// <returns>True if comment was posted successfully</returns>
    Task<bool> PostReviewCommentAsync(string owner, string repo, int pullNumber, ReviewComment comment);

    /// <summary>
    /// Posts a general comment on a pull request
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="pullNumber">Pull request number</param>
    /// <param name="body">Comment body</param>
    /// <returns>True if comment was posted successfully</returns>
    Task<bool> PostGeneralCommentAsync(string owner, string repo, int pullNumber, string body);
}
