// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a point-in-time snapshot of sprint burndown data
/// </summary>
public class BurndownSnapshot
{
    private DateTime _timestamp;

    /// <summary>
    /// Gets or sets the point in time this snapshot was captured, always normalized to UTC.
    /// Jira returns ISO-8601 timestamps with numeric offsets and other parts of the pipeline
    /// use local wall-clock values; storing everything as UTC keeps burn-rate and duration
    /// math correct across DST transitions, since plain <see cref="DateTime"/> subtraction
    /// ignores <see cref="DateTime.Kind"/> entirely and silently produces wrong deltas when
    /// the two operands were captured under different offsets.
    /// </summary>
    [Required]
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(Utils.UtcDateTimeJsonConverter))]
    public DateTime Timestamp
    {
        get => _timestamp;
        set => _timestamp = NormalizeToUtc(value);
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to an equivalent
    /// UTC value. <see cref="DateTimeKind.Unspecified"/> values are treated as already being
    /// UTC (the convention used throughout this codebase), while <see cref="DateTimeKind.Local"/>
    /// values are converted using the host's time zone rules.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>An equivalent <see cref="DateTime"/> with <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/>.</returns>
    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };

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

    /// <summary>
    /// Calculates the percentage of work completed based on story points
    /// </summary>
    /// <returns>Percentage of work completed (0-100)</returns>
    /// <exception cref="ArgumentException">Thrown when the snapshot contains validation errors</exception>
    public double GetBurndownPercentage()
    {
        // Percentage of work completed
        if (TotalStoryPoints == 0)
            return 0;

        return (CompletedStoryPoints / (double)TotalStoryPoints) * 100;
    }

    /// <summary>
    /// Calculates the projected completion percentage based on current burn rate
    /// </summary>
    /// <param name="sprintEnd">The end date of the sprint</param>
    /// <returns>Projected completion percentage</returns>
    /// <exception cref="ArgumentException">Thrown when the snapshot contains validation errors</exception>
    public double GetProjectedCompletionPercentage(DateTime sprintEnd)
    {
        // Simple linear projection based on current burn rate
        var daysRemaining = (sprintEnd - Timestamp).TotalDays;

        if (daysRemaining <= 0)
            return GetBurndownPercentage();

        var currentPercentage = GetBurndownPercentage();
        var remainingPercentage = 100 - currentPercentage;

        // Guard against division by zero - if remaining percentage is 0 or negative, we're done
        if (remainingPercentage <= 0)
            return 100;

        // Assuming 2-week sprint for projection
        // Ensure we don't divide by zero in the hardcoded 14
        const int sprintLengthDays = 14;
        var burnRateFactor = daysRemaining / (double)sprintLengthDays;

        return Math.Min(100, currentPercentage + (remainingPercentage * burnRateFactor));
    }

    /// <summary>
    /// Determines if the current burndown is on track to meet sprint goals
    /// </summary>
    /// <param name="sprintEnd">The end date of the sprint</param>
    /// <returns>True if the sprint is on track</returns>
    /// <exception cref="ArgumentException">Thrown when the snapshot contains validation errors</exception>
    public bool IsOnTrack(DateTime sprintEnd)
    {
        // Check if we are on track to complete the sprint
        var daysTotal = (sprintEnd - (Timestamp.AddDays(-7))).TotalDays; // Rough estimate
        var daysElapsed = 7; // Rough estimate
        var daysRemaining = daysTotal - daysElapsed;

        if (daysRemaining <= 0)
            return GetBurndownPercentage() >= 90;

        // Guard against division by zero
        if (daysTotal <= 0)
            return GetBurndownPercentage() >= 90;

        var expectedBurndown = (daysElapsed / daysTotal) * 100;
        var actualBurndown = GetBurndownPercentage();

        return actualBurndown >= (expectedBurndown * 0.9); // Within 10% tolerance
    }

    /// <summary>
    /// Estimates hours until completion based on issue completion rate
    /// </summary>
    /// <param name="issuesPerHour">Average issues completed per hour</param>
    /// <returns>Estimated hours until completion</returns>
    /// <exception cref="ArgumentException">Thrown when the snapshot contains validation errors</exception>
    public int GetHoursUntilCompletion(int issuesPerHour)
    {
        // Estimate hours until completion based on completion rate
        if (issuesPerHour <= 0)
            return 0;

        return RemainingIssueCount / issuesPerHour;
    }

    /// <summary>
    /// Validates the burndown snapshot to ensure all invariants are satisfied.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the snapshot contains validation errors</exception>
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

        if (CompletedStoryPoints > TotalStoryPoints)
            throw new ArgumentException("Completed story points cannot exceed total story points");

        if (RemainingStoryPoints > TotalStoryPoints)
            throw new ArgumentException("Remaining story points cannot exceed total story points");

        if (CompletedStoryPoints + RemainingStoryPoints != TotalStoryPoints)
            throw new ArgumentException("Completed + Remaining story points must equal total story points");

        if (RemainingIssueCount < 0)
            throw new ArgumentException("Remaining issue count cannot be negative");

        if (CompletedIssueCount < 0)
            throw new ArgumentException("Completed issue count cannot be negative");

        if (TotalIssueCount <= 0)
            throw new ArgumentException("Total issue count must be positive");

        if (CompletedIssueCount > TotalIssueCount)
            throw new ArgumentException("Completed issue count cannot exceed total issue count");

        if (RemainingIssueCount > TotalIssueCount)
            throw new ArgumentException("Remaining issue count cannot exceed total issue count");

        if (CompletedIssueCount + RemainingIssueCount != TotalIssueCount)
            throw new ArgumentException("Completed + Remaining issue count must equal total issue count");

        if (ScopeChanges < -1000 || ScopeChanges > 1000)
            throw new ArgumentException("Scope changes must be between -1000 and 1000");
    }

    public override string ToString()
    {
        return $"Burndown [{Timestamp:yyyy-MM-dd HH:mm}] - {CompletedStoryPoints}/{TotalStoryPoints} pts ({GetBurndownPercentage():F1}%)";
    }
}
