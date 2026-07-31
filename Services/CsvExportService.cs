// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using System.IO;
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
    /// <exception cref="IOException">Thrown when the output path is outside the intended output root</exception>
    public async Task ExportSprintMetrics(IEnumerable<SprintMetric> metrics, string path, int bufferSize = 4096)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var outputPath = GetCanonicalizedPath(path);
        if (!IsPathUnderOutputRoot(outputPath))
        {
            throw new IOException("Output path is outside the intended output root");
        }

        _logger.LogInformation("Exporting sprint metrics to CSV at {Path}", outputPath);

        try
        {
            await using var writer = new StreamWriter(outputPath, append: false, encoding: Encoding.UTF8, bufferSize: bufferSize);
            await ExportSprintMetricsAsync(metrics, writer);
            _logger.LogInformation("Successfully exported sprint metrics to {Path}", outputPath);
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

        // Reusable StringBuilder with a reasonable initial capacity to avoid per‑field allocations
        var sb = new StringBuilder(256);

        do
        {
            var metric = enumerator.Current;
            sb.Clear();

            // Append each column, escaping / sanitising as required
            AppendEscaped(sb, metric.SprintId.ToString());
            sb.Append(',');

            AppendEscaped(sb, SanitizeCsvValue(metric.SprintName));
            sb.Append(',');

            AppendEscaped(sb, metric.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            sb.Append(',');

            AppendEscaped(sb, metric.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.PlannedStoryPoints));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.CompletedStoryPoints));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.CommittedStoryPoints));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.CompletedIssueCount));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.TotalIssueCount));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.DefectsCount));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.AverageCycleTime));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.OverdueIssueCount));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.TeamSize));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.ScopeChangeCount));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.GetVelocity()));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.GetCompletionRate()));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.GetCommitmentAccuracy()));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.GetQualityScore()));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.GetProductivityPerTeamMember()));
            sb.Append(',');

            AppendEscaped(sb, FormatCsvCell(metric.GetDailyBurndownRate()));
            sb.Append(',');

            AppendEscaped(sb, SanitizeCsvValue(metric.GetHealthStatus()));

            await writer.WriteLineAsync(sb.ToString());
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
    /// <exception cref="IOException">Thrown when the output path is outside the intended output root</exception>
    public async Task ExportTeamMetrics(IEnumerable<KeyValuePair<string, int>> metrics, string path, int bufferSize = 4096)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var outputPath = GetCanonicalizedPath(path);
        if (!IsPathUnderOutputRoot(outputPath))
        {
            throw new IOException("Output path is outside the intended output root");
        }

        _logger.LogInformation("Exporting team metrics to CSV at {Path}", outputPath);

        try
        {
            await using var writer = new StreamWriter(outputPath, append: false, encoding: Encoding.UTF8, bufferSize: bufferSize);
            await ExportTeamMetricsAsync(metrics, writer);
            _logger.LogInformation("Successfully exported team metrics to {Path}", outputPath);
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

        var sb = new StringBuilder(128);

        do
        {
            var kvp = enumerator.Current;
            sb.Clear();

            AppendEscaped(sb, SanitizeCsvValue(kvp.Key));
            sb.Append(',');
            AppendEscaped(sb, kvp.Value.ToString());

            await writer.WriteLineAsync(sb.ToString());
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

    /// <summary>
    /// Sanitizes a value for CSV output, prefixing with a leading ' if necessary
    /// </summary>
    /// <param name="value">Value to sanitize</param>
    /// <returns>Sanitized CSV value</returns>
    private string SanitizeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.StartsWith("=") || value.StartsWith("+") || value.StartsWith("-") || value.StartsWith("@") || value.StartsWith("\t") || value.StartsWith("\r"))
        {
            return "'" + value;
        }

        return value;
    }

    /// <summary>
    /// Appends an escaped CSV field to the supplied StringBuilder.
    /// This method centralises the escaping logic used by the builder‑based row construction.
    /// </summary>
    private void AppendEscaped(StringBuilder sb, string? raw)
    {
        sb.Append(EscapeCsvValue(raw));
    }

    /// <summary>
    /// Gets the canonicalized path for the given path
    /// </summary>
    /// <param name="path">Path to canonicalize</param>
    /// <returns>Canonicalized path</returns>
    private string GetCanonicalizedPath(string path)
    {
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Checks if the given path is under the intended output root
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>True if the path is under the intended output root, false otherwise</returns>
    private bool IsPathUnderOutputRoot(string path)
    {
        // For this example, assume the output root is the current working directory
        var outputRoot = Directory.GetCurrentDirectory();
        return path.StartsWith(outputRoot, StringComparison.OrdinalIgnoreCase);
    }
}
