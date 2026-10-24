// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text.Json;
using JiraAnalyticsCli.Exceptions;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Configuration provider that loads settings from environment variables, JSON config file, and defaults
/// </summary>
public interface IConfigurationProvider
{
    public ICliConfig LoadConfiguration();
}

/// <summary>
/// Builds <see cref="ICliConfig"/> from appsettings.json, environment variables and the resolved <see cref="JiraOptions"/>.
/// Numeric environment variables are rejected when they are not whole numbers or fall outside their allowed range,
/// so a bad value can neither be silently ignored nor set an unbounded page size or a zero timeout.
/// </summary>
public class AppConfigurationProvider : IConfigurationProvider
{
    private readonly ILogger<AppConfigurationProvider> _logger;
    private readonly JiraOptions _jiraOptions;
    private readonly Func<string, string?> _getEnv;
    private const string ConfigFileName = "appsettings.json";

    /// <summary>Upper bound for CACHE_EXPIRATION_MINUTES (one week).</summary>
    private const int MaxCacheExpirationMinutes = 10080;

    /// <summary>Upper bound for DEFAULT_SPRINT_COUNT.</summary>
    private const int MaxDefaultSprintCount = 100;

    /// <summary>Upper bound for JIRA_API_MAX_RETRY_ATTEMPTS.</summary>
    private const int MaxRetryAttempts = 10;

    /// <summary>Upper bound for JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD.</summary>
    private const int MaxCircuitBreakerFailureThreshold = 100;

    /// <summary>Upper bound for JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS (one hour).</summary>
    private const int MaxCircuitBreakerDurationSeconds = 3600;

    /// <summary>Upper bound for JIRA_API_MAX_RESULTS_PER_REQUEST.</summary>
    private const int MaxResultsPerRequestLimit = 1000;

    /// <summary>Upper bound for JIRA_API_MAX_PAGES.</summary>
    private const int MaxPagesLimit = 1000;

    /// <summary>Upper bound for JIRA_API_MAX_PAGE_SIZE.</summary>
    private const int MaxPageSizeLimit = 1000;

    /// <summary>
    /// Creates a provider that reads environment variables from the process.
    /// </summary>
    /// <param name="logger">Logger for configuration diagnostics.</param>
    /// <param name="jiraOptions">Jira settings already resolved from CLI flags, environment and appsettings.json.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="jiraOptions"/> is <see langword="null"/>.</exception>
    public AppConfigurationProvider(ILogger<AppConfigurationProvider> logger, JiraOptions jiraOptions)
        : this(logger, jiraOptions, Environment.GetEnvironmentVariable)
    {
    }

    /// <summary>
    /// Creates a provider that reads environment variables through <paramref name="getEnv"/>, so tests can supply their own.
    /// </summary>
    /// <param name="logger">Logger for configuration diagnostics.</param>
    /// <param name="jiraOptions">Jira settings already resolved from CLI flags, environment and appsettings.json.</param>
    /// <param name="getEnv">Environment variable lookup.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/>, <paramref name="jiraOptions"/> or <paramref name="getEnv"/> is <see langword="null"/>.</exception>
    internal AppConfigurationProvider(ILogger<AppConfigurationProvider> logger, JiraOptions jiraOptions, Func<string, string?> getEnv)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(jiraOptions);
        ArgumentNullException.ThrowIfNull(getEnv);

        this._logger = logger;
        this._jiraOptions = jiraOptions;
        this._getEnv = getEnv;
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

        var jiraUrl = this._getEnv("JIRA_URL") ?? this._getEnv("JIRA_BASE_URL");
        if (!string.IsNullOrEmpty(jiraUrl))
        {
            config.JiraBaseUrl = jiraUrl;
            this._logger.LogDebug("JIRA_URL/JIRA_BASE_URL set from environment");
        }

        // The token is never logged or put into an exception message: only the variable name is mentioned.
        var jiraToken = this._getEnv("JIRA_TOKEN") ?? this._getEnv("JIRA_API_TOKEN");
        if (!string.IsNullOrEmpty(jiraToken))
        {
            config.JiraApiToken = jiraToken;
            this._logger.LogDebug("JIRA_TOKEN/JIRA_API_TOKEN set from environment");
        }

        var defaultProject = this._getEnv("JIRA_PROJECT") ?? this._getEnv("JIRA_DEFAULT_PROJECT");
        if (!string.IsNullOrEmpty(defaultProject))
        {
            config.DefaultProject = defaultProject;
            this._logger.LogDebug("JIRA_PROJECT/JIRA_DEFAULT_PROJECT set from environment");
        }

        if (this.ReadEnvironmentInt("CACHE_EXPIRATION_MINUTES", 0, MaxCacheExpirationMinutes) is { } minutes)
        {
            config.CacheExpirationMinutes = minutes;
            this._logger.LogDebug("CACHE_EXPIRATION_MINUTES set to {Minutes}", minutes);
        }

        var logging = this._getEnv("DETAILED_LOGGING");
        if (bool.TryParse(logging, out var enabled))
        {
            config.EnableDetailedLogging = enabled;
            this._logger.LogDebug("DETAILED_LOGGING set to {Enabled}", enabled);
        }

        if (this.ReadEnvironmentInt("DEFAULT_SPRINT_COUNT", 1, MaxDefaultSprintCount) is { } count)
        {
            config.DefaultSprintCount = count;
            this._logger.LogDebug("DEFAULT_SPRINT_COUNT set to {Count}", count);
        }

        var format = this._getEnv("EXPORT_FORMAT");
        if (!string.IsNullOrEmpty(format))
        {
            config.ExportFormat = format;
            this._logger.LogDebug("EXPORT_FORMAT set to {Format}", format);
        }

        if (this.ReadEnvironmentInt("JIRA_API_MAX_RETRY_ATTEMPTS", 1, MaxRetryAttempts) is { } retryAttempts)
        {
            config.JiraApiMaxRetryAttempts = retryAttempts;
            this._logger.LogDebug("JIRA_API_MAX_RETRY_ATTEMPTS set to {Attempts}", retryAttempts);
        }

        if (this.ReadEnvironmentInt("JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD", 1, MaxCircuitBreakerFailureThreshold) is { } threshold)
        {
            config.JiraApiCircuitBreakerFailureThreshold = threshold;
            this._logger.LogDebug("JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD set to {Threshold}", threshold);
        }

        if (this.ReadEnvironmentInt("JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS", 1, MaxCircuitBreakerDurationSeconds) is { } duration)
        {
            config.JiraApiCircuitBreakerDurationSeconds = duration;
            this._logger.LogDebug("JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS set to {Duration}", duration);
        }

        if (this.ReadEnvironmentInt("JIRA_API_TIMEOUT_SECONDS", JiraOptionsValidator.MinTimeoutSeconds, JiraOptionsValidator.MaxTimeoutSeconds) is { } timeout)
        {
            config.JiraApiTimeoutSeconds = timeout;
            this._logger.LogDebug("JIRA_API_TIMEOUT_SECONDS set to {Timeout}", timeout);
        }

        if (this.ReadEnvironmentInt("JIRA_API_MAX_RESULTS_PER_REQUEST", 1, MaxResultsPerRequestLimit) is { } maxResults)
        {
            config.JiraApiMaxResultsPerRequest = maxResults;
            this._logger.LogDebug("JIRA_API_MAX_RESULTS_PER_REQUEST set to {MaxResults}", maxResults);
        }

        if (this.ReadEnvironmentInt("JIRA_API_MAX_PAGES", 1, MaxPagesLimit) is { } pages)
        {
            config.JiraApiMaxPages = pages;
            this._logger.LogDebug("JIRA_API_MAX_PAGES set to {Pages}", pages);
        }

        if (this.ReadEnvironmentInt("JIRA_API_MAX_PAGE_SIZE", 1, MaxPageSizeLimit) is { } pageSize)
        {
            config.JiraApiMaxPageSize = pageSize;
            this._logger.LogDebug("JIRA_API_MAX_PAGE_SIZE set to {PageSize}", pageSize);
        }
    }

    /// <summary>
    /// Reads an integer environment variable. An unset or empty variable returns <see langword="null"/>;
    /// anything else must be a whole number within [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <param name="name">Environment variable name.</param>
    /// <param name="min">Smallest allowed value, inclusive.</param>
    /// <param name="max">Largest allowed value, inclusive.</param>
    /// <returns>The parsed value, or <see langword="null"/> when the variable is unset or empty.</returns>
    /// <exception cref="ConfigurationException">The value is not a whole number or is outside the allowed range.</exception>
    private int? ReadEnvironmentInt(string name, int min, int max)
    {
        var raw = this._getEnv(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        // The raw value is not echoed: it could be a credential pasted into the wrong variable.
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value < min || value > max)
        {
            throw new ConfigurationException(
                $"{name} must be a whole number from {min} to {max}. Fix the environment variable and run the command again.",
                name);
        }

        return value;
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
        catch (JsonException ex)
        {
            // JsonException messages quote the offending character, which may belong to a token, so only the position is logged.
            this._logger.LogWarning("Invalid JSON in {ConfigFile} at line {Line}, byte {Position}. Using defaults", ConfigFileName, ex.LineNumber, ex.BytePositionInLine);
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Error loading configuration from JSON file, using defaults");
        }
    }
}