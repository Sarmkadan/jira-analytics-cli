// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

public class SprintMetricTests
{
    private static SprintMetric BuildMetric(
        int planned = 100,
        int completed = 80,
        int committed = 80,
        int completedIssues = 10,
        int totalIssues = 12,
        int defects = 0,
        int overdue = 0,
        int scopeChanges = 0,
        int teamSize = 5,
        int durationDays = 14)
    {
        var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new SprintMetric
        {
            SprintId = 1,
            SprintName = "Sprint 1",
            StartDate = start,
            EndDate = start.AddDays(durationDays),
            PlannedStoryPoints = planned,
            CompletedStoryPoints = completed,
            CommittedStoryPoints = committed,
            CompletedIssueCount = completedIssues,
            TotalIssueCount = totalIssues,
            DefectsCount = defects,
            OverdueIssueCount = overdue,
            ScopeChangeCount = scopeChanges,
            TeamSize = teamSize
        };
    }

    [Fact]
    public void GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage()
    {
        var metric = BuildMetric(planned: 100, completed: 75);

        var rate = metric.GetCompletionRate();

        rate.Should().BeApproximately(75.0, precision: 0.001);
    }

    [Fact]
    public void GetCompletionRate_WithZeroPlannedPoints_ReturnsZero()
    {
        var metric = BuildMetric(planned: 0, completed: 0);

        var rate = metric.GetCompletionRate();

        rate.Should().Be(0);
    }

    [Fact]
    public void GetHealthStatus_WithAllMetricsMeetingExcellentThresholds_ReturnsExcellent()
    {
        // completionRate >= 95, qualityScore >= 90, riskScore < 20
        var metric = BuildMetric(planned: 100, completed: 96, defects: 0, overdue: 0, scopeChanges: 0);

        var status = metric.GetHealthStatus();

        status.Should().Be("Excellent");
    }

    [Fact]
    public void GetHealthStatus_WithLowCompletionRateAndHighDefects_ReturnsCritical()
    {
        // completionRate = 50, defectRate = 100% → qualityScore = 0, riskScore = 100
        var metric = BuildMetric(planned: 100, completed: 50, defects: 10, completedIssues: 10, overdue: 5, scopeChanges: 5);

        var status = metric.GetHealthStatus();

        status.Should().Be("Critical");
    }

    [Fact]
    public void GetRiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors()
    {
        // overdueRisk = 2*5=10, scopeRisk = 2*3=6, qualityRisk = (100-100)*2=0 → total=16
        var metric = BuildMetric(planned: 100, completed: 100, defects: 0, completedIssues: 10, overdue: 2, scopeChanges: 2);

        var risk = metric.GetRiskScore();

        risk.Should().BeApproximately(16.0, precision: 0.001);
    }

    [Fact]
    public void Validate_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        var metric = BuildMetric();
        metric.EndDate = metric.StartDate.AddDays(-1);

        var act = () => metric.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("*End date must be after start date*");
    }
}
