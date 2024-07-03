// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a Jira issue with all relevant metadata for analytics
/// </summary>
public class JiraIssue
{
    [Required]
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("type")]
    public string IssueType { get; set; } = string.Empty;

    [JsonPropertyName("assignee")]
    public string? Assignee { get; set; }

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium";

    [JsonPropertyName("storyPoints")]
    public int? StoryPoints { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    [Required]
    [JsonPropertyName("created")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("updated")]
    public DateTime UpdatedDate { get; set; }

    [JsonPropertyName("resolutionDate")]
    public DateTime? ResolutionDate { get; set; }

    [JsonPropertyName("labels")]
    public List<string> Labels { get; set; } = new();

    [JsonPropertyName("components")]
    public List<string> Components { get; set; } = new();

    [JsonPropertyName("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    [JsonPropertyName("sprintId")]
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
