// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Interface for exporting analytics to various formats (PNG, PDF, CSV, JSON)
/// </summary>
public interface IExportService
{
    Task ExportAnalytics(string projectKey, string format, string outputPath);
    Task ExportBurndownChart(int sprintId, string format, string outputPath);
    Task ExportTeamMetrics(string projectKey, string format, string outputPath);
    Task ExportAsJson(object data, string outputPath);
    Task ExportAsCsv(List<Dictionary<string, object>> data, string outputPath);
}
