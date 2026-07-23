// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a single observation in a numeric time series, used as the common
/// input shape for trend/delta calculations shared across the various metric types
/// (burndown snapshots, sprint metrics, cycle time results, etc).
/// </summary>
public interface ITimeSeriesPoint
{
    /// <summary>Gets the point in time the observation was recorded at.</summary>
    DateTime Timestamp { get; }

    /// <summary>Gets the numeric value observed at <see cref="Timestamp"/>.</summary>
    double Value { get; }
}

/// <summary>
/// Immutable, minimal implementation of <see cref="ITimeSeriesPoint"/> used by the
/// adapter extension methods to project domain-specific metric types into a shape
/// the shared <see cref="TrendAnalysis"/> helper understands.
/// </summary>
/// <param name="Timestamp">The point in time the observation was recorded at.</param>
/// <param name="Value">The numeric value observed at <paramref name="Timestamp"/>.</param>
public sealed record TimeSeriesPoint(DateTime Timestamp, double Value) : ITimeSeriesPoint;

/// <summary>
/// Single implementation of the trend/delta/formatting math shared by every metric
/// family in the codebase (<see cref="BurndownSnapshot"/>, <see cref="SprintMetric"/>,
/// <see cref="IssueCycleTime"/>). Each type's extension class is a thin adapter that
/// projects its own data into <see cref="ITimeSeriesPoint"/> and delegates here, which
/// guarantees identical numeric series always produce identical trend results
/// regardless of which domain type they originated from.
/// </summary>
public static class TrendAnalysis
{
    /// <summary>
    /// Calculates the linear trend slope (units of <see cref="ITimeSeriesPoint.Value"/>
    /// per day) across the series using ordinary least-squares regression against
    /// elapsed days since the earliest point.
    /// </summary>
    /// <param name="points">The series to analyze. Order is not assumed; points are sorted by timestamp internally.</param>
    /// <returns>The slope in value-per-day, or 0 when fewer than two distinct-timestamp points are available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
    public static double CalculateSlope(IReadOnlyList<ITimeSeriesPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var ordered = points.OrderBy(p => p.Timestamp).ToList();
        if (ordered.Count < 2)
            return 0;

        var origin = ordered[0].Timestamp;
        var xs = ordered.Select(p => (p.Timestamp - origin).TotalDays).ToList();
        var ys = ordered.Select(p => p.Value).ToList();

        return LeastSquaresSlope(xs, ys);
    }

    /// <summary>
    /// Calculates the acceleration of the series: the difference between the trend
    /// slope of the second half and the trend slope of the first half, expressed in
    /// value-per-day-squared. A positive result means the series is growing faster
    /// (or shrinking slower) over time; a negative result means the opposite.
    /// </summary>
    /// <param name="points">The series to analyze. Order is not assumed; points are sorted by timestamp internally.</param>
    /// <returns>The acceleration, or 0 when fewer than four distinct-timestamp points are available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
    public static double CalculateAcceleration(IReadOnlyList<ITimeSeriesPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var ordered = points.OrderBy(p => p.Timestamp).ToList();
        if (ordered.Count < 4)
            return 0;

        var mid = ordered.Count / 2;
        var firstHalf = ordered.Take(mid).ToList();
        var secondHalf = ordered.Skip(mid).ToList();

        var firstSlope = CalculateSlope(firstHalf);
        var secondSlope = CalculateSlope(secondHalf);

        var daysBetweenMidpoints = (Midpoint(secondHalf) - Midpoint(firstHalf)).TotalDays;
        if (daysBetweenMidpoints <= 0)
            return 0;

        return (secondSlope - firstSlope) / daysBetweenMidpoints;
    }

    /// <summary>
    /// Computes a simple trailing moving average of the series values, ordered by
    /// timestamp ascending. Each output element is the average of up to
    /// <paramref name="windowSize"/> preceding values (inclusive of the current one).
    /// </summary>
    /// <param name="points">The series to analyze. Order is not assumed; points are sorted by timestamp internally.</param>
    /// <param name="windowSize">The number of trailing points to average over.</param>
    /// <returns>A list of moving-average values, one per input point, in chronological order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
    public static IReadOnlyList<double> CalculateMovingAverage(IReadOnlyList<ITimeSeriesPoint> points, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(windowSize, 0);

        var ordered = points.OrderBy(p => p.Timestamp).Select(p => p.Value).ToList();
        var result = new List<double>(ordered.Count);

        for (var i = 0; i < ordered.Count; i++)
        {
            var start = Math.Max(0, i - windowSize + 1);
            var window = ordered.Skip(start).Take(i - start + 1);
            result.Add(window.Average());
        }

        return result;
    }

    private static double LeastSquaresSlope(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
    {
        var n = xs.Count;
        var meanX = xs.Average();
        var meanY = ys.Average();

        double numerator = 0;
        double denominator = 0;
        for (var i = 0; i < n; i++)
        {
            numerator += (xs[i] - meanX) * (ys[i] - meanY);
            denominator += (xs[i] - meanX) * (xs[i] - meanX);
        }

        return denominator == 0 ? 0 : numerator / denominator;
    }

    private static DateTime Midpoint(IReadOnlyList<ITimeSeriesPoint> ordered) =>
        ordered[ordered.Count / 2].Timestamp;
}

/// <summary>
/// Extension methods for <see cref="BurndownSnapshot"/> that provide additional functionality
/// for analysis, comparison, and reporting on burndown data.
/// </summary>
public static class BurndownSnapshotExtensions
{
    /// <summary>
    /// Calculates the velocity trend over time by comparing the current burndown
    /// to historical snapshots from previous days.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="historicalSnapshots">Historical snapshots sorted by timestamp (newest first)</param>
    /// <returns>Velocity trend in story points per day, or 0 if insufficient data or snapshot is invalid</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot or historicalSnapshots is null</exception>
    /// <exception cref="ArgumentException">Thrown when snapshot contains validation errors</exception>
    public static double CalculateVelocityTrend(
        this BurndownSnapshot snapshot,
        IReadOnlyList<BurndownSnapshot> historicalSnapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(historicalSnapshots);

        // Validate the snapshot before processing
        snapshot.EnsureValid();

        if (historicalSnapshots.Count == 0)
            return 0;

        // Ensure snapshots are in chronological order (oldest first) for proper comparison
        // Sort by timestamp to handle unordered input
        var sortedHistorical = historicalSnapshots
            .OrderBy(h => h.Timestamp)
            .ToList();

        // Find snapshots from previous days
        var previousDaySnapshots = sortedHistorical
            .Where(h => h.Timestamp.Date < snapshot.Timestamp.Date)
            .ToList();

        if (previousDaySnapshots.Count == 0)
            return 0;

        // Calculate total story points burned between historical and current
        var oldestHistorical = previousDaySnapshots.First().Timestamp;
        var daysBetween = (snapshot.Timestamp - oldestHistorical).TotalDays;

        if (daysBetween <= 0)
            return 0;

        var maxHistoricalCompleted = previousDaySnapshots.Max(h => h.CompletedStoryPoints);
        var totalStoryPointsBurned = snapshot.CompletedStoryPoints - maxHistoricalCompleted;

        // Guard against division by zero - if no work was completed historically, return 0
        if (maxHistoricalCompleted <= 0 && snapshot.CompletedStoryPoints <= 0)
            return 0;

        return totalStoryPointsBurned / daysBetween;
    }

    /// <summary>
    /// Determines if the current burndown is accelerating or decelerating
    /// based on comparing recent velocity to overall velocity.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="historicalSnapshots">Historical snapshots sorted by timestamp (newest first)</param>
    /// <returns>True if velocity is accelerating (improving), false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot or historicalSnapshots is null</exception>
    /// <exception cref="ArgumentException">Thrown when snapshot contains validation errors</exception>
    public static bool IsVelocityAccelerating(
        this BurndownSnapshot snapshot,
        IReadOnlyList<BurndownSnapshot> historicalSnapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(historicalSnapshots);

        // Validate the snapshot before processing
        snapshot.EnsureValid();

        var velocityTrend = snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Handle case where we don't have enough historical data
        if (historicalSnapshots.Count == 0)
            return false;

        // Sort historical snapshots chronologically to find oldest timestamp
        var sortedHistorical = historicalSnapshots
            .OrderBy(h => h.Timestamp)
            .ToList();

        var oldestTimestamp = sortedHistorical.First().Timestamp;
        var daysBetween = (snapshot.Timestamp - oldestTimestamp).TotalDays;

        // Guard against division by zero
        if (daysBetween <= 0)
            return false;

        var overallVelocity = snapshot.CompletedStoryPoints / daysBetween;

        // If we're burning story points significantly faster now than overall, velocity is accelerating
        // Use a threshold to avoid false positives with small numbers
        if (overallVelocity <= 0)
            return false;

        return velocityTrend > overallVelocity * 1.1; // 10% improvement threshold
    }

    /// <summary>
    /// Gets the burn rate in story points per day based on the current snapshot.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="daysInSprint">Total days in the sprint (default 14 for 2-week sprint)</param>
    /// <returns>Burn rate in story points per day</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when daysInSprint is not positive</exception>
    /// <exception cref="ArgumentException">Thrown when snapshot contains validation errors</exception>
    public static double GetBurnRate(
        this BurndownSnapshot snapshot,
        int daysInSprint = 14)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(daysInSprint, 0);

        // Validate the snapshot before processing
        snapshot.EnsureValid();

        if (daysInSprint <= 0)
            return 0;

        var startDate = snapshot.Timestamp.AddDays(-daysInSprint);
        var daysElapsed = (snapshot.Timestamp - startDate).TotalDays;

        if (daysElapsed <= 0)
            return 0;

        return snapshot.CompletedStoryPoints / daysElapsed;
    }

    /// <summary>
    /// Creates a comparison snapshot showing the difference between this snapshot
    /// and a previous snapshot.
    /// </summary>
    /// <param name="current">The current snapshot</param>
    /// <param name="previous">The previous snapshot to compare against</param>
    /// <returns>A new <see cref="BurndownSnapshot"/> representing the delta, or null if comparison not possible (e.g., timestamps are not in chronological order)</returns>
    /// <exception cref="ArgumentNullException">Thrown when current or previous is null</exception>
    /// <exception cref="ArgumentException">Thrown when current or previous snapshot contains validation errors</exception>
    public static BurndownSnapshot? CreateDeltaSnapshot(
        this BurndownSnapshot current,
        BurndownSnapshot previous)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(previous);

        // Validate both snapshots before processing
        current.EnsureValid();
        previous.EnsureValid();

        // Only create delta if timestamps are comparable
        if (current.Timestamp <= previous.Timestamp)
            return null;

        return new BurndownSnapshot
        {
            Timestamp = current.Timestamp,
            SprintId = current.SprintId,
            RemainingStoryPoints = current.RemainingStoryPoints - previous.RemainingStoryPoints,
            CompletedStoryPoints = current.CompletedStoryPoints - previous.CompletedStoryPoints,
            TotalStoryPoints = current.TotalStoryPoints - previous.TotalStoryPoints,
            RemainingIssueCount = current.RemainingIssueCount - previous.RemainingIssueCount,
            CompletedIssueCount = current.CompletedIssueCount - previous.CompletedIssueCount,
            TotalIssueCount = current.TotalIssueCount - previous.TotalIssueCount,
            ScopeChanges = current.ScopeChanges - previous.ScopeChanges
        };
    }

    /// <summary>
    /// Formats the burndown data as a compact status string suitable for logging
    /// or dashboard display.
    /// </summary>
    /// <param name="snapshot">The snapshot to format</param>
    /// <param name="includeProjected">Whether to include projected completion info</param>
    /// <returns>Formatted status string</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null</exception>
    /// <exception cref="ArgumentException">Thrown when snapshot contains validation errors</exception>
    public static string ToStatusString(
        this BurndownSnapshot snapshot,
        bool includeProjected = true)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        // Validate the snapshot before processing
        snapshot.EnsureValid();

        var parts = new List<string>
        {
            $"Sprint {snapshot.SprintId} @ {snapshot.Timestamp:yyyy-MM-dd HH:mm}",
            $"{snapshot.CompletedStoryPoints}/{snapshot.TotalStoryPoints} pts ({snapshot.GetBurndownPercentage():F1}%)",
            $"{snapshot.CompletedIssueCount}/{snapshot.TotalIssueCount} issues"
        };

        if (includeProjected && snapshot.TotalStoryPoints > 0)
        {
            var projectedDate = snapshot.Timestamp.AddDays(7);
            var projected = snapshot.GetProjectedCompletionPercentage(projectedDate);
            parts.Add($"Projected: {projected:F1}% in 7 days");
        }

        if (snapshot.ScopeChanges != 0)
        {
            parts.Add($"Scope change: {(snapshot.ScopeChanges > 0 ? "+" : "")}{snapshot.ScopeChanges}");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Determines if the current burndown indicates potential scope creep
    /// based on the number of scope changes.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="threshold">Minimum scope changes to consider as creep (default 3)</param>
    /// <returns>True if scope creep is detected</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when threshold is negative</exception>
    /// <exception cref="ArgumentException">Thrown when snapshot contains validation errors</exception>
    public static bool HasScopeCreep(
        this BurndownSnapshot snapshot,
        int threshold = 3)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfNegative(threshold);

        // Validate the snapshot before processing
        snapshot.EnsureValid();

        return snapshot.ScopeChanges >= threshold;
    }

    /// <summary>
    /// Gets a list of completed story points over time as a sequence of values.
    /// Useful for generating charts and visualizations.
    /// </summary>
    /// <param name="snapshots">List of snapshots sorted by timestamp (oldest first)</param>
    /// <returns>Sequence of completed story points values</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshots is null</exception>
    /// <exception cref="ArgumentException">Thrown when any snapshot contains validation errors</exception>
    public static IEnumerable<int> GetCompletedStoryPointsOverTime(
        this IReadOnlyList<BurndownSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        // Validate all snapshots before processing
        foreach (var snapshot in snapshots)
        {
            snapshot.EnsureValid();
        }

        return snapshots.Select(s => s.CompletedStoryPoints).ToList();
    }

    /// <summary>
    /// Gets a list of remaining story points over time as a sequence of values.
    /// Useful for generating charts and visualizations.
    /// </summary>
    /// <param name="snapshots">List of snapshots sorted by timestamp (oldest first)</param>
    /// <returns>Sequence of remaining story points values</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshots is null</exception>
    /// <exception cref="ArgumentException">Thrown when any snapshot contains validation errors</exception>
    public static IEnumerable<int> GetRemainingStoryPointsOverTime(
        this IReadOnlyList<BurndownSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        // Validate all snapshots before processing
        foreach (var snapshot in snapshots)
        {
            snapshot.EnsureValid();
        }

        return snapshots.Select(s => s.RemainingStoryPoints).ToList();
    }

    /// <summary>
    /// Projects completed story points over time into the shared
    /// <see cref="ITimeSeriesPoint"/> shape consumed by <see cref="TrendAnalysis"/>.
    /// </summary>
    /// <param name="snapshots">List of snapshots in any order.</param>
    /// <returns>The completed story points series as time-series points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshots"/> is null.</exception>
    public static IReadOnlyList<ITimeSeriesPoint> ToCompletedStoryPointsSeries(
        this IReadOnlyList<BurndownSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        foreach (var snapshot in snapshots)
        {
            snapshot.EnsureValid();
        }

        return snapshots
            .Select(s => (ITimeSeriesPoint)new TimeSeriesPoint(s.Timestamp, s.CompletedStoryPoints))
            .ToList();
    }

    /// <summary>
    /// Calculates the linear trend slope of completed story points, in points per day,
    /// by delegating to <see cref="TrendAnalysis.CalculateSlope"/>.
    /// </summary>
    /// <param name="snapshots">List of snapshots in any order.</param>
    /// <returns>The slope in story points per day, or 0 when insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshots"/> is null.</exception>
    public static double GetCompletedStoryPointsTrendSlope(this IReadOnlyList<BurndownSnapshot> snapshots) =>
        TrendAnalysis.CalculateSlope(snapshots.ToCompletedStoryPointsSeries());

    /// <summary>
    /// Calculates the acceleration of completed story points by delegating to
    /// <see cref="TrendAnalysis.CalculateAcceleration"/>.
    /// </summary>
    /// <param name="snapshots">List of snapshots in any order.</param>
    /// <returns>The acceleration in story points per day squared, or 0 when insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshots"/> is null.</exception>
    public static double GetCompletedStoryPointsAcceleration(this IReadOnlyList<BurndownSnapshot> snapshots) =>
        TrendAnalysis.CalculateAcceleration(snapshots.ToCompletedStoryPointsSeries());

    /// <summary>
    /// Calculates a trailing moving average of completed story points by delegating
    /// to <see cref="TrendAnalysis.CalculateMovingAverage"/>.
    /// </summary>
    /// <param name="snapshots">List of snapshots in any order.</param>
    /// <param name="windowSize">The number of trailing points to average over.</param>
    /// <returns>A list of moving-average values, one per snapshot, in chronological order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshots"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
    public static IReadOnlyList<double> GetCompletedStoryPointsMovingAverage(
        this IReadOnlyList<BurndownSnapshot> snapshots,
        int windowSize) =>
        TrendAnalysis.CalculateMovingAverage(snapshots.ToCompletedStoryPointsSeries(), windowSize);
}
