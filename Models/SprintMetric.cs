// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Aggregated metrics for a sprint including velocity, quality indicators
/// </summary>
public class SprintMetric
{
    [Required]
    [JsonPropertyName("sprintId")]
    public int SprintId { get; set; }

    [Required]
    [JsonPropertyName("sprintName")]
    public string SprintName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("plannedStoryPoints")]
    public int PlannedStoryPoints { get; set; }

    [JsonPropertyName("completedStoryPoints")]
    public int CompletedStoryPoints { get; set; }

    [JsonPropertyName("committedStoryPoints")]
    public int CommittedStoryPoints { get; set; }

    [JsonPropertyName("completedIssueCount")]
    public int CompletedIssueCount { get; set; }

    [JsonPropertyName("totalIssueCount")]
    public int TotalIssueCount { get; set; }

    [JsonPropertyName("defectsCount")]
    public int DefectsCount { get; set; }

    [JsonPropertyName("avgCycleTime")]
    public double AverageCycleTime { get; set; }

    [JsonPropertyName("overdueIssueCount")]
    public int OverdueIssueCount { get; set; }

    [JsonPropertyName("teamSize")]
    public int TeamSize { get; set; }

    [JsonPropertyName("scopeChangeCount")]
    public int ScopeChangeCount { get; set; }

    public double GetVelocity()
    {
        // Velocity as story points per day
        var duration = (EndDate - StartDate).TotalDays;
        if (duration <= 0) return 0;

        return CompletedStoryPoints / duration;
    }

    public double GetCompletionRate()
    {
        // Percentage of planned work completed
        if (PlannedStoryPoints <= 0) return 0;
        return (CompletedStoryPoints / (double)PlannedStoryPoints) * 100;
    }

    public double GetCommitmentAccuracy()
    {
        // How well team hit their commitment
        if (CommittedStoryPoints <= 0) return 0;
        return (CompletedStoryPoints / (double)CommittedStoryPoints) * 100;
    }

    public double GetQualityScore()
    {
        // Quality based on defects and completed items
        if (CompletedIssueCount == 0) return 100;

        var defectRate = DefectsCount / (double)CompletedIssueCount;
        var qualityScore = Math.Max(0, 100 - (defectRate * 100));

        return qualityScore;
    }

    public double GetProductivityPerTeamMember()
    {
        // Average story points per team member
        if (TeamSize <= 0) return 0;
        return CompletedStoryPoints / (double)TeamSize;
    }

    public double GetDailyBurndownRate()
    {
        // Average story points burned per day
        var duration = (EndDate - StartDate).TotalDays;
        if (duration <= 0) return 0;

        return CompletedStoryPoints / duration;
    }

    public double GetRiskScore()
    {
        // Risk score: overdue issues, scope changes, quality
        var overdueRisk = OverdueIssueCount * 5;
        var scopeRisk = ScopeChangeCount * 3;
        var qualityRisk = (100 - GetQualityScore()) * 2;

        return Math.Min(100, overdueRisk + scopeRisk + qualityRisk);
    }

    public string GetHealthStatus()
    {
        // Overall sprint health status
        var completionRate = GetCompletionRate();
        var qualityScore = GetQualityScore();
        var riskScore = GetRiskScore();

        if (completionRate >= 95 && qualityScore >= 90 && riskScore < 20)
            return "Excellent";

        if (completionRate >= 85 && qualityScore >= 80 && riskScore < 40)
            return "Healthy";

        if (completionRate >= 70 && qualityScore >= 70 && riskScore < 60)
            return "At Risk";

        return "Critical";
    }

    public void Validate()
    {
        if (SprintId <= 0)
            throw new ArgumentException("Sprint ID must be positive");

        if (string.IsNullOrWhiteSpace(SprintName))
            throw new ArgumentException("Sprint name cannot be empty");

        if (EndDate <= StartDate)
            throw new ArgumentException("End date must be after start date");

        if (PlannedStoryPoints < 0)
            throw new ArgumentException("Planned story points cannot be negative");

        if (CompletedStoryPoints > PlannedStoryPoints)
            throw new ArgumentException("Completed story points cannot exceed planned");
    }

    public override string ToString()
    {
        return $"Sprint {SprintName}: {CompletedStoryPoints}/{PlannedStoryPoints} pts ({GetCompletionRate():F1}%) - {GetHealthStatus()}";
    }
}
