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
}
