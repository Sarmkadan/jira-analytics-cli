// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Interface for generating Markdown reports from analytics data
/// </summary>
public interface IMarkdownReportService
{
    /// <summary>
    /// Generates a complete Markdown report for the given project and writes it to <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="projectKey">The Jira project key to analyse.</param>
    /// <param name="sprintCount">Number of recent closed sprints to include.</param>
    /// <param name="outputPath">File path where the Markdown report will be written.</param>
    Task GenerateReportAsync(string projectKey, int sprintCount, string outputPath);

    /// <summary>
    /// Builds a Markdown report string from pre-computed analytics results.
    /// </summary>
    /// <param name="projectKey">The Jira project key (used in the report title).</param>
    /// <param name="sprintAnalysis">Sprint velocity and health metrics.</param>
    /// <param name="teamAnalysis">Team workload and performance data.</param>
    /// <returns>A complete Markdown document string.</returns>
    string BuildMarkdown(string projectKey, SprintAnalysisResult sprintAnalysis, TeamAnalysisResult teamAnalysis);

    /// <summary>
    /// Generates a Markdown report for cycle time metrics.
    /// </summary>
    /// <param name="projectKey">The Jira project key.</param>
    /// <param name="cycleTimeResult">Cycle time analysis results.</param>
    /// <param name="outputPath">File path where the Markdown report will be written.</param>
    Task GenerateCycleTimeReportAsync(string projectKey, CycleTimeResult cycleTimeResult, string outputPath);
}
