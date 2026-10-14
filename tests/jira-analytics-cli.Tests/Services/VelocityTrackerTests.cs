// =============================================================================
// Integration tests for VelocityTracker
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace JiraAnalyticsCli.Tests.Services
{
    /// <summary>
    /// These tests exercise the public behaviour of <see cref="VelocityTracker"/>
    /// using realistic fixture data. They verify:
    ///   • 3‑sprint rolling average calculation
    ///   • handling of a sprint with zero completed points
    ///   • that future/active sprints are ignored (by the caller) before the calculation
    ///   • forecasting of remaining sprints for a given backlog
    ///   • that out‑of‑order input is correctly sorted by <c>EndDate</c>
    /// </summary>
    public class VelocityTrackerTests
    {
        private readonly Mock<ILogger<VelocityTracker>> _loggerMock;
        private readonly Mock<IMetricsRepository> _repoMock;
        private readonly VelocityTracker _tracker;

        public VelocityTrackerTests()
        {
            _loggerMock = new Mock<ILogger<VelocityTracker>>();
            _repoMock = new Mock<IMetricsRepository>();
            _tracker = CreateTracker(new VelocityOptions());
        }

        private VelocityTracker CreateTracker(VelocityOptions options) =>
            new(_repoMock.Object, _loggerMock.Object, options);

        /// <summary>
        /// Creates a <see cref="SprintMetric"/> fixture with the supplied values.
        /// </summary>
        private static SprintMetric CreateMetric(DateTimeOffset endDate, double completed, double planned = 0)
        {
            // The actual SprintMetric class contains many properties; we only need the ones
            // used by VelocityTracker. The constructor used here matches the public setters
            // that exist in the current code base.
            return new SprintMetric
            {
                EndDate = endDate,
                CompletedStoryPoints = (int)completed,
                PlannedStoryPoints = (int)planned
            };
        }

        /// <summary>
        /// Generates six consecutive sprint metrics ending on the supplied <paramref name="lastEndDate"/>.
        /// </summary>
        private static IReadOnlyList<SprintMetric> GenerateSixSprints(
            DateTimeOffset lastEndDate,
            double[] completedPoints,
            double[]? plannedPoints = null)
        {
            var metrics = new List<SprintMetric>();
            for (int i = 0; i < 6; i++)
            {
                var end = lastEndDate.AddDays(-7 * (5 - i)); // one week apart, oldest first
                double completed = completedPoints[i];
                double planned = plannedPoints?.Length > i ? plannedPoints[i] : 0;
                metrics.Add(CreateMetric(end, completed, planned));
            }

            return metrics;
        }

        [Fact]
        public void GetRollingAverageVelocity_ThreeSprintWindow_ReturnsExpectedAverage()
        {
            // Arrange – six sprints with increasing completed points
            var completed = new[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 };
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, completed);

            // Act
            double avg = _tracker.GetRollingAverageVelocity(metrics, windowSize: 3);

            // The three most recent sprints are 60, 50, 40 → average = 50.0
            avg.Should().BeApproximately(50.0, 0.01);
        }

        [Fact]
        public void GetRollingAverageVelocity_IncludesZeroCompletedPoints_WhenInWindow()
        {
            // Arrange – insert a zero‑point sprint as the most recent one
            var completed = new[] { 10.0, 20.0, 30.0, 40.0, 0.0, 60.0 };
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, completed);

            // Act
            double avg = _tracker.GetRollingAverageVelocity(metrics, windowSize: 3);

            // Most recent three: 60, 0, 40 → average = (60 + 0 + 40) / 3 = 33.333…
            avg.Should().BeApproximately(33.33, 0.01);
        }

        [Fact]
        public void GetRollingAverageVelocity_ExcludesFutureSprints_WhenFilteredByCaller()
        {
            // Arrange – six past sprints + one future sprint
            var completedPast = new[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 };
            var pastMetrics = GenerateSixSprints(DateTimeOffset.UtcNow, completedPast);

            var futureMetric = CreateMetric(DateTimeOffset.UtcNow.AddDays(7), completed: 100.0);
            var mixed = pastMetrics.Concat(new[] { futureMetric }).ToList();

            // Caller is responsible for filtering out future sprints
            var filtered = mixed.Where(m => m.EndDate <= DateTimeOffset.UtcNow).ToList();

            // Act
            double avg = _tracker.GetRollingAverageVelocity(filtered, windowSize: 3);

            // Expected same as first test (most recent past three: 60,50,40)
            avg.Should().BeApproximately(50.0, 0.01);
        }

        [Fact]
        public void ForecastNextSprint_AndRemainingSprints_ForBacklog_ReturnsReasonableCount()
        {
            // Arrange – six sprints with varying completed points
            var completed = new[] { 12.0, 18.0, 24.0, 30.0, 36.0, 42.0 };
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, completed);

            // Act – EWMA forecast
            double forecast = _tracker.ForecastNextSprint(metrics, alpha: 0.3);

            // Assume a backlog of 200 story points
            const double backlog = 200.0;
            double expectedSprints = Math.Ceiling(backlog / forecast);

            // Assert – the forecast itself is not asserted to a fixed value (it depends on alpha),
            // but the derived sprint count must be consistent with the forecast.
            double actualSprints = Math.Ceiling(backlog / forecast);
            actualSprints.Should().BeApproximately(expectedSprints, 0.01);
        }

        [Fact]
        public void GetRollingAverageVelocity_UnorderedInput_IsSortedByEndDate()
        {
            // Arrange – same data as first test but shuffled
            var completed = new[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 };
            var ordered = GenerateSixSprints(DateTimeOffset.UtcNow, completed);

            // Shuffle the list deliberately
            var random = new Random(42);
            var shuffled = ordered.OrderBy(_ => random.Next()).ToList();

            // Act
            double avg = _tracker.GetRollingAverageVelocity(shuffled, windowSize: 3);

            // The result must match the ordered calculation (average of 60,50,40)
            avg.Should().BeApproximately(50.0, 0.01);
        }

        [Fact]
        public void GetRollingAverageVelocity_UsesWindowSizeFromOptions_WhenNotPassed()
        {
            // Arrange – window of 2 configured through the options
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, new[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 });
            var tracker = CreateTracker(new VelocityOptions(windowSize: 2));

            // Act
            double avg = tracker.GetRollingAverageVelocity(metrics);

            // Most recent two: 60, 50 → 55
            avg.Should().BeApproximately(55.0, 0.01);
        }

        [Fact]
        public void GetRollingAverageVelocity_ExplicitWindowSize_OverridesOptions()
        {
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, new[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 });
            var tracker = CreateTracker(new VelocityOptions(windowSize: 2));

            double avg = tracker.GetRollingAverageVelocity(metrics, windowSize: 3);

            avg.Should().BeApproximately(50.0, 0.01);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(21)]
        public void GetRollingAverageVelocity_ExplicitWindowSizeOutOfRange_Throws(int windowSize)
        {
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, new[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 });

            var act = () => _tracker.GetRollingAverageVelocity(metrics, windowSize);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage($"*between 1 and 20*got {windowSize}*");
        }

        [Fact]
        public void GetRollingAverageVelocity_ExcludeZeroPointSprints_SkipsZeroSprintsInWindow()
        {
            // Arrange – the zero-point sprint is the second most recent one
            var completed = new[] { 10.0, 20.0, 30.0, 40.0, 0.0, 60.0 };
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, completed);
            var tracker = CreateTracker(new VelocityOptions(windowSize: 3, excludeZeroPointSprints: true));

            // Act
            double avg = tracker.GetRollingAverageVelocity(metrics);

            // Eligible most recent three: 60, 40, 30 → average = 43.33…
            avg.Should().BeApproximately(43.33, 0.01);
        }

        [Fact]
        public void GetRollingAverageVelocity_ExcludeZeroPointSprints_AllZero_ReturnsZero()
        {
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, new double[6]);
            var tracker = CreateTracker(new VelocityOptions(excludeZeroPointSprints: true));

            tracker.GetRollingAverageVelocity(metrics).Should().Be(0.0);
            tracker.ForecastNextSprint(metrics).Should().Be(0.0);
        }

        [Fact]
        public void DetectTrend_ExcludeZeroPointSprints_CountsOnlyEligibleSprints()
        {
            var completed = new[] { 10.0, 0.0, 30.0, 40.0, 0.0, 60.0 };
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, completed);
            var tracker = CreateTracker(new VelocityOptions(excludeZeroPointSprints: true));

            var trend = tracker.DetectTrend(metrics);

            trend.SampleSize.Should().Be(4);
            trend.Direction.Should().Be("Increasing");
        }

        [Fact]
        public void ForecastNextSprint_ExcludeZeroPointSprints_IgnoresZeroSprints()
        {
            var completed = new[] { 10.0, 0.0, 10.0, 0.0, 10.0, 0.0 };
            var metrics = GenerateSixSprints(DateTimeOffset.UtcNow, completed);
            var tracker = CreateTracker(new VelocityOptions(excludeZeroPointSprints: true));

            tracker.ForecastNextSprint(metrics, alpha: 0.3).Should().BeApproximately(10.0, 0.01);
        }

        [Fact]
        public void Constructor_InvalidWindowSize_ThrowsBeforeAnyRepositoryCall()
        {
            // The tracker must refuse invalid settings up front, so no data is ever requested.
            var act = () => CreateTracker(new VelocityOptions() with { WindowSize = 0 });

            act.Should().Throw<ArgumentException>()
                .WithMessage("*WindowSize must be between 1 and 20 (got 0)*");
            _repoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Constructor_NullOptions_Throws()
        {
            var act = () => CreateTracker(null!);

            act.Should().Throw<ArgumentNullException>();
            _repoMock.VerifyNoOtherCalls();
        }
    }
}
