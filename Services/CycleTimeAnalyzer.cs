// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Analyses cycle time distributions across issues to identify bottlenecks and efficiency patterns.
/// </summary>
public class CycleTimeAnalyzer
{
    private readonly IJiraApiService _jiraService;
    private readonly ILogger<CycleTimeAnalyzer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CycleTimeAnalyzer"/>.
    /// </summary>
    /// <param name="jiraService">Service for retrieving issue data.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public CycleTimeAnalyzer(IJiraApiService jiraService, ILogger<CycleTimeAnalyzer> logger)
    {
        this._jiraService = jiraService ?? throw new ArgumentNullException(nameof(jiraService));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates cycle time statistics for all resolved issues in a project.
    /// </summary>
    /// <param name="projectKey">The Jira project key.</param>
    /// <returns>A <see cref="CycleTimeResult"/> with percentile breakdowns.</returns>
    public async Task<CycleTimeResult> AnalyzeProjectAsync(string projectKey)
    {
        var issues = await this._jiraService.GetProjectIssuesAsync(projectKey);
        var cycleTimes = ComputeCycleTimes(issues);

        if (cycleTimes.Count == 0)
        {
            this._logger.LogWarning("No resolved issues found for project {Project}", projectKey);
            return new CycleTimeResult { ProjectKey = projectKey };
        }

        cycleTimes.Sort();

        var result = new CycleTimeResult
        {
            ProjectKey = projectKey,
            AverageCycleTime = cycleTimes.Average(),
            MedianCycleTime = GetPercentile(cycleTimes, 50),
            P50CycleTime = GetPercentile(cycleTimes, 50),
            P75CycleTime = GetPercentile(cycleTimes, 75),
        };

        this._logger.LogInformation(
            "Project {Project}: avg={Avg:F1}d, median={Med:F1}d, P75={P75:F1}d ({Count} issues)",
            projectKey, result.AverageCycleTime, result.MedianCycleTime, result.P75CycleTime, cycleTimes.Count);

        return result;
    }

    /// <summary>
    /// Computes cycle times grouped by issue type (Bug, Story, Task, etc.).
    /// </summary>
    /// <param name="issues">Issues to analyse.</param>
    /// <returns>Dictionary mapping issue type to average cycle time in days.</returns>
    public Dictionary<string, double> GetCycleTimeByIssueType(IReadOnlyList<JiraIssue> issues)
    {
        return issues
            .Where(i => i.ResolutionDate.HasValue)
            .GroupBy(i => i.IssueType)
            .ToDictionary(
                g => g.Key,
                g => g.Average(i => (i.ResolutionDate!.Value - i.CreatedDate).TotalDays));
    }

    /// <summary>
    /// Identifies the top N slowest issues by cycle time.
    /// </summary>
    /// <param name="issues">Issues to analyse.</param>
    /// <param name="count">Number of issues to return.</param>
    /// <returns>List of issue keys with their cycle time in days, slowest first.</returns>
    public List<(string Key, double Days)> GetSlowestIssues(IReadOnlyList<JiraIssue> issues, int count = 10)
    {
        return issues
            .Where(i => i.ResolutionDate.HasValue)
            .Select(i => (i.Key, Days: (i.ResolutionDate!.Value - i.CreatedDate).TotalDays))
            .OrderByDescending(x => x.Days)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Computes a weekly cycle time trend over the specified period.
    /// </summary>
    /// <param name="issues">Resolved issues to analyse.</param>
    /// <param name="weeks">Number of weeks to look back.</param>
    /// <returns>Weekly average cycle times, oldest first.</returns>
    public List<TimeSeriesPoint> GetWeeklyTrend(IReadOnlyList<JiraIssue> issues, int weeks = 12)
    {
        var cutoff = DateTime.UtcNow.AddDays(-weeks * 7);

        return issues
            .Where(i => i.ResolutionDate.HasValue && i.ResolutionDate.Value >= cutoff)
            .GroupBy(i =>
            {
                var resolved = i.ResolutionDate!.Value;
                // ISO week grouping
                int dayOfYear = resolved.DayOfYear;
                int weekNumber = (dayOfYear - 1) / 7;
                return new DateTime(resolved.Year, 1, 1).AddDays(weekNumber * 7);
            })
            .OrderBy(g => g.Key)
            .Select(g => new TimeSeriesPoint(
                g.Key,
                g.Average(i => (i.ResolutionDate!.Value - i.CreatedDate).TotalDays)))
            .ToList();
    }

    private static List<double> ComputeCycleTimes(IReadOnlyList<JiraIssue> issues)
    {
        return issues
            .Where(i => i.ResolutionDate.HasValue)
            .Select(i => (i.ResolutionDate!.Value - i.CreatedDate).TotalDays)
            .Where(d => d >= 0)
            .ToList();
    }

    private static double GetPercentile(List<double> sorted, double percentile)
    {
        if (sorted.Count == 0)
        {
            return 0;
        }

        double index = (percentile / 100.0) * (sorted.Count - 1);
        int lower = (int)Math.Floor(index);
        int upper = (int)Math.Ceiling(index);
        if (lower == upper)
        {
            return sorted[lower];
        }

        double fraction = index - lower;
        return sorted[lower] + fraction * (sorted[upper] - sorted[lower]);
    }
}
