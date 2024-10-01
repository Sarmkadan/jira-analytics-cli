// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Provides unit tests for the <see cref="SprintMetric"/> class.
/// </summary>
public class SprintMetricTests
{
    /// <summary>
    /// Builds a <see cref="SprintMetric"/> instance with default values.
    /// </summary>
    /// <param name="planned">The planned story points.</param>
    /// <param name="completed">The completed story points.</param>
    /// <param name="committed">The committed story points.</param>
    /// <param name="completedIssues">The completed issue count.</param>
    /// <param name="totalIssues">The total issue count.</param>
    /// <param name="defects">The defects count.</param>
    /// <param name="overdue">The overdue issue count.</param>
    /// <param name="scopeChanges">The scope change count.</param>
    /// <param name="teamSize">The team size.</param>
    /// <param name="durationDays">The sprint duration in days.</param>
    /// <returns>A <see cref="SprintMetric"/> instance.</returns>
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

    /// <summary>
    /// Verifies that the <see cref="GetCompletionRate"/> method returns the correct completion rate when the planned points are non-zero.
    /// </summary>
    [Fact]
    public void GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage()
    {
        var metric = BuildMetric(planned: 100, completed: 75);

        var rate = metric.GetCompletionRate();

        rate.Should().BeApproximately(75.0, precision: 0.001);
    }

    /// <summary>
    /// Verifies that the <see cref="GetCompletionRate"/> method returns zero when the planned points are zero.
    /// </summary>
    [Fact]
    public void GetCompletionRate_WithZeroPlannedPoints_ReturnsZero()
    {
        var metric = BuildMetric(planned: 0, completed: 0);

        var rate = metric.GetCompletionRate();

        rate.Should().Be(0);
    }

    /// <summary>
    /// Verifies that the <see cref="GetHealthStatus"/> method returns "Excellent" when all metrics meet the excellent thresholds.
    /// </summary>
    [Fact]
    public void GetHealthStatus_WithAllMetricsMeetingExcellentThresholds_ReturnsExcellent()
    {
        // completionRate >= 95, qualityScore >= 90, riskScore < 20
        var metric = BuildMetric(planned: 100, completed: 96, defects: 0, overdue: 0, scopeChanges: 0);

        var status = metric.GetHealthStatus();

        status.Should().Be("Excellent");
    }

    /// <summary>
    /// Verifies that the <see cref="GetHealthStatus"/> method returns "Critical" when the completion rate is low and the defect rate is high.
    /// </summary>
    [Fact]
    public void GetHealthStatus_WithLowCompletionRateAndHighDefects_ReturnsCritical()
    {
        // completionRate = 50, defectRate = 100% → qualityScore = 0, riskScore = 100
        var metric = BuildMetric(planned: 100, completed: 50, defects: 10, completedIssues: 10, overdue: 5, scopeChanges: 5);

        var status = metric.GetHealthStatus();

        status.Should().Be("Critical");
    }

    /// <summary>
    /// Verifies that the <see cref="GetRiskScore"/> method aggregates all risk factors when there are overdue and scope changes.
    /// </summary>
    [Fact]
    public void GetRiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors()
    {
        // overdueRisk = 2*5=10, scopeRisk = 2*3=6, qualityRisk = (100-100)*2=0 → total=16
        var metric = BuildMetric(planned: 100, completed: 100, defects: 0, completedIssues: 10, overdue: 2, scopeChanges: 2);

        var risk = metric.GetRiskScore();

        risk.Should().BeApproximately(16.0, precision: 0.001);
    }

    /// <summary>
    /// Verifies that the <see cref="Validate"/> method throws an <see cref="ArgumentException"/> when the end date is before the start date.
    /// </summary>
    [Fact]
    public void Validate_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        var metric = BuildMetric();
        metric.EndDate = metric.StartDate.AddDays(-1);

        var act = () => metric.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("*End date must be after start date*");
    }
}
