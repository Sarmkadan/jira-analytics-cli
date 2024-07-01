// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a Jira sprint with its metadata and state
/// </summary>
public class Sprint
{
    [Required]
    [JsonProperty("id")]
    public int Id { get; set; }

    [Required]
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("state")]
    public string State { get; set; } = "Open"; // Open, Active, Closed

    [JsonProperty("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonProperty("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonProperty("completeDate")]
    public DateTime? CompleteDate { get; set; }

    [JsonProperty("goal")]
    public string? Goal { get; set; }

    [JsonProperty("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    public List<JiraIssue> Issues { get; set; } = new();

    public bool IsActive()
    {
        return State == "Active" || (StartDate.HasValue && StartDate <= DateTime.UtcNow &&
               (!EndDate.HasValue || EndDate >= DateTime.UtcNow));
    }

    public bool IsClosed()
    {
        return State == "Closed" || CompleteDate.HasValue;
    }

    public int GetDuration()
    {
        // Duration in days from start to end
        if (!StartDate.HasValue || !EndDate.HasValue)
            return 0;

        return (int)(EndDate.Value - StartDate.Value).TotalDays;
    }

    public int GetPlannedStoryPoints()
    {
        // Sum of story points for all issues in the sprint
        return Issues.Sum(i => i.StoryPoints ?? 0);
    }

    public int GetCompletedStoryPoints()
    {
        // Sum of story points for completed issues
        return Issues.Where(i => i.Status is "Done" or "Closed")
                     .Sum(i => i.StoryPoints ?? 0);
    }

    public double GetVelocity()
    {
        // Velocity as percentage of planned points completed
        var planned = GetPlannedStoryPoints();
        if (planned == 0) return 0;

        return (GetCompletedStoryPoints() / (double)planned) * 100;
    }

    public int GetCompletedIssueCount()
    {
        return Issues.Count(i => i.Status is "Done" or "Closed");
    }

    public int GetTotalIssueCount()
    {
        return Issues.Count;
    }

    public List<JiraIssue> GetOverdueIssues()
    {
        return Issues.Where(i => i.IsOverdue()).ToList();
    }

    public List<JiraIssue> GetInProgressIssues()
    {
        return Issues.Where(i => i.IsInProgress()).ToList();
    }

    public List<JiraIssue> GetBlockedIssues()
    {
        return Issues.Where(i => i.Status == "Blocked").ToList();
    }

    public void AddIssue(JiraIssue issue)
    {
        if (issue == null)
            throw new ArgumentNullException(nameof(issue));

        issue.Validate();
        Issues.Add(issue);
    }

    public void Validate()
    {
        if (Id <= 0)
            throw new ArgumentException("Sprint ID must be positive");

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Sprint name cannot be empty");

        if (StartDate.HasValue && EndDate.HasValue && EndDate <= StartDate)
            throw new ArgumentException("End date must be after start date");
    }

    public override string ToString()
    {
        return $"Sprint: {Name} ({State}) - {GetCompletedIssueCount()}/{GetTotalIssueCount()} issues completed";
    }
}
