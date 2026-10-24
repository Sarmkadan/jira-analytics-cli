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
    public string GenerateReport(SprintAnalysisResult analysis);
    public Task GenerateBurndownChart(string projectKey, int sprintId, string outputPath);
    public string GenerateHtmlReport(SprintAnalysisResult analysis, TeamAnalysisResult teamAnalysis);
    public string GenerateSummaryReport(SprintAnalysisResult analysis);
}
