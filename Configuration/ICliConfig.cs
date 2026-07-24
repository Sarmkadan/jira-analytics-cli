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
    string JiraBaseUrl { get; }
    string JiraApiToken { get; }
    string? DefaultProject { get; }
    int CacheExpirationMinutes { get; }
    bool EnableDetailedLogging { get; }
    int DefaultSprintCount { get; }
    string ExportFormat { get; }
    int JiraApiMaxRetryAttempts { get; }
    int JiraApiCircuitBreakerFailureThreshold { get; }
    int JiraApiCircuitBreakerDurationSeconds { get; }
    int JiraApiTimeoutSeconds { get; }
    /// <summary>
    /// Maximum number of issues to fetch per API request to prevent excessive data retrieval.
    /// Default: 1000
    /// </summary>
    int JiraApiMaxResultsPerRequest { get; }

    /// <summary>
    /// Maximum number of pages to fetch when streaming results to prevent unbounded pagination loops.
    /// Default: 100
    /// </summary>
    int JiraApiMaxPages { get; }

    /// <summary>
    /// Maximum allowed page size for pagination operations.
    /// Default: 200
    /// </summary>
    int JiraApiMaxPageSize { get; }
}
