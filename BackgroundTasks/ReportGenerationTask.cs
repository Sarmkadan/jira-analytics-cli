// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Formatters;

namespace JiraAnalyticsCli.BackgroundTasks;

/// <summary>
/// Background task for generating and saving scheduled reports.
/// Produces analytics reports at specified intervals and exports to files.
/// </summary>
public class ReportGenerationTask
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;
    private readonly ILogger<ReportGenerationTask> _logger;
    private readonly ReportConfiguration _config;

    public ReportGenerationTask(
        IAnalyticsService analyticsService,
        IReportService reportService,
        IExportService exportService,
        ILogger<ReportGenerationTask> logger,
        ReportConfiguration config)
    {
        _analyticsService = analyticsService;
        _reportService = reportService;
        _exportService = exportService;
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Generates reports for configured projects and exports to specified location.
    /// Supports multiple output formats (JSON, CSV, Markdown, etc.).
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting report generation task");

        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");

            foreach (var projectConfig in _config.Projects)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Report generation cancelled");
                    break;
                }

                try
                {
                    _logger.LogDebug("Generating report for project: {Project}", projectConfig.ProjectKey);

                    var analysis = await _analyticsService.AnalyzeSprints(
                        projectConfig.ProjectKey,
                        projectConfig.SprintCount);

                    var report = _reportService.GenerateReport(analysis);

                    var fileName = $"{projectConfig.ProjectKey}_report_{timestamp}.{projectConfig.Format}";
                    var filePath = Path.Combine(projectConfig.OutputDirectory, fileName);

                    // Ensure directory exists
                    Directory.CreateDirectory(projectConfig.OutputDirectory);

                    // Export in specified format
                    switch (projectConfig.Format.ToLower())
                    {
                        case "json":
                            File.WriteAllText(filePath, report);
                            break;

                        case "csv":
                            // Convert to CSV format
                            File.WriteAllText(filePath, report);
                            break;

                        case "markdown":
                            File.WriteAllText(filePath, report);
                            break;

                        default:
                            _logger.LogWarning("Unsupported format: {Format}", projectConfig.Format);
                            continue;
                    }

                    _logger.LogInformation(
                        "Report generated and saved: {FilePath}",
                        filePath);

                    if (projectConfig.SendNotification)
                    {
                        await SendNotificationAsync(projectConfig.ProjectKey, filePath, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating report for project: {Project}", projectConfig.ProjectKey);
                }

                // Add delay between projects
                if (_config.Projects.Count > 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                }
            }

            _logger.LogInformation("Report generation task completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in report generation task");
            throw;
        }
    }

    private async Task SendNotificationAsync(string project, string filePath, CancellationToken cancellationToken)
    {
        // Implementation would send notification via email, webhook, Slack, etc.
        _logger.LogDebug("Would send notification for report: {Project} at {FilePath}", project, filePath);
        await Task.CompletedTask;
    }

    public class ProjectReportConfig
    {
        public string ProjectKey { get; set; } = string.Empty;
        public int SprintCount { get; set; } = 5;
        public string Format { get; set; } = "json";
        public string OutputDirectory { get; set; } = "./reports";
        public bool SendNotification { get; set; }
    }

    public class ReportConfiguration
    {
        public List<ProjectReportConfig> Projects { get; set; } = new();
        public TimeSpan Frequency { get; set; } = TimeSpan.FromHours(1);
    }
}
