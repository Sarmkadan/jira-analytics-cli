// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;
namespace JiraAnalyticsCli.Services;

/// <summary>
/// Interface for exporting analytics to various formats (PNG, PDF, CSV, JSON)
/// </summary>
public interface IExportService
{
    public Task ExportAnalytics(string projectKey, string format, string outputPath);
    public Task ExportBurndownChart(int sprintId, string format, string outputPath);
    public Task ExportTeamMetrics(string projectKey, string format, string outputPath);
    public Task ExportAsJson(object data, string outputPath);
    public Task ExportAsCsv(List<Dictionary<string, object>> data, string outputPath);
    public Task ExportSprintMetricsCsv(IEnumerable<SprintMetric> metrics, string path);
    public Task ExportTeamMetricsCsv(IEnumerable<KeyValuePair<string, int>> metrics, string path);
}
