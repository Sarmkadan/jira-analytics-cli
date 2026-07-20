// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Service for exporting metrics to CSV format
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Exports sprint metrics to a CSV file
    /// </summary>
    /// <param name="metrics">Collection of sprint metrics to export</param>
    /// <param name="path">Output file path</param>
    /// <returns>Task representing the async operation</returns>
    Task ExportSprintMetrics(IEnumerable<SprintMetric> metrics, string path);

    /// <summary>
    /// Exports team metrics to a CSV file
    /// </summary>
    /// <param name="metrics">Collection of team metrics as key-value pairs</param>
    /// <param name="path">Output file path</param>
    /// <returns>Task representing the async operation</returns>
    Task ExportTeamMetrics(IEnumerable<KeyValuePair<string, int>> metrics, string path);
}
