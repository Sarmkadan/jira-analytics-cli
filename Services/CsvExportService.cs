// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using System.Text;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Service for exporting metrics to CSV format with proper escaping and formatting
/// </summary>
public class CsvExportService : ICsvExportService
{
    private readonly ILogger<CsvExportService> _logger;

    public CsvExportService(ILogger<CsvExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Exports sprint metrics to a CSV file with proper escaping and formatting
    /// </summary>
    /// <param name="metrics">Collection of sprint metrics to export</param>
    /// <param name="path">Output file path</param>
    /// <param name="bufferSize">Optional buffer size for StreamWriter (default: 4096)</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when metrics or path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    public async Task ExportSprintMetrics(IEnumerable<SprintMetric> metrics, string path, int bufferSize = 4096)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger.LogInformation("Exporting sprint metrics to CSV at {Path}", path);

        try
        {
            await using var writer = new StreamWriter(path, append: false, encoding: Encoding.UTF8, bufferSize: bufferSize);
            await ExportSprintMetricsAsync(metrics, writer);
            _logger.LogInformation("Successfully exported sprint metrics to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sprint metrics to CSV");
            throw;
        }
    }

    /// <summary>
    /// Exports sprint metrics to a CSV file using an existing StreamWriter for streaming
    /// </summary>
    /// <param name="metrics">Collection of sprint metrics to export</param>
    /// <param name="writer">StreamWriter to write to</param>
    /// <returns>Task representing the async operation</returns>
    private async Task ExportSprintMetricsAsync(IEnumerable<SprintMetric> metrics, StreamWriter writer)
    {
        // Write CSV headers
        await writer.WriteLineAsync("SprintId,SprintName,StartDate,EndDate,PlannedStoryPoints,CompletedStoryPoints,CommittedStoryPoints,CompletedIssueCount,TotalIssueCount,DefectsCount,AverageCycleTime,OverdueIssueCount,TeamSize,ScopeChangeCount,Velocity,CompletionRate%,CommitmentAccuracy%,QualityScore,ProductivityPerTeamMember,DailyBurndownRate,HealthStatus");

        var enumerator = metrics.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            // Empty dataset produces header only
            _logger.LogWarning("No metrics provided for export");
            return;
        }

        // Write data rows one by one to avoid loading everything into memory
        do
        {
            var metric = enumerator.Current;
            var values = new object[]
            {
                metric.SprintId,
                metric.SprintName,
                metric.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                metric.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                metric.PlannedStoryPoints,
                metric.CompletedStoryPoints,
                metric.CommittedStoryPoints,
                metric.CompletedIssueCount,
                metric.TotalIssueCount,
                metric.DefectsCount,
                FormatCsvCell(metric.AverageCycleTime),
                metric.OverdueIssueCount,
                metric.TeamSize,
                metric.ScopeChangeCount,
                FormatCsvCell(metric.GetVelocity()),
                FormatCsvCell(metric.GetCompletionRate()),
                FormatCsvCell(metric.GetCommitmentAccuracy()),
                FormatCsvCell(metric.GetQualityScore()),
                FormatCsvCell(metric.GetProductivityPerTeamMember()),
                FormatCsvCell(metric.GetDailyBurndownRate()),
                metric.GetHealthStatus()
            };

            var line = string.Join(",", values.Select(v => EscapeCsvValue(v?.ToString() ?? string.Empty)));
            await writer.WriteLineAsync(line);
        }
        while (enumerator.MoveNext());
    }

    /// <summary>
    /// Exports team metrics to a CSV file
    /// </summary>
    /// <param name="metrics">Collection of team metrics as key-value pairs</param>
    /// <param name="path">Output file path</param>
    /// <param name="bufferSize">Optional buffer size for StreamWriter (default: 4096)</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when metrics or path is null</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace</exception>
    public async Task ExportTeamMetrics(IEnumerable<KeyValuePair<string, int>> metrics, string path, int bufferSize = 4096)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger.LogInformation("Exporting team metrics to CSV at {Path}", path);

        try
        {
            await using var writer = new StreamWriter(path, append: false, encoding: Encoding.UTF8, bufferSize: bufferSize);
            await ExportTeamMetricsAsync(metrics, writer);
            _logger.LogInformation("Successfully exported team metrics to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting team metrics to CSV");
            throw;
        }
    }

    /// <summary>
    /// Exports team metrics to a CSV file using an existing StreamWriter for streaming
    /// </summary>
    /// <param name="metrics">Collection of team metrics as key-value pairs</param>
    /// <param name="writer">StreamWriter to write to</param>
    /// <returns>Task representing the async operation</returns>
    private async Task ExportTeamMetricsAsync(IEnumerable<KeyValuePair<string, int>> metrics, StreamWriter writer)
    {
        // Write CSV headers
        await writer.WriteLineAsync("Developer,AssignedIssues");

        var enumerator = metrics.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            // Empty dataset produces header only
            _logger.LogWarning("No team metrics provided for export");
            return;
        }

        // Write data rows one by one to avoid loading everything into memory
        do
        {
            var kvp = enumerator.Current;
            var values = new object[]
            {
                kvp.Key,
                kvp.Value
            };

            var line = string.Join(",", values.Select(v => EscapeCsvValue(v?.ToString() ?? string.Empty)));
            await writer.WriteLineAsync(line);
        }
        while (enumerator.MoveNext());
    }

    /// <summary>
    /// Formats a value for CSV output using invariant culture
    /// </summary>
    /// <param name="value">Value to format</param>
    /// <returns>Formatted string</returns>
    private static string FormatCsvCell(object? value)
    {
        return value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Escapes a value for CSV output, adding quotes if needed
    /// </summary>
    /// <param name="value">Value to escape</param>
    /// <returns>Escaped CSV value</returns>
    private string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
