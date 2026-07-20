// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Generates Markdown reports from sprint and team analytics data.
/// The output is a plain text Markdown file that can be viewed in any Markdown viewer.
/// </summary>
public class MarkdownReportService : IMarkdownReportService
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<MarkdownReportService> _logger;

    /// <summary>Initialises a new instance of <see cref="MarkdownReportService"/>.</summary>
    public MarkdownReportService(IAnalyticsService analyticsService, ILogger<MarkdownReportService> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task GenerateReportAsync(string projectKey, int sprintCount, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));

        if (sprintCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(sprintCount), "Sprint count must be positive.");

        _logger.LogInformation(
            "Generating Markdown report for project {ProjectKey} ({SprintCount} sprints) → {OutputPath}",
            projectKey, sprintCount, outputPath);

        try
        {
            var sprintTask = _analyticsService.AnalyzeSprints(projectKey, sprintCount);
            var teamTask = _analyticsService.AnalyzeTeam(projectKey);

            await Task.WhenAll(sprintTask, teamTask);

            var markdown = BuildMarkdown(projectKey, sprintTask.Result, teamTask.Result);

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(outputPath, markdown, Encoding.UTF8);

            _logger.LogInformation("Markdown report written to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Markdown report for project {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public string BuildMarkdown(string projectKey, SprintAnalysisResult sprintAnalysis, TeamAnalysisResult teamAnalysis)
    {
        ArgumentNullException.ThrowIfNull(sprintAnalysis);
        ArgumentNullException.ThrowIfNull(teamAnalysis);

        var sb = new StringBuilder();
        var generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        var trendSymbol = sprintAnalysis.TrendPercentage >= 0 ? "↑" : "↓";
        var healthClass = sprintAnalysis.OverallHealth.ToLower().Replace(" ", "-");

        // Header
        sb.AppendLine("# Jira Analytics Report");
        sb.AppendLine($"**Project:** {projectKey}");
        sb.AppendLine($"**Generated:** {generatedAt}");
        sb.AppendLine($"**Sprints Analyzed:** {sprintAnalysis.Metrics.Count}");
        sb.AppendLine();

        // Key Metrics
        sb.AppendLine("## 📊 Key Metrics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Average Velocity | {sprintAnalysis.AverageVelocity:F2} pts/sprint |");
        sb.AppendLine($"| Velocity Trend | {trendSymbol} {Math.Abs(sprintAnalysis.TrendPercentage):F1}% |");
        sb.AppendLine($"| Overall Health | **{sprintAnalysis.OverallHealth}** |");
        sb.AppendLine($"| Total Issues Delivered | {sprintAnalysis.Metrics.Sum(m => m.CompletedIssueCount)} |");
        sb.AppendLine($"| Total Bugs Found | {sprintAnalysis.Metrics.Sum(m => m.DefectsCount)} |");
        sb.AppendLine($"| Total Overdue Issues | {sprintAnalysis.Metrics.Sum(m => m.OverdueIssueCount)} |");
        sb.AppendLine();

        // Sprint Breakdown
        sb.AppendLine("## 📈 Sprint Breakdown");
        sb.AppendLine();
        sb.AppendLine("| Sprint | Period | Planned | Completed | Completion % | Cycle Time | Health |");
        sb.AppendLine("|--------|--------|---------|-----------|--------------|------------|--------|");

        foreach (var m in sprintAnalysis.Metrics.OrderBy(x => x.EndDate))
        {
            var rate = m.GetCompletionRate();
            sb.AppendLine($"| **{m.SprintName}** | {m.StartDate:yyyy-MM-dd} – {m.EndDate:yyyy-MM-dd} | " +
                         $"{m.PlannedStoryPoints} | {m.CompletedStoryPoints} | {rate:F1}% | " +
                         $"{m.AverageCycleTime:F1} d | **{m.GetHealthStatus()}** |");
        }
        sb.AppendLine();

        // Team Workload Distribution
        if (teamAnalysis.WorkloadDistribution.Any())
        {
            sb.AppendLine("## 👥 Team Workload Distribution");
            sb.AppendLine();
            sb.AppendLine("| Developer | Assigned Issues | Load |");
            sb.AppendLine("|-----------|----------------|------|");

            var maxLoad = teamAnalysis.WorkloadDistribution.Values.DefaultIfEmpty(1).Max();
            if (maxLoad == 0) maxLoad = 1;

            foreach (var (dev, count) in teamAnalysis.WorkloadDistribution.OrderByDescending(kv => kv.Value))
            {
                var pct = (count / (double)maxLoad) * 100;
                sb.AppendLine($"| {dev} | {count} | {pct:F0}% |");
            }
            sb.AppendLine();
        }

        // Top Performers
        if (teamAnalysis.TopPerformers.Any())
        {
            sb.AppendLine("## 🏆 Top Performers");
            sb.AppendLine();
            sb.AppendLine("| Developer | Completed Issues | Story Points | Productivity Score |");
            sb.AppendLine("|-----------|----------------|--------------|-------------------|");

            foreach (var dev in teamAnalysis.TopPerformers)
            {
                sb.AppendLine($"| {dev.DisplayName ?? dev.Name} | {dev.GetCompletedIssues()} | " +
                             $"{dev.GetCompletedStoryPoints()} | {dev.GetProductivity():F2} |");
            }
            sb.AppendLine();
        }

        // Footer
        sb.AppendLine("---");
        sb.AppendLine($"*Jira Analytics CLI - Report generated {generatedAt}*");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public async Task GenerateCycleTimeReportAsync(string projectKey, CycleTimeResult cycleTimeResult, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));
        ArgumentNullException.ThrowIfNull(cycleTimeResult);

        _logger.LogInformation(
            "Generating Markdown cycle time report for project {ProjectKey} → {OutputPath}",
            projectKey, outputPath);

        try
        {
            var markdown = BuildCycleTimeMarkdown(projectKey, cycleTimeResult);

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(outputPath, markdown, Encoding.UTF8);

            _logger.LogInformation("Cycle time Markdown report written to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cycle time Markdown report for project {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <summary>
    /// Builds a Markdown report string for cycle time metrics.
    /// </summary>
    private string BuildCycleTimeMarkdown(string projectKey, CycleTimeResult cycleTimeResult)
    {
        var sb = new StringBuilder();
        var generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");

        // Header
        sb.AppendLine("# Cycle Time Analysis Report");
        sb.AppendLine($"**Project:** {projectKey}");
        sb.AppendLine($"**Generated:** {generatedAt}");
        sb.AppendLine();

        // Summary Metrics
        sb.AppendLine("## 📊 Summary Metrics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Average Cycle Time | {cycleTimeResult.AverageCycleTime:F2} days |");
        sb.AppendLine($"| Median Cycle Time | {cycleTimeResult.MedianCycleTime:F2} days |");
        sb.AppendLine($"| P90 Cycle Time | {cycleTimeResult.P90CycleTime:F2} days |");
        sb.AppendLine($"| Total Issues Analyzed | {cycleTimeResult.IssueCycleTimes.Count} |");
        sb.AppendLine();

        // Detailed Issue List
        if (cycleTimeResult.IssueCycleTimes.Any())
        {
            sb.AppendLine("## 📋 Issue Cycle Time Details");
            sb.AppendLine();
            sb.AppendLine("| Issue Key | Summary | Cycle Time | Created | Resolved |");
            sb.AppendLine("|-----------|---------|------------|---------|----------|");

            foreach (var issue in cycleTimeResult.IssueCycleTimes.OrderByDescending(i => i.CycleTimeDays))
            {
                var resolutionDate = issue.ResolutionDate.HasValue
                    ? issue.ResolutionDate.Value.ToString("yyyy-MM-dd")
                    : "Not resolved";
                sb.AppendLine($"| **{issue.IssueKey}** | {issue.Summary} | " +
                             $"{issue.CycleTimeDays:F2} d | {issue.CreatedDate:yyyy-MM-dd} | {resolutionDate} |");
            }
            sb.AppendLine();
        }

        // Footer
        sb.AppendLine("---");
        sb.AppendLine($"*Jira Analytics CLI - Report generated {generatedAt}*");

        return sb.ToString();
    }
}
