// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Qualitative direction of a time series, derived from its linear trend slope.
/// </summary>
public enum TrendDirection
{
    /// <summary>The slope lies within the flat tolerance, or there is not enough data to infer a trend.</summary>
    Flat,

    /// <summary>The slope is greater than the flat tolerance: values grow over time.</summary>
    Rising,

    /// <summary>The slope is less than the negative flat tolerance: values shrink over time.</summary>
    Falling
}

/// <summary>
/// Extension methods over raw time series that classify the result of
/// <see cref="TrendAnalysis.CalculateSlope"/> into a <see cref="TrendDirection"/>.
/// </summary>
public static class TrendAnalysisExtensions
{
    /// <summary>
    /// Classifies the direction of the series using its least-squares slope. A slope whose
    /// absolute value does not exceed <paramref name="flatTolerance"/> is reported as flat,
    /// which keeps noise around zero from being read as a trend.
    /// </summary>
    /// <param name="points">The series to classify. Order is not assumed.</param>
    /// <param name="flatTolerance">The largest absolute slope (value per day) still treated as flat. Must not be negative.</param>
    /// <returns>
    /// <see cref="TrendDirection.Rising"/> or <see cref="TrendDirection.Falling"/> when the slope exceeds the
    /// tolerance in either direction; otherwise <see cref="TrendDirection.Flat"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="flatTolerance"/> is negative.</exception>
    public static TrendDirection GetTrendDirection(
        this IReadOnlyList<ITimeSeriesPoint> points,
        double flatTolerance = 0)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentOutOfRangeException.ThrowIfNegative(flatTolerance);

        var slope = TrendAnalysis.CalculateSlope(points);

        if (slope > flatTolerance)
        {
            return TrendDirection.Rising;
        }

        if (slope < -flatTolerance)
        {
            return TrendDirection.Falling;
        }

        return TrendDirection.Flat;
    }
}
