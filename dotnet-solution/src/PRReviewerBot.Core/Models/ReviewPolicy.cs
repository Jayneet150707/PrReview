namespace PRReviewerBot.Core.Models;

/// <summary>
/// Defines the review policy for the PR Reviewer Bot
/// CRITICAL: This bot is designed to ONLY review and comment - NEVER merge PRs
/// </summary>
public static class ReviewPolicy
{
    /// <summary>
    /// IMMUTABLE POLICY: This bot NEVER merges pull requests
    /// This is a safety measure to ensure manual review is always required
    /// </summary>
    public const bool NEVER_MERGE_PRS = true;

    /// <summary>
    /// IMMUTABLE POLICY: This bot NEVER approves pull requests automatically
    /// All PRs require manual human approval
    /// </summary>
    public const bool NEVER_AUTO_APPROVE = true;

    /// <summary>
    /// IMMUTABLE POLICY: This bot NEVER dismisses existing reviews
    /// Human reviews are always preserved
    /// </summary>
    public const bool NEVER_DISMISS_REVIEWS = true;

    /// <summary>
    /// Bot operation mode - COMMENT_ONLY is the only supported mode
    /// </summary>
    public const string OPERATION_MODE = "COMMENT_ONLY";

    /// <summary>
    /// Validates that the bot is operating in safe mode (comment-only)
    /// </summary>
    /// <returns>True if operating safely, throws exception if unsafe operations detected</returns>
    public static bool ValidateSafeOperation()
    {
        if (!NEVER_MERGE_PRS)
            throw new InvalidOperationException("SECURITY VIOLATION: Bot configured to merge PRs - this is not allowed");

        if (!NEVER_AUTO_APPROVE)
            throw new InvalidOperationException("SECURITY VIOLATION: Bot configured to auto-approve PRs - this is not allowed");

        if (!NEVER_DISMISS_REVIEWS)
            throw new InvalidOperationException("SECURITY VIOLATION: Bot configured to dismiss reviews - this is not allowed");

        if (OPERATION_MODE != "COMMENT_ONLY")
            throw new InvalidOperationException("SECURITY VIOLATION: Bot not in COMMENT_ONLY mode - this is not allowed");

        return true;
    }

    /// <summary>
    /// Gets the safety disclaimer for bot comments
    /// </summary>
    public static string GetSafetyDisclaimer()
    {
        return "⚠️ **IMPORTANT**: This is an automated review bot that ONLY posts comments. " +
               "It will NEVER merge, approve, or modify your PR. Manual human review and approval are still required.";
    }

    /// <summary>
    /// Gets the bot operation summary
    /// </summary>
    public static string GetOperationSummary()
    {
        return $"🤖 **Bot Mode**: {OPERATION_MODE}\n" +
               $"🚫 **Never Merges**: {(NEVER_MERGE_PRS ? "✅ Guaranteed" : "❌ UNSAFE")}\n" +
               $"🚫 **Never Auto-Approves**: {(NEVER_AUTO_APPROVE ? "✅ Guaranteed" : "❌ UNSAFE")}\n" +
               $"🚫 **Never Dismisses Reviews**: {(NEVER_DISMISS_REVIEWS ? "✅ Guaranteed" : "❌ UNSAFE")}";
    }
}
