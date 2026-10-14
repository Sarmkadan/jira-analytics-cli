// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text.Json;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Raw velocity values given as CLI flags. A null property means the flag was not passed.
/// </summary>
public sealed record VelocityOptionsOverrides(
    string? WindowSize = null,
    string? ExcludeZeroPointSprints = null,
    string? StoryPointsField = null);

/// <summary>
/// Result of <see cref="VelocityOptionsLoader.Load"/>: the options when they are valid, plus every problem found.
/// </summary>
public sealed record VelocityOptionsLoadResult(VelocityOptions? Options, IReadOnlyList<string> Problems)
{
    /// <summary>True when no problems were found and <see cref="Options"/> is set.</summary>
    public bool IsValid => Problems.Count == 0;
}

/// <summary>
/// Resolves <see cref="VelocityOptions"/> from CLI flags, environment variables and appsettings.json.
/// </summary>
/// <remarks>
/// Precedence, highest first: CLI flag, environment variable, appsettings.json, built-in default.
/// An empty or whitespace environment variable or JSON value counts as unset and falls through to the next source.
/// Unparseable values and out-of-range values are all collected in <see cref="VelocityOptionsLoadResult.Problems"/>.
/// </remarks>
public static class VelocityOptionsLoader
{
    /// <summary>Where the window size can be set.</summary>
    public static readonly JiraSettingLocation WindowSizeLocation =
        new("WindowSize", "--velocity-window", ["VELOCITY_WINDOW_SIZE"], "velocityWindowSize");

    /// <summary>Where the zero-point exclusion can be set.</summary>
    public static readonly JiraSettingLocation ExcludeZeroPointSprintsLocation =
        new("ExcludeZeroPointSprints", "--velocity-exclude-zero-points", ["VELOCITY_EXCLUDE_ZERO_POINTS"], "velocityExcludeZeroPoints");

    /// <summary>Where the story-points field id can be set.</summary>
    public static readonly JiraSettingLocation StoryPointsFieldLocation =
        new("StoryPointsField", "--story-points-field", ["VELOCITY_STORY_POINTS_FIELD"], "velocityStoryPointsField");

    /// <summary>
    /// Resolves the velocity settings and validates them.
    /// </summary>
    /// <param name="overrides">Values from CLI flags.</param>
    /// <param name="getEnv">Environment variable lookup, e.g. <see cref="Environment.GetEnvironmentVariable(string)"/>.</param>
    /// <param name="appSettingsJson">Contents of appsettings.json, or null when the file does not exist.</param>
    /// <returns>The options (null when invalid) and all problems found.</returns>
    public static VelocityOptionsLoadResult Load(VelocityOptionsOverrides overrides, Func<string, string?> getEnv, string? appSettingsJson)
    {
        ArgumentNullException.ThrowIfNull(overrides);
        ArgumentNullException.ThrowIfNull(getEnv);

        var problems = new List<string>();
        var json = ParseAppSettings(appSettingsJson, problems);

        int windowSize = new VelocityOptions().WindowSize;
        var windowText = Resolve(overrides.WindowSize, WindowSizeLocation, getEnv, json);
        if (windowText is not null)
        {
            if (!int.TryParse(windowText, NumberStyles.Integer, CultureInfo.InvariantCulture, out windowSize))
                problems.Add(WindowSizeLocation.Problem($"must be a whole number from {VelocityOptionsValidator.MinWindowSize} to {VelocityOptionsValidator.MaxWindowSize}", windowText));
            else if (!VelocityOptionsValidator.IsValidWindowSize(windowSize))
                problems.Add(WindowSizeLocation.Problem($"must be between {VelocityOptionsValidator.MinWindowSize} and {VelocityOptionsValidator.MaxWindowSize}", windowText));
        }

        bool excludeZero = new VelocityOptions().ExcludeZeroPointSprints;
        var excludeText = Resolve(overrides.ExcludeZeroPointSprints, ExcludeZeroPointSprintsLocation, getEnv, json);
        if (excludeText is not null && !bool.TryParse(excludeText, out excludeZero))
            problems.Add(ExcludeZeroPointSprintsLocation.Problem("must be true or false", excludeText));

        var storyPointsField = Resolve(overrides.StoryPointsField, StoryPointsFieldLocation, getEnv, json)
            ?? VelocityOptions.DefaultStoryPointsField;
        if (!VelocityOptionsValidator.IsValidStoryPointsField(storyPointsField))
            problems.Add(StoryPointsFieldLocation.Problem("must match customfield_<digits>, for example customfield_10016", storyPointsField));

        if (problems.Count > 0)
            return new VelocityOptionsLoadResult(null, problems);

        return new VelocityOptionsLoadResult(
            new VelocityOptions(windowSize, excludeZero, storyPointsField),
            problems);
    }

    /// <summary>
    /// Formats problems as one message that lists each of them, for printing to stderr.
    /// </summary>
    public static string FormatProblems(IReadOnlyCollection<string> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);

        var lines = problems.Select(problem => $"  - {problem}");
        return $"Invalid velocity configuration ({problems.Count} problem(s)):{Environment.NewLine}"
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
                problems.Add($"{JiraOptionsLoader.AppSettingsFileName} must contain a JSON object at the top level. Fix the file.");
                return null;
            }

            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            problems.Add($"{JiraOptionsLoader.AppSettingsFileName} is not valid JSON ({ex.Message}). Fix the file.");
            return null;
        }
    }
}
