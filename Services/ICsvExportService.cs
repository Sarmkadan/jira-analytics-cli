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
    /// <param name="bufferSize">Optional buffer size for StreamWriter (default: 4096)</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when metrics or path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    Task ExportSprintMetrics(IEnumerable<SprintMetric> metrics, string path, int bufferSize = 4096);

    /// <summary>
    /// Exports team metrics to a CSV file
    /// </summary>
    /// <param name="metrics">Collection of team metrics as key-value pairs</param>
    /// <param name="path">Output file path</param>
    /// <param name="bufferSize">Optional buffer size for StreamWriter (default: 4096)</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when metrics or path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    Task ExportTeamMetrics(IEnumerable<KeyValuePair<string, int>> metrics, string path, int bufferSize = 4096);
}
