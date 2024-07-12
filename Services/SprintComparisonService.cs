// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Default implementation of <see cref="ISprintComparisonService"/> that analyses sprint
/// history from <see cref="ISprintRepository"/> and enriches entries with pre-computed
/// cycle-time data from <see cref="IMetricsRepository"/> when available.
/// </summary>
public sealed class SprintComparisonService : ISprintComparisonService
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IMetricsRepository _metricsRepository;
    private readonly ILogger<SprintComparisonService> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="SprintComparisonService"/>.
    /// </summary>
    /// <param name="sprintRepository">Sprint data access layer.</param>
    /// <param name="metricsRepository">Pre-computed sprint metrics store.</param>
    /// <param name="logger">Logger instance.</param>
    public SprintComparisonService(
        ISprintRepository sprintRepository,
        IMetricsRepository metricsRepository,
        ILogger<SprintComparisonService> logger)
    {
        _sprintRepository = sprintRepository ?? throw new ArgumentNullException(nameof(sprintRepository));
        _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<SprintComparisonReport> CompareSprintsAsync(
        string projectKey,
        IEnumerable<int> sprintIds,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectKey))
            throw new ArgumentException("Project key cannot be empty.", nameof(projectKey));

        var ids = sprintIds?.ToList() ?? throw new ArgumentNullException(nameof(sprintIds));

        _logger.LogInformation(
            "Comparing {SprintCount} sprints for project {ProjectKey}", ids.Count, projectKey);

        try
        {
            var sprintTasks = ids.Select(id => _sprintRepository.GetByIdAsync(id));
            var metricTasks = ids.Select(id => _metricsRepository.GetBySprintAsync(id));

            var resolvedSprints = await Task.WhenAll(sprintTasks).ConfigureAwait(false);
            var resolvedMetrics = await Task.WhenAll(metricTasks).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            // Build a quick lookup so BuildComparisonEntry can pull pre-computed AverageCycleTime
            var metricLookup = resolvedMetrics
                .SelectMany(m => m)
                .GroupBy(m => m.SprintId)
                .ToDictionary(g => g.Key, g => g.First());

            var sprints = resolvedSprints
                .Where(s => s != null && s.ProjectKey == projectKey)
                .Select(s => s!)
                .OrderBy(s => s.StartDate)
                .ToList();

            var entries = sprints
                .Select(s => BuildComparisonEntry(s, metricLookup.GetValueOrDefault(s.Id)))
                .ToList();

            return BuildReport(projectKey, entries, BuildDeveloperTrends(sprints));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Sprint comparison cancelled for project {ProjectKey}", projectKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing sprints for project {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DeveloperVelocityTrend>> GetTeamVelocityTrendsAsync(
        string projectKey,
        int sprintCount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectKey))
            throw new ArgumentException("Project key cannot be empty.", nameof(projectKey));

        if (sprintCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(sprintCount), "Sprint count must be positive.");

        _logger.LogInformation(
            "Computing team velocity trends for {ProjectKey} across {SprintCount} sprints",
            projectKey, sprintCount);

        try
        {
            var sprints = await _sprintRepository.GetRecentClosedSprints(sprintCount).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var filtered = sprints
                .Where(s => s.ProjectKey == projectKey)
                .OrderBy(s => s.StartDate)
                .ToList();

            _logger.LogInformation(
                "Found {SprintCount} closed sprints for {ProjectKey}", filtered.Count, projectKey);

            return BuildDeveloperTrends(filtered);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Team velocity trend computation cancelled for {ProjectKey}", projectKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error computing team velocity trends for {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<SprintComparisonReport> GenerateComparisonReportAsync(
        string projectKey,
        int sprintCount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectKey))
            throw new ArgumentException("Project key cannot be empty.", nameof(projectKey));

        if (sprintCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(sprintCount), "Sprint count must be positive.");

        _logger.LogInformation(
            "Generating sprint comparison report for {ProjectKey}, last {SprintCount} sprints",
            projectKey, sprintCount);

        try
        {
            var sprints = await _sprintRepository.GetRecentClosedSprints(sprintCount).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var filtered = sprints
                .Where(s => s.ProjectKey == projectKey)
                .OrderBy(s => s.StartDate)
                .ToList();

            var entries = filtered.Select(s => BuildComparisonEntry(s, null)).ToList();
            var trends  = BuildDeveloperTrends(filtered);

            _logger.LogInformation(
                "Sprint comparison report complete: {EntryCount} sprints, {DeveloperCount} contributors",
                entries.Count, trends.Count);

            return BuildReport(projectKey, entries, trends);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Sprint comparison report cancelled for {ProjectKey}", projectKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating sprint comparison report for {ProjectKey}", projectKey);
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static SprintComparisonEntry BuildComparisonEntry(Sprint sprint, SprintMetric? metric)
    {
        var completed      = sprint.GetCompletedStoryPoints();
        var planned        = sprint.GetPlannedStoryPoints();
        var duration       = sprint.GetDuration();
        var velocity       = duration > 0 ? completed / (double)duration : 0d;
        var completionRate = planned  > 0 ? (completed / (double)planned) * 100 : 0d;

        var defectCount = sprint.Issues.Count(
            i => i.IssueType is "Bug" or "Defect" && i.Status is "Done" or "Closed");

        // Prefer pre-computed cycle time; fall back to live calculation when not available
        var avgCycleTime = metric?.AverageCycleTime
            ?? (sprint.Issues.Count > 0 ? sprint.Issues.Average(i => i.GetCycleTime()) : 0d);

        var healthStatus = completionRate switch
        {
            >= 95 => "Excellent",
            >= 85 => "Healthy",
            >= 70 => "At Risk",
            _     => "Critical"
        };

        return new SprintComparisonEntry
        {
            SprintId         = sprint.Id,
            SprintName       = sprint.Name,
            StartDate        = sprint.StartDate,
            EndDate          = sprint.EndDate,
            DurationDays     = duration,
            PlannedPoints    = planned,
            CompletedPoints  = completed,
            Velocity         = Math.Round(velocity, 2),
            CompletionRate   = Math.Round(completionRate, 2),
            AverageCycleTime = Math.Round(avgCycleTime, 2),
            TotalIssues      = sprint.GetTotalIssueCount(),
            CompletedIssues  = sprint.GetCompletedIssueCount(),
            OverdueIssues    = sprint.GetOverdueIssues().Count,
            DefectCount      = defectCount,
            HealthStatus     = healthStatus
        };
    }

    private static List<DeveloperVelocityTrend> BuildDeveloperTrends(List<Sprint> sprints)
    {
        var developerData = new Dictionary<string, List<(string SprintName, int Points)>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var sprint in sprints)
        {
            var pointsByDev = sprint.Issues
                .Where(i => !string.IsNullOrWhiteSpace(i.Assignee) && i.Status is "Done" or "Closed")
                .GroupBy(i => i.Assignee!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.StoryPoints ?? 0));

            foreach (var (dev, points) in pointsByDev)
            {
                if (!developerData.TryGetValue(dev, out var list))
                {
                    list = new List<(string, int)>();
                    developerData[dev] = list;
                }
                list.Add((sprint.Name, points));
            }
        }

        return developerData
            .Select(kv =>
            {
                var pointSeries = kv.Value.Select(v => (double)v.Points).ToList();
                var avg         = pointSeries.Count > 0 ? pointSeries.Average() : 0d;
                var trendPct    = CalculateTrendPercentage(pointSeries);

                return new DeveloperVelocityTrend
                {
                    DeveloperKey     = kv.Key,
                    DisplayName      = kv.Key,
                    SprintVelocities = kv.Value,
                    AverageVelocity  = Math.Round(avg, 2),
                    TrendDirection   = ClassifyTrend(trendPct),
                    TrendPercentage  = Math.Round(trendPct, 2)
                };
            })
            .OrderByDescending(t => t.AverageVelocity)
            .ToList();
    }

    private static SprintComparisonReport BuildReport(
        string projectKey,
        List<SprintComparisonEntry> entries,
        List<DeveloperVelocityTrend> trends)
    {
        var avg      = entries.Count > 0 ? entries.Average(e => e.Velocity) : 0d;
        var trendPct = CalculateTrendPercentage(entries.Select(e => e.Velocity).ToList());

        return new SprintComparisonReport
        {
            ProjectKey              = projectKey,
            GeneratedAt             = DateTime.UtcNow,
            SprintCount             = entries.Count,
            Sprints                 = entries,
            TeamVelocityTrends      = trends,
            AverageVelocity         = Math.Round(avg, 2),
            VelocityTrendPercentage = Math.Round(trendPct, 2),
            OverallTrend            = ClassifyTrend(trendPct),
            BestSprint              = entries.MaxBy(e => e.Velocity),
            WorstSprint             = entries.MinBy(e => e.Velocity)
        };
    }

    private static double CalculateTrendPercentage(List<double> values)
    {
        if (values.Count < 2) return 0d;
        var first = values[0];
        var last  = values[^1];
        if (first == 0) return last > 0 ? 100d : 0d;
        return ((last - first) / first) * 100;
    }

    private static string ClassifyTrend(double trendPercentage) => trendPercentage switch
    {
        >  5 => "Improving",
        < -5 => "Declining",
        _    => "Stable"
    };
}
