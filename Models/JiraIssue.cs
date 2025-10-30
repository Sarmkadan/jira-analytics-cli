// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a Jira issue with all relevant metadata for analytics
/// </summary>
public class JiraIssue
{
    [Required]
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [Required]
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [Required]
    [JsonProperty("type")]
    public string IssueType { get; set; } = string.Empty;

    [JsonProperty("assignee")]
    public string? Assignee { get; set; }

    [JsonProperty("priority")]
    public string Priority { get; set; } = "Medium";

    [JsonProperty("storyPoints")]
    public int? StoryPoints { get; set; }

    [JsonProperty("dueDate")]
    public DateTime? DueDate { get; set; }

    [Required]
    [JsonProperty("created")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("updated")]
    public DateTime UpdatedDate { get; set; }

    [JsonProperty("resolutionDate")]
    public DateTime? ResolutionDate { get; set; }

    [JsonProperty("labels")]
    public List<string> Labels { get; set; } = new();

    [JsonProperty("components")]
    public List<string> Components { get; set; } = new();

    [JsonProperty("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    [JsonProperty("sprintId")]
    public int? SprintId { get; set; }

    public bool IsOverdue()
    {
        // An issue is overdue if it has a due date that has passed and isn't resolved
        return DueDate.HasValue &&
               DueDate.Value < DateTime.UtcNow &&
               Status != "Done" &&
               Status != "Closed" &&
               !ResolutionDate.HasValue;
    }

    public bool IsHighPriority()
    {
        return Priority is "Critical" or "Blocker" or "High";
    }

    public int GetDaysOpenWithoutProgress()
    {
        // Calculate days open without status change
        var daysSinceCreation = (int)(DateTime.UtcNow - CreatedDate).TotalDays;
        return Math.Max(daysSinceCreation, 0);
    }

    public double GetCycleTime()
    {
        // Cycle time: from created to resolved (in days)
        if (!ResolutionDate.HasValue)
            return (DateTime.UtcNow - CreatedDate).TotalDays;

        return (ResolutionDate.Value - CreatedDate).TotalDays;
    }

    public bool IsInProgress()
    {
        return Status is "In Progress" or "In Review" or "Testing";
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new ArgumentException("Issue key cannot be empty");

        if (string.IsNullOrWhiteSpace(Summary))
            throw new ArgumentException("Issue summary cannot be empty");

        if (CreatedDate == default)
            throw new ArgumentException("Created date must be set");

        if (DueDate.HasValue && DueDate.Value < CreatedDate)
            throw new ArgumentException("Due date cannot be before creation date");
    }

    public override string ToString()
    {
        return $"[{Key}] {Summary} ({Status}) - {StoryPoints ?? 0} pts";
    }
}
