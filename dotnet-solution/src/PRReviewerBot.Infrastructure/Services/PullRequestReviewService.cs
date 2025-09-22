using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;
using PRReviewerBot.Core.Interfaces;
using PRReviewerBot.Core.Models;
using System.Diagnostics;

namespace PRReviewerBot.Infrastructure.Services;

/// <summary>
/// Service for reviewing pull requests and posting comments (NO AUTO-MERGE)
/// </summary>
public class PullRequestReviewService : IPullRequestReviewService
{
    private readonly IGitHubClient _gitHubClient;
    private readonly ICodeAnalysisService _codeAnalysisService;
    private readonly ILogger<PullRequestReviewService> _logger;
    private readonly IConfiguration _configuration;

    public PullRequestReviewService(
        IGitHubClient gitHubClient,
        ICodeAnalysisService codeAnalysisService,
        ILogger<PullRequestReviewService> logger,
        IConfiguration configuration)
    {
        _gitHubClient = gitHubClient;
        _codeAnalysisService = codeAnalysisService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Reviews a pull request and posts comments (does NOT merge)
    /// </summary>
    public async Task<ReviewResult> ReviewPullRequestAsync(GitHubWebhookEvent webhookEvent)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (webhookEvent.PullRequest == null || webhookEvent.Repository == null)
            {
                return ReviewResult.Failure("Invalid webhook event: missing pull request or repository data");
            }

            var pr = webhookEvent.PullRequest;
            var repo = webhookEvent.Repository;
            var owner = repo.Owner?.Login ?? "";
            var repoName = repo.Name;

            _logger.LogInformation("Starting review for PR #{PrNumber} in {Repository}", 
                pr.Number, repo.FullName);

            // Get changed files
            var files = await GetPullRequestFilesAsync(owner, repoName, pr.Number);
            if (!files.Any())
            {
                _logger.LogInformation("No files to review in PR #{PrNumber}", pr.Number);
                return ReviewResult.Success(0, new List<string>());
            }

            // Filter files that should be reviewed
            var reviewableFiles = files.Where(f => _codeAnalysisService.ShouldReviewFile(f.Filename)).ToList();
            if (!reviewableFiles.Any())
            {
                _logger.LogInformation("No reviewable files found in PR #{PrNumber}", pr.Number);
                
                // Post a general comment explaining why no review was performed
                await PostGeneralCommentAsync(owner, repoName, pr.Number,
                    "🤖 **PR Reviewer Bot**\n\nNo reviewable code files found in this PR. " +
                    "The bot only reviews source code files and skips binary files, dependencies, and generated content.");
                
                return ReviewResult.Success(1, new List<string>());
            }

            // Check if PR is too large
            var maxFiles = _configuration.GetValue<int>("Review:MaxFilesToReview", 20);
            var maxChanges = _configuration.GetValue<int>("Review:MaxChangesToReview", 1000);
            var totalChanges = reviewableFiles.Sum(f => f.Changes);

            if (reviewableFiles.Count > maxFiles || totalChanges > maxChanges)
            {
                _logger.LogInformation("PR #{PrNumber} is too large for review: {FileCount} files, {ChangeCount} changes", 
                    pr.Number, reviewableFiles.Count, totalChanges);

                await PostGeneralCommentAsync(owner, repoName, pr.Number,
                    $"🤖 **PR Reviewer Bot**\n\n" +
                    $"This PR is too large for automated review:\n" +
                    $"- **Files**: {reviewableFiles.Count} (max: {maxFiles})\n" +
                    $"- **Changes**: {totalChanges} (max: {maxChanges})\n\n" +
                    $"Please consider breaking this into smaller PRs for better review quality and faster processing.");

                return ReviewResult.Success(1, reviewableFiles.Select(f => f.Filename).ToList());
            }

            // Create PR context
            var prContext = new PullRequestContext
            {
                Title = pr.Title,
                Description = pr.Body,
                TargetBranch = pr.Base?.Ref ?? "",
                SourceBranch = pr.Head?.Ref ?? "",
                Author = pr.User?.Login ?? "",
                Repository = repoName,
                Owner = owner,
                PrimaryLanguage = repo.Language,
                TotalAdditions = pr.Additions,
                TotalDeletions = pr.Deletions,
                ChangedFiles = pr.ChangedFiles
            };

            // Analyze code changes
            var reviewComments = await _codeAnalysisService.AnalyzeCodeChangesAsync(reviewableFiles, prContext);
            
            // Post review comments
            var commentsPosted = 0;
            foreach (var comment in reviewComments)
            {
                var success = await PostReviewCommentAsync(owner, repoName, pr.Number, comment);
                if (success)
                {
                    commentsPosted++;
                }
                
                // Add small delay to avoid rate limiting
                await Task.Delay(100);
            }

            // Post summary comment if we have specific comments
            if (reviewComments.Any())
            {
                var summaryComment = GenerateSummaryComment(reviewComments, prContext);
                await PostGeneralCommentAsync(owner, repoName, pr.Number, summaryComment);
                commentsPosted++;
            }
            else
            {
                // Post positive feedback if no issues found
                await PostGeneralCommentAsync(owner, repoName, pr.Number,
                    "🤖 **PR Reviewer Bot**\n\n" +
                    "✅ **Great work!** No significant issues found in this PR. " +
                    "The code looks clean and follows good practices.\n\n" +
                    "*This is an automated review. Please ensure manual review is also conducted.*");
                commentsPosted++;
            }

            stopwatch.Stop();
            
            var result = ReviewResult.Success(commentsPosted, reviewableFiles.Select(f => f.Filename).ToList());
            result.LinesReviewed = totalChanges;
            result.ReviewDuration = stopwatch.Elapsed;
            result.Metadata["reviewCommentsGenerated"] = reviewComments.Count;
            result.Metadata["securityIssues"] = reviewComments.Count(c => c.Category == ReviewCategory.Security);
            result.Metadata["performanceIssues"] = reviewComments.Count(c => c.Category == ReviewCategory.Performance);

            _logger.LogInformation("Completed review for PR #{PrNumber}: {CommentsPosted} comments posted in {Duration}ms", 
                pr.Number, commentsPosted, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing pull request");
            return ReviewResult.Failure($"Review failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the files changed in a pull request
    /// </summary>
    public async Task<List<GitHubFile>> GetPullRequestFilesAsync(string owner, string repo, int pullNumber)
    {
        try
        {
            var files = await _gitHubClient.PullRequest.Files(owner, repo, pullNumber);
            
            return files.Select(f => new GitHubFile
            {
                Filename = f.Filename,
                Status = f.Status.StringValue,
                Additions = f.Additions,
                Deletions = f.Deletions,
                Changes = f.Changes,
                Patch = f.Patch,
                BlobUrl = f.BlobUrl,
                RawUrl = f.RawUrl,
                ContentsUrl = f.ContentsUrl
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PR files for {Owner}/{Repo}#{PrNumber}", owner, repo, pullNumber);
            return new List<GitHubFile>();
        }
    }

    /// <summary>
    /// Posts a review comment on a pull request
    /// </summary>
    public async Task<bool> PostReviewCommentAsync(string owner, string repo, int pullNumber, ReviewComment comment)
    {
        try
        {
            // For general comments without specific line numbers, post as issue comment
            if (!comment.LineNumber.HasValue)
            {
                return await PostGeneralCommentAsync(owner, repo, pullNumber, comment.Body);
            }

            // For line-specific comments, we would need the commit SHA and diff position
            // For simplicity, posting as general comment with file reference
            var commentBody = $"**{comment.FilePath}**";
            if (comment.LineNumber.HasValue)
            {
                commentBody += $" (around line {comment.LineNumber})";
            }
            commentBody += $"\n\n{comment.Body}";

            return await PostGeneralCommentAsync(owner, repo, pullNumber, commentBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting review comment on {Owner}/{Repo}#{PrNumber}", owner, repo, pullNumber);
            return false;
        }
    }

    /// <summary>
    /// Posts a general comment on a pull request
    /// </summary>
    public async Task<bool> PostGeneralCommentAsync(string owner, string repo, int pullNumber, string body)
    {
        try
        {
            await _gitHubClient.Issue.Comment.Create(owner, repo, pullNumber, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting general comment on {Owner}/{Repo}#{PrNumber}", owner, repo, pullNumber);
            return false;
        }
    }

    private string GenerateSummaryComment(List<ReviewComment> comments, PullRequestContext context)
    {
        var securityIssues = comments.Count(c => c.Category == ReviewCategory.Security);
        var performanceIssues = comments.Count(c => c.Category == ReviewCategory.Performance);
        var codeQualityIssues = comments.Count(c => c.Category == ReviewCategory.CodeQuality);
        var criticalIssues = comments.Count(c => c.Severity == CommentSeverity.Critical);
        var errorIssues = comments.Count(c => c.Severity == CommentSeverity.Error);

        var summary = "🤖 **PR Reviewer Bot - Review Summary**\n\n";
        
        if (criticalIssues > 0 || errorIssues > 0)
        {
            summary += "⚠️ **Issues Found:**\n";
            if (criticalIssues > 0) summary += $"- 🔴 **Critical**: {criticalIssues} issue(s)\n";
            if (errorIssues > 0) summary += $"- 🟠 **Error**: {errorIssues} issue(s)\n";
            summary += "\n";
        }

        if (securityIssues > 0 || performanceIssues > 0 || codeQualityIssues > 0)
        {
            summary += "📊 **Issue Categories:**\n";
            if (securityIssues > 0) summary += $"- 🔒 **Security**: {securityIssues} issue(s)\n";
            if (performanceIssues > 0) summary += $"- ⚡ **Performance**: {performanceIssues} issue(s)\n";
            if (codeQualityIssues > 0) summary += $"- 📝 **Code Quality**: {codeQualityIssues} issue(s)\n";
            summary += "\n";
        }

        summary += $"📈 **PR Stats:**\n";
        summary += $"- **Files Changed**: {context.ChangedFiles}\n";
        summary += $"- **Lines Added**: +{context.TotalAdditions}\n";
        summary += $"- **Lines Removed**: -{context.TotalDeletions}\n";
        summary += $"- **Target Branch**: `{context.TargetBranch}`\n\n";

        summary += "---\n";
        summary += "*This is an automated review. Please ensure manual review is also conducted for critical changes.*";

        return summary;
    }
}
