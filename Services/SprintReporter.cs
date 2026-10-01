// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Generates structured sprint reports combining velocity, burndown and cycle time data.
/// </summary>
public class SprintReporter
{
    private readonly IJiraApiService _jiraService;
    private readonly BurndownCalculator _burndownCalculator;
    private readonly VelocityTracker _velocityTracker;
    private readonly ILogger<SprintReporter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SprintReporter"/>.
    /// </summary>
    /// <param name="jiraService">Service for retrieving sprint and issue data.</param>
    /// <param name="burndownCalculator">Calculator for burndown analysis.</param>
    /// <param name="velocityTracker">Tracker for velocity trends.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public SprintReporter(
        IJiraApiService jiraService,
        BurndownCalculator burndownCalculator,
        VelocityTracker velocityTracker,
        ILogger<SprintReporter> logger)
    {
        _jiraService = jiraService ?? throw new ArgumentNullException(nameof(jiraService));
        _burndownCalculator = burndownCalculator ?? throw new ArgumentNullException(nameof(burndownCalculator));
        _velocityTracker = velocityTracker ?? throw new ArgumentNullException(nameof(velocityTracker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a summary report for a single sprint in Markdown format.
    /// </summary>
    /// <param name="sprintId">The sprint to report on.</param>
    /// <returns>Markdown-formatted sprint report string.</returns>
    public async Task<string> GenerateSprintSummaryAsync(int sprintId)
    {
        var sprint = await _jiraService.GetSprintAsync(sprintId);
        if (sprint is null)
        {
            _logger.LogWarning("Sprint {SprintId} not found", sprintId);
            return $"# Sprint {sprintId}\n\nSprint not found.";
        }

        var issues = await _jiraService.GetSprintIssuesAsync(sprintId);
        var burndown = await _burndownCalculator.GetActualBurndownAsync(sprintId);

        int totalPoints = issues.Sum(i => i.StoryPoints ?? 0);
        int completedPoints = issues.Where(i => i.Status == "Done").Sum(i => i.StoryPoints ?? 0);
        int completedCount = issues.Count(i => i.Status == "Done");

        var sb = new StringBuilder();
        sb.AppendLine($"# Sprint Report: {sprint.Name}");
        sb.AppendLine();
        sb.AppendLine($"- **State:** {sprint.State}");
        sb.AppendLine($"- **Issues:** {completedCount}/{issues.Count} completed");
        sb.AppendLine($"- **Story Points:** {completedPoints}/{totalPoints} completed");
        sb.AppendLine($"- **Burndown Data Points:** {burndown.Count}");
        sb.AppendLine();

        if (issues.Count > 0)
        {
            double completionRate = (double)completedCount / issues.Count * 100;
            sb.AppendLine($"- **Completion Rate:** {completionRate:F1}%");
        }

        _logger.LogInformation("Generated summary for sprint {SprintName} ({Id})", sprint.Name, sprintId);
        return sb.ToString();
    }

    /// <summary>
    /// Generates a multi-sprint comparison report for a project.
    /// </summary>
    /// <param name="projectKey">The Jira project key.</param>
    /// <param name="sprintCount">Number of recent sprints to include.</param>
    /// <returns>Markdown-formatted comparison report.</returns>
    public async Task<string> GenerateProjectReportAsync(string projectKey, int sprintCount = 5)
    {
        var sprints = await _jiraService.GetProjectSprintsAsync(projectKey);
        var recentSprints = sprints
            .OrderByDescending(s => s.Id)
            .Take(sprintCount)
            .Reverse()
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"# Project Report: {projectKey}");
        sb.AppendLine();
        sb.AppendLine($"| Sprint | Issues | Completed | Points |");
        sb.AppendLine($"|--------|--------|-----------|--------|");

        foreach (var sprint in recentSprints)
        {
            var issues = await _jiraService.GetSprintIssuesAsync(sprint.Id);
            int total = issues.Count;
            int done = issues.Count(i => i.Status == "Done");
            int points = issues.Where(i => i.Status == "Done").Sum(i => i.StoryPoints ?? 0);
            sb.AppendLine($"| {sprint.Name} | {total} | {done} | {points} |");
        }

        _logger.LogInformation("Generated project report for {Project} with {Count} sprints", projectKey, recentSprints.Count);
        return sb.ToString();
    }

    /// <summary>
    /// Identifies issues that are at risk of not completing within the sprint.
    /// </summary>
    /// <param name="issues">All issues in the current sprint.</param>
    /// <param name="sprintEnd">The sprint end date.</param>
    /// <returns>List of issue keys that are at risk.</returns>
    public List<string> IdentifyAtRiskIssues(IReadOnlyList<JiraIssue> issues, DateTimeOffset sprintEnd)
    {
        var now = DateTimeOffset.UtcNow;
        double remainingDays = (sprintEnd - now).TotalDays;

        if (remainingDays <= 0)
            return issues.Where(i => i.Status != "Done").Select(i => i.Key).ToList();

        // Issues in progress for more than half the sprint without resolution
        var atRisk = issues
            .Where(i => i.Status != "Done"
                && i.Status != "Closed"
                && (now - i.CreatedDate).TotalDays > remainingDays * 2)
            .Select(i => i.Key)
            .ToList();

        _logger.LogDebug("Found {Count} at-risk issues with {Days:F1} days remaining", atRisk.Count, remainingDays);
        return atRisk;
    }
}
