// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a point-in-time snapshot of sprint burndown data
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public class BurndownSnapshot
{
    private DateTimeOffset _timestamp;

    /// <summary>
    /// Gets or sets the point in time this snapshot was captured, always normalized to UTC.
    /// Storing timestamps as <see cref="DateTimeOffset"/> with UTC offset ensures correct
    /// burn-rate and duration math across DST transitions, as the offset information is
    /// preserved and used in arithmetic operations.
    /// </summary>
    [Required]
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp
    {
        get => this._timestamp;
        set => this._timestamp = value.ToUniversalTime();
    }

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
    /// Calculates the percentage of work completed based on story points.
    /// </summary>
    /// <returns>Percentage of work completed (0-100).</returns>
    public double GetBurndownPercentage()
    {
        if (this.TotalStoryPoints == 0)
        {
            return 0;
        }

        return (this.CompletedStoryPoints == 0 || this.TotalStoryPoints == 0)
            ? 0
            : (this.CompletedStoryPoints / (double)this.TotalStoryPoints) * 100;
    }

    /// <summary>
    /// Calculates the projected completion percentage based on current burn rate.
    /// </summary>
    /// <param name="sprintEnd">The end date of the sprint.</param>
    /// <returns>Projected completion percentage.</returns>
    public double GetProjectedCompletionPercentage(DateTimeOffset sprintEnd)
    {
        var daysRemaining = (sprintEnd - this.Timestamp).TotalDays;

        if (daysRemaining <= 0)
        {
            return this.GetBurndownPercentage();
        }

        var currentPercentage = this.GetBurndownPercentage();
        var remainingPercentage = 100 - currentPercentage;

        if (remainingPercentage <= 0)
        {
            return 100;
        }

        const int sprintLengthDays = 14;
        var burnRateFactor = daysRemaining / (double)sprintLengthDays;

        return Math.Min(100, currentPercentage + (remainingPercentage * burnRateFactor));
    }

    /// <summary>
    /// Determines if the current burndown is on track to meet sprint goals.
    /// </summary>
    /// <param name="sprintEnd">The end date of the sprint.</param>
    /// <returns>True if the sprint is on track.</returns>
    public bool IsOnTrack(DateTimeOffset sprintEnd)
    {
        var daysTotal = (sprintEnd - (this.Timestamp.AddDays(-7))).TotalDays;
        var daysElapsed = 7;
        var daysRemaining = daysTotal - daysElapsed;

        if (daysRemaining <= 0)
        {
            return this.GetBurndownPercentage() >= 90;
        }

        if (daysTotal <= 0)
        {
            return this.GetBurndownPercentage() >= 90;
        }

        var expectedBurndown = (daysElapsed / daysTotal) * 100;
        var actualBurndown = this.GetBurndownPercentage();

        return actualBurndown >= (expectedBurndown * 0.9);
    }

    /// <summary>
    /// Estimates hours until completion based on issue completion rate.
    /// </summary>
    /// <param name="issuesPerHour">Average issues completed per hour.</param>
    /// <returns>Estimated hours until completion.</returns>
    public int GetHoursUntilCompletion(int issuesPerHour)
    {
        if (issuesPerHour <= 0)
        {
            return 0;
        }

        return this.RemainingIssueCount / issuesPerHour;
    }

    /// <summary>
    /// Validates the burndown snapshot to ensure all invariants are satisfied.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the snapshot contains validation errors.</exception>
    public void Validate()
    {
        if (this.SprintId <= 0)
        {
            throw new ArgumentException("Sprint ID must be positive");
        }

        if (this.Timestamp == default)
        {
            throw new ArgumentException("Timestamp must be set");
        }

        if (this.RemainingStoryPoints < 0)
        {
            throw new ArgumentException("Remaining story points cannot be negative");
        }

        if (this.CompletedStoryPoints < 0)
        {
            throw new ArgumentException("Completed story points cannot be negative");
        }

        if (this.TotalStoryPoints <= 0)
        {
            throw new ArgumentException("Total story points must be positive");
        }

        if (this.CompletedStoryPoints > this.TotalStoryPoints)
        {
            throw new ArgumentException("Completed story points cannot exceed total story points");
        }

        if (this.RemainingStoryPoints > this.TotalStoryPoints)
        {
            throw new ArgumentException("Remaining story points cannot exceed total story points");
        }

        if (this.CompletedStoryPoints + this.RemainingStoryPoints != this.TotalStoryPoints)
        {
            throw new ArgumentException("Completed + Remaining story points must equal total story points");
        }

        if (this.RemainingIssueCount < 0)
        {
            throw new ArgumentException("Remaining issue count cannot be negative");
        }

        if (this.CompletedIssueCount < 0)
        {
            throw new ArgumentException("Completed issue count cannot be negative");
        }

        if (this.TotalIssueCount <= 0)
        {
            throw new ArgumentException("Total issue count must be positive");
        }

        if (this.CompletedIssueCount > this.TotalIssueCount)
        {
            throw new ArgumentException("Completed issue count cannot exceed total issue count");
        }

        if (this.RemainingIssueCount > this.TotalIssueCount)
        {
            throw new ArgumentException("Remaining issue count cannot exceed total issue count");
        }

        if (this.CompletedIssueCount + this.RemainingIssueCount != this.TotalIssueCount)
        {
            throw new ArgumentException("Completed + Remaining issue count must equal total issue count");
        }

        if (this.ScopeChanges < -1000 || this.ScopeChanges > 1000)
        {
            throw new ArgumentException("Scope changes must be between -1000 and 1000");
        }
    }

    public override string ToString()
    {
        return $"[sprint {this.SprintId}] {this.Timestamp:yyyy-MM-dd HH:mm} UTC - {this.CompletedStoryPoints}/{this.TotalStoryPoints} pts done " +
               $"({this.GetBurndownPercentage():F1}%), {this.RemainingStoryPoints} remaining";
    }
}
