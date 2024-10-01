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

/// <summary>
/// Tests for the AnalyticsService class.
/// </summary>
namespace JiraAnalyticsCli.Tests.Services;

public class AnalyticsServiceTests
{
    private readonly Mock<IJiraApiService> _jiraServiceMock;
    private readonly Mock<IMetricsRepository> _metricsRepoMock;
    private readonly Mock<ILogger<AnalyticsService>> _loggerMock;
    private readonly AnalyticsService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsServiceTests"/> class.
    /// </summary>
    public AnalyticsServiceTests()
    {
        _jiraServiceMock = new Mock<IJiraApiService>();
        _metricsRepoMock = new Mock<IMetricsRepository>();
        _loggerMock = new Mock<ILogger<AnalyticsService>>();

        _sut = new AnalyticsService(
            _jiraServiceMock.Object,
            _metricsRepoMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Tests that AnalyzeOverdueIssues returns zero count when the project has no issues.
    /// </summary>
    [Fact]
    public async Task AnalyzeOverdueIssues_WhenProjectHasNoIssues_ReturnsZeroCount()
    {
        _jiraServiceMock
            .Setup(s => s.GetProjectIssuesAsync("EMPTY"))
            .ReturnsAsync(new List<JiraIssue>());

        var result = await _sut.AnalyzeOverdueIssues("EMPTY");

        result.TotalOverdueCount.Should().Be(0);
        result.CriticalCount.Should().Be(0);
        result.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that AnalyzeOverdueIssues counts only overdue issues when the project has mixed issues.
    /// </summary>
    /// <param name="issues">A list of Jira issues.</param>
    [Fact]
    public async Task AnalyzeOverdueIssues_WithMixedIssues_CountsOnlyOverdueOnes()
    {
        var issues = new List<JiraIssue>
        {
            // Overdue: past due date, not resolved, not done
            new JiraIssue
            {
                Key = "PROJ-1", Id = "1", Summary = "Late task",
                Status = "In Progress", IssueType = "Task", Priority = "Medium",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                UpdatedDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(-3)
            },
            // Not overdue: future due date
            new JiraIssue
            {
                Key = "PROJ-2", Id = "2", Summary = "On-track task",
                Status = "In Progress", IssueType = "Task", Priority = "Low",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                UpdatedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5)
            },
            // Not overdue: resolved
            new JiraIssue
            {
                Key = "PROJ-3", Id = "3", Summary = "Completed task",
                Status = "Done", IssueType = "Story", Priority = "High",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                UpdatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-1),
                ResolutionDate = DateTime.UtcNow.AddDays(-2)
            }
        };

        _jiraServiceMock
            .Setup(s => s.GetProjectIssuesAsync("PROJ"))
            .ReturnsAsync(issues);

        var result = await _sut.AnalyzeOverdueIssues("PROJ");

        result.TotalOverdueCount.Should().Be(1);
        result.Issues.Should().ContainSingle(i => i.Key == "PROJ-1");
    }

    /// <summary>
    /// Tests that AnalyzeOverdueIssues correctly counts critical issues when the project has high-priority overdue issues.
    /// </summary>
    /// <param name="issues">A list of Jira issues.</param>
    [Fact]
    public async Task AnalyzeOverdueIssues_WithHighPriorityOverdueIssues_CorrectlyCountsCritical()
    {
        var issues = new List<JiraIssue>
        {
            new JiraIssue
            {
                Key = "PROJ-10", Id = "10", Summary = "Critical overdue",
                Status = "Open", IssueType = "Bug", Priority = "Critical",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                UpdatedDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(-7)
            },
            new JiraIssue
            {
                Key = "PROJ-11", Id = "11", Summary = "Blocker overdue",
                Status = "In Progress", IssueType = "Bug", Priority = "Blocker",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                UpdatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-2)
            },
            new JiraIssue
            {
                Key = "PROJ-12", Id = "12", Summary = "Low priority overdue",
                Status = "In Progress", IssueType = "Task", Priority = "Low",
                CreatedDate = DateTime.UtcNow.AddDays(-12),
                UpdatedDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(-4)
            }
        };

        _jiraServiceMock
            .Setup(s => s.GetProjectIssuesAsync("PROJ"))
            .ReturnsAsync(issues);

        var result = await _sut.AnalyzeOverdueIssues("PROJ");

        result.TotalOverdueCount.Should().Be(3);
        result.CriticalCount.Should().Be(2);
    }
}
