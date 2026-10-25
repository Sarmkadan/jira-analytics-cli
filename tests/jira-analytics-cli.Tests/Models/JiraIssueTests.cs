// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Tests for the JiraIssue class.
/// </summary>
public class JiraIssueTests
{
    /// <summary>
    /// Builds a JiraIssue instance with default values.
    /// </summary>
    /// <param name="key">The issue key.</param>
    /// <param name="summary">The issue summary.</param>
    /// <param name="status">The issue status.</param>
    /// <param name="priority">The issue priority.</param>
    /// <param name="dueDate">The due date of the issue.</param>
    /// <param name="resolutionDate">The resolution date of the issue.</param>
    /// <param name="storyPoints">The story points of the issue.</param>
    /// <returns>A JiraIssue instance.</returns>
    private static JiraIssue BuildIssue(
        string key = "PROJ-1",
        string summary = "Sample issue",
        string status = "In Progress",
        string priority = "Medium",
        DateTime? dueDate = null,
        DateTime? resolutionDate = null,
        int? storyPoints = null)
    {
        return new JiraIssue
        {
            Key = key,
            Id = "10001",
            Summary = summary,
            Status = status,
            IssueType = "Story",
            Priority = priority,
            CreatedDate = DateTime.UtcNow.AddDays(-30),
            UpdatedDate = DateTime.UtcNow.AddDays(-1),
            DueDate = dueDate,
            ResolutionDate = resolutionDate,
            StoryPoints = storyPoints
        };
    }

    /// <summary>
    /// Verifies that IsOverdue returns true when the due date is in the past and the status is open.
    /// </summary>
    [Fact]
    public void IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue()
    {
        var issue = BuildIssue(
            status: "In Progress",
            dueDate: DateTime.UtcNow.AddDays(-5));

        issue.IsOverdue().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsOverdue returns false when the status is done.
    /// </summary>
    [Fact]
    public void IsOverdue_WhenStatusIsDone_ReturnsFalse()
    {
        var issue = BuildIssue(
            status: "Done",
            dueDate: DateTime.UtcNow.AddDays(-3));

        issue.IsOverdue().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsOverdue returns false when the resolution date is set.
    /// </summary>
    [Fact]
    public void IsOverdue_WhenResolutionDateIsSet_ReturnsFalse()
    {
        var issue = BuildIssue(
            status: "In Progress",
            dueDate: DateTime.UtcNow.AddDays(-5),
            resolutionDate: DateTime.UtcNow.AddDays(-1));

        issue.IsOverdue().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsHighPriority returns true when the priority is critical.
    /// </summary>
    [Fact]
    public void IsHighPriority_WithCriticalPriority_ReturnsTrue()
    {
        var issue = BuildIssue(priority: "Critical");

        issue.IsHighPriority().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsHighPriority returns false when the priority is medium.
    /// </summary>
    [Fact]
    public void IsHighPriority_WithMediumPriority_ReturnsFalse()
    {
        var issue = BuildIssue(priority: "Medium");

        issue.IsHighPriority().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetCycleTime returns the days between creation and resolution when the resolution date is set.
    /// </summary>
    [Fact]
    public void GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution()
    {
        var created = DateTime.UtcNow.AddDays(-10);
        var resolved = DateTime.UtcNow.AddDays(-2);

        var issue = new JiraIssue
        {
            Key = "PROJ-42",
            Id = "42",
            Summary = "Fix critical bug",
            Status = "Done",
            IssueType = "Bug",
            CreatedDate = created,
            UpdatedDate = resolved,
            ResolutionDate = resolved
        };

        var cycleTime = issue.GetCycleTime();

        cycleTime.Should().BeApproximately(8.0, precision: 0.1);
    }

    /// <summary>
    /// Verifies that Validate throws an ArgumentException when the key is null.
    /// </summary>
    [Fact]
    public void Validate_WhenKeyIsNull_ThrowsArgumentException()
    {
        var issue = BuildIssue();
        issue.Key = null!;

        var act = () => issue.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("Issue key cannot be empty*");
    }

    /// <summary>
    /// Verifies that Validate throws an ArgumentException when the summary is null.
    /// </summary>
    [Fact]
    public void Validate_WhenSummaryIsNull_ThrowsArgumentException()
    {
        var issue = BuildIssue();
        issue.Summary = null!;

        var act = () => issue.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("Issue summary cannot be empty*");
    }

    /// <summary>
    /// Verifies that Validate on a default-constructed issue reports the missing key rather than throwing a null reference.
    /// </summary>
    [Fact]
    public void Validate_OnDefaultInstance_ThrowsArgumentExceptionForMissingKey()
    {
        var issue = new JiraIssue();

        var act = () => issue.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("Issue key cannot be empty*");
    }

    /// <summary>
    /// Verifies that IsOverdue does not throw when the status is null and the due date has passed.
    /// </summary>
    [Fact]
    public void IsOverdue_WhenStatusIsNull_DoesNotThrowAndReturnsTrue()
    {
        var issue = BuildIssue(dueDate: DateTime.UtcNow.AddDays(-5));
        issue.Status = null!;

        var act = () => issue.IsOverdue();

        act.Should().NotThrow();
        act().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsHighPriority returns false without throwing when the priority is null.
    /// </summary>
    [Fact]
    public void IsHighPriority_WhenPriorityIsNull_ReturnsFalse()
    {
        var issue = BuildIssue();
        issue.Priority = null!;

        var act = () => issue.IsHighPriority();

        act.Should().NotThrow();
        act().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsInProgress returns false without throwing when the status is null.
    /// </summary>
    [Fact]
    public void IsInProgress_WhenStatusIsNull_ReturnsFalse()
    {
        var issue = BuildIssue();
        issue.Status = null!;

        var act = () => issue.IsInProgress();

        act.Should().NotThrow();
        act().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ToString does not throw when the key and status are null.
    /// </summary>
    [Fact]
    public void ToString_WhenKeyAndStatusAreNull_DoesNotThrow()
    {
        var issue = BuildIssue();
        issue.Key = null!;
        issue.Status = null!;

        var act = () => issue.ToString();

        act.Should().NotThrow();
        act().Should().Contain("created");
    }
}
