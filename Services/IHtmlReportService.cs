// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Generates self-contained, styled HTML reports from sprint and team analytics data.
/// </summary>
public interface IHtmlReportService
{
    /// <summary>
    /// Generates a complete HTML report for the given project and writes it to <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="projectKey">The Jira project key to analyse.</param>
    /// <param name="sprintCount">Number of recent closed sprints to include.</param>
    /// <param name="outputPath">File path where the HTML report will be written.</param>
    Task GenerateReportAsync(string projectKey, int sprintCount, string outputPath);

    /// <summary>
    /// Builds a self-contained HTML report string from pre-computed analytics results.
    /// </summary>
    /// <param name="projectKey">The Jira project key (used in the report title).</param>
    /// <param name="sprintAnalysis">Sprint velocity and health metrics.</param>
    /// <param name="teamAnalysis">Team workload and performance data.</param>
    /// <returns>A complete, standalone HTML document string.</returns>
    string BuildHtml(string projectKey, SprintAnalysisResult sprintAnalysis, TeamAnalysisResult teamAnalysis);
}
