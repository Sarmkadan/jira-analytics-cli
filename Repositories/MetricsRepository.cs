// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// In-memory repository for sprint metrics and burndown data with historical tracking
/// </summary>
public class MetricsRepository : IMetricsRepository
{
    private readonly ConcurrentDictionary<string, List<SprintMetric>> _metrics = new();
    private readonly ConcurrentDictionary<int, List<BurndownSnapshot>> _burndownData = new();
    private readonly ILogger<MetricsRepository> _logger;

    public MetricsRepository(ILogger<MetricsRepository> logger)
    {
        this._logger = logger;
    }

    public async Task<List<SprintMetric>> GetByProjectAsync(string projectKey)
    {
        try
        {
            this._logger.LogDebug("Retrieving metrics for project {ProjectKey}", projectKey);

            if (this._metrics.TryGetValue(projectKey, out var projectMetrics))
            {
                return projectMetrics.OrderByDescending(m => m.EndDate).ToList();
            }

            return new List<SprintMetric>();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving metrics for project {ProjectKey}", projectKey);
            return new List<SprintMetric>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<SprintMetric>> GetBySprintAsync(int sprintId)
    {
        try
        {
            this._logger.LogDebug("Retrieving metrics for sprint {SprintId}", sprintId);

            var allMetrics = this._metrics.Values.SelectMany(m => m).ToList();
            return allMetrics.Where(m => m.SprintId == sprintId).ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving metrics for sprint {SprintId}", sprintId);
            return new List<SprintMetric>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId)
    {
        try
        {
            this._logger.LogDebug("Retrieving burndown data for sprint {SprintId}", sprintId);

            if (this._burndownData.TryGetValue(sprintId, out var burndownSnapshots))
            {
                return burndownSnapshots.OrderBy(b => b.Timestamp).ToList();
            }

            return new List<BurndownSnapshot>();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving burndown data for sprint {SprintId}", sprintId);
            return new List<BurndownSnapshot>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<SprintMetric?> GetLatestForProjectAsync(string projectKey)
    {
        try
        {
            this._logger.LogDebug("Retrieving latest metrics for project {ProjectKey}", projectKey);

            if (this._metrics.TryGetValue(projectKey, out var projectMetrics))
            {
                return projectMetrics.OrderByDescending(m => m.EndDate).FirstOrDefault();
            }

            return null;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving latest metrics for project {ProjectKey}", projectKey);
            return null;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task SaveMetricAsync(SprintMetric metric)
    {
        try
        {
            if (metric == null)
            {
                throw new ArgumentNullException(nameof(metric));
            }

            metric.Validate();

            this._metrics.AddOrUpdate(
                metric.SprintName,
                new List<SprintMetric> { metric },
                (key, existing) =>
                {
                    existing.Add(metric);
                    return existing;
                });

            this._logger.LogDebug("Saved metric for sprint {SprintId}: {SprintName}", metric.SprintId, metric.SprintName);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error saving metric");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task SaveBurndownAsync(BurndownSnapshot snapshot)
    {
        try
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            snapshot.Validate();

            this._burndownData.AddOrUpdate(
                snapshot.SprintId,
                new List<BurndownSnapshot> { snapshot },
                (id, existing) =>
                {
                    existing.Add(snapshot);
                    return existing;
                });

            this._logger.LogDebug("Saved burndown snapshot for sprint {SprintId}", snapshot.SprintId);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error saving burndown snapshot");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task SaveBurndownRangeAsync(List<BurndownSnapshot> snapshots)
    {
        try
        {
            if (snapshots == null || !snapshots.Any())
            {
                throw new ArgumentException("Snapshots collection cannot be null or empty");
            }

            var groupedBySprintId = snapshots.GroupBy(s => s.SprintId);

            foreach (var group in groupedBySprintId)
            {
                foreach (var snapshot in group)
                {
                    snapshot.Validate();
                }

                this._burndownData.AddOrUpdate(
                    group.Key,
                    group.ToList(),
                    (id, existing) =>
                    {
                        existing.AddRange(group);
                        return existing;
                    });
            }

            this._logger.LogInformation("Saved {SnapshotCount} burndown snapshots to repository", snapshots.Count);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error saving burndown snapshots");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public int GetMetricCount()
    {
        return this._metrics.Values.Sum(m => m.Count);
    }

    public int GetBurndownCount()
    {
        return this._burndownData.Values.Sum(b => b.Count);
    }

    public void Clear()
    {
        this._metrics.Clear();
        this._burndownData.Clear();
        this._logger.LogInformation("Metrics repository cleared");
    }
}
