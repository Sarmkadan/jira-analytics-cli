namespace JiraAnalyticsCli.Models;

public static class SprintMetricExtensions
{
    /// <summary>
    /// Calculates the sprint progress as a percentage.
    /// </summary>
    /// <param name="sprintMetric">The sprint metric.</param>
    /// <returns>The sprint progress as a percentage.</returns>
    public static double GetProgressPercentage(this SprintMetric sprintMetric)
    {
        ArgumentNullException.ThrowIfNull(sprintMetric);

        return (double)sprintMetric.CompletedStoryPoints / sprintMetric.PlannedStoryPoints * 100;
    }

    /// <summary>
    /// Determines if the sprint is complete.
    /// </summary>
    /// <param name="sprintMetric">The sprint metric.</param>
    /// <returns>True if the sprint is complete; otherwise, false.</returns>
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
    public static double GetAverageDailyProgress(this SprintMetric sprintMetric)
    {
        ArgumentNullException.ThrowIfNull(sprintMetric);

        var daysElapsed = (sprintMetric.EndDate - sprintMetric.StartDate).Days;
        return (double)sprintMetric.CompletedStoryPoints / daysElapsed;
    }
}
