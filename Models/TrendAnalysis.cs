// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraAnalyticsCli.Models;

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