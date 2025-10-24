using FluentAssertions;
using PRReviewerBot.Core.Models;
using Xunit;

namespace PRReviewerBot.Tests;

/// <summary>
/// Tests for ReviewPolicy safety mechanisms
/// These tests ensure the bot operates in COMMENT-ONLY mode and never performs unsafe operations
/// </summary>
public class ReviewPolicyTests
{
    [Fact]
    public void ReviewPolicy_Should_Never_Allow_Merging_PRs()
    {
        // Arrange & Act & Assert
        ReviewPolicy.NEVER_MERGE_PRS.Should().BeTrue("Bot must never merge PRs - this is a critical safety requirement");
    }

    [Fact]
    public void ReviewPolicy_Should_Never_Allow_Auto_Approval()
    {
        // Arrange & Act & Assert
        ReviewPolicy.NEVER_AUTO_APPROVE.Should().BeTrue("Bot must never auto-approve PRs - human approval is always required");
    }

    [Fact]
    public void ReviewPolicy_Should_Never_Allow_Dismissing_Reviews()
    {
        // Arrange & Act & Assert
        ReviewPolicy.NEVER_DISMISS_REVIEWS.Should().BeTrue("Bot must never dismiss human reviews - they must be preserved");
    }

    [Fact]
    public void ReviewPolicy_Should_Only_Operate_In_Comment_Mode()
    {
        // Arrange & Act & Assert
        ReviewPolicy.OPERATION_MODE.Should().Be("COMMENT_ONLY", "Bot must only operate in comment-only mode for safety");
    }

    [Fact]
    public void ValidateSafeOperation_Should_Pass_With_Default_Settings()
    {
        // Arrange & Act
        var result = ReviewPolicy.ValidateSafeOperation();

        // Assert
        result.Should().BeTrue("Default safety settings should pass validation");
    }

    [Fact]
    public void GetSafetyDisclaimer_Should_Return_Clear_Warning()
    {
        // Arrange & Act
        var disclaimer = ReviewPolicy.GetSafetyDisclaimer();

        // Assert
        disclaimer.Should().NotBeNullOrEmpty("Safety disclaimer must be provided");
        disclaimer.Should().Contain("ONLY posts comments", "Disclaimer must clarify comment-only operation");
        disclaimer.Should().Contain("NEVER merge", "Disclaimer must explicitly state no merging");
        disclaimer.Should().Contain("Manual human review", "Disclaimer must require human review");
    }

    [Fact]
    public void GetOperationSummary_Should_Show_All_Safety_Guarantees()
    {
        // Arrange & Act
        var summary = ReviewPolicy.GetOperationSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty("Operation summary must be provided");
        summary.Should().Contain("COMMENT_ONLY", "Summary must show operation mode");
        summary.Should().Contain("Never Merges", "Summary must show merge protection");
        summary.Should().Contain("Never Auto-Approves", "Summary must show approval protection");
        summary.Should().Contain("Never Dismisses Reviews", "Summary must show review protection");
        summary.Should().Contain("✅ Guaranteed", "Summary must show safety guarantees");
    }

    [Theory]
    [InlineData("MERGE_MODE")]
    [InlineData("AUTO_APPROVE")]
    [InlineData("FULL_AUTOMATION")]
    [InlineData("")]
    [InlineData(null)]
    public void Operation_Mode_Should_Never_Be_Anything_Other_Than_Comment_Only(string? invalidMode)
    {
        // Arrange & Act & Assert
        ReviewPolicy.OPERATION_MODE.Should().NotBe(invalidMode, 
            $"Operation mode must never be '{invalidMode}' - only COMMENT_ONLY is safe");
        ReviewPolicy.OPERATION_MODE.Should().Be("COMMENT_ONLY", 
            "Operation mode must always be COMMENT_ONLY for safety");
    }

    [Fact]
    public void All_Safety_Constants_Should_Be_Immutable()
    {
        // This test verifies that safety constants are compile-time constants
        // and cannot be changed at runtime
        
        // Arrange & Act & Assert
        var neverMergeField = typeof(ReviewPolicy).GetField(nameof(ReviewPolicy.NEVER_MERGE_PRS));
        var neverApproveField = typeof(ReviewPolicy).GetField(nameof(ReviewPolicy.NEVER_AUTO_APPROVE));
        var neverDismissField = typeof(ReviewPolicy).GetField(nameof(ReviewPolicy.NEVER_DISMISS_REVIEWS));
        var operationModeField = typeof(ReviewPolicy).GetField(nameof(ReviewPolicy.OPERATION_MODE));

        neverMergeField.Should().NotBeNull("NEVER_MERGE_PRS field must exist");
        neverMergeField!.IsLiteral.Should().BeTrue("NEVER_MERGE_PRS must be a compile-time constant");
        neverMergeField.IsInitOnly.Should().BeFalse("NEVER_MERGE_PRS should be const, not readonly");

        neverApproveField.Should().NotBeNull("NEVER_AUTO_APPROVE field must exist");
        neverApproveField!.IsLiteral.Should().BeTrue("NEVER_AUTO_APPROVE must be a compile-time constant");

        neverDismissField.Should().NotBeNull("NEVER_DISMISS_REVIEWS field must exist");
        neverDismissField!.IsLiteral.Should().BeTrue("NEVER_DISMISS_REVIEWS must be a compile-time constant");

        operationModeField.Should().NotBeNull("OPERATION_MODE field must exist");
        operationModeField!.IsLiteral.Should().BeTrue("OPERATION_MODE must be a compile-time constant");
    }
}
