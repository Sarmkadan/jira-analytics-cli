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
    /// <returns>Task representing the async operation</returns>
    public async Task ExportSprintMetrics(IEnumerable<SprintMetric> metrics, string path)
    {
        _logger.LogInformation("Exporting {Count} sprint metrics to CSV at {Path}", metrics.Count(), path);

        try
        {
            var data = metrics.ToList();
            if (!data.Any())
            {
                _logger.LogWarning("No metrics provided for export");
                await File.WriteAllTextAsync(path, string.Empty);
                return;
            }

            var sb = new StringBuilder();

            // Write CSV headers
            sb.AppendLine("SprintId,SprintName,StartDate,EndDate,PlannedStoryPoints,CompletedStoryPoints,CommittedStoryPoints,CompletedIssueCount,TotalIssueCount,DefectsCount,AverageCycleTime,OverdueIssueCount,TeamSize,ScopeChangeCount,Velocity,CompletionRate%,CommitmentAccuracy%,QualityScore,ProductivityPerTeamMember,DailyBurndownRate,HealthStatus");

            // Write data rows
            foreach (var metric in data)
            {
                var values = new object[]
                {
                    metric.SprintId,
                    EscapeCsvValue(metric.SprintName),
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

                sb.AppendLine(string.Join(",", values.Select(v => EscapeCsvValue(v?.ToString() ?? string.Empty))));
            }

            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
            _logger.LogInformation("Successfully exported {Count} sprint metrics to {Path}", data.Count, path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sprint metrics to CSV");
            throw;
        }
    }

    /// <summary>
    /// Exports team metrics to a CSV file
    /// </summary>
    /// <param name="metrics">Collection of team metrics as key-value pairs</param>
    /// <param name="path">Output file path</param>
    /// <returns>Task representing the async operation</returns>
    public async Task ExportTeamMetrics(IEnumerable<KeyValuePair<string, int>> metrics, string path)
    {
        _logger.LogInformation("Exporting team metrics to CSV at {Path}", path);

        try
        {
            var data = metrics.ToList();
            if (!data.Any())
            {
                _logger.LogWarning("No team metrics provided for export");
                await File.WriteAllTextAsync(path, string.Empty);
                return;
            }

            var sb = new StringBuilder();

            // Write CSV headers
            sb.AppendLine("Developer,AssignedIssues");

            // Write data rows
            foreach (var kvp in data)
            {
                var values = new object[]
                {
                    EscapeCsvValue(kvp.Key),
                    kvp.Value
                };

                sb.AppendLine(string.Join(",", values.Select(v => EscapeCsvValue(v?.ToString() ?? string.Empty))));
            }

            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
            _logger.LogInformation("Successfully exported {Count} team metrics to {Path}", data.Count, path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting team metrics to CSV");
            throw;
        }
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
