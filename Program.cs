// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Repositories;

namespace JiraAnalyticsCli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Initialize dependency injection container
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Get logger and log startup
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Jira Analytics CLI starting...");

            // Build CLI commands
            var rootCommand = BuildRootCommand(serviceProvider);

            // Execute command
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    static void ConfigureServices(IServiceCollection services)
    {
        // Logging configuration
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Configuration
        services.AddSingleton<IConfigurationProvider, AppConfigurationProvider>();
        services.AddSingleton<ICliConfig>(sp =>
        {
            var provider = sp.GetRequiredService<IConfigurationProvider>();
            return provider.LoadConfiguration();
        });

        // HTTP client for Jira API
        services.AddHttpClient("jira")
            .ConfigureHttpClient((sp, client) =>
            {
                var config = sp.GetRequiredService<ICliConfig>();
                client.BaseAddress = new Uri(config.JiraBaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.JiraApiToken}");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

        // Data access layer
        services.AddSingleton<IIssueRepository, IssueRepository>();
        services.AddSingleton<ISprintRepository, SprintRepository>();
        services.AddSingleton<IMetricsRepository, MetricsRepository>();

        // Business logic layer
        services.AddSingleton<IJiraApiService, JiraApiService>();
        services.AddSingleton<IAnalyticsService, AnalyticsService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<IExportService, ExportService>();
    }

    static RootCommand BuildRootCommand(IServiceProvider serviceProvider)
    {
        var rootCommand = new RootCommand("Jira Analytics CLI - Advanced sprint and team metrics analysis");

        // Analytics command
        var analyticsCommand = new Command("analytics", "Run analytics and generate reports");
        var analyticsProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key");
        var analyticsSprintsOpt = new Option<int>(new[] { "-s", "--sprints" }, "Number of recent sprints to analyze") { IsRequired = false };
        var analyticsOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output file path") { IsRequired = false };
        var analyticsOutputDirOpt = new Option<string>(new[] { "--output-dir" }, "Directory to save reports to (created if it does not exist)");
        analyticsCommand.AddOption(analyticsProjectOpt);
        analyticsCommand.AddOption(analyticsSprintsOpt);
        analyticsCommand.AddOption(analyticsOutputOpt);
        analyticsCommand.AddOption(analyticsOutputDirOpt);

        analyticsCommand.SetHandler(async (project, sprints, output, outputDir) =>
        {
            var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>();
            var reportService = serviceProvider.GetRequiredService<IReportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Starting analytics for project {Project} with {SprintCount} sprints", project, sprints);
                var analysis = await analyticsService.AnalyzeSprints(project, sprints > 0 ? sprints : 5);
                var report = reportService.GenerateReport(analysis);

                var resolvedOutput = ResolveOutputPath(output, outputDir, "report.txt");
                if (!string.IsNullOrEmpty(resolvedOutput))
                {
                    File.WriteAllText(resolvedOutput, report);
                    logger.LogInformation("Report written to {OutputPath}", resolvedOutput);
                }
                else
                {
                    Console.WriteLine(report);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Analytics command failed");
                throw;
            }
        }, analyticsProjectOpt, analyticsSprintsOpt, analyticsOutputOpt, analyticsOutputDirOpt);

        // Export command
        var exportCommand = new Command("export", "Export analytics data to image or file");
        var exportProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key");
        var exportFormatOpt = new Option<string>(new[] { "-f", "--format" }, "Export format (png, jpg, pdf, json, csv)");
        var exportOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output file path");
        var exportOutputDirOpt = new Option<string>(new[] { "--output-dir" }, "Directory to save reports to (created if it does not exist)");
        exportCommand.AddOption(exportProjectOpt);
        exportCommand.AddOption(exportFormatOpt);
        exportCommand.AddOption(exportOutputOpt);
        exportCommand.AddOption(exportOutputDirOpt);

        exportCommand.SetHandler(async (project, format, output, outputDir) =>
        {
            var exportService = serviceProvider.GetRequiredService<IExportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var resolvedOutput = ResolveOutputPath(output, outputDir, $"report.{format ?? "json"}");
                logger.LogInformation("Exporting analytics as {Format} to {Output}", format, resolvedOutput);
                await exportService.ExportAnalytics(project, format, resolvedOutput);
                logger.LogInformation("Export completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Export command failed");
                throw;
            }
        }, exportProjectOpt, exportFormatOpt, exportOutputOpt, exportOutputDirOpt);

        // Burndown command
        var burndownCommand = new Command("burndown", "Generate burndown chart for sprint");
        var burndownProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key");
        var burndownSprintIdOpt = new Option<int>(new[] { "--sprint-id" }, "Sprint ID");
        var burndownOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output image path");
        var burndownOutputDirOpt = new Option<string>(new[] { "--output-dir" }, "Directory to save reports to (created if it does not exist)");
        burndownCommand.AddOption(burndownProjectOpt);
        burndownCommand.AddOption(burndownSprintIdOpt);
        burndownCommand.AddOption(burndownOutputOpt);
        burndownCommand.AddOption(burndownOutputDirOpt);

        burndownCommand.SetHandler(async (project, sprintId, output, outputDir) =>
        {
            var reportService = serviceProvider.GetRequiredService<IReportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var resolvedOutput = ResolveOutputPath(output, outputDir, "burndown.png");
                logger.LogInformation("Generating burndown chart for sprint {SprintId}", sprintId);
                await reportService.GenerateBurndownChart(project, sprintId, resolvedOutput);
                logger.LogInformation("Burndown chart saved to {Output}", resolvedOutput);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Burndown command failed");
                throw;
            }
        }, burndownProjectOpt, burndownSprintIdOpt, burndownOutputOpt, burndownOutputDirOpt);

        rootCommand.AddCommand(analyticsCommand);
        rootCommand.AddCommand(exportCommand);
        rootCommand.AddCommand(burndownCommand);

        return rootCommand;
    }

    /// <summary>
    /// Resolves the effective output file path given an optional explicit output file and an
    /// optional output directory.  Creates the directory if it does not already exist.
    /// </summary>
    /// <param name="output">File name or path supplied via <c>--output</c> (may be null).</param>
    /// <param name="outputDir">Directory supplied via <c>--output-dir</c> (may be null).</param>
    /// <param name="defaultFileName">Fallback file name when <paramref name="output"/> is null.</param>
    /// <returns>
    /// The resolved file path, or <c>null</c> when neither <paramref name="output"/> nor
    /// <paramref name="outputDir"/> was provided (callers should write to stdout in that case).
    /// </returns>
    static string? ResolveOutputPath(string? output, string? outputDir, string defaultFileName)
    {
        if (string.IsNullOrEmpty(outputDir) && string.IsNullOrEmpty(output))
            return null;

        var fileName = string.IsNullOrEmpty(output)
            ? defaultFileName
            : Path.GetFileName(output);

        if (string.IsNullOrEmpty(outputDir))
        {
            // No output-dir: use the path as-is, but ensure parent directory exists
            var dir = Path.GetDirectoryName(output!);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            return output!;
        }

        Directory.CreateDirectory(outputDir);
        return Path.Combine(outputDir, fileName!);
    }
}
