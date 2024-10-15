// =============================================================================
// Integration Example for jira-analytics-cli
// Demonstrates how to integrate jira-analytics-cli into ASP.NET applications
// using dependency injection and configuration patterns
// =============================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Repositories;

// This example shows how to integrate jira-analytics-cli services into an ASP.NET Core application
// The same pattern used in Program.cs can be applied in your web application

namespace JiraAnalyticsIntegration
{
    class IntegrationExample
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ASP.NET Core Integration Example ===\n");

            // Example 1: Setting up dependency injection in ASP.NET Core
            Console.WriteLine("Example 1: Registering jira-analytics-cli services in ASP.NET Core");
            Console.WriteLine(@"// In Program.cs or Startup.cs

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register jira-analytics-cli services (same as Program.cs in the CLI)
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddSingleton<IConfigurationProvider, AppConfigurationProvider>();
builder.Services.AddSingleton<ICliConfig>(sp =>
{
    var provider = sp.GetRequiredService<IConfigurationProvider>();
    return provider.LoadConfiguration();
});

builder.Services.AddHttpClient("jira")
    .ConfigureHttpClient((sp, client) =>
    {
        var config = sp.GetRequiredService<ICliConfig>();
        client.BaseAddress = new Uri(config.JiraBaseUrl);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.JiraApiToken}");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

// Data access layer
builder.Services.AddSingleton<IIssueRepository, IssueRepository>();
builder.Services.AddSingleton<ISprintRepository, SprintRepository>();
builder.Services.AddSingleton<IMetricsRepository, MetricsRepository>();

// Business logic layer
builder.Services.AddSingleton<IJiraApiService, JiraApiService>();
builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
builder.Services.AddSingleton<IReportService, ReportService>();
builder.Services.AddSingleton<IExportService, ExportService>();
builder.Services.AddSingleton<IJqlQueryService, JqlQueryService>();
builder.Services.AddSingleton<IHtmlReportService, HtmlReportService>();
builder.Services.AddSingleton<ITeamComparisonService, TeamComparisonService>();

var app = builder.Build();");

            // Example 2: Creating a custom analytics controller
            Console.WriteLine("\nExample 2: Creating a custom controller that uses jira-analytics-cli services");
            Console.WriteLine(@"// AnalyticsController.cs
using Microsoft.AspNetCore.Mvc;
using JiraAnalyticsCli.Services;

/// <summary>
/// Controller for generating and retrieving Jira analytics reports.
/// Provides endpoints for analyzing project data and generating reports from jira-analytics-cli services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    /// <summary>
    /// The analytics service for processing Jira data and generating analysis.
    /// </summary>
    private readonly IAnalyticsService _analyticsService;

    /// <summary>
    /// The report service for generating formatted reports from analysis data.
    /// </summary>
    private readonly IReportService _reportService;

    /// <summary>
    /// Logger for tracking controller operations and errors.
    /// </summary>
    private readonly ILogger<AnalyticsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsController"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service for processing Jira data.</param>
    /// <param name="reportService">The report service for generating formatted reports.</param>
    /// <param name="logger">Logger for tracking operations and errors.</param>
    public AnalyticsController(
        IAnalyticsService analyticsService,
        IReportService reportService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves analytics for a specific Jira project.
    /// </summary>
    /// <param name="projectKey">The Jira project key (e.g., "PROJ").</param>
    /// <param name="sprints">Number of recent sprints to analyze (default: 5).</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - Project key
    /// - Number of sprints analyzed
    /// - Analysis data
    /// - Generated report
    /// </returns>
    [HttpGet("project/{projectKey}")]
    public async Task<IActionResult> GetProjectAnalytics(string projectKey, [FromQuery] int sprints = 5)
    {
        try
        {
            _logger.LogInformation("Generating analytics for project {ProjectKey}", projectKey);

            var analysis = await _analyticsService.AnalyzeSprints(projectKey, sprints);
            var report = _reportService.GenerateReport(analysis);

            return Ok(new {
                Project = projectKey,
                SprintCount = sprints,
                Analysis = analysis,
                Report = report
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate analytics");
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Register the controller
builder.Services.AddControllers();");

            // Example 3: Creating a background service for scheduled reports
            Console.WriteLine("\nExample 3: Background service for scheduled Jira analytics");
            Console.WriteLine(@"// ScheduledReportService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Services;

/// <summary>
/// Background service for generating scheduled Jira analytics reports.
/// Periodically generates and exports analytics reports based on configured schedule.
/// </summary>
public class ScheduledReportService : BackgroundService
{
    /// <summary>
    /// The service provider for resolving scoped services.
    /// </summary>
    private readonly IServiceProvider _services;

    /// <summary>
    /// Logger for tracking service operations and errors.
    /// </summary>
    private readonly ILogger<ScheduledReportService> _logger;

    /// <summary>
    /// The time interval between scheduled report generations.
    /// </summary>
    private readonly TimeSpan _scheduleInterval;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledReportService"/> class.
    /// </summary>
    /// <param name="services">The service provider for resolving scoped services.</param>
    /// <param name="logger">Logger for tracking operations and errors.</param>
    /// <param name="scheduleInterval">The time interval between scheduled report generations.</param>
    public ScheduledReportService(
        IServiceProvider services,
        ILogger<ScheduledReportService> logger,
        TimeSpan scheduleInterval)
    {
        _services = services;
        _logger = logger;
        _scheduleInterval = scheduleInterval;
    }

    /// <summary>
    /// Executes the background service loop for generating scheduled reports.
    /// Continuously generates weekly reports at the configured interval until cancellation is requested.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token for graceful shutdown.</param>
    /// <returns>Task representing the background service execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled Report Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();

                // Generate weekly report
                var analysis = await analyticsService.AnalyzeSprints("PROJECT_KEY", 5);
                var report = reportService.GenerateReport(analysis);

                // Save to file
                var filePath = $"./reports/weekly-{DateTime.Now:yyyy-MM-dd}.txt";
                await File.WriteAllTextAsync(filePath, report, stoppingToken);

                _logger.LogInformation("Weekly report generated: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating scheduled report");
            }

            await Task.Delay(_scheduleInterval, stoppingToken);
        }
    }
}

// Register the background service
builder.Services.AddHostedService<ScheduledReportService>();");

            // Example 4: Using configuration from appsettings.json
            Console.WriteLine("\nExample 4: Configuration in appsettings.json");
            Console.WriteLine(@"{
  "Jira": {
    "BaseUrl": "https://your-instance.atlassian.net",
    "ApiToken": "your-api-token",
    "Username": "your-email@example.com",
    "CacheEnabled": true,
    "CacheDurationMinutes": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}

// In your configuration provider, read from the Jira section");

            // Example 5: Creating a custom service that combines multiple CLI features
            Console.WriteLine("\nExample 5: Custom service combining multiple CLI features");
            Console.WriteLine(@"// TeamDashboardService.cs
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;

/// <summary>
/// Service for generating comprehensive team dashboards from multiple Jira projects.
/// Combines analytics from multiple projects, performs team comparisons, and generates HTML dashboards.
/// </summary>
public class TeamDashboardService
{
    /// <summary>
    /// The analytics service for processing Jira data and generating analysis.
    /// </summary>
    private readonly IAnalyticsService _analytics;

    /// <summary>
    /// The service for comparing team performance across projects.
    /// </summary>
    private readonly ITeamComparisonService _comparison;

    /// <summary>
    /// The service for generating HTML reports and dashboards.
    /// </summary>
    private readonly IHtmlReportService _htmlReport;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamDashboardService"/> class.
    /// </summary>
    /// <param name="analytics">The analytics service for processing Jira data.</param>
    /// <param name="comparison">The service for comparing team performance across projects.</param>
    /// <param name="htmlReport">The service for generating HTML reports and dashboards.</param>
    public TeamDashboardService(
        IAnalyticsService analytics,
        ITeamComparisonService comparison,
        IHtmlReportService htmlReport)
    {
        _analytics = analytics;
        _comparison = comparison;
        _htmlReport = htmlReport;
    }

    /// <summary>
    /// Generates a comprehensive dashboard combining analytics from multiple Jira projects.
    /// </summary>
    /// <param name="projectKeys">Array of Jira project keys to analyze.</param>
    /// <param name="sprintCount">Number of sprints to analyze per project (default: 5).</param>
    /// <returns>A <see cref="TeamDashboard"/> containing project analyses, team comparisons, and HTML dashboard.</returns>
    public async Task<TeamDashboard> GenerateTeamDashboard(string[] projectKeys, int sprintCount = 5)
    {
        // Get individual project analytics
        var projectAnalyses = new List<ProjectAnalysis>();
        foreach (var projectKey in projectKeys)
        {
            var analysis = await _analytics.AnalyzeSprints(projectKey, sprintCount);
            projectAnalyses.Add(analysis);
        }

        // Compare teams
        var comparison = await _comparison.CompareTeamsAsync(projectKeys, sprintCount);

        // Generate HTML dashboard
        var dashboardHtml = await _htmlReport.GenerateDashboardAsync(projectAnalyses, comparison);

        return new TeamDashboard
        {
            Projects = projectAnalyses,
            Comparison = comparison,
            DashboardHtml = dashboardHtml
        };
    }
}

// Register custom service
builder.Services.AddSingleton<TeamDashboardService>();");

            // Example 6: Using the CLI library as a NuGet package
            Console.WriteLine("\nExample 6: Using as NuGet package (future state)");
            Console.WriteLine(@"// Once published to NuGet, you can install it:
// dotnet add package JiraAnalyticsCli

// Then use services directly:
builder.Services.AddJiraAnalyticsCli(options =>
{
    options.JiraBaseUrl = builder.Configuration["Jira:BaseUrl"];
    options.JiraApiToken = builder.Configuration["Jira:ApiToken"];
    options.JiraUsername = builder.Configuration["Jira:Username"];
    options.CacheEnabled = true;
});");

            // Example 7: Health checks for Jira connectivity
            Console.WriteLine("\nExample 7: Health check for Jira connectivity");
            Console.WriteLine(@"// HealthChecks/JiraHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;
using JiraAnalyticsCli.Services;

/// <summary>
/// Health check for verifying Jira API connectivity.
/// Used to monitor the availability of the Jira API service.
/// </summary>
public class JiraHealthCheck : IHealthCheck
{
    /// <summary>
    /// The Jira API service for making requests to Jira.
    /// </summary>
    private readonly IJiraApiService _jiraService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JiraHealthCheck"/> class.
    /// </summary>
    /// <param name="jiraService">The Jira API service for making requests to Jira.</param>
    public JiraHealthCheck(IJiraApiService jiraService)
    {
        _jiraService = jiraService;
    }

    /// <summary>
    /// Checks the health of the Jira API service.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A <see cref="HealthCheckResult"/> indicating whether the Jira API is accessible:
    /// - Healthy if the API is accessible and returns projects
    /// - Unhealthy if the API is not accessible
    /// </returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _jiraService.GetProjectsAsync();
            return HealthCheckResult.Healthy("Jira API is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Jira API is not accessible", ex);
        }
    }
}

// Register health check
builder.Services.AddHealthChecks()
    .AddCheck<JiraHealthCheck>("jira");");

            // Example 8: Caching layer
            Console.WriteLine("\nExample 8: Adding caching layer");
            Console.WriteLine(@"// In your Startup.cs or Program.cs
builder.Services.AddMemoryCache();

// Then in your services, use IMemoryCache
/// <summary>
/// Decorator service that adds caching functionality to the analytics service.
/// Caches analysis results to reduce API calls and improve performance.
/// </summary>
public class CachedAnalyticsService : IAnalyticsService
{
    /// <summary>
    /// The decorated analytics service that performs the actual analysis.
    /// </summary>
    private readonly IAnalyticsService _decorated;

    /// <summary>
    /// The memory cache for storing cached analysis results.
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedAnalyticsService"/> class.
    /// </summary>
    /// <param name="decorated">The decorated analytics service that performs the actual analysis.</param>
    /// <param name="cache">The memory cache for storing cached analysis results.</param>
    public CachedAnalyticsService(IAnalyticsService decorated, IMemoryCache cache)
    {
        _decorated = decorated;
        _cache = cache;
    }

    /// <summary>
    /// Analyzes sprints for a project, using cached results when available.
    /// Results are cached for 30 minutes to reduce API calls to Jira.
    /// </summary>
    /// <param name="projectKey">The Jira project key to analyze.</param>
    /// <param name="sprintCount">Number of sprints to analyze.</param>
    /// <returns>A <see cref="ProjectAnalysis"/> containing the analysis results.</returns>
    public async Task<ProjectAnalysis> AnalyzeSprints(string projectKey, int sprintCount)
    {
        var cacheKey = $"Analytics_{projectKey}_{sprintCount}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _decorated.AnalyzeSprints(projectKey, sprintCount);
        });
    }
}

// Register cached version
builder.Services.AddSingleton<IAnalyticsService, CachedAnalyticsService>();");

            Console.WriteLine("\n=== Key Integration Points ===");
            Console.WriteLine("1. Use the same service registration pattern as Program.cs");
            Console.WriteLine("2. Register services as Singleton for CLI-style usage");
            Console.WriteLine("3. Use BackgroundService for scheduled reports");
            Console.WriteLine("4. Create custom controllers for web APIs");
            Console.WriteLine("5. Add health checks for monitoring");
            Console.WriteLine("6. Use caching to reduce API calls");
            Console.WriteLine("7. Combine multiple services for complex workflows");
            Console.WriteLine("8. Configuration follows standard ASP.NET patterns");
        }
    }
}
