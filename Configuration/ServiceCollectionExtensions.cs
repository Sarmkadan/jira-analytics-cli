// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Cli;
using JiraAnalyticsCli.Middleware;
using JiraAnalyticsCli.Formatters;
using JiraAnalyticsCli.Integration;
using JiraAnalyticsCli.Caching;
using JiraAnalyticsCli.Performance;
using JiraAnalyticsCli.Filters;
using JiraAnalyticsCli.Converters;
using JiraAnalyticsCli.BackgroundTasks;
using JiraAnalyticsCli.Events;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Extension methods for registering Phase 2 services in DI container.
/// Centralizes all service registration for clean architecture and dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all CLI and command-related services.
    /// </summary>
    public static IServiceCollection AddCliServices(this IServiceCollection services)
    {
        services.AddSingleton<CommandDefinitions>();
        services.AddSingleton<CommandParser>();
        services.AddSingleton<ConsoleInterface>();

        return services;
    }

    /// <summary>
    /// Registers middleware components and pipeline builder.
    /// </summary>
    public static IServiceCollection AddMiddlewareServices(this IServiceCollection services)
    {
        services.AddSingleton<PipelineBuilder>();
        services.AddScoped<RequestLoggingMiddleware>();
        services.AddScoped<ErrorHandlingMiddleware>();
        services.AddScoped<RateLimitMiddleware>();

        return services;
    }

    /// <summary>
    /// Registers all formatter services for different output formats.
    /// </summary>
    public static IServiceCollection AddFormatterServices(this IServiceCollection services)
    {
        services.AddSingleton(sp => new JsonFormatter(sp.GetRequiredService<ILogger<JsonFormatter>>()));
        services.AddSingleton(sp => new CsvFormatter(sp.GetRequiredService<ILogger<CsvFormatter>>()));
        services.AddSingleton(sp => new XmlFormatter(sp.GetRequiredService<ILogger<XmlFormatter>>()));
        services.AddSingleton(sp => new MarkdownFormatter(sp.GetRequiredService<ILogger<MarkdownFormatter>>()));

        return services;
    }

    /// <summary>
    /// Registers HTTP client and API integration services.
    /// </summary>
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services, string? apiToken = null, string? baseUrl = null)
    {
        services.AddSingleton(sp => new HttpClientFactory(
            sp.GetRequiredService<ILogger<HttpClientFactory>>(),
            apiToken,
            baseUrl));

        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<HttpClientFactory>();
            return factory.CreateJiraClient();
        });

        services.AddScoped(sp => new JiraApiClient(
            sp.GetRequiredService<HttpClient>(),
            sp.GetRequiredService<ILogger<JiraApiClient>>()));

        services.AddSingleton(sp => new WebhookHandler(
            sp.GetRequiredService<ILogger<WebhookHandler>>()));

        return services;
    }

    /// <summary>
    /// Registers caching services with memory cache.
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, TimeSpan? defaultExpiration = null)
    {
        services.AddSingleton(sp => new CacheManager(
            sp.GetRequiredService<ILogger<CacheManager>>(),
            defaultExpiration ?? TimeSpan.FromMinutes(15)));

        return services;
    }

    /// <summary>
    /// Registers performance monitoring and diagnostics services.
    /// </summary>
    public static IServiceCollection AddPerformanceServices(this IServiceCollection services)
    {
        services.AddSingleton(sp => new MetricsCollector(
            sp.GetRequiredService<ILogger<MetricsCollector>>()));

        services.AddSingleton(sp => new DiagnosticsService(
            sp.GetRequiredService<ILogger<DiagnosticsService>>(),
            sp.GetRequiredService<CacheManager>(),
            sp.GetRequiredService<MetricsCollector>()));

        return services;
    }

    /// <summary>
    /// Registers filtering and validation services.
    /// </summary>
    public static IServiceCollection AddFilterServices(this IServiceCollection services)
    {
        // CommandFilter is static utility, no need to register
        return services;
    }

    /// <summary>
    /// Registers data conversion services.
    /// </summary>
    public static IServiceCollection AddConverterServices(this IServiceCollection services)
    {
        services.AddSingleton(sp => new DataConverter(
            sp.GetRequiredService<ILogger<DataConverter>>()));

        return services;
    }

    /// <summary>
    /// Registers background task execution services.
    /// </summary>
    public static IServiceCollection AddBackgroundTaskServices(this IServiceCollection services)
    {
        services.AddSingleton(sp => new BackgroundTaskRunner(
            sp.GetRequiredService<ILogger<BackgroundTaskRunner>>()));

        services.AddSingleton(sp => new MetricSyncTask(
            sp.GetRequiredService<IAnalyticsService>(),
            sp.GetRequiredService<CacheManager>(),
            sp.GetRequiredService<ILogger<MetricSyncTask>>()));

        services.AddSingleton(sp => new ReportGenerationTask(
            sp.GetRequiredService<IAnalyticsService>(),
            sp.GetRequiredService<IReportService>(),
            sp.GetRequiredService<IExportService>(),
            sp.GetRequiredService<ILogger<ReportGenerationTask>>(),
            new ReportGenerationTask.ReportConfiguration()));

        return services;
    }

    /// <summary>
    /// Registers event system services.
    /// </summary>
    public static IServiceCollection AddEventServices(this IServiceCollection services)
    {
        services.AddSingleton(sp => new EventBus(
            sp.GetRequiredService<ILogger<EventBus>>()));

        // Register sample event subscribers
        services.AddSingleton(sp => new SprintAnalysisEventSubscriber(
            sp.GetRequiredService<ILogger<EventSubscriber<SprintAnalysisCompletedEvent>>>()));

        services.AddSingleton(sp => new ErrorEventSubscriber(
            sp.GetRequiredService<ILogger<EventSubscriber<ProcessingErrorEvent>>>()));

        return services;
    }

    /// <summary>
    /// Registers feature flag services.
    /// </summary>
    public static IServiceCollection AddFeatureFlagServices(this IServiceCollection services)
    {
        services.AddSingleton(sp => new FeatureFlags(
            sp.GetRequiredService<ILogger<FeatureFlags>>()));

        return services;
    }

    /// <summary>
    /// Convenience method to register all Phase 2 services at once.
    /// </summary>
    public static IServiceCollection AddPhase2Services(
        this IServiceCollection services,
        string? jiraApiToken = null,
        string? jiraBaseUrl = null,
        TimeSpan? cacheExpiration = null)
    {
        services
            .AddCliServices()
            .AddMiddlewareServices()
            .AddFormatterServices()
            .AddIntegrationServices(jiraApiToken, jiraBaseUrl)
            .AddCachingServices(cacheExpiration)
            .AddPerformanceServices()
            .AddFilterServices()
            .AddConverterServices()
            .AddBackgroundTaskServices()
            .AddEventServices()
            .AddFeatureFlagServices();

        return services;
    }
}
