using System.Linq;

namespace JiraAnalyticsCli.Models;

public static class SprintMetricExtensions
{
    /// <summary>
    /// Calculates the sprint progress as a percentage.
    /// </summary>
    /// <param name="sprintMetric">The sprint metric.</param>
    /// <returns>The sprint progress as a percentage.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprintMetric"/> is null.</exception>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="sprintMetric"/>.PlannedStoryPoints is zero.</exception>
    public static double GetProgressPercentage(this SprintMetric sprintMetric)
    {
        ArgumentNullException.ThrowIfNull(sprintMetric);

        if (sprintMetric.PlannedStoryPoints == 0)
        {
            throw new DivideByZeroException("PlannedStoryPoints cannot be zero for progress calculation.");
        }

        return (double)sprintMetric.CompletedStoryPoints / sprintMetric.PlannedStoryPoints * 100;
    }

    /// <summary>
    /// Determines if the sprint is complete.
    /// </summary>
    /// <param name="sprintMetric">The sprint metric.</param>
    /// <returns>True if the sprint is complete; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprintMetric"/> is null.</exception>
    public static bool IsSprintComplete(this SprintMetric sprintMetric)
    {
        ArgumentNullException.ThrowIfNull(sprintMetric);

        return sprintMetric.EndDate < DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the average story points completed per day.
    /// </summary>
    /// <param name="sprintMetric">The sprint metric.</param>
    /// <returns>The average story points completed per day.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprintMetric"/> is null.</exception>
    /// <exception cref="DivideByZeroException">Thrown when the sprint duration is zero days.</exception>
    public static double GetAverageDailyProgress(this SprintMetric sprintMetric)
    {
        ArgumentNullException.ThrowIfNull(sprintMetric);

        var daysElapsed = (sprintMetric.EndDate - sprintMetric.StartDate).Days;
        if (daysElapsed == 0)
        {
            throw new DivideByZeroException("Sprint duration cannot be zero days.");
        }

        return (double)sprintMetric.CompletedStoryPoints / daysElapsed;
    }

    /// <summary>
    /// Projects completed story points across sprints into the shared
    /// <see cref="ITimeSeriesPoint"/> shape consumed by <see cref="TrendAnalysis"/>,
    /// using each sprint's end date as the observation timestamp.
    /// </summary>
    /// <param name="sprintMetrics">List of sprint metrics in any order.</param>
    /// <returns>The completed story points series as time-series points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprintMetrics"/> is null.</exception>
    public static IReadOnlyList<ITimeSeriesPoint> ToCompletedStoryPointsSeries(
        this IReadOnlyList<SprintMetric> sprintMetrics)
    {
        ArgumentNullException.ThrowIfNull(sprintMetrics);

        return sprintMetrics
            .Select(m => (ITimeSeriesPoint)new TimeSeriesPoint(m.EndDate.UtcDateTime, m.CompletedStoryPoints))
            .ToList();
    }

    /// <summary>
    /// Calculates the linear trend slope of completed story points across sprints,
    /// in points per day, by delegating to <see cref="TrendAnalysis.CalculateSlope"/>.
    /// </summary>
    /// <param name="sprintMetrics">List of sprint metrics in any order.</param>
    /// <returns>The slope in story points per day, or 0 when insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprintMetrics"/> is null.</exception>
    public static double GetCompletedStoryPointsTrendSlope(this IReadOnlyList<SprintMetric> sprintMetrics) =>
        TrendAnalysis.CalculateSlope(sprintMetrics.ToCompletedStoryPointsSeries());

    /// <summary>
    /// Calculates the acceleration of completed story points across sprints by
    /// delegating to <see cref="TrendAnalysis.CalculateAcceleration"/>.
    /// </summary>
    /// <param name="sprintMetrics">List of sprint metrics in any order.</param>
    /// <returns>The acceleration in story points per day squared, or 0 when insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprintMetrics"/> is null.</exception>
    public static double GetCompletedStoryPointsAcceleration(this IReadOnlyList<SprintMetric> sprintMetrics) =>
        TrendAnalysis.CalculateAcceleration(sprintMetrics.ToCompletedStoryPointsSeries());

    /// <summary>
    /// Calculates a trailing moving average of completed story points across sprints
    /// by delegating to <see cref="TrendAnalysis.CalculateMovingAverage"/>.
    /// </summary>
    /// <param name="sprintMetrics">List of sprint metrics in any order.</param>
    /// <param name="windowSize">The number of trailing points to average over.</param>
    /// <returns>A list of moving-average values, one per sprint, in chronological order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprintMetrics"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
    public static IReadOnlyList<double> GetCompletedStoryPointsMovingAverage(
        this IReadOnlyList<SprintMetric> sprintMetrics,
        int windowSize) =>
        TrendAnalysis.CalculateMovingAverage(sprintMetrics.ToCompletedStoryPointsSeries(), windowSize);
}