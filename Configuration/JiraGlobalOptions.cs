// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.CommandLine;
using System.CommandLine.Parsing;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Global CLI flags that override Jira settings. They are added to every command and read before startup validation.
/// </summary>
public sealed record JiraGlobalOptions(
    Option<string> BaseUrl,
    Option<string> Email,
    Option<string> ApiToken,
    Option<string> BoardId,
    Option<string> TimeoutSeconds)
{
    /// <summary>
    /// Creates the Jira flags and adds them to <paramref name="rootCommand"/> as global options.
    /// </summary>
    /// <param name="rootCommand">Root command that receives the flags.</param>
    /// <returns>The created flags, for reading their values from a parse result.</returns>
    public static JiraGlobalOptions AddTo(RootCommand rootCommand)
    {
        ArgumentNullException.ThrowIfNull(rootCommand);

        var options = new JiraGlobalOptions(
            BaseUrl: new Option<string>("--jira-url", "Jira base URL (overrides JIRA_URL and appsettings.json)"),
            Email: new Option<string>("--jira-email", "Jira account email (overrides JIRA_EMAIL and appsettings.json)"),
            ApiToken: new Option<string>("--jira-token", "Jira API token (overrides JIRA_API_TOKEN and appsettings.json; prefer the env var, since flags are visible in process listings)"),
            BoardId: new Option<string>("--board-id", "Jira board ID (overrides JIRA_BOARD_ID and appsettings.json)"),
            TimeoutSeconds: new Option<string>("--timeout-seconds", "Jira request timeout in seconds, 1-300 (overrides JIRA_API_TIMEOUT_SECONDS and appsettings.json)"));

        foreach (var option in new Option[] { options.BaseUrl, options.Email, options.ApiToken, options.BoardId, options.TimeoutSeconds })
        {
            rootCommand.AddGlobalOption(option);
        }

        return options;
    }

    /// <summary>
    /// Reads the flag values that were passed on the command line.
    /// </summary>
    /// <param name="parseResult">Result of parsing the command line.</param>
    /// <returns>Overrides for the flags that were given; null for the rest.</returns>
    public JiraOptionsOverrides Read(ParseResult parseResult)
    {
        ArgumentNullException.ThrowIfNull(parseResult);

        return new JiraOptionsOverrides(
            BaseUrl: parseResult.GetValueForOption(this.BaseUrl),
            Email: parseResult.GetValueForOption(this.Email),
            ApiToken: parseResult.GetValueForOption(this.ApiToken),
            BoardId: parseResult.GetValueForOption(this.BoardId),
            TimeoutSeconds: parseResult.GetValueForOption(this.TimeoutSeconds));
    }
}
