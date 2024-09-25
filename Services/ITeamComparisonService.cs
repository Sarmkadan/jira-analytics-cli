// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Compares analytics metrics across multiple Jira projects (teams) side by side.
/// </summary>
public interface ITeamComparisonService
{
    /// <summary>
    /// Compares sprint and quality metrics for two or more projects and returns a side-by-side report.
    /// </summary>
    /// <param name="projectKeys">The Jira project keys to include in the comparison.</param>
    /// <param name="sprintCount">Number of recent closed sprints to consider per project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TeamComparisonReport> CompareTeamsAsync(
        IEnumerable<string> projectKeys,
        int sprintCount = 5,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated analytics snapshot for a single project within a team comparison.
/// </summary>
public class TeamProjectSnapshot
{
    /// <summary>Gets or sets the Jira project key.</summary>
    [JsonPropertyName("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the average velocity in story points per sprint.</summary>
    [JsonPropertyName("averageVelocity")]
    public double AverageVelocity { get; set; }

    /// <summary>Gets or sets the average sprint completion rate as a percentage.</summary>
    [JsonPropertyName("avgCompletionRate")]
    public double AvgCompletionRate { get; set; }

    /// <summary>Gets or sets the total story points delivered across all analysed sprints.</summary>
    [JsonPropertyName("totalPointsDelivered")]
    public int TotalPointsDelivered { get; set; }

    /// <summary>Gets or sets the total number of issues completed.</summary>
    [JsonPropertyName("totalIssuesCompleted")]
    public int TotalIssuesCompleted { get; set; }

    /// <summary>Gets or sets the total defect count across all analysed sprints.</summary>
    [JsonPropertyName("totalDefects")]
    public int TotalDefects { get; set; }

    /// <summary>Gets or sets the defect rate as a percentage of total issues.</summary>
    [JsonPropertyName("defectRate")]
    public double DefectRate { get; set; }

    /// <summary>Gets or sets the average cycle time in days across all sprints.</summary>
    [JsonPropertyName("avgCycleTime")]
    public double AvgCycleTime { get; set; }

    /// <summary>Gets or sets the overall health status: Excellent, Healthy, At Risk, or Critical.</summary>
    [JsonPropertyName("overallHealth")]
    public string OverallHealth { get; set; } = "Unknown";

    /// <summary>Gets or sets the velocity trend percentage (positive = improving).</summary>
    [JsonPropertyName("velocityTrend")]
    public double VelocityTrend { get; set; }

    /// <summary>Gets or sets the number of sprints included in this snapshot.</summary>
    [JsonPropertyName("sprintCount")]
    public int SprintCount { get; set; }
}

/// <summary>
/// Full side-by-side comparison report across two or more Jira projects.
/// </summary>
public class TeamComparisonReport
{
    /// <summary>Gets or sets the UTC timestamp when this report was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the per-project analytics snapshots.</summary>
    [JsonPropertyName("teams")]
    public List<TeamProjectSnapshot> Teams { get; set; } = new();

    /// <summary>Gets or sets the project key of the team with the highest average velocity.</summary>
    [JsonPropertyName("fastestTeam")]
    public string? FastestTeam { get; set; }

    /// <summary>Gets or sets the project key of the team with the lowest defect rate.</summary>
    [JsonPropertyName("highestQualityTeam")]
    public string? HighestQualityTeam { get; set; }

    /// <summary>Gets or sets the project key of the team with the highest completion rate.</summary>
    [JsonPropertyName("mostConsistentTeam")]
    public string? MostConsistentTeam { get; set; }
}
