using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Repositories;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli;

public static class ServiceFactory
{
    /// <summary>
    /// Builds the DI container. <paramref name="jiraOptions"/> must already be validated.
    /// </summary>
    /// <param name="jiraOptions">Validated Jira settings.</param>
    /// <param name="velocityOptions">Validated velocity settings; defaults are used when null.</param>
    /// <param name="verbose">When true, the CLI's own logs and the Jira request log (method, URL, status, duration) are written at Debug level.</param>
    public static IServiceProvider CreateServiceProvider(JiraOptions jiraOptions, VelocityOptions? velocityOptions = null, bool verbose = false)
    {
        ArgumentNullException.ThrowIfNull(jiraOptions);

        var services = new ServiceCollection();
        services.AddSingleton(jiraOptions);
        services.AddSingleton(velocityOptions ?? new VelocityOptions());

        // Logging configuration
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            // --verbose lowers only this CLI's categories, so framework logs stay quiet.
            builder.AddFilter("JiraAnalyticsCli", verbose ? LogLevel.Debug : LogLevel.Information);
        });

        // Configuration
        services.AddSingleton<IConfigurationProvider, AppConfigurationProvider>();
        services.AddSingleton<ICliConfig>(sp =>
        {
            var provider = sp.GetRequiredService<IConfigurationProvider>();
            return provider.LoadConfiguration();
        });

        // HTTP client for Jira API
        services.AddTransient<JiraRequestLoggingHandler>();
        services.AddHttpClient("jira")
            .ConfigureHttpClient((sp, client) =>
            {
                var config = sp.GetRequiredService<ICliConfig>();
                client.BaseAddress = new Uri(config.JiraBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(config.JiraApiTimeoutSeconds);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.JiraApiToken}");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<JiraRequestLoggingHandler>();

        // Data access layer
        services.AddSingleton<IIssueRepository, IssueRepository>();
        services.AddSingleton<ISprintRepository, SprintRepository>();
        services.AddSingleton<IMetricsRepository, MetricsRepository>();

        // Business logic layer
        services.AddSingleton<IJiraApiService, JiraApiService>();
        services.AddSingleton<VelocityTracker>();
        services.AddSingleton<IAnalyticsService, AnalyticsService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<ICsvExportService, CsvExportService>();
        services.AddSingleton<IJqlQueryService, JqlQueryService>();
        services.AddSingleton<IHtmlReportService, HtmlReportService>();
        services.AddSingleton<IMarkdownReportService, MarkdownReportService>();
        services.AddSingleton<ITeamComparisonService, TeamComparisonService>();
        services.AddSingleton<ISnapshotStore, SnapshotStore>();

        return services.BuildServiceProvider();
    }
}