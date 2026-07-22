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
}
