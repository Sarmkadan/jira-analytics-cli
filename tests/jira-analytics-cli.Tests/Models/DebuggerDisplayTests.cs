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
    public void SprintMetric_ToString_WithNoPlannedPoints_ShowsCriticalAndZeroPercent()
    {
        var metric = new SprintMetric
        {
            SprintId = 7,
            SprintName = "Sprint 7",
            StartDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 1, 18, 0, 0, 0, TimeSpan.Zero)
        };

        metric.ToString().Should().Be("[7] Sprint 7 (Critical) - 0/0 pts (0.0%), 2026-01-05 to 2026-01-18");
    }

    [Fact]
    public void SprintMetric_ToString_WithPartialCompletion_RoundsPercentageToOneDecimal()
    {
        var metric = new SprintMetric
        {
            SprintId = 3,
            SprintName = "Sprint 3",
            PlannedStoryPoints = 3,
            CompletedStoryPoints = 1,
            CommittedStoryPoints = 3,
            CompletedIssueCount = 1,
            StartDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 1, 18, 0, 0, 0, TimeSpan.Zero)
        };

        metric.ToString().Should().Be("[3] Sprint 3 (Critical) - 1/3 pts (33.3%), 2026-01-05 to 2026-01-18");
    }

    [Fact]
    public void SprintMetric_ToString_WithTimeOfDayAndOffset_FormatsDatesWithoutTime()
    {
        var metric = new SprintMetric
        {
            SprintId = 11,
            SprintName = "Sprint 11",
            PlannedStoryPoints = 10,
            CompletedStoryPoints = 10,
            CommittedStoryPoints = 10,
            CompletedIssueCount = 10,
            StartDate = new DateTimeOffset(2026, 3, 2, 23, 30, 0, TimeSpan.FromHours(2)),
            EndDate = new DateTimeOffset(2026, 3, 16, 8, 15, 0, TimeSpan.FromHours(-5))
        };

        metric.ToString().Should().EndWith(", 2026-03-02 to 2026-03-16");
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

    [Fact]
    public void BurndownSnapshot_ToString_WithNoStoryPoints_ShowsZeroPercent()
    {
        var snapshot = new BurndownSnapshot
        {
            SprintId = 7,
            Timestamp = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero),
            TotalStoryPoints = 0,
            CompletedStoryPoints = 0,
            RemainingStoryPoints = 0
        };

        snapshot.ToString().Should().Be("[sprint 7] 2026-01-05 00:00 UTC - 0/0 pts done (0.0%), 0 remaining");
    }

    [Fact]
    public void BurndownSnapshot_ToString_WithNonUtcOffset_ShowsTimestampInUtc()
    {
        var snapshot = new BurndownSnapshot
        {
            SprintId = 42,
            Timestamp = new DateTimeOffset(2026, 9, 5, 14, 30, 0, TimeSpan.FromHours(2)),
            TotalStoryPoints = 20,
            CompletedStoryPoints = 5,
            RemainingStoryPoints = 15
        };

        snapshot.ToString().Should().Be("[sprint 42] 2026-09-05 12:30 UTC - 5/20 pts done (25.0%), 15 remaining");
    }

    [Fact]
    public void Developer_ToString_ShowsDisplayNameAndCompletedAndInProgressCounts()
    {
        var developer = new Developer
        {
            Key = "jdoe",
            Name = "jdoe",
            DisplayName = "Jane Doe",
            AssignedIssues =
            {
                new JiraIssue { Key = "PROJ-1", Status = "Done" },
                new JiraIssue { Key = "PROJ-2", Status = "Closed" },
                new JiraIssue { Key = "PROJ-3", Status = "In Progress" },
                new JiraIssue { Key = "PROJ-4", Status = "To Do" }
            }
        };

        developer.ToString().Should().Be("Jane Doe - 2 completed, 1 in progress");
    }

    [Fact]
    public void Developer_ToString_WithNoAssignedIssues_ShowsZeroCounts()
    {
        var developer = new Developer { Key = "jdoe", Name = "jdoe", DisplayName = "Jane Doe" };

        developer.ToString().Should().Be("Jane Doe - 0 completed, 0 in progress");
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
