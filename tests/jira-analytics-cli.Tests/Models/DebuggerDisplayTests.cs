// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Verifies that the core models expose key, status, points and dates through <c>ToString()</c>
/// and that the debugger shows that same text.
/// </summary>
public class DebuggerDisplayTests
{
    [Fact]
    public void Sprint_ToString_ShowsKeyStateStoryPointsAndDates()
    {
        var sprint = new Sprint
        {
            Id = 42,
            Key = "PROJ-S42",
            Name = "Sprint 42",
            State = "Closed",
            StartDate = new DateTime(2026, 9, 1),
            EndDate = new DateTime(2026, 9, 14),
            Issues =
            {
                new JiraIssue { Key = "PROJ-1", Status = "Done", IssueType = "Story", StoryPoints = 5, CreatedDate = new DateTime(2026, 8, 30) },
                new JiraIssue { Key = "PROJ-2", Status = "In Progress", IssueType = "Story", StoryPoints = 3, CreatedDate = new DateTime(2026, 8, 30) }
            }
        };

        sprint.ToString().Should().Be("[PROJ-S42] Sprint 42 (Closed) - 5/8 pts, 2026-09-01 to 2026-09-14, 1/2 issues completed");
    }

    [Fact]
    public void Sprint_ToString_UsesPlaceholderWhenDatesMissing()
    {
        var sprint = new Sprint { Key = "PROJ-S1", Name = "Sprint 1", State = "Future" };

        sprint.ToString().Should().Contain("n/a to n/a");
    }

    [Fact]
    public void JiraIssue_ToString_ShowsKeyStatusPointsAndDates()
    {
        var issue = new JiraIssue
        {
            Key = "PROJ-7",
            Status = "Done",
            IssueType = "Story",
            StoryPoints = 8,
            CreatedDate = new DateTime(2026, 9, 2),
            ResolutionDate = new DateTime(2026, 9, 10)
        };

        issue.ToString().Should().Be("[PROJ-7] Done - 8 pts, created 2026-09-02, resolved 2026-09-10");
    }

    [Fact]
    public void JiraIssue_ToString_ShowsOpenWhenUnresolvedAndZeroPointsWhenUnestimated()
    {
        var issue = new JiraIssue { Key = "PROJ-8", Status = "To Do", IssueType = "Task", CreatedDate = new DateTime(2026, 9, 2) };

        issue.ToString().Should().Be("[PROJ-8] To Do - 0 pts, created 2026-09-02, resolved open");
    }

    [Fact]
    public void SprintMetric_ToString_ShowsSprintIdHealthPointsAndDates()
    {
        var metric = new SprintMetric
        {
            SprintId = 42,
            SprintName = "Sprint 42",
            PlannedStoryPoints = 20,
            CompletedStoryPoints = 20,
            CommittedStoryPoints = 20,
            StartDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 9, 14, 0, 0, 0, TimeSpan.Zero)
        };

        metric.ToString().Should().Be("[42] Sprint 42 (Excellent) - 20/20 pts (100.0%), 2026-09-01 to 2026-09-14");
    }

    [Fact]
    public void BurndownSnapshot_ToString_ShowsSprintIdTimestampPointsAndRemaining()
    {
        var snapshot = new BurndownSnapshot
        {
            SprintId = 42,
            Timestamp = new DateTimeOffset(2026, 9, 5, 12, 30, 0, TimeSpan.Zero),
            TotalStoryPoints = 20,
            CompletedStoryPoints = 5,
            RemainingStoryPoints = 15,
            TotalIssueCount = 4,
            CompletedIssueCount = 1,
            RemainingIssueCount = 3
        };

        snapshot.ToString().Should().Be("[sprint 42] 2026-09-05 12:30 UTC - 5/20 pts done (25.0%), 15 remaining");
    }

    [Theory]
    [InlineData(typeof(Sprint))]
    [InlineData(typeof(JiraIssue))]
    [InlineData(typeof(SprintMetric))]
    [InlineData(typeof(BurndownSnapshot))]
    public void CoreModels_HaveDebuggerDisplayThatUsesToString(Type modelType)
    {
        var attribute = modelType.GetCustomAttributes(typeof(DebuggerDisplayAttribute), inherit: false)
            .OfType<DebuggerDisplayAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Value.Should().Be("{ToString(),nq}");
    }
}
