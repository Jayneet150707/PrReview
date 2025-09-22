namespace PRReviewerBot.Core.Models;

/// <summary>
/// Represents the result of a pull request review operation
/// </summary>
public class ReviewResult
{
    /// <summary>
    /// Indicates whether the review operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of comments added during the review
    /// </summary>
    public int CommentsAdded { get; set; }

    /// <summary>
    /// List of files that were reviewed
    /// </summary>
    public List<string> ReviewedFiles { get; set; } = new();

    /// <summary>
    /// Total number of lines of code reviewed
    /// </summary>
    public int LinesReviewed { get; set; }

    /// <summary>
    /// Time taken to complete the review
    /// </summary>
    public TimeSpan ReviewDuration { get; set; }

    /// <summary>
    /// Additional metadata about the review
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Creates a successful review result
    /// </summary>
    public static ReviewResult CreateSuccess(int commentsAdded = 0, List<string>? reviewedFiles = null)
    {
        return new ReviewResult
        {
            Success = true,
            CommentsAdded = commentsAdded,
            ReviewedFiles = reviewedFiles ?? new List<string>()
        };
    }

    /// <summary>
    /// Creates a failed review result
    /// </summary>
    public static ReviewResult CreateFailure(string errorMessage)
    {
        return new ReviewResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Represents a code review comment to be posted
/// </summary>
public class ReviewComment
{
    /// <summary>
    /// The file path where the comment should be posted
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// The line number where the comment should be posted
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// The comment body/content
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// The severity level of the comment
    /// </summary>
    public CommentSeverity Severity { get; set; } = CommentSeverity.Info;

    /// <summary>
    /// Category of the review comment
    /// </summary>
    public ReviewCategory Category { get; set; } = ReviewCategory.General;
}

/// <summary>
/// Severity levels for review comments
/// </summary>
public enum CommentSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Categories for review comments
/// </summary>
public enum ReviewCategory
{
    General,
    Security,
    Performance,
    CodeQuality,
    BestPractices,
    Testing,
    Documentation
}
