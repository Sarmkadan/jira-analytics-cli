// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Configuration interface for CLI application settings
/// </summary>
public interface ICliConfig
{
    public string JiraBaseUrl { get; }
    public string JiraApiToken { get; }
    public string? DefaultProject { get; }
    public int CacheExpirationMinutes { get; }
    public bool EnableDetailedLogging { get; }
    public int DefaultSprintCount { get; }
    public string ExportFormat { get; }
    public int JiraApiMaxRetryAttempts { get; }
    public int JiraApiCircuitBreakerFailureThreshold { get; }
    public int JiraApiCircuitBreakerDurationSeconds { get; }
    public int JiraApiTimeoutSeconds { get; }
    /// <summary>
    /// Maximum number of issues to fetch per API request to prevent excessive data retrieval.
    /// Default: 1000
    /// </summary>
    public int JiraApiMaxResultsPerRequest { get; }

    /// <summary>
    /// Maximum number of pages to fetch when streaming results to prevent unbounded pagination loops.
    /// Default: 100
    /// </summary>
    public int JiraApiMaxPages { get; }

    /// <summary>
    /// Maximum allowed page size for pagination operations.
    /// Default: 200
    /// </summary>
    public int JiraApiMaxPageSize { get; }
}
