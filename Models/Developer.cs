// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a developer/team member with their work metrics
/// </summary>
public class Developer
{
    [Required]
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonProperty("active")]
    public bool Active { get; set; } = true;

    [JsonProperty("joinDate")]
    public DateTime? JoinDate { get; set; }

    public List<JiraIssue> AssignedIssues { get; set; } = new();

    public List<SprintMetric> SprintMetrics { get; set; } = new();

    public int GetTotalAssignedIssues()
    {
        return AssignedIssues.Count;
    }

    public int GetCompletedIssues()
    {
        return AssignedIssues.Count(i => i.Status is "Done" or "Closed");
    }

    public int GetInProgressIssues()
    {
        return AssignedIssues.Count(i => i.IsInProgress());
    }

    public int GetTotalStoryPoints()
    {
        return AssignedIssues.Sum(i => i.StoryPoints ?? 0);
    }

    public int GetCompletedStoryPoints()
    {
        return AssignedIssues.Where(i => i.Status is "Done" or "Closed")
                             .Sum(i => i.StoryPoints ?? 0);
    }

    public int GetOverdueIssueCount()
    {
        return AssignedIssues.Count(i => i.IsOverdue());
    }

    public double GetCompletionRate()
    {
        var total = GetTotalAssignedIssues();
        if (total == 0) return 0;

        return (GetCompletedIssues() / (double)total) * 100;
    }

    public double GetAverageIssuesPerDay()
    {
        if (!JoinDate.HasValue) return 0;

        var daysActive = (DateTime.UtcNow - JoinDate.Value).TotalDays;
        if (daysActive <= 0) return 0;

        return GetCompletedIssues() / daysActive;
    }

    public double GetAverageStoryPointsPerIssue()
    {
        var completed = GetCompletedIssues();
        if (completed == 0) return 0;

        return GetCompletedStoryPoints() / (double)completed;
    }

    public double GetProductivity()
    {
        // Productivity score: story points completed / days active
        if (!JoinDate.HasValue) return 0;

        var daysActive = (DateTime.UtcNow - JoinDate.Value).TotalDays;
        if (daysActive <= 0) return 0;

        return GetCompletedStoryPoints() / daysActive;
    }

    public List<JiraIssue> GetHighPriorityIssues()
    {
        return AssignedIssues.Where(i => i.IsHighPriority()).ToList();
    }

    public double GetAverageCycleTime()
    {
        if (!AssignedIssues.Any()) return 0;

        return AssignedIssues.Average(i => i.GetCycleTime());
    }

    public void AssignIssue(JiraIssue issue)
    {
        if (issue == null)
            throw new ArgumentNullException(nameof(issue));

        if (!AssignedIssues.Any(i => i.Key == issue.Key))
            AssignedIssues.Add(issue);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new ArgumentException("Developer key cannot be empty");

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Developer name cannot be empty");
    }

    public override string ToString()
    {
        return $"{DisplayName ?? Name} - {GetCompletedIssues()} completed, {GetInProgressIssues()} in progress";
    }
}
