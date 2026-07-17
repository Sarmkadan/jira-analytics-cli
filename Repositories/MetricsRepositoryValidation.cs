// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// Validation helpers for MetricsRepository to ensure data integrity before operations
/// </summary>
public static class MetricsRepositoryValidation
{
    /// <summary>
    /// Validates all metrics in the repository
    /// </summary>
    /// <param name="value">The repository to validate</param>
    /// <returns>List of validation problems, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null</exception>
    public static IReadOnlyList<string> Validate(this MetricsRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate metrics data
        foreach (var projectMetrics in value.GetAllMetrics())
        {
            foreach (var metric in projectMetrics)
            {
                try
                {
                    metric.Validate();
                }
                catch (ArgumentException ex)
                {
                    problems.Add($"Invalid metric in project {metric.SprintName}: {ex.Message} (Sprint ID: {metric.SprintId}, EndDate: {metric.EndDate:yyyy-MM-dd})");
                }
            }
        }

        // Validate burndown data
        foreach (var burndownSnapshots in value.GetAllBurndownData())
        {
            foreach (var snapshot in burndownSnapshots)
            {
                try
                {
                    snapshot.Validate();
                }
                catch (ArgumentException ex)
                {
                    problems.Add($"Invalid burndown snapshot for sprint {snapshot.SprintId} at {snapshot.Timestamp:yyyy-MM-dd HH:mm}: {ex.Message}");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the repository contains valid data
    /// </summary>
    /// <param name="value">The repository to validate</param>
    /// <returns>True if repository is valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null</exception>
    public static bool IsValid(this MetricsRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        return problems.Count == 0;
    }

    /// <summary>
    /// Ensures the repository contains valid data, throwing if invalid
    /// </summary>
    /// <param name="value">The repository to validate</param>
    /// <exception cref="ArgumentException">Thrown when repository contains invalid data</exception>
    public static void EnsureValid(this MetricsRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Repository contains {problems.Count} validation problem(s):{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Gets all metrics from the repository for validation purposes
    /// </summary>
    /// <param name="repository">The repository to get metrics from</param>
    /// <returns>Collection of metric lists, one per project</returns>
    private static IEnumerable<IReadOnlyList<SprintMetric>> GetAllMetrics(this MetricsRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        // Use reflection to access the private _metrics field
        var metricsField = typeof(MetricsRepository).GetField("_metrics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return metricsField?.GetValue(repository) switch
        {
            ConcurrentDictionary<string, List<SprintMetric>> metricsDict when metricsDict.Count > 0 => metricsDict.Values,
            _ => Enumerable.Empty<IReadOnlyList<SprintMetric>>()
        };
    }

    /// <summary>
    /// Gets all burndown data from the repository for validation purposes
    /// </summary>
    /// <param name="repository">The repository to get burndown data from</param>
    /// <returns>Collection of burndown snapshot lists, one per sprint</returns>
    private static IEnumerable<IReadOnlyList<BurndownSnapshot>> GetAllBurndownData(this MetricsRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        // Use reflection to access the private _burndownData field
        var burndownField = typeof(MetricsRepository).GetField("_burndownData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return burndownField?.GetValue(repository) switch
        {
            ConcurrentDictionary<int, List<BurndownSnapshot>> burndownDict when burndownDict.Count > 0 => burndownDict.Values,
            _ => Enumerable.Empty<IReadOnlyList<BurndownSnapshot>>()
        };
    }
}