// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JiraAnalyticsCli.Tests.Services;

/// <summary>
/// Verifies the Debug-level summary of analysis inputs that <see cref="AnalyticsService.AnalyzeSprints"/> writes for <c>--verbose</c>.
/// </summary>
public class AnalyticsServiceVerboseLoggingTests
{
    [Fact]
    public async Task AnalyzeSprints_LogsIssueCountsAndTotalPointsAtDebugLevel()
    {
        var jira = new Mock<IJiraApiService>();
        jira.Setup(s => s.GetProjectSprintsAsync("PROJ")).ReturnsAsync(new List<Sprint>
        {
            new() { Id = 1, Key = "PROJ-S1", Name = "Sprint 1", State = "Closed", EndDate = new DateTime(2026, 9, 14) },
            new() { Id = 2, Key = "PROJ-S2", Name = "Sprint 2", State = "Closed", EndDate = new DateTime(2026, 9, 28) }
        });
        jira.Setup(s => s.GetSprintIssuesAsync(1)).ReturnsAsync(new List<JiraIssue>
        {
            new() { Key = "PROJ-1", Status = "Done", IssueType = "Story", StoryPoints = 5, CreatedDate = new DateTime(2026, 9, 1) },
            new() { Key = "PROJ-2", Status = "To Do", IssueType = "Story", StoryPoints = 3, CreatedDate = new DateTime(2026, 9, 1) }
        });
        jira.Setup(s => s.GetSprintIssuesAsync(2)).ReturnsAsync(new List<JiraIssue>
        {
            new() { Key = "PROJ-3", Status = "Done", IssueType = "Story", StoryPoints = 8, CreatedDate = new DateTime(2026, 9, 15) }
        });

        var logger = new CapturingLogger<AnalyticsService>();
        var sut = new AnalyticsService(jira.Object, new Mock<IMetricsRepository>().Object, logger);

        await sut.AnalyzeSprints("PROJ", 5);

        var debugLine = logger.Messages.Should().ContainSingle(m => m.Level == LogLevel.Debug).Subject;
        debugLine.Message.Should().Be(
            "Analysis inputs for project PROJ: 2 sprints, 3 issues, 16 planned SP, 13 completed SP");
    }
}
