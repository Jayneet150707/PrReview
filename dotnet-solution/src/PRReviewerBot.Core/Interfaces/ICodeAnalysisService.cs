using PRReviewerBot.Core.Models;

namespace PRReviewerBot.Core.Interfaces;

/// <summary>
/// Service for analyzing code and generating review comments
/// </summary>
public interface ICodeAnalysisService
{
    /// <summary>
    /// Analyzes code changes and generates review comments
    /// </summary>
    /// <param name="files">List of changed files to analyze</param>
    /// <param name="prContext">Pull request context information</param>
    /// <returns>List of review comments</returns>
    Task<List<ReviewComment>> AnalyzeCodeChangesAsync(List<GitHubFile> files, PullRequestContext prContext);

    /// <summary>
    /// Determines if a file should be reviewed based on its type and content
    /// </summary>
    /// <param name="filename">Name of the file</param>
    /// <returns>True if the file should be reviewed</returns>
    bool ShouldReviewFile(string filename);

    /// <summary>
    /// Gets the programming language for a file based on its extension
    /// </summary>
    /// <param name="filename">Name of the file</param>
    /// <returns>Programming language identifier</returns>
    string GetFileLanguage(string filename);
}

/// <summary>
/// Context information for a pull request
/// </summary>
public class PullRequestContext
{
    /// <summary>
    /// Pull request title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Pull request description/body
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Target branch name
    /// </summary>
    public string TargetBranch { get; set; } = string.Empty;

    /// <summary>
    /// Source branch name
    /// </summary>
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>
    /// Author of the pull request
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Repository name
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Repository owner
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Primary programming language of the repository
    /// </summary>
    public string? PrimaryLanguage { get; set; }

    /// <summary>
    /// Total number of additions in the PR
    /// </summary>
    public int TotalAdditions { get; set; }

    /// <summary>
    /// Total number of deletions in the PR
    /// </summary>
    public int TotalDeletions { get; set; }

    /// <summary>
    /// Total number of changed files
    /// </summary>
    public int ChangedFiles { get; set; }
}
