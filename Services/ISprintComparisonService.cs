// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Provides sprint comparison and team velocity trend analysis across multiple sprints.
/// </summary>
public interface ISprintComparisonService
{
    /// <summary>
    /// Compares a specific set of sprints by their IDs and returns a side-by-side analysis.
    /// </summary>
    /// <param name="projectKey">Jira project key.</param>
    /// <param name="sprintIds">Collection of sprint IDs to compare.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<SprintComparisonReport> CompareSprintsAsync(
        string projectKey,
        IEnumerable<int> sprintIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes per-developer velocity trends across the most recent closed sprints.
    /// </summary>
    /// <param name="projectKey">Jira project key.</param>
    /// <param name="sprintCount">Number of recent sprints to analyse.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<List<DeveloperVelocityTrend>> GetTeamVelocityTrendsAsync(
        string projectKey,
        int sprintCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a full sprint comparison report for the most recent closed sprints.
    /// </summary>
    /// <param name="projectKey">Jira project key.</param>
    /// <param name="sprintCount">Number of recent sprints to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<SprintComparisonReport> GenerateComparisonReportAsync(
        string projectKey,
        int sprintCount,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Analytics snapshot for a single sprint within a comparison set.
/// </summary>
public class SprintComparisonEntry
{
    /// <summary>Gets or sets the unique sprint identifier.</summary>
    [JsonPropertyName("sprintId")]
    public int SprintId { get; set; }

    /// <summary>Gets or sets the sprint name.</summary>
    [JsonPropertyName("sprintName")]
    public string SprintName { get; set; } = string.Empty;

    /// <summary>Gets or sets the sprint start date.</summary>
    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    /// <summary>Gets or sets the sprint end date.</summary>
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets the sprint duration in calendar days.</summary>
    [JsonPropertyName("durationDays")]
    public int DurationDays { get; set; }

    /// <summary>Gets or sets total planned story points at sprint start.</summary>
    [JsonPropertyName("plannedPoints")]
    public int PlannedPoints { get; set; }

    /// <summary>Gets or sets story points completed by sprint end.</summary>
    [JsonPropertyName("completedPoints")]
    public int CompletedPoints { get; set; }

    /// <summary>Gets or sets velocity in story points per day.</summary>
    [JsonPropertyName("velocity")]
    public double Velocity { get; set; }

    /// <summary>Gets or sets the completion rate as a percentage of planned points.</summary>
    [JsonPropertyName("completionRate")]
    public double CompletionRate { get; set; }

    /// <summary>Gets or sets the average cycle time in days across all completed issues.</summary>
    [JsonPropertyName("averageCycleTime")]
    public double AverageCycleTime { get; set; }

    /// <summary>Gets or sets the total number of issues included in the sprint.</summary>
    [JsonPropertyName("totalIssues")]
    public int TotalIssues { get; set; }

    /// <summary>Gets or sets the number of issues completed during the sprint.</summary>
    [JsonPropertyName("completedIssues")]
    public int CompletedIssues { get; set; }

    /// <summary>Gets or sets the number of overdue issues at sprint close.</summary>
    [JsonPropertyName("overdueIssues")]
    public int OverdueIssues { get; set; }

    /// <summary>Gets or sets the count of defect/bug issues resolved in the sprint.</summary>
    [JsonPropertyName("defectCount")]
    public int DefectCount { get; set; }

    /// <summary>Gets or sets the overall health status: Excellent, Healthy, At Risk, or Critical.</summary>
    [JsonPropertyName("healthStatus")]
    public string HealthStatus { get; set; } = "Unknown";
}

/// <summary>
/// Tracks a single developer's story-point output across a sequence of sprints.
/// </summary>
public class DeveloperVelocityTrend
{
    /// <summary>Gets or sets the developer's Jira assignee key.</summary>
    [JsonPropertyName("developerKey")]
    public string DeveloperKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the developer's display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets per-sprint (name, completed-points) pairs in chronological order.</summary>
    [JsonPropertyName("sprintVelocities")]
    public List<(string SprintName, int CompletedPoints)> SprintVelocities { get; set; } = new();

    /// <summary>Gets or sets the average story points delivered per sprint.</summary>
    [JsonPropertyName("averageVelocity")]
    public double AverageVelocity { get; set; }

    /// <summary>Gets or sets the trend direction: Improving, Declining, or Stable.</summary>
    [JsonPropertyName("trendDirection")]
    public string TrendDirection { get; set; } = "Stable";

    /// <summary>Gets or sets the percentage change in velocity from the first to the last sprint.</summary>
    [JsonPropertyName("trendPercentage")]
    public double TrendPercentage { get; set; }
}

/// <summary>
/// Aggregated sprint comparison report for a project covering multiple consecutive sprints.
/// </summary>
public class SprintComparisonReport
{
    /// <summary>Gets or sets the Jira project key.</summary>
    [JsonPropertyName("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when this report was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the number of sprints included in the comparison.</summary>
    [JsonPropertyName("sprintCount")]
    public int SprintCount { get; set; }

    /// <summary>Gets or sets the ordered list of sprint comparison entries.</summary>
    [JsonPropertyName("sprints")]
    public List<SprintComparisonEntry> Sprints { get; set; } = new();

    /// <summary>Gets or sets per-developer velocity trend data, ordered by average velocity descending.</summary>
    [JsonPropertyName("teamVelocityTrends")]
    public List<DeveloperVelocityTrend> TeamVelocityTrends { get; set; } = new();

    /// <summary>Gets or sets the average velocity in story points per day across all sprints.</summary>
    [JsonPropertyName("averageVelocity")]
    public double AverageVelocity { get; set; }

    /// <summary>Gets or sets the percentage change in team velocity from first to last sprint.</summary>
    [JsonPropertyName("velocityTrendPercentage")]
    public double VelocityTrendPercentage { get; set; }

    /// <summary>Gets or sets the overall team trend: Improving, Declining, or Stable.</summary>
    [JsonPropertyName("overallTrend")]
    public string OverallTrend { get; set; } = "Stable";

    /// <summary>Gets or sets the sprint with the highest velocity in the comparison set.</summary>
    [JsonPropertyName("bestSprint")]
    public SprintComparisonEntry? BestSprint { get; set; }

    /// <summary>Gets or sets the sprint with the lowest velocity in the comparison set.</summary>
    [JsonPropertyName("worstSprint")]
    public SprintComparisonEntry? WorstSprint { get; set; }
}
