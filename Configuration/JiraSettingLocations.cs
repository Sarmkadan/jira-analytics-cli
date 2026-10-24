// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Every place a Jira setting can be provided, used to tell the user where to fix a bad value.
/// </summary>
/// <param name="Setting">Name of the <see cref="JiraOptions"/> property.</param>
/// <param name="Flag">CLI flag that sets the value.</param>
/// <param name="EnvVars">Environment variables that set the value, in lookup order.</param>
/// <param name="JsonKey">Key in appsettings.json that sets the value.</param>
public sealed record JiraSettingLocation(string Setting, string Flag, string[] EnvVars, string JsonKey)
{
    /// <summary>Lists every place this setting can be set.</summary>
    public string Describe() =>
        $"CLI flag {this.Flag}, env {string.Join(" or ", this.EnvVars)}, or \"{this.JsonKey}\" in appsettings.json";

    /// <summary>
    /// Builds a problem message that names the setting, the offending value (when it is not a secret)
    /// and where to set it.
    /// </summary>
    public string Problem(string message, string? offendingValue = null)
    {
        var got = offendingValue is null ? string.Empty : $" (got '{offendingValue}')";
        return $"{this.Setting} {message}{got}. Set it via {this.Describe()}.";
    }
}

/// <summary>
/// Locations for every <see cref="JiraOptions"/> setting.
/// </summary>
public static class JiraSettingLocations
{
    public static readonly JiraSettingLocation BaseUrl =
        new("BaseUrl", "--jira-url", ["JIRA_URL", "JIRA_BASE_URL"], "jiraBaseUrl");

    public static readonly JiraSettingLocation Email =
        new("Email", "--jira-email", ["JIRA_EMAIL"], "jiraEmail");

    public static readonly JiraSettingLocation ApiToken =
        new("ApiToken", "--jira-token", ["JIRA_API_TOKEN", "JIRA_TOKEN"], "jiraApiToken");

    public static readonly JiraSettingLocation ProjectKey =
        new("ProjectKey", "-p/--project (per command)", ["JIRA_PROJECT", "JIRA_DEFAULT_PROJECT"], "defaultProject");

    public static readonly JiraSettingLocation BoardId =
        new("BoardId", "--board-id", ["JIRA_BOARD_ID"], "jiraBoardId");

    public static readonly JiraSettingLocation TimeoutSeconds =
        new("TimeoutSeconds", "--timeout-seconds", ["JIRA_API_TIMEOUT_SECONDS"], "jiraApiTimeoutSeconds");
}
