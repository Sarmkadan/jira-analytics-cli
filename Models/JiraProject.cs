// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a Jira project with all relevant metadata
/// </summary>
public class JiraProject
{
    [Required]
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("projectType")]
    public string ProjectType { get; set; } = "software"; // software, service_management, business

    [JsonPropertyName("lead")]
    public string? Lead { get; set; }

    [JsonPropertyName("created")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    public List<Sprint> Sprints { get; set; } = new();

    public List<Developer> TeamMembers { get; set; } = new();

    public List<SprintMetric> MetricsHistory { get; set; } = new();

    public int GetTotalSprintCount()
    {
        return this.Sprints.Count;
    }

    public int GetCompletedSprintCount()
    {
        return this.Sprints.Count(s => s.IsClosed());
    }

    public Sprint? GetCurrentActiveSprint()
    {
        return this.Sprints.FirstOrDefault(s => s.IsActive());
    }

    public List<Sprint> GetRecentSprints(int count)
    {
        return this.Sprints.Where(s => s.IsClosed())
                      .OrderByDescending(s => s.EndDate)
                      .Take(count)
                      .ToList();
    }

    public double GetAverageVelocity()
    {
        var completedSprints = this.GetRecentSprints(5);
        if (!completedSprints.Any())
        {
            return 0;
        }

        return completedSprints.Average(s => s.GetVelocity());
    }

    public int GetTotalTeamSize()
    {
        return this.TeamMembers.Count(m => m.Active);
    }

    public List<Developer> GetTopPerformers(int count = 5)
    {
        return this.TeamMembers
            .OrderByDescending(d => d.GetProductivity())
            .Take(count)
            .ToList();
    }

    public List<JiraIssue> GetAllOverdueIssues()
    {
        var allIssues = this.Sprints.SelectMany(s => s.Issues).ToList();
        return allIssues.Where(i => i.IsOverdue()).ToList();
    }

    public List<JiraIssue> GetAllBlockedIssues()
    {
        var allIssues = this.Sprints.SelectMany(s => s.Issues).ToList();
        return allIssues.Where(i => i.Status == "Blocked").ToList();
    }

    public double GetProjectHealthScore()
    {
        if (!this.MetricsHistory.Any())
        {
            return 0;
        }

        var recentMetrics = this.MetricsHistory.OrderByDescending(m => m.EndDate).Take(3);
        if (!recentMetrics.Any())
        {
            return 0;
        }

        var avgCompletion = recentMetrics.Average(m => m.GetCompletionRate());
        var avgQuality = recentMetrics.Average(m => m.GetQualityScore());
        var avgRisk = recentMetrics.Average(m => m.GetRiskScore());

        // Health score: 60% completion, 30% quality, 10% risk
        return (avgCompletion * 0.6) + (avgQuality * 0.3) + ((100 - avgRisk) * 0.1);
    }

    public void AddSprint(Sprint sprint)
    {
        if (sprint == null)
        {
            throw new ArgumentNullException(nameof(sprint));
        }

        sprint.Validate();
        if (!this.Sprints.Any(s => s.Id == sprint.Id))
        {
            this.Sprints.Add(sprint);
        }
    }

    public void AddTeamMember(Developer developer)
    {
        if (developer == null)
        {
            throw new ArgumentNullException(nameof(developer));
        }

        developer.Validate();
        if (!this.TeamMembers.Any(m => m.Key == developer.Key))
        {
            this.TeamMembers.Add(developer);
        }
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.Key))
        {
            throw new ArgumentException("Project key cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(this.Name))
        {
            throw new ArgumentException("Project name cannot be empty");
        }

        if (this.CreatedDate == default)
        {
            throw new ArgumentException("Created date must be set");
        }
    }

    public override string ToString()
    {
        return $"Project: {this.Name} ({this.Key}) - {this.GetTotalSprintCount()} sprints, {this.GetTotalTeamSize()} team members";
    }
}
