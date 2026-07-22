// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Configuration provider that loads settings from environment variables, JSON config file, and defaults
/// </summary>
public interface IConfigurationProvider
{
    ICliConfig LoadConfiguration();
}

public class AppConfigurationProvider : IConfigurationProvider
{
    private readonly ILogger<AppConfigurationProvider> _logger;
    private const string ConfigFileName = "appsettings.json";

    public AppConfigurationProvider(ILogger<AppConfigurationProvider> logger)
    {
        _logger = logger;
    }

    public ICliConfig LoadConfiguration()
    {
        _logger.LogInformation("Loading application configuration");

        var config = new CliConfig();

        // Load from JSON file if exists
        if (File.Exists(ConfigFileName))
        {
            _logger.LogInformation("Found configuration file: {ConfigFile}", ConfigFileName);
            LoadFromJsonFile(config);
        }

        // Load from environment variables (highest priority, overrides JSON file)
        LoadFromEnvironment(config);

        // Validate configuration
        try
        {
            config.Validate();
            _logger.LogInformation("Configuration loaded and validated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration validation failed");
            throw;
        }

        return config;
    }

    private void LoadFromEnvironment(CliConfig config)
    {
        _logger.LogDebug("Loading configuration from environment variables");

        var jiraUrl = Environment.GetEnvironmentVariable("JIRA_URL") ?? Environment.GetEnvironmentVariable("JIRA_BASE_URL");
        if (!string.IsNullOrEmpty(jiraUrl))
        {
            config.JiraBaseUrl = jiraUrl;
            _logger.LogDebug("JIRA_URL/JIRA_BASE_URL set from environment");
        }

        var jiraToken = Environment.GetEnvironmentVariable("JIRA_TOKEN") ?? Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
        if (!string.IsNullOrEmpty(jiraToken))
        {
            config.JiraApiToken = jiraToken;
            _logger.LogDebug("JIRA_TOKEN/JIRA_API_TOKEN set from environment");
        }

        var defaultProject = Environment.GetEnvironmentVariable("JIRA_PROJECT") ?? Environment.GetEnvironmentVariable("JIRA_DEFAULT_PROJECT");
        if (!string.IsNullOrEmpty(defaultProject))
        {
            config.DefaultProject = defaultProject;
            _logger.LogDebug("JIRA_PROJECT/JIRA_DEFAULT_PROJECT set from environment");
        }

        var cacheMinutes = Environment.GetEnvironmentVariable("CACHE_EXPIRATION_MINUTES");
        if (int.TryParse(cacheMinutes, out var minutes))
        {
            config.CacheExpirationMinutes = minutes;
            _logger.LogDebug("CACHE_EXPIRATION_MINUTES set to {Minutes}", minutes);
        }

        var logging = Environment.GetEnvironmentVariable("DETAILED_LOGGING");
        if (bool.TryParse(logging, out var enabled))
        {
            config.EnableDetailedLogging = enabled;
            _logger.LogDebug("DETAILED_LOGGING set to {Enabled}", enabled);
        }

        var sprintCount = Environment.GetEnvironmentVariable("DEFAULT_SPRINT_COUNT");
        if (int.TryParse(sprintCount, out var count))
        {
            config.DefaultSprintCount = count;
            _logger.LogDebug("DEFAULT_SPRINT_COUNT set to {Count}", count);
        }

        var format = Environment.GetEnvironmentVariable("EXPORT_FORMAT");
        if (!string.IsNullOrEmpty(format))
        {
            config.ExportFormat = format;
            _logger.LogDebug("EXPORT_FORMAT set to {Format}", format);
        }

        var maxRetryAttempts = Environment.GetEnvironmentVariable("JIRA_API_MAX_RETRY_ATTEMPTS");
        if (int.TryParse(maxRetryAttempts, out var retryAttempts))
        {
            config.JiraApiMaxRetryAttempts = retryAttempts;
            _logger.LogDebug("JIRA_API_MAX_RETRY_ATTEMPTS set to {Attempts}", retryAttempts);
        }

        var circuitBreakerThreshold = Environment.GetEnvironmentVariable("JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD");
        if (int.TryParse(circuitBreakerThreshold, out var threshold))
        {
            config.JiraApiCircuitBreakerFailureThreshold = threshold;
            _logger.LogDebug("JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD set to {Threshold}", threshold);
        }

        var circuitBreakerDuration = Environment.GetEnvironmentVariable("JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS");
        if (int.TryParse(circuitBreakerDuration, out var duration))
        {
            config.JiraApiCircuitBreakerDurationSeconds = duration;
            _logger.LogDebug("JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS set to {Duration}", duration);
        }

        var timeoutSeconds = Environment.GetEnvironmentVariable("JIRA_API_TIMEOUT_SECONDS");
        if (int.TryParse(timeoutSeconds, out var timeout))
        {
            config.JiraApiTimeoutSeconds = timeout;
            _logger.LogDebug("JIRA_API_TIMEOUT_SECONDS set to {Timeout}", timeout);
        }
    }

    private void LoadFromJsonFile(CliConfig config)
    {
        try
        {
            _logger.LogDebug("Loading configuration from JSON file");

            var json = File.ReadAllText(ConfigFileName);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("jiraBaseUrl", out var jiraUrl))
                config.JiraBaseUrl = jiraUrl.GetString() ?? config.JiraBaseUrl;

            if (root.TryGetProperty("jiraApiToken", out var jiraToken))
                config.JiraApiToken = jiraToken.GetString() ?? config.JiraApiToken;

            if (root.TryGetProperty("defaultProject", out var defaultProject))
                config.DefaultProject = defaultProject.GetString();

            if (root.TryGetProperty("cacheExpirationMinutes", out var cacheMinutes) && cacheMinutes.TryGetInt32(out var minutes))
                config.CacheExpirationMinutes = minutes;

            if (root.TryGetProperty("enableDetailedLogging", out var logging) && (logging.ValueKind == JsonValueKind.True || logging.ValueKind == JsonValueKind.False))
                config.EnableDetailedLogging = logging.GetBoolean();

            if (root.TryGetProperty("defaultSprintCount", out var sprintCount) && sprintCount.TryGetInt32(out var count))
                config.DefaultSprintCount = count;

            if (root.TryGetProperty("exportFormat", out var format))
                config.ExportFormat = format.GetString() ?? config.ExportFormat;

            if (root.TryGetProperty("jiraApiMaxRetryAttempts", out var maxRetryAttempts) && maxRetryAttempts.TryGetInt32(out var retryAttempts))
                config.JiraApiMaxRetryAttempts = retryAttempts;

            if (root.TryGetProperty("jiraApiCircuitBreakerFailureThreshold", out var circuitBreakerThreshold) && circuitBreakerThreshold.TryGetInt32(out var threshold))
                config.JiraApiCircuitBreakerFailureThreshold = threshold;

            if (root.TryGetProperty("jiraApiCircuitBreakerDurationSeconds", out var circuitBreakerDuration) && circuitBreakerDuration.TryGetInt32(out var duration))
                config.JiraApiCircuitBreakerDurationSeconds = duration;

            if (root.TryGetProperty("jiraApiTimeoutSeconds", out var timeoutSeconds) && timeoutSeconds.TryGetInt32(out var timeout))
                config.JiraApiTimeoutSeconds = timeout;

            _logger.LogDebug("Configuration successfully loaded from JSON file");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading configuration from JSON file, using defaults");
        }
    }
}