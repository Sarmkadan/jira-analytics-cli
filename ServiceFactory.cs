using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Repositories;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli
{
    public static class ServiceFactory
    {
        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

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
            services.AddSingleton<ICsvExportService, CsvExportService>();
            services.AddSingleton<IJqlQueryService, JqlQueryService>();
            services.AddSingleton<IHtmlReportService, HtmlReportService>();
            services.AddSingleton<IMarkdownReportService, MarkdownReportService>();
            services.AddSingleton<ITeamComparisonService, TeamComparisonService>();
            services.AddSingleton<ISnapshotStore, SnapshotStore>();

            return services.BuildServiceProvider();
        }
    }
}