// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// Repository interface for metrics and historical data
/// </summary>
public interface IMetricsRepository
{
    Task<List<SprintMetric>> GetByProjectAsync(string projectKey);
    Task<List<SprintMetric>> GetBySprintAsync(int sprintId);
    Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId);
    Task<SprintMetric?> GetLatestForProjectAsync(string projectKey);
    Task SaveMetricAsync(SprintMetric metric);
    Task SaveBurndownAsync(BurndownSnapshot snapshot);
    Task SaveBurndownRangeAsync(List<BurndownSnapshot> snapshots);
}
