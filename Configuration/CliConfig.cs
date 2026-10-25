// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Configuration implementation for CLI application settings with defaults
/// </summary>
public class CliConfig : ICliConfig
{
    private const string JiraBaseUrlDefault = "https://jira.atlassian.net";
    private const int CacheExpirationMinutesDefault = 15;
    private const bool EnableDetailedLoggingDefault = false;
    private const int DefaultSprintCountDefault = 5;
    private const string ExportFormatDefault = "txt";
    private const int JiraApiMaxRetryAttemptsDefault = 5;
    private const int JiraApiCircuitBreakerFailureThresholdDefault = 5;
    private const int JiraApiCircuitBreakerDurationSecondsDefault = 30;
    private const int JiraApiTimeoutSecondsDefault = 30;
    private const int JiraApiMaxResultsPerRequestDefault = 1000;
    private const int JiraApiMaxPagesDefault = 100;
    private const int JiraApiMaxPageSizeDefault = 200;

    private const string HttpScheme = "http";
    private const string HttpsScheme = "https";
    private const string RedactedValue = "<redacted>";

    public string JiraBaseUrl { get; set; } = JiraBaseUrlDefault;
    public string JiraApiToken { get; set; } = string.Empty;
    public string? DefaultProject { get; set; }
    public int CacheExpirationMinutes { get; set; } = CacheExpirationMinutesDefault;
    public bool EnableDetailedLogging { get; set; } = EnableDetailedLoggingDefault;
    public int DefaultSprintCount { get; set; } = DefaultSprintCountDefault;
    public string ExportFormat { get; set; } = ExportFormatDefault;
    public int JiraApiMaxRetryAttempts { get; set; } = JiraApiMaxRetryAttemptsDefault;
    public int JiraApiCircuitBreakerFailureThreshold { get; set; } = JiraApiCircuitBreakerFailureThresholdDefault;
    public int JiraApiCircuitBreakerDurationSeconds { get; set; } = JiraApiCircuitBreakerDurationSecondsDefault;
    public int JiraApiTimeoutSeconds { get; set; } = JiraApiTimeoutSecondsDefault;
    public int JiraApiMaxResultsPerRequest { get; set; } = JiraApiMaxResultsPerRequestDefault;
    public int JiraApiMaxPages { get; set; } = JiraApiMaxPagesDefault;
    public int JiraApiMaxPageSize { get; set; } = JiraApiMaxPageSizeDefault;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.JiraBaseUrl))
        {
            throw new InvalidOperationException("JiraBaseUrl cannot be empty");
        }

        // The URL is not echoed: it may carry user info, and it is also the host every request is sent to.
        if (!Uri.TryCreate(this.JiraBaseUrl, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme is not (HttpScheme or HttpsScheme)
            || string.IsNullOrEmpty(baseUri.Host))
        {
            throw new InvalidOperationException("JiraBaseUrl must be an absolute http or https URL");
        }

        if (string.IsNullOrWhiteSpace(this.JiraApiToken))
        {
            throw new InvalidOperationException("JiraApiToken must be set. Set JIRA_API_TOKEN environment variable");
        }

        if (this.CacheExpirationMinutes < 0)
        {
            throw new InvalidOperationException("CacheExpirationMinutes must be non-negative");
        }

        if (this.DefaultSprintCount <= 0)
        {
            throw new InvalidOperationException("DefaultSprintCount must be positive");
        }

        if (this.JiraApiMaxRetryAttempts <= 0)
        {
            throw new InvalidOperationException("JiraApiMaxRetryAttempts must be positive");
        }

        if (this.JiraApiCircuitBreakerFailureThreshold <= 0)
        {
            throw new InvalidOperationException("JiraApiCircuitBreakerFailureThreshold must be positive");
        }

        if (this.JiraApiCircuitBreakerDurationSeconds <= 0)
        {
            throw new InvalidOperationException("JiraApiCircuitBreakerDurationSeconds must be positive");
        }

        if (this.JiraApiTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("JiraApiTimeoutSeconds must be positive");
        }

        if (this.JiraApiMaxResultsPerRequest <= 0)
        {
            throw new InvalidOperationException("JiraApiMaxResultsPerRequest must be positive");
        }

        if (this.JiraApiMaxPages <= 0)
        {
            throw new InvalidOperationException("JiraApiMaxPages must be positive");
        }

        if (this.JiraApiMaxPageSize <= 0)
        {
            throw new InvalidOperationException("JiraApiMaxPageSize must be positive");
        }
    }

    /// <summary>
    /// Describes the settings for diagnostics. The API token is replaced with a placeholder so it never reaches logs.
    /// </summary>
    /// <returns>A single-line summary without the API token.</returns>
    public override string ToString() =>
        $"CliConfig {{ JiraBaseUrl = {this.JiraBaseUrl}, JiraApiToken = {RedactedValue}, DefaultProject = {this.DefaultProject}, " +
        $"JiraApiTimeoutSeconds = {this.JiraApiTimeoutSeconds}, JiraApiMaxPageSize = {this.JiraApiMaxPageSize}, JiraApiMaxPages = {this.JiraApiMaxPages} }}";
}