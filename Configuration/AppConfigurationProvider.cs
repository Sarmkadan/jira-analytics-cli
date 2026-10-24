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
    public ICliConfig LoadConfiguration();
}

public class AppConfigurationProvider : IConfigurationProvider
{
    private readonly ILogger<AppConfigurationProvider> _logger;
    private readonly JiraOptions _jiraOptions;
    private const string ConfigFileName = "appsettings.json";

    public AppConfigurationProvider(ILogger<AppConfigurationProvider> logger, JiraOptions jiraOptions)
    {
        this._logger = logger;
        this._jiraOptions = jiraOptions;
    }

    public ICliConfig LoadConfiguration()
    {
        this._logger.LogInformation("Loading application configuration");

        var config = new CliConfig();

        // Load from JSON file if exists
        if (File.Exists(ConfigFileName))
        {
            this._logger.LogInformation("Found configuration file: {ConfigFile}", ConfigFileName);
            this.LoadFromJsonFile(config);
        }

        // Load from environment variables (highest priority, overrides JSON file)
        this.LoadFromEnvironment(config);

        // Jira settings resolved at startup (CLI flags > env > appsettings) take precedence over the values above
        config.JiraBaseUrl = this._jiraOptions.BaseUrl;
        config.JiraApiToken = this._jiraOptions.ApiToken;
        config.JiraApiTimeoutSeconds = this._jiraOptions.TimeoutSeconds;
        config.DefaultProject = this._jiraOptions.ProjectKey;

        // Validate configuration
        try
        {
            config.Validate();
            this._logger.LogInformation("Configuration loaded and validated successfully");
        }
        catch (InvalidOperationException ex)
        {
            this._logger.LogError(ex, "Configuration validation failed");
            throw;
        }

        return config;
    }

    private void LoadFromEnvironment(CliConfig config)
    {
        this._logger.LogDebug("Loading configuration from environment variables");

        var jiraUrl = Environment.GetEnvironmentVariable("JIRA_URL") ?? Environment.GetEnvironmentVariable("JIRA_BASE_URL");
        if (!string.IsNullOrEmpty(jiraUrl))
        {
            config.JiraBaseUrl = jiraUrl;
            this._logger.LogDebug("JIRA_URL/JIRA_BASE_URL set from environment");
        }

        var jiraToken = Environment.GetEnvironmentVariable("JIRA_TOKEN") ?? Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
        if (!string.IsNullOrEmpty(jiraToken))
        {
            config.JiraApiToken = jiraToken;
            this._logger.LogDebug("JIRA_TOKEN/JIRA_API_TOKEN set from environment");
        }

        var defaultProject = Environment.GetEnvironmentVariable("JIRA_PROJECT") ?? Environment.GetEnvironmentVariable("JIRA_DEFAULT_PROJECT");
        if (!string.IsNullOrEmpty(defaultProject))
        {
            config.DefaultProject = defaultProject;
            this._logger.LogDebug("JIRA_PROJECT/JIRA_DEFAULT_PROJECT set from environment");
        }

        var cacheMinutes = Environment.GetEnvironmentVariable("CACHE_EXPIRATION_MINUTES");
        if (int.TryParse(cacheMinutes, out var minutes))
        {
            config.CacheExpirationMinutes = minutes;
            this._logger.LogDebug("CACHE_EXPIRATION_MINUTES set to {Minutes}", minutes);
        }

        var logging = Environment.GetEnvironmentVariable("DETAILED_LOGGING");
        if (bool.TryParse(logging, out var enabled))
        {
            config.EnableDetailedLogging = enabled;
            this._logger.LogDebug("DETAILED_LOGGING set to {Enabled}", enabled);
        }

        var sprintCount = Environment.GetEnvironmentVariable("DEFAULT_SPRINT_COUNT");
        if (int.TryParse(sprintCount, out var count))
        {
            config.DefaultSprintCount = count;
            this._logger.LogDebug("DEFAULT_SPRINT_COUNT set to {Count}", count);
        }

        var format = Environment.GetEnvironmentVariable("EXPORT_FORMAT");
        if (!string.IsNullOrEmpty(format))
        {
            config.ExportFormat = format;
            this._logger.LogDebug("EXPORT_FORMAT set to {Format}", format);
        }

        var maxRetryAttempts = Environment.GetEnvironmentVariable("JIRA_API_MAX_RETRY_ATTEMPTS");
        if (int.TryParse(maxRetryAttempts, out var retryAttempts))
        {
            config.JiraApiMaxRetryAttempts = retryAttempts;
            this._logger.LogDebug("JIRA_API_MAX_RETRY_ATTEMPTS set to {Attempts}", retryAttempts);
        }

        var circuitBreakerThreshold = Environment.GetEnvironmentVariable("JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD");
        if (int.TryParse(circuitBreakerThreshold, out var threshold))
        {
            config.JiraApiCircuitBreakerFailureThreshold = threshold;
            this._logger.LogDebug("JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD set to {Threshold}", threshold);
        }

        var circuitBreakerDuration = Environment.GetEnvironmentVariable("JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS");
        if (int.TryParse(circuitBreakerDuration, out var duration))
        {
            config.JiraApiCircuitBreakerDurationSeconds = duration;
            this._logger.LogDebug("JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS set to {Duration}", duration);
        }

        var timeoutSeconds = Environment.GetEnvironmentVariable("JIRA_API_TIMEOUT_SECONDS");
        if (int.TryParse(timeoutSeconds, out var timeout))
        {
            config.JiraApiTimeoutSeconds = timeout;
            this._logger.LogDebug("JIRA_API_TIMEOUT_SECONDS set to {Timeout}", timeout);
        }

        var maxResultsPerRequest = Environment.GetEnvironmentVariable("JIRA_API_MAX_RESULTS_PER_REQUEST");
        if (int.TryParse(maxResultsPerRequest, out var maxResults))
        {
            config.JiraApiMaxResultsPerRequest = maxResults;
            this._logger.LogDebug("JIRA_API_MAX_RESULTS_PER_REQUEST set to {MaxResults}", maxResults);
        }

        var maxPages = Environment.GetEnvironmentVariable("JIRA_API_MAX_PAGES");
        if (int.TryParse(maxPages, out var pages))
        {
            config.JiraApiMaxPages = pages;
            this._logger.LogDebug("JIRA_API_MAX_PAGES set to {Pages}", pages);
        }

        var maxPageSize = Environment.GetEnvironmentVariable("JIRA_API_MAX_PAGE_SIZE");
        if (int.TryParse(maxPageSize, out var pageSize))
        {
            config.JiraApiMaxPageSize = pageSize;
            this._logger.LogDebug("JIRA_API_MAX_PAGE_SIZE set to {PageSize}", pageSize);
        }
    }

    private void LoadFromJsonFile(CliConfig config)
    {
        try
        {
            this._logger.LogDebug("Loading configuration from JSON file");

            var json = File.ReadAllText(ConfigFileName);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("jiraBaseUrl", out var jiraUrl))
            {
                config.JiraBaseUrl = jiraUrl.GetString() ?? config.JiraBaseUrl;
            }

            if (root.TryGetProperty("jiraApiToken", out var jiraToken))
            {
                config.JiraApiToken = jiraToken.GetString() ?? config.JiraApiToken;
            }

            if (root.TryGetProperty("defaultProject", out var defaultProject))
            {
                config.DefaultProject = defaultProject.GetString();
            }

            if (root.TryGetProperty("cacheExpirationMinutes", out var cacheMinutes) && cacheMinutes.TryGetInt32(out var minutes))
            {
                config.CacheExpirationMinutes = minutes;
            }

            if (root.TryGetProperty("enableDetailedLogging", out var logging) && (logging.ValueKind == JsonValueKind.True || logging.ValueKind == JsonValueKind.False))
            {
                config.EnableDetailedLogging = logging.GetBoolean();
            }

            if (root.TryGetProperty("defaultSprintCount", out var sprintCount) && sprintCount.TryGetInt32(out var count))
            {
                config.DefaultSprintCount = count;
            }

            if (root.TryGetProperty("exportFormat", out var format))
            {
                config.ExportFormat = format.GetString() ?? config.ExportFormat;
            }

            if (root.TryGetProperty("jiraApiMaxRetryAttempts", out var maxRetryAttempts) && maxRetryAttempts.TryGetInt32(out var retryAttempts))
            {
                config.JiraApiMaxRetryAttempts = retryAttempts;
            }

            if (root.TryGetProperty("jiraApiCircuitBreakerFailureThreshold", out var circuitBreakerThreshold) && circuitBreakerThreshold.TryGetInt32(out var threshold))
            {
                config.JiraApiCircuitBreakerFailureThreshold = threshold;
            }

            if (root.TryGetProperty("jiraApiCircuitBreakerDurationSeconds", out var circuitBreakerDuration) && circuitBreakerDuration.TryGetInt32(out var duration))
            {
                config.JiraApiCircuitBreakerDurationSeconds = duration;
            }

            if (root.TryGetProperty("jiraApiTimeoutSeconds", out var timeoutSeconds) && timeoutSeconds.TryGetInt32(out var timeout))
            {
                config.JiraApiTimeoutSeconds = timeout;
            }

            if (root.TryGetProperty("jiraApiMaxResultsPerRequest", out var maxResultsPerRequest) && maxResultsPerRequest.TryGetInt32(out var maxResults))
            {
                config.JiraApiMaxResultsPerRequest = maxResults;
            }

            if (root.TryGetProperty("jiraApiMaxPages", out var maxPages) && maxPages.TryGetInt32(out var pages))
            {
                config.JiraApiMaxPages = pages;
            }

            if (root.TryGetProperty("jiraApiMaxPageSize", out var maxPageSize) && maxPageSize.TryGetInt32(out var pageSize))
            {
                config.JiraApiMaxPageSize = pageSize;
            }

            this._logger.LogDebug("Configuration successfully loaded from JSON file");
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Error loading configuration from JSON file, using defaults");
        }
    }
}