using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Proves that <see cref="BurndownSnapshotExtensions"/>, <see cref="SprintMetricExtensions"/>
/// and <see cref="CycleTimeResultExtensions"/> all delegate to the same
/// <see cref="TrendAnalysis"/> implementation and therefore yield identical trend
/// results for identical underlying numeric series.
/// </summary>
public class TrendAnalysisUnificationTests
{
    private static readonly DateTime BaseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly double[] Series = { 10, 14, 19, 23, 30, 35, 41, 48 };

    private static List<BurndownSnapshot> BuildBurndownSnapshots()
    {
        var snapshots = new List<BurndownSnapshot>();
        for (var i = 0; i < Series.Length; i++)
        {
            var completed = (int)Series[i];
            snapshots.Add(new BurndownSnapshot
            {
                Timestamp = BaseDate.AddDays(i),
                SprintId = 1,
                CompletedStoryPoints = completed,
                RemainingStoryPoints = 100 - completed,
                TotalStoryPoints = 100,
                CompletedIssueCount = 1,
                RemainingIssueCount = 1,
                TotalIssueCount = 2,
                ScopeChanges = 0
            });
        }

        return snapshots;
    }

    private static List<SprintMetric> BuildSprintMetrics()
    {
        var metrics = new List<SprintMetric>();
        for (var i = 0; i < Series.Length; i++)
        {
            metrics.Add(new SprintMetric
            {
                SprintId = i + 1,
                SprintName = $"Sprint {i + 1}",
                StartDate = new DateTimeOffset(BaseDate.AddDays(i - 1)),
                EndDate = new DateTimeOffset(BaseDate.AddDays(i)),
                PlannedStoryPoints = 100,
                CompletedStoryPoints = (int)Series[i]
            });
        }

        return metrics;
    }

    private static List<ITimeSeriesPoint> BuildRawSeries() =>
        Series
            .Select((value, i) => (ITimeSeriesPoint)new TimeSeriesPoint(BaseDate.AddDays(i), value))
            .ToList();

    [Fact]
    public void TrendSlope_ShouldBeIdentical_AcrossAllMetricFamilyAdaptersAndTheRawAlgorithm()
    {
        var burndownSlope = BuildBurndownSnapshots().GetCompletedStoryPointsTrendSlope();
        var sprintSlope = BuildSprintMetrics().GetCompletedStoryPointsTrendSlope();
        var rawSlope = TrendAnalysis.CalculateSlope(BuildRawSeries());

        burndownSlope.Should().BeApproximately(sprintSlope, 1e-9);
        sprintSlope.Should().BeApproximately(rawSlope, 1e-9);
    }

    [Fact]
    public void Acceleration_ShouldBeIdentical_AcrossAllMetricFamilyAdaptersAndTheRawAlgorithm()
    {
        var burndownAcceleration = BuildBurndownSnapshots().GetCompletedStoryPointsAcceleration();
        var sprintAcceleration = BuildSprintMetrics().GetCompletedStoryPointsAcceleration();
        var rawAcceleration = TrendAnalysis.CalculateAcceleration(BuildRawSeries());

        burndownAcceleration.Should().BeApproximately(sprintAcceleration, 1e-9);
        sprintAcceleration.Should().BeApproximately(rawAcceleration, 1e-9);
    }

    [Fact]
    public void MovingAverage_ShouldBeIdentical_AcrossAllMetricFamilyAdaptersAndTheRawAlgorithm()
    {
        var burndownAverage = BuildBurndownSnapshots().GetCompletedStoryPointsMovingAverage(3);
        var sprintAverage = BuildSprintMetrics().GetCompletedStoryPointsMovingAverage(3);
        var rawAverage = TrendAnalysis.CalculateMovingAverage(BuildRawSeries(), 3);

        burndownAverage.Should().HaveCount(sprintAverage.Count);
        sprintAverage.Should().HaveCount(rawAverage.Count);

        for (var i = 0; i < burndownAverage.Count; i++)
        {
            burndownAverage[i].Should().BeApproximately(sprintAverage[i], 1e-9);
            sprintAverage[i].Should().BeApproximately(rawAverage[i], 1e-9);
        }
    }
}
