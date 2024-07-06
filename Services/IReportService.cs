// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Interface for generating reports from analytics data
/// </summary>
public interface IReportService
{
    string GenerateReport(SprintAnalysisResult analysis);
    Task GenerateBurndownChart(string projectKey, int sprintId, string outputPath);
    string GenerateHtmlReport(SprintAnalysisResult analysis, TeamAnalysisResult teamAnalysis);
    string GenerateSummaryReport(SprintAnalysisResult analysis);
}
