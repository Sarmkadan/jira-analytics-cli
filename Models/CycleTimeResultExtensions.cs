// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Extension methods for <see cref="CycleTimeResult"/> and <see cref="IssueCycleTime"/>
/// that provide trend analysis over cycle time data, sharing the same
/// <see cref="TrendAnalysis"/> algorithm used by the burndown and sprint metric families.
/// </summary>
public static class CycleTimeResultExtensions
{
    /// <summary>
    /// Projects individual issue cycle times into the shared <see cref="ITimeSeriesPoint"/>
    /// shape consumed by <see cref="TrendAnalysis"/>, using each issue's created date as
    /// the observation timestamp and its cycle time in days as the value.
    /// </summary>
    /// <param name="issueCycleTimes">List of issue cycle times in any order.</param>
    /// <returns>The cycle time series as time-series points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issueCycleTimes"/> is null.</exception>
    public static IReadOnlyList<ITimeSeriesPoint> ToCycleTimeSeries(
        this IReadOnlyList<IssueCycleTime> issueCycleTimes)
    {
        ArgumentNullException.ThrowIfNull(issueCycleTimes);

        return issueCycleTimes
            .Select(i => (ITimeSeriesPoint)new TimeSeriesPoint(i.CreatedDate, i.CycleTimeDays))
            .ToList();
    }

    /// <summary>
    /// Calculates the linear trend slope of individual cycle times, in days per day,
    /// by delegating to <see cref="TrendAnalysis.CalculateSlope"/>. A positive value
    /// means cycle time is worsening (growing) over time.
    /// </summary>
    /// <param name="issueCycleTimes">List of issue cycle times in any order.</param>
    /// <returns>The slope in cycle-time-days per day, or 0 when insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issueCycleTimes"/> is null.</exception>
    public static double GetCycleTimeTrendSlope(this IReadOnlyList<IssueCycleTime> issueCycleTimes) =>
        TrendAnalysis.CalculateSlope(issueCycleTimes.ToCycleTimeSeries());

    /// <summary>
    /// Calculates the acceleration of individual cycle times by delegating to
    /// <see cref="TrendAnalysis.CalculateAcceleration"/>.
    /// </summary>
    /// <param name="issueCycleTimes">List of issue cycle times in any order.</param>
    /// <returns>The acceleration in cycle-time-days per day squared, or 0 when insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issueCycleTimes"/> is null.</exception>
    public static double GetCycleTimeAcceleration(this IReadOnlyList<IssueCycleTime> issueCycleTimes) =>
        TrendAnalysis.CalculateAcceleration(issueCycleTimes.ToCycleTimeSeries());

    /// <summary>
    /// Calculates a trailing moving average of individual cycle times by delegating to
    /// <see cref="TrendAnalysis.CalculateMovingAverage"/>.
    /// </summary>
    /// <param name="issueCycleTimes">List of issue cycle times in any order.</param>
    /// <param name="windowSize">The number of trailing points to average over.</param>
    /// <returns>A list of moving-average values, one per issue, in chronological order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="issueCycleTimes"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
    public static IReadOnlyList<double> GetCycleTimeMovingAverage(
        this IReadOnlyList<IssueCycleTime> issueCycleTimes,
        int windowSize) =>
        TrendAnalysis.CalculateMovingAverage(issueCycleTimes.ToCycleTimeSeries(), windowSize);

    /// <summary>
    /// Calculates the linear trend slope of the project's underlying per-issue cycle
    /// times by delegating to <see cref="GetCycleTimeTrendSlope(IReadOnlyList{IssueCycleTime})"/>.
    /// </summary>
    /// <param name="cycleTimeResult">The cycle time result whose issues are analyzed.</param>
    /// <returns>The slope in cycle-time-days per day, or 0 when insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cycleTimeResult"/> is null.</exception>
    public static double GetIssueCycleTimeTrendSlope(this CycleTimeResult cycleTimeResult)
    {
        ArgumentNullException.ThrowIfNull(cycleTimeResult);

        return cycleTimeResult.IssueCycleTimes.GetCycleTimeTrendSlope();
    }
}
