// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Startup validation rules for <see cref="JiraOptions"/>. Reports every violation, not just the first.
/// </summary>
public static class JiraOptionsValidator
{
    /// <summary>Smallest allowed <see cref="JiraOptions.TimeoutSeconds"/>.</summary>
    public const int MinTimeoutSeconds = 1;

    /// <summary>Largest allowed <see cref="JiraOptions.TimeoutSeconds"/>.</summary>
    public const int MaxTimeoutSeconds = 300;

    /// <summary>
    /// Checks the options against every rule.
    /// </summary>
    /// <param name="options">Resolved options to check.</param>
    /// <returns>One message per violated rule; empty when the options are valid.</returns>
    public static IReadOnlyList<string> Validate(JiraOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var problems = new List<string>();

        if (!IsAbsoluteHttpsUri(options.BaseUrl))
        {
            // A URL containing '@' may carry user-info credentials, so it is not echoed back.
            var echoedUrl = options.BaseUrl is { } url && !url.Contains('@') ? url : null;
            problems.Add(JiraSettingLocations.BaseUrl.Problem("must be an absolute https URI, e.g. https://your-company.atlassian.net", echoedUrl));
        }

        // The token is a secret, so the offending value is deliberately not echoed.
        if (string.IsNullOrWhiteSpace(options.ApiToken))
        {
            problems.Add(JiraSettingLocations.ApiToken.Problem("must not be empty"));
        }

        if (options.BoardId is { } boardId && boardId <= 0)
        {
            problems.Add(JiraSettingLocations.BoardId.Problem("must be a positive integer", boardId.ToString(CultureInfo.InvariantCulture)));
        }

        if (options.TimeoutSeconds < MinTimeoutSeconds || options.TimeoutSeconds > MaxTimeoutSeconds)
        {
            problems.Add(JiraSettingLocations.TimeoutSeconds.Problem(
                $"must be between {MinTimeoutSeconds} and {MaxTimeoutSeconds} seconds",
                options.TimeoutSeconds.ToString(CultureInfo.InvariantCulture)));
        }

        return problems;
    }

    private static bool IsAbsoluteHttpsUri(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && !string.IsNullOrEmpty(uri.Host);
}
