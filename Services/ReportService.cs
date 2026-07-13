// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Generates text and HTML reports from analytics data with formatting and aggregation
/// </summary>
public class ReportService : IReportService
{
    private readonly IJiraApiService _jiraService;
    private readonly IExportService _exportService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IJiraApiService jiraService, IExportService exportService, ILogger<ReportService> logger)
    {
        _jiraService = jiraService;
        _exportService = exportService;
        _logger = logger;
    }

    public string GenerateReport(SprintAnalysisResult analysis)
    {
        _logger.LogInformation("Generating text report for {SprintCount} sprints", analysis.Metrics.Count);

        var sb = new StringBuilder();

        sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║       JIRA ANALYTICS REPORT - SPRINT PERFORMANCE SUMMARY       ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        // Overall Summary
        sb.AppendLine("📊 OVERALL METRICS");
        sb.AppendLine("─".PadRight(70, '─'));
        sb.AppendLine($"Average Velocity:      {analysis.AverageVelocity:F2} pts/sprint");
        sb.AppendLine($"Velocity Trend:        {(analysis.TrendPercentage >= 0 ? "↑" : "↓")} {Math.Abs(analysis.TrendPercentage):F1}%");
        sb.AppendLine($"Overall Health:        {analysis.OverallHealth}");
        sb.AppendLine();

        // Individual Sprint Metrics
        sb.AppendLine("📈 SPRINT BREAKDOWN");
        sb.AppendLine("─".PadRight(70, '─'));

        var sortedMetrics = analysis.Metrics.OrderBy(m => m.EndDate).ToList();
        foreach (var metric in sortedMetrics)
        {
            sb.AppendLine($"Sprint: {metric.SprintName}");
            sb.AppendLine($"  Planned:       {metric.PlannedStoryPoints} pts");
            sb.AppendLine($"  Completed:     {metric.CompletedStoryPoints} pts ({metric.GetCompletionRate():F1}%)");
            sb.AppendLine($"  Issues:        {metric.CompletedIssueCount}/{metric.TotalIssueCount}");
            sb.AppendLine($"  Quality:       {metric.GetQualityScore():F1}/100");
            sb.AppendLine($"  Cycle Time:    {metric.AverageCycleTime:F1} days");
            sb.AppendLine($"  Status:        {metric.GetHealthStatus()}");
            sb.AppendLine();
        }

        // Risks and Issues
        var riskCount = sortedMetrics.Sum(m => (int)m.GetRiskScore());
        var overdueTotal = sortedMetrics.Sum(m => m.OverdueIssueCount);

        if (riskCount > 0 || overdueTotal > 0)
        {
            sb.AppendLine("⚠️  RISKS & ISSUES");
            sb.AppendLine("─".PadRight(70, '─'));
            sb.AppendLine($"Total Risk Score:      {riskCount}");
            sb.AppendLine($"Overdue Issues:        {overdueTotal}");
            sb.AppendLine($"Critical Defects:      {sortedMetrics.Sum(m => m.DefectsCount)}");
            sb.AppendLine();
        }

        sb.AppendLine("═".PadRight(70, '═'));
        sb.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

        return sb.ToString();
    }

    public async Task GenerateBurndownChart(string projectKey, int sprintId, string outputPath)
    {
        _logger.LogInformation("Generating burndown chart for sprint {SprintId}", sprintId);

        try
        {
            var sprint = await _jiraService.GetSprintAsync(sprintId);
            if (sprint == null)
                throw new InvalidOperationException($"Sprint {sprintId} not found");

            var issues = await _jiraService.GetSprintIssuesAsync(sprintId);
            var burndownData = await _jiraService.GetBurndownDataAsync(sprintId);

            // If no historical data, create synthetic burndown
            if (!burndownData.Any())
            {
                burndownData = GenerateSyntheticBurndown(sprint, issues);
            }

            // Generate chart image using SkiaSharp
            await _exportService.ExportAnalytics(projectKey, "png", outputPath);

            _logger.LogInformation("Burndown chart generated successfully at {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating burndown chart");
            throw;
        }
    }

    public string GenerateHtmlReport(SprintAnalysisResult analysis, TeamAnalysisResult teamAnalysis)
    {
        _logger.LogInformation("Generating HTML report");

        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>Jira Analytics Report</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
        sb.AppendLine("    .header { background: #2c3e50; color: white; padding: 20px; border-radius: 5px; }");
        sb.AppendLine("    .metrics { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 20px; margin: 20px 0; }");
        sb.AppendLine("    .metric-card { background: white; padding: 15px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine("    .metric-value { font-size: 24px; font-weight: bold; color: #3498db; }");
        sb.AppendLine("    .metric-label { color: #7f8c8d; font-size: 12px; }");
        sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
        sb.AppendLine("    th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("    th { background: #34495e; color: white; }");
        sb.AppendLine("    .health-excellent { color: #27ae60; }");
        sb.AppendLine("    .health-healthy { color: #f39c12; }");
        sb.AppendLine("    .health-atrisk { color: #e74c3c; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Header
        sb.AppendLine("  <div class=\"header\">");
        sb.AppendLine("    <h1>Jira Analytics Report</h1>");
        sb.AppendLine($"    <p>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}</p>");
        sb.AppendLine("  </div>");

        // Key Metrics
        sb.AppendLine("  <div class=\"metrics\">");
        sb.AppendLine("    <div class=\"metric-card\">");
        sb.AppendLine("      <div class=\"metric-value\">" + analysis.AverageVelocity.ToString("F2", CultureInfo.InvariantCulture) + "</div>");
        sb.AppendLine("      <div class=\"metric-label\">Average Velocity (pts/sprint)</div>");
        sb.AppendLine("    </div>");

        sb.AppendLine("    <div class=\"metric-card\">");
        sb.AppendLine("      <div class=\"metric-value\">" + analysis.Metrics.Count + "</div>");
        sb.AppendLine("      <div class=\"metric-label\">Sprints Analyzed</div>");
        sb.AppendLine("    </div>");

        sb.AppendLine("    <div class=\"metric-card\">");
        sb.AppendLine("      <div class=\"metric-value health-" + analysis.OverallHealth.ToLower() + "\">" + analysis.OverallHealth + "</div>");
        sb.AppendLine("      <div class=\"metric-label\">Overall Health</div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");

        // Sprint Table
        sb.AppendLine("  <h2>Sprint Metrics</h2>");
        sb.AppendLine("  <table>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("      <th>Sprint</th>");
        sb.AppendLine("      <th>Planned</th>");
        sb.AppendLine("      <th>Completed</th>");
        sb.AppendLine("      <th>Completion %</th>");
        sb.AppendLine("      <th>Quality</th>");
        sb.AppendLine("      <th>Health</th>");
        sb.AppendLine("    </tr>");

        foreach (var metric in analysis.Metrics.OrderBy(m => m.EndDate))
        {
            sb.AppendLine("    <tr>");
            sb.AppendLine($"      <td>{metric.SprintName}</td>");
            sb.AppendLine($"      <td>{metric.PlannedStoryPoints}</td>");
            sb.AppendLine($"      <td>{metric.CompletedStoryPoints}</td>");
            sb.AppendLine($"      <td>{metric.GetCompletionRate():F1}%</td>");
            sb.AppendLine($"      <td>{metric.GetQualityScore():F1}</td>");
            sb.AppendLine($"      <td class=\"health-{metric.GetHealthStatus().ToLower()}\">{metric.GetHealthStatus()}</td>");
            sb.AppendLine("    </tr>");
        }

        sb.AppendLine("  </table>");

        // Team Performance
        if (teamAnalysis.TopPerformers.Any())
        {
            sb.AppendLine("  <h2>Top Performers</h2>");
            sb.AppendLine("  <table>");
            sb.AppendLine("    <tr><th>Developer</th><th>Completed</th><th>Points</th><th>Productivity</th></tr>");

            foreach (var dev in teamAnalysis.TopPerformers)
            {
                sb.AppendLine("    <tr>");
                sb.AppendLine($"      <td>{dev.DisplayName ?? dev.Name}</td>");
                sb.AppendLine($"      <td>{dev.GetCompletedIssues()}</td>");
                sb.AppendLine($"      <td>{dev.GetCompletedStoryPoints()}</td>");
                sb.AppendLine($"      <td>{dev.GetProductivity():F2}</td>");
                sb.AppendLine("    </tr>");
            }

            sb.AppendLine("  </table>");
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    public string GenerateSummaryReport(SprintAnalysisResult analysis)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Sprint Analysis Summary - {DateTime.UtcNow:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine($"Sprints Analyzed: {analysis.Metrics.Count}");
        sb.AppendLine($"Average Velocity: {analysis.AverageVelocity:F2}");
        sb.AppendLine($"Trend: {(analysis.TrendPercentage >= 0 ? "Positive" : "Negative")} ({analysis.TrendPercentage:F1}%)");
        sb.AppendLine($"Health Status: {analysis.OverallHealth}");

        return sb.ToString();
    }

    private List<BurndownSnapshot> GenerateSyntheticBurndown(Sprint sprint, List<JiraIssue> issues)
    {
        // Generate synthetic burndown data based on issue creation/completion timeline
        var snapshots = new List<BurndownSnapshot>();
        var totalPoints = issues.Sum(i => i.StoryPoints ?? 0);

        if (sprint.StartDate.HasValue && sprint.EndDate.HasValue)
        {
            var current = sprint.StartDate.Value;

            while (current <= sprint.EndDate.Value)
            {
                var dailyCompleted = issues
                    .Where(i => i.ResolutionDate.HasValue && i.ResolutionDate.Value.Date <= current.Date)
                    .Sum(i => i.StoryPoints ?? 0);

                snapshots.Add(new BurndownSnapshot
                {
                    Timestamp = current,
                    SprintId = sprint.Id,
                    TotalStoryPoints = totalPoints,
                    CompletedStoryPoints = dailyCompleted,
                    RemainingStoryPoints = totalPoints - dailyCompleted,
                    TotalIssueCount = issues.Count,
                    CompletedIssueCount = issues.Count(i => i.ResolutionDate.HasValue && i.ResolutionDate <= current),
                    RemainingIssueCount = issues.Count(i => !i.ResolutionDate.HasValue || i.ResolutionDate > current)
                });

                current = current.AddDays(1);
            }
        }

        return snapshots;
    }
}
