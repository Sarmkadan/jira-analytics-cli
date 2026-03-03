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
    public string JiraBaseUrl { get; set; } = "https://jira.atlassian.net";
    public string JiraApiToken { get; set; } = string.Empty;
    public string? DefaultProject { get; set; }
    public int CacheExpirationMinutes { get; set; } = 15;
    public bool EnableDetailedLogging { get; set; } = false;
    public int DefaultSprintCount { get; set; } = 5;
    public string ExportFormat { get; set; } = "txt";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(JiraBaseUrl))
            throw new InvalidOperationException("JiraBaseUrl cannot be empty");

        if (string.IsNullOrWhiteSpace(JiraApiToken))
            throw new InvalidOperationException("JiraApiToken must be set. Set JIRA_API_TOKEN environment variable");

        if (CacheExpirationMinutes < 0)
            throw new InvalidOperationException("CacheExpirationMinutes must be non-negative");

        if (DefaultSprintCount <= 0)
            throw new InvalidOperationException("DefaultSprintCount must be positive");
    }
}
