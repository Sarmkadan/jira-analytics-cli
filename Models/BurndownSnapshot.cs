// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a point-in-time snapshot of sprint burndown data
/// </summary>
public class BurndownSnapshot
{
    [Required]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [Required]
    [JsonPropertyName("sprintId")]
    public int SprintId { get; set; }

    [JsonPropertyName("remainingStoryPoints")]
    public int RemainingStoryPoints { get; set; }

    [JsonPropertyName("completedStoryPoints")]
    public int CompletedStoryPoints { get; set; }

    [JsonPropertyName("totalStoryPoints")]
    public int TotalStoryPoints { get; set; }

    [JsonPropertyName("remainingIssueCount")]
    public int RemainingIssueCount { get; set; }

    [JsonPropertyName("completedIssueCount")]
    public int CompletedIssueCount { get; set; }

    [JsonPropertyName("totalIssueCount")]
    public int TotalIssueCount { get; set; }

    [JsonPropertyName("scopeChanges")]
    public int ScopeChanges { get; set; }

    public double GetBurndownPercentage()
    {
        // Percentage of work completed
        if (TotalStoryPoints == 0) return 0;
        return (CompletedStoryPoints / (double)TotalStoryPoints) * 100;
    }

    public double GetProjectedCompletionPercentage(DateTime sprintEnd)
    {
        // Simple linear projection based on current burn rate
        var timeSinceStart = Timestamp;
        var daysRemaining = (sprintEnd - Timestamp).TotalDays;

        if (daysRemaining <= 0) return GetBurndownPercentage();

        var remainingPercentage = 100 - GetBurndownPercentage();
        return Math.Min(100, GetBurndownPercentage() + (remainingPercentage * (daysRemaining / 14))); // Assuming 2-week sprint
    }

    public bool IsOnTrack(DateTime sprintEnd)
    {
        // Check if we're on track to complete the sprint
        var daysTotal = (sprintEnd - (Timestamp.AddDays(-7))).TotalDays; // Rough estimate
        var daysElapsed = 7; // Rough estimate
        var daysRemaining = daysTotal - daysElapsed;

        if (daysRemaining <= 0) return GetBurndownPercentage() >= 90;

        var expectedBurndown = (daysElapsed / daysTotal) * 100;
        var actualBurndown = GetBurndownPercentage();

        return actualBurndown >= (expectedBurndown * 0.9); // Within 10% tolerance
    }

    public int GetHoursUntilCompletion(int issuesPerHour)
    {
        // Estimate hours until completion based on completion rate
        if (issuesPerHour <= 0) return 0;
        return RemainingIssueCount / issuesPerHour;
    }

    public void Validate()
    {
        if (SprintId <= 0)
            throw new ArgumentException("Sprint ID must be positive");

        if (Timestamp == default)
            throw new ArgumentException("Timestamp must be set");

        if (RemainingStoryPoints < 0)
            throw new ArgumentException("Remaining story points cannot be negative");

        if (CompletedStoryPoints < 0)
            throw new ArgumentException("Completed story points cannot be negative");

        if (TotalStoryPoints <= 0)
            throw new ArgumentException("Total story points must be positive");

        if (CompletedStoryPoints + RemainingStoryPoints != TotalStoryPoints)
            throw new ArgumentException("Completed + Remaining story points must equal total");
    }

    public override string ToString()
    {
        return $"Burndown [{Timestamp:yyyy-MM-dd HH:mm}] - {CompletedStoryPoints}/{TotalStoryPoints} pts ({GetBurndownPercentage():F1}%)";
    }
}
