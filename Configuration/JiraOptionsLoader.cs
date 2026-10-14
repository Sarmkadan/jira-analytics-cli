// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text.Json;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Raw values given as CLI flags. A null property means the flag was not passed.
/// </summary>
public sealed record JiraOptionsOverrides(
    string? BaseUrl = null,
    string? Email = null,
    string? ApiToken = null,
    string? BoardId = null,
    string? TimeoutSeconds = null);

/// <summary>
/// Result of <see cref="JiraOptionsLoader.Load"/>: the resolved options plus every problem found.
/// </summary>
public sealed record JiraOptionsLoadResult(JiraOptions Options, IReadOnlyList<string> Problems)
{
    /// <summary>True when no problems were found.</summary>
    public bool IsValid => Problems.Count == 0;
}

/// <summary>
/// Resolves Jira settings from CLI flags, environment variables and appsettings.json.
/// </summary>
/// <remarks>
/// Precedence, highest first: CLI flag, environment variable, appsettings.json, built-in default.
/// An empty or whitespace environment variable or JSON value counts as unset and falls through to the next source.
/// A CLI flag that is passed is always used, even when empty, so that an empty value is reported as a problem.
/// Unparseable values and validation failures are all collected in <see cref="JiraOptionsLoadResult.Problems"/>.
/// </remarks>
public static class JiraOptionsLoader
{
    /// <summary>Name of the JSON settings file, read from the current directory.</summary>
    public const string AppSettingsFileName = "appsettings.json";

    /// <summary>
    /// Resolves the settings and validates them.
    /// </summary>
    /// <param name="overrides">Values from CLI flags.</param>
    /// <param name="getEnv">Environment variable lookup, e.g. <see cref="Environment.GetEnvironmentVariable(string)"/>.</param>
    /// <param name="appSettingsJson">Contents of appsettings.json, or null when the file does not exist.</param>
    /// <returns>The resolved options and all problems found.</returns>
    public static JiraOptionsLoadResult Load(JiraOptionsOverrides overrides, Func<string, string?> getEnv, string? appSettingsJson)
    {
        ArgumentNullException.ThrowIfNull(overrides);
        ArgumentNullException.ThrowIfNull(getEnv);

        var problems = new List<string>();
        var json = ParseAppSettings(appSettingsJson, problems);
        var options = new JiraOptions();

        options.BaseUrl = Resolve(overrides.BaseUrl, JiraSettingLocations.BaseUrl, getEnv, json) ?? options.BaseUrl;
        options.Email = Resolve(overrides.Email, JiraSettingLocations.Email, getEnv, json);
        options.ApiToken = Resolve(overrides.ApiToken, JiraSettingLocations.ApiToken, getEnv, json) ?? string.Empty;
        options.ProjectKey = Resolve(null, JiraSettingLocations.ProjectKey, getEnv, json);

        var boardId = Resolve(overrides.BoardId, JiraSettingLocations.BoardId, getEnv, json);
        if (boardId is not null)
        {
            if (int.TryParse(boardId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedBoardId))
                options.BoardId = parsedBoardId;
            else
                problems.Add(JiraSettingLocations.BoardId.Problem("must be a whole number", boardId));
        }

        var timeout = Resolve(overrides.TimeoutSeconds, JiraSettingLocations.TimeoutSeconds, getEnv, json);
        if (timeout is not null)
        {
            if (int.TryParse(timeout, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedTimeout))
                options.TimeoutSeconds = parsedTimeout;
            else
                problems.Add(JiraSettingLocations.TimeoutSeconds.Problem("must be a whole number", timeout));
        }

        problems.AddRange(JiraOptionsValidator.Validate(options));

        return new JiraOptionsLoadResult(options, problems);
    }

    /// <summary>
    /// Formats problems as one message that lists each of them, for printing to stderr.
    /// </summary>
    public static string FormatProblems(IReadOnlyCollection<string> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);

        var lines = problems.Select(problem => $"  - {problem}");
        return $"Invalid Jira configuration ({problems.Count} problem(s)):{Environment.NewLine}"
            + string.Join(Environment.NewLine, lines)
            + $"{Environment.NewLine}Fix the settings above and run the command again.";
    }

    private static string? Resolve(string? cliValue, JiraSettingLocation location, Func<string, string?> getEnv, JsonElement? json)
    {
        if (cliValue is not null)
            return cliValue;

        foreach (var name in location.EnvVars)
        {
            var envValue = getEnv(name);
            if (!string.IsNullOrWhiteSpace(envValue))
                return envValue;
        }

        if (json is { } root && root.TryGetProperty(location.JsonKey, out var element) && element.ValueKind != JsonValueKind.Null)
        {
            var jsonValue = element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText();
            if (!string.IsNullOrWhiteSpace(jsonValue))
                return jsonValue;
        }

        return null;
    }

    private static JsonElement? ParseAppSettings(string? text, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        try
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                problems.Add($"{AppSettingsFileName} must contain a JSON object at the top level. Fix the file.");
                return null;
            }

            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            problems.Add($"{AppSettingsFileName} is not valid JSON ({ex.Message}). Fix the file.");
            return null;
        }
    }
}
