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

    /// <summary>
    /// Initializes a new instance of <see cref="AnalyticsService"/>.
    /// </summary>
    /// <param name="jiraService">Client used to fetch sprints, issues and team data from Jira.</param>
    /// <param name="metricsRepository">Repository used to persist computed metrics.</param>
    /// <param name="logger">Logger used to record analysis progress and failures.</param>
    public AnalyticsService(IJiraApiService jiraService, IMetricsRepository metricsRepository, ILogger<AnalyticsService> logger)
    {
        ArgumentNullException.ThrowIfNull(jiraService);
        ArgumentNullException.ThrowIfNull(metricsRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _jiraService = jiraService;
        _metricsRepository = metricsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes the most recently closed sprints for a project, computing per-sprint
    /// metrics (velocity, completion, defects, overdue issues) and aggregate trend/health.
    /// </summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <param name="sprintCount">Number of most recent closed sprints to analyze.</param>
    /// <returns>
    /// A <see cref="SprintAnalysisResult"/> with per-sprint metrics and aggregate figures.
    /// Returns an empty result (not thrown) if the analysis fails.
    /// </returns>
    public async Task<SprintAnalysisResult> AnalyzeSprints(string projectKey, int sprintCount)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(projectKey));
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
                    TeamSize = DeriveTeamSize(issues)
                };

                metric.AverageCycleTime = issues.Any() ? issues.Average(i => i.GetCycleTime()) : 0;
                result.Metrics.Add(metric);
            }

            // Calculate aggregate metrics
            if (result.Metrics.Any())
            {
                result.AverageVelocity = result.Metrics.Any() ? result.Metrics.Average(m => m.GetVelocity()) : 0;
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

    /// <summary>
    /// Analyzes team performance for a project by assigning issues to team members
    /// and computing per-developer productivity and workload distribution.
    /// </summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>
    /// A <see cref="TeamAnalysisResult"/> with top/low performers, average productivity
    /// and workload distribution. Returns an empty result (not thrown) if the analysis fails.
    /// </returns>
    public async Task<TeamAnalysisResult> AnalyzeTeam(string projectKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(projectKey));
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
            result.AverageProductivity = team.Any() ? team.Average(d => d.GetProductivity()) : 0;

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

    /// <summary>
    /// Analyzes quality metrics for a project across all closed sprints, computing
    /// defect count, defect rate and the components with the highest bug concentration.
    /// </summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>
    /// A <see cref="QualityMetricsResult"/> with defect statistics and high-risk areas.
    /// Returns an empty result (not thrown) if the analysis fails.
    /// </returns>
    public async Task<QualityMetricsResult> AnalyzeQuality(string projectKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(projectKey));
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

    /// <summary>
    /// Analyzes velocity trends over the most recent closed sprints, comparing the
    /// average velocity of the earlier half against the later half of the range.
    /// </summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <param name="sprintCount">Number of most recent closed sprints to include.</param>
    /// <returns>
    /// A <see cref="VelocityTrendResult"/> with per-sprint velocities, trend slope and
    /// trend classification. Returns an empty result (not thrown) if the analysis fails.
    /// </returns>
    public async Task<VelocityTrendResult> AnalyzeVelocityTrend(string projectKey, int sprintCount)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(projectKey));
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

    /// <summary>
    /// Derives the team size from unique issue assignees. Returns at least 1 to
    /// avoid division-by-zero in per-developer metric calculations.
    /// </summary>
    private static int DeriveTeamSize(IReadOnlyCollection<JiraIssue> issues)
    {
        var uniqueAssignees = issues
            .Where(i => !string.IsNullOrWhiteSpace(i.Assignee))
            .Select(i => i.Assignee)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        return Math.Max(uniqueAssignees, 1);
    }

    /// <summary>
    /// Analyzes overdue issues in a project, computing the total and critical overdue
    /// counts along with the average number of days past their due date.
    /// </summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>
    /// An <see cref="OverdueIssuesResult"/> with overdue issues and derived statistics.
    /// Returns an empty result (not thrown) if the analysis fails.
    /// </returns>
    public async Task<OverdueIssuesResult> AnalyzeOverdueIssues(string projectKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(projectKey));
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

    /// <summary>
    /// Analyzes cycle time metrics for a project's resolved issues, computing the average,
    /// median and percentile (P50/P75/P90) cycle times along with a per-issue breakdown.
    /// </summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>
    /// A <see cref="CycleTimeResult"/> with aggregate and per-issue cycle time statistics.
    /// Returns a partial result (not thrown) if the analysis fails.
    /// </returns>
    public async Task<CycleTimeResult> AnalyzeCycleTime(string projectKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(projectKey));
        _logger.LogInformation("Analyzing cycle time for project {ProjectKey}", projectKey);

        var result = new CycleTimeResult
        {
            ProjectKey = projectKey
        };

        try
        {
            // Fetch all issues for the project
            var issues = await _jiraService.GetProjectIssuesAsync(projectKey);

            // Filter to only resolved issues for accurate cycle time calculation
            var resolvedIssues = issues
                .Where(i => i.ResolutionDate.HasValue)
                .ToList();

            if (resolvedIssues.Any())
            {
                // Calculate cycle times for all resolved issues
                var cycleTimes = resolvedIssues
                    .Select(i => i.GetCycleTime())
                    .Where(ct => ct > 0)
                    .ToList();

                if (cycleTimes.Any())
                {
                    result.AverageCycleTime = cycleTimes.Average();
                    result.MedianCycleTime = CalculateMedian(cycleTimes);
                    result.P50CycleTime = CalculatePercentile(cycleTimes, 50);
                result.P75CycleTime = CalculatePercentile(cycleTimes, 75);
                result.P90CycleTime = CalculatePercentile(cycleTimes, 90);
                }

                // Create detailed per-issue list
                result.IssueCycleTimes = resolvedIssues
                    .Where(i => i.GetCycleTime() > 0)
                    .Select(i => new IssueCycleTime
                    {
                        IssueKey = i.Key,
                        Summary = i.Summary,
                        CycleTimeDays = i.GetCycleTime(),
                        CreatedDate = i.CreatedDate,
                        ResolutionDate = i.ResolutionDate
                    })
                    .OrderByDescending(ict => ict.CycleTimeDays)
                    .ToList();
            }

            _logger.LogInformation("Cycle time analysis completed: {IssueCount} issues analyzed, avg {Average:F2} days",
                result.IssueCycleTimes.Count,
                result.AverageCycleTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing cycle time");
        }

        return result;
    }

    /// <summary>
    /// Calculates the median of a list of double values
    /// </summary>
    private static double CalculateMedian(List<double> values)
    {
        if (values.Count == 0) return 0;

        var sorted = values.OrderBy(v => v).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
        {
            // Even number of elements - average the middle two
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            // Odd number of elements - return the middle one
            return sorted[count / 2];
        }
    }

    /// <summary>
    /// Calculates the Nth percentile of a list of double values
    /// </summary>
    private static double CalculatePercentile(List<double> values, double percentile)
    {
        if (values.Count == 0) return 0;

        var sorted = values.OrderBy(v => v).ToList();
        var n = (int)Math.Ceiling((percentile / 100.0) * sorted.Count);
        return sorted[Math.Max(0, n - 1)];
    }
}
