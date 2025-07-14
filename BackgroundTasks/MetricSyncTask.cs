// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Caching;

namespace JiraAnalyticsCli.BackgroundTasks;

/// <summary>
/// Background task for synchronizing analytics metrics from Jira.
/// Refreshes cached metrics at regular intervals to keep data current.
/// </summary>
public class MetricSyncTask
{
    private readonly IAnalyticsService _analyticsService;
    private readonly CacheManager _cacheManager;
    private readonly ILogger<MetricSyncTask> _logger;
    private readonly string[] _projects;

    public MetricSyncTask(
        IAnalyticsService analyticsService,
        CacheManager cacheManager,
        ILogger<MetricSyncTask> logger,
        params string[] projects)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _projects = projects ?? Array.Empty<string>();
    }

    /// <summary>
    /// Executes metric synchronization for configured projects.
    /// Fetches latest metrics and updates cache entries.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting metric sync task for {ProjectCount} projects", _projects.Length);

        foreach (var project in _projects)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Metric sync cancelled");
                break;
            }

            try
            {
                _logger.LogDebug("Syncing metrics for project: {Project}", project);

                var analysis = await _analyticsService.AnalyzeSprints(project, 5);

                // Store in cache with 30-minute expiration
                var cachePolicy = CachePolicy.WithAbsoluteExpiration($"metrics_{project}", TimeSpan.FromMinutes(30));
                _cacheManager.Set("metrics", $"metrics_{project}", analysis, cachePolicy);

                _logger.LogInformation("Synced metrics for project: {Project}", project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing metrics for project: {Project}", project);
            }

            // Add delay between projects to avoid overwhelming Jira API
            if (_projects.Length > 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        _logger.LogInformation("Metric sync task completed");
    }
}
