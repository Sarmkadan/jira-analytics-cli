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
    /// <summary>Retrieves metrics by project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>A list of sprint metrics.</returns>
    Task<List<SprintMetric>> GetByProjectAsync(string projectKey);
    
    /// <summary>Retrieves metrics by sprint.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <returns>A list of sprint metrics.</returns>
    Task<List<SprintMetric>> GetBySprintAsync(int sprintId);
    
    /// <summary>Retrieves burndown data by sprint.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <returns>A list of burndown snapshots.</returns>
    Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId);
    
    /// <summary>Retrieves the latest metrics for a project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>The latest sprint metrics or null.</returns>
    Task<SprintMetric?> GetLatestForProjectAsync(string projectKey);
    
    /// <summary>Saves a sprint metric.</summary>
    /// <param name="metric">The metric to save.</param>
    Task SaveMetricAsync(SprintMetric metric);
    
    /// <summary>Saves a burndown snapshot.</summary>
    /// <param name="snapshot">The snapshot to save.</param>
    Task SaveBurndownAsync(BurndownSnapshot snapshot);
    
    /// <summary>Saves a range of burndown snapshots.</summary>
    /// <param name="snapshots">The list of snapshots to save.</param>
    Task SaveBurndownRangeAsync(List<BurndownSnapshot> snapshots);
}
