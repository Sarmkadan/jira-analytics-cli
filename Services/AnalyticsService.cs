// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Frozen;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Performs analytics calculations on Jira data including velocity, team metrics, quality scores
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    // FrozenDictionary is optimised for read-heavy workloads: its internal layout allows
    // the JIT to generate branchless lookups, outperforming a switch on string keys.
    private static readonly FrozenDictionary<string, int> _healthScoreMap =
        new Dictionary<string, int>(4)
        {
            ["Excellent"] = 4,
            ["Healthy"]   = 3,
            ["At Risk"]   = 2,
            ["Critical"]  = 1
        }.ToFrozenDictionary();

    private readonly IJiraApiService _jiraService;
    private readonly IMetricsRepository _metricsRepository;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(IJiraApiService jiraService, IMetricsRepository metricsRepository, ILogger<AnalyticsService> logger)
    {
        _jiraService = jiraService;
        _metricsRepository = metricsRepository;
        _logger = logger;
    }

    public async Task<SprintAnalysisResult> AnalyzeSprints(string projectKey, int sprintCount)
    {
        _logger.LogInformation("Analyzing {SprintCount} sprints for project {ProjectKey}", sprintCount, projectKey);

        var result = new SprintAnalysisResult();

        try
        {
            // Fetch sprints
            var sprints = await _jiraService.GetProjectSprintsAsync(projectKey);
            var recentSprints = sprints
                .Where(s => s.IsClosed())
                .OrderByDescending(s => s.EndDate)
                .Take(sprintCount)
                .ToList();

            // Analyze each sprint
            foreach (var sprint in recentSprints)
            {
                var issues = await _jiraService.GetSprintIssuesAsync(sprint.Id);
                sprint.Issues.AddRange(issues);

                var metric = new SprintMetric
                {
                    SprintId = sprint.Id,
                    SprintName = sprint.Name,
                    StartDate = sprint.StartDate ?? DateTime.UtcNow.AddDays(-14),
                    EndDate = sprint.EndDate ?? DateTime.UtcNow,
                    PlannedStoryPoints = sprint.GetPlannedStoryPoints(),
                    CompletedStoryPoints = sprint.GetCompletedStoryPoints(),
                    CommittedStoryPoints = sprint.GetPlannedStoryPoints(),
                    CompletedIssueCount = sprint.GetCompletedIssueCount(),
                    TotalIssueCount = sprint.GetTotalIssueCount(),
                    DefectsCount = issues.Count(i => i.IssueType == "Bug"),
                    OverdueIssueCount = sprint.GetOverdueIssues().Count,
                    TeamSize = 5 // Would aggregate from team data
                };

                metric.AverageCycleTime = issues.Any() ? issues.Average(i => i.GetCycleTime()) : 0;
                result.Metrics.Add(metric);
            }

            // Calculate aggregate metrics
            if (result.Metrics.Any())
            {
                result.AverageVelocity = result.Metrics.Average(m => m.GetVelocity());
                var sortedMetrics = result.Metrics.OrderBy(m => m.EndDate).ToList();

                if (sortedMetrics.Count >= 2)
                {
                    var oldVelocity = sortedMetrics[0].GetVelocity();
                    var newVelocity = sortedMetrics[sortedMetrics.Count - 1].GetVelocity();

                    if (oldVelocity > 0)
                        result.TrendPercentage = ((newVelocity - oldVelocity) / oldVelocity) * 100;
                }

                var healthScores = result.Metrics
                    .Average(m => _healthScoreMap.GetValueOrDefault(m.GetHealthStatus(), 1));

                result.OverallHealth = healthScores switch
                {
                    >= 3.5 => "Excellent",
                    >= 2.5 => "Healthy",
                    >= 1.5 => "At Risk",
                    _ => "Critical"
                };
            }

            _logger.LogInformation("Sprint analysis completed: {MetricsCount} sprints analyzed, overall health {Health}",
                result.Metrics.Count, result.OverallHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sprints");
        }

        return result;
    }

    public async Task<TeamAnalysisResult> AnalyzeTeam(string projectKey)
    {
        _logger.LogInformation("Analyzing team for project {ProjectKey}", projectKey);

        var result = new TeamAnalysisResult();

        try
        {
            var team = await _jiraService.GetProjectTeamAsync(projectKey);
            var issues = await _jiraService.GetProjectIssuesAsync(projectKey);

            // Assign issues to team members
            foreach (var issue in issues)
            {
                if (!string.IsNullOrEmpty(issue.Assignee))
                {
                    var developer = team.FirstOrDefault(d => d.DisplayName == issue.Assignee);
                    if (developer != null)
                        developer.AssignIssue(issue);
                }
            }

            // Calculate metrics
            var sortedByProductivity = team.OrderByDescending(d => d.GetProductivity()).ToList();
            result.TopPerformers = sortedByProductivity.Take(3).ToList();
            result.LowPerformers = sortedByProductivity.TakeLast(3).Reverse().ToList();
            result.AverageProductivity = team.Average(d => d.GetProductivity());

            // Workload distribution
            foreach (var developer in team)
            {
                result.WorkloadDistribution[developer.Name] = developer.GetTotalAssignedIssues();
            }

            _logger.LogInformation("Team analysis completed: {TeamSize} members analyzed", team.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing team");
        }

        return result;
    }

    public async Task<QualityMetricsResult> AnalyzeQuality(string projectKey)
    {
        _logger.LogInformation("Analyzing quality metrics for project {ProjectKey}", projectKey);

        var result = new QualityMetricsResult();

        try
        {
            var sprints = await _jiraService.GetProjectSprintsAsync(projectKey);
            var allIssues = new List<JiraIssue>();

            foreach (var sprint in sprints.Where(s => s.IsClosed()))
            {
                var issues = await _jiraService.GetSprintIssuesAsync(sprint.Id);
                allIssues.AddRange(issues);
            }

            // Count defects
            var defects = allIssues.Where(i => i.IssueType == "Bug").ToList();
            result.TotalDefects = defects.Count;

            if (allIssues.Any())
                result.DefectRate = (result.TotalDefects / (double)allIssues.Count) * 100;

            // Identify high-risk areas
            var componentWithMostBugs = allIssues
                .Where(i => i.Components.Any())
                .SelectMany(i => i.Components, (i, c) => new { Component = c, Issue = i })
                .Where(x => x.Issue.IssueType == "Bug")
                .GroupBy(x => x.Component)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            result.HighRiskAreas = componentWithMostBugs;

            _logger.LogInformation("Quality analysis completed: {DefectCount} defects found, defect rate {DefectRate:F2}%",
                result.TotalDefects, result.DefectRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing quality");
        }

        return result;
    }

    public async Task<VelocityTrendResult> AnalyzeVelocityTrend(string projectKey, int sprintCount)
    {
        _logger.LogInformation("Analyzing velocity trend for project {ProjectKey}", projectKey);

        var result = new VelocityTrendResult();

        try
        {
            var sprints = await _jiraService.GetProjectSprintsAsync(projectKey);
            var recentSprints = sprints
                .Where(s => s.IsClosed())
                .OrderBy(s => s.EndDate)
                .TakeLast(sprintCount)
                .ToList();

            foreach (var sprint in recentSprints)
            {
                var issues = await _jiraService.GetSprintIssuesAsync(sprint.Id);
                sprint.Issues.AddRange(issues);

                var velocity = sprint.GetVelocity();
                result.Velocities.Add((sprint.Name, velocity));
            }

            // Calculate trend
            if (result.Velocities.Count >= 2)
            {
                var velocityValues = result.Velocities.Select(v => v.Velocity).ToList();
                var firstHalf = velocityValues.Take(velocityValues.Count / 2).Average();
                var secondHalf = velocityValues.Skip(velocityValues.Count / 2).Average();

                result.TrendSlope = (secondHalf - firstHalf) / firstHalf * 100;
                result.Trend = result.TrendSlope switch
                {
                    > 10 => "Increasing",
                    < -10 => "Decreasing",
                    _ => "Stable"
                };
            }

            _logger.LogInformation("Velocity trend analysis completed: trend {Trend}, slope {Slope:F2}%",
                result.Trend, result.TrendSlope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing velocity trend");
        }

        return result;
    }

    public async Task<OverdueIssuesResult> AnalyzeOverdueIssues(string projectKey)
    {
        _logger.LogInformation("Analyzing overdue issues for project {ProjectKey}", projectKey);

        var result = new OverdueIssuesResult();

        try
        {
            var issues = await _jiraService.GetProjectIssuesAsync(projectKey);
            var overdueIssues = issues.Where(i => i.IsOverdue()).ToList();

            result.Issues = overdueIssues;
            result.TotalOverdueCount = overdueIssues.Count;
            result.CriticalCount = overdueIssues.Count(i => i.IsHighPriority());

            if (overdueIssues.Any())
            {
                result.AverageDaysOverdue = overdueIssues
                    .Where(i => i.DueDate.HasValue)
                    .Average(i => (DateTime.UtcNow - i.DueDate!.Value).TotalDays);
            }

            _logger.LogInformation("Overdue issues analysis completed: {OverdueCount} overdue, {CriticalCount} critical",
                result.TotalOverdueCount, result.CriticalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing overdue issues");
        }

        return result;
    }
}
