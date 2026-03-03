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
        analyticsCommand.AddOption(new Option<string>(new[] { "-p", "--project" }, "Jira project key"));
        analyticsCommand.AddOption(new Option<int>(new[] { "-s", "--sprints" }, "Number of recent sprints to analyze") { IsRequired = false });
        analyticsCommand.AddOption(new Option<string>(new[] { "-o", "--output" }, "Output file path") { IsRequired = false });

        analyticsCommand.SetHandler(async (project, sprints, output) =>
        {
            var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>();
            var reportService = serviceProvider.GetRequiredService<IReportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Starting analytics for project {Project} with {SprintCount} sprints", project, sprints);
                var analysis = await analyticsService.AnalyzeSprints(project, sprints > 0 ? sprints : 5);
                var report = reportService.GenerateReport(analysis);

                if (!string.IsNullOrEmpty(output))
                {
                    File.WriteAllText(output, report);
                    logger.LogInformation("Report written to {OutputPath}", output);
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
        }, new Option<string>(new[] { "-p", "--project" }),
           new Option<int>(new[] { "-s", "--sprints" }),
           new Option<string>(new[] { "-o", "--output" }));

        // Export command
        var exportCommand = new Command("export", "Export analytics data to image or file");
        exportCommand.AddOption(new Option<string>(new[] { "-p", "--project" }, "Jira project key"));
        exportCommand.AddOption(new Option<string>(new[] { "-f", "--format" }, "Export format (png, jpg, pdf, json, csv)"));
        exportCommand.AddOption(new Option<string>(new[] { "-o", "--output" }, "Output file path"));

        exportCommand.SetHandler(async (project, format, output) =>
        {
            var exportService = serviceProvider.GetRequiredService<IExportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Exporting analytics as {Format} to {Output}", format, output);
                await exportService.ExportAnalytics(project, format, output);
                logger.LogInformation("Export completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Export command failed");
                throw;
            }
        }, new Option<string>(new[] { "-p", "--project" }),
           new Option<string>(new[] { "-f", "--format" }),
           new Option<string>(new[] { "-o", "--output" }));

        // Burndown command
        var burndownCommand = new Command("burndown", "Generate burndown chart for sprint");
        burndownCommand.AddOption(new Option<string>(new[] { "-p", "--project" }, "Jira project key"));
        burndownCommand.AddOption(new Option<int>(new[] { "--sprint-id" }, "Sprint ID"));
        burndownCommand.AddOption(new Option<string>(new[] { "-o", "--output" }, "Output image path"));

        burndownCommand.SetHandler(async (project, sprintId, output) =>
        {
            var reportService = serviceProvider.GetRequiredService<IReportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Generating burndown chart for sprint {SprintId}", sprintId);
                await reportService.GenerateBurndownChart(project, sprintId, output);
                logger.LogInformation("Burndown chart saved to {Output}", output);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Burndown command failed");
                throw;
            }
        }, new Option<string>(new[] { "-p", "--project" }),
           new Option<int>(new[] { "--sprint-id" }),
           new Option<string>(new[] { "-o", "--output" }));

        rootCommand.AddCommand(analyticsCommand);
        rootCommand.AddCommand(exportCommand);
        rootCommand.AddCommand(burndownCommand);

        return rootCommand;
    }
}
