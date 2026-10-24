// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Jira connection settings. Resolved once at startup by <see cref="JiraOptionsLoader"/> from
/// CLI flags, environment variables and appsettings.json, then validated by <see cref="JiraOptionsValidator"/>
/// before any HTTP call is made.
/// </summary>
public class JiraOptions
{
    /// <summary>Base URL of the Jira instance. Must be an absolute https URI.</summary>
    public string BaseUrl { get; set; } = "https://jira.atlassian.net";

    /// <summary>Account email. Loaded for clients that use Basic auth; the CLI itself sends the Bearer token only.</summary>
    public string? Email { get; set; }

    /// <summary>API token sent as a Bearer token. Required. Never printed in messages.</summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>Default project key. A per-command <c>-p/--project</c> still takes precedence.</summary>
    public string? ProjectKey { get; set; }

    /// <summary>Jira board ID. Optional; when provided it must be positive.</summary>
    public int? BoardId { get; set; }

    /// <summary>HTTP timeout for Jira requests, in seconds. Must be between 1 and 300.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Describes the options for diagnostics. The API token is replaced with a placeholder so it never reaches logs.
    /// </summary>
    /// <returns>A single-line summary without the API token.</returns>
    public override string ToString() =>
        $"JiraOptions {{ BaseUrl = {this.BaseUrl}, ApiToken = <redacted>, ProjectKey = {this.ProjectKey}, " +
        $"BoardId = {this.BoardId}, TimeoutSeconds = {this.TimeoutSeconds} }}";
}
