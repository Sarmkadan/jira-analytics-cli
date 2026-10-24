// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a Jira sprint with its metadata and state
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public class Sprint
{
    [Required]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = "Open"; // Open, Active, Closed

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("completeDate")]
    public DateTime? CompleteDate { get; set; }

    [JsonPropertyName("goal")]
    public string? Goal { get; set; }

    [JsonPropertyName("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    public List<JiraIssue> Issues { get; set; } = new();

    public bool IsActive()
    {
        return this.State == "Active" || (this.StartDate.HasValue && this.StartDate <= DateTime.UtcNow &&
               (!this.EndDate.HasValue || this.EndDate >= DateTime.UtcNow));
    }

    public bool IsClosed()
    {
        return this.State == "Closed" || this.CompleteDate.HasValue;
    }

    public int GetDuration()
    {
        // Duration in days from start to end
        if (!this.StartDate.HasValue || !this.EndDate.HasValue)
        {
            return 0;
        }

        return (int)(this.EndDate.Value - this.StartDate.Value).TotalDays;
    }

    /// <summary>
    /// Issue types counted toward story point totals. Sub-tasks are excluded to
    /// prevent double-counting when parent issues also carry estimates.
    /// </summary>
    public static readonly IReadOnlySet<string> CountableIssueTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Story", "Epic", "Task", "Feature", "Improvement", "New Feature"
        };

    private static bool IsCountableIssueType(string issueType) =>
        !string.Equals(issueType, "Sub-task", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(issueType, "Subtask", StringComparison.OrdinalIgnoreCase);

    public int GetPlannedStoryPoints()
    {
        // Sum of story points for non-sub-task issues only to avoid double-counting
        return this.Issues.Where(i => IsCountableIssueType(i.IssueType))
                     .Sum(i => i.StoryPoints ?? 0);
    }

    public int GetCompletedStoryPoints()
    {
        // Sum of story points for completed non-sub-task issues
        return this.Issues.Where(i => i.Status is "Done" or "Closed")
                     .Where(i => IsCountableIssueType(i.IssueType))
                     .Sum(i => i.StoryPoints ?? 0);
    }

    public double GetVelocity()
    {
        // Velocity as percentage of planned points completed
        var planned = this.GetPlannedStoryPoints();
        if (planned == 0)
        {
            return 0;
        }

        return (this.GetCompletedStoryPoints() / (double)planned) * 100;
    }

    public int GetCompletedIssueCount()
    {
        return this.Issues.Count(i => i.Status is "Done" or "Closed");
    }

    public int GetTotalIssueCount()
    {
        return this.Issues.Count;
    }

    public List<JiraIssue> GetOverdueIssues()
    {
        return this.Issues.Where(i => i.IsOverdue()).ToList();
    }

    public List<JiraIssue> GetInProgressIssues()
    {
        return this.Issues.Where(i => i.IsInProgress()).ToList();
    }

    public List<JiraIssue> GetBlockedIssues()
    {
        return this.Issues.Where(i => i.Status == "Blocked").ToList();
    }

    public void AddIssue(JiraIssue issue)
    {
        if (issue == null)
        {
            throw new ArgumentNullException(nameof(issue));
        }

        issue.Validate();
        this.Issues.Add(issue);
    }

    public void Validate()
    {
        if (this.Id <= 0)
        {
            throw new ArgumentException("Sprint ID must be positive");
        }

        if (string.IsNullOrWhiteSpace(this.Name))
        {
            throw new ArgumentException("Sprint name cannot be empty");
        }

        if (this.StartDate.HasValue && this.EndDate.HasValue && this.EndDate <= this.StartDate)
        {
            throw new ArgumentException("End date must be after start date");
        }
    }

    public override string ToString()
    {
        return $"[{this.Key}] {this.Name} ({this.State}) - {this.GetCompletedStoryPoints()}/{this.GetPlannedStoryPoints()} pts, " +
               $"{this.StartDate?.ToString("yyyy-MM-dd") ?? "n/a"} to {this.EndDate?.ToString("yyyy-MM-dd") ?? "n/a"}, " +
               $"{this.GetCompletedIssueCount()}/{this.GetTotalIssueCount()} issues completed";
    }
}
