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

        // Load from environment variables (highest priority)
        LoadFromEnvironment(config);

        // Load from JSON file if exists
        if (File.Exists(ConfigFileName))
        {
            _logger.LogInformation("Found configuration file: {ConfigFile}", ConfigFileName);
            LoadFromJsonFile(config);
        }

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

        var jiraUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL");
        if (!string.IsNullOrEmpty(jiraUrl))
        {
            config.JiraBaseUrl = jiraUrl;
            _logger.LogDebug("JIRA_BASE_URL set from environment");
        }

        var jiraToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
        if (!string.IsNullOrEmpty(jiraToken))
        {
            config.JiraApiToken = jiraToken;
            _logger.LogDebug("JIRA_API_TOKEN set from environment");
        }

        var defaultProject = Environment.GetEnvironmentVariable("JIRA_DEFAULT_PROJECT");
        if (!string.IsNullOrEmpty(defaultProject))
        {
            config.DefaultProject = defaultProject;
            _logger.LogDebug("JIRA_DEFAULT_PROJECT set from environment");
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

            if (root.TryGetProperty("enableDetailedLogging", out var logging) && logging.TryGetBoolean(out var enabled))
                config.EnableDetailedLogging = enabled;

            if (root.TryGetProperty("defaultSprintCount", out var sprintCount) && sprintCount.TryGetInt32(out var count))
                config.DefaultSprintCount = count;

            if (root.TryGetProperty("exportFormat", out var format))
                config.ExportFormat = format.GetString() ?? config.ExportFormat;

            _logger.LogDebug("Configuration successfully loaded from JSON file");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading configuration from JSON file, using defaults");
        }
    }
}
