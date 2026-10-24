// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.CommandLine;
using System.CommandLine.Parsing;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Global CLI flags that override velocity settings. They are added to every command and read before startup validation.
/// </summary>
public sealed record VelocityGlobalOptions(
    Option<string> WindowSize,
    Option<string> ExcludeZeroPointSprints,
    Option<string> StoryPointsField)
{
    /// <summary>
    /// Creates the velocity flags and adds them to <paramref name="rootCommand"/> as global options.
    /// </summary>
    /// <param name="rootCommand">Root command that receives the flags.</param>
    /// <returns>The created flags, for reading their values from a parse result.</returns>
    public static VelocityGlobalOptions AddTo(RootCommand rootCommand)
    {
        ArgumentNullException.ThrowIfNull(rootCommand);

        var options = new VelocityGlobalOptions(
            WindowSize: new Option<string>("--velocity-window", "Moving-average window in sprints, 1-20 (overrides VELOCITY_WINDOW_SIZE and appsettings.json)"),
            ExcludeZeroPointSprints: new Option<string>("--velocity-exclude-zero-points", "Leave out sprints with zero completed story points, true or false (overrides VELOCITY_EXCLUDE_ZERO_POINTS and appsettings.json)"),
            StoryPointsField: new Option<string>("--story-points-field", "Jira story-points custom field id, e.g. customfield_10016 (overrides VELOCITY_STORY_POINTS_FIELD and appsettings.json)"));

        foreach (var option in new Option[] { options.WindowSize, options.ExcludeZeroPointSprints, options.StoryPointsField })
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
    public VelocityOptionsOverrides Read(ParseResult parseResult)
    {
        ArgumentNullException.ThrowIfNull(parseResult);

        return new VelocityOptionsOverrides(
            WindowSize: parseResult.GetValueForOption(this.WindowSize),
            ExcludeZeroPointSprints: parseResult.GetValueForOption(this.ExcludeZeroPointSprints),
            StoryPointsField: parseResult.GetValueForOption(this.StoryPointsField));
    }
}
