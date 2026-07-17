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
}