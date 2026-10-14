// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Settings for <c>VelocityTracker</c>. The constructor throws <see cref="ArgumentException"/> when a value is
/// outside its allowed range, so an invalid instance cannot be created. Bind from CLI flags, environment
/// variables and appsettings.json with <see cref="VelocityOptionsLoader"/>.
/// </summary>
public sealed record VelocityOptions
{
    /// <summary>Default story-points field id, the one used by most Jira Cloud sites. Override it when your site differs.</summary>
    public const string DefaultStoryPointsField = "customfield_10016";

    /// <summary>
    /// Creates validated velocity settings.
    /// </summary>
    /// <param name="windowSize">Number of most recent sprints in the moving average. Must be 1 to 20.</param>
    /// <param name="excludeZeroPointSprints">When true, sprints with zero completed story points are left out of the calculations.</param>
    /// <param name="storyPointsField">Jira custom field id that holds story points, in the form <c>customfield_XXXXX</c>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="windowSize"/> or <paramref name="storyPointsField"/> is invalid.</exception>
    public VelocityOptions(
        int windowSize = 3,
        bool excludeZeroPointSprints = false,
        string storyPointsField = DefaultStoryPointsField)
    {
        VelocityOptionsValidator.ThrowIfInvalid(windowSize, storyPointsField);

        WindowSize = windowSize;
        ExcludeZeroPointSprints = excludeZeroPointSprints;
        StoryPointsField = storyPointsField;
    }

    /// <summary>Number of most recent sprints in the moving average. Must be 1 to 20.</summary>
    public int WindowSize { get; init; }

    /// <summary>When true, sprints with zero completed story points are left out of the calculations.</summary>
    public bool ExcludeZeroPointSprints { get; init; }

    /// <summary>Jira custom field id that holds story points, in the form <c>customfield_XXXXX</c>.</summary>
    public string StoryPointsField { get; init; }
}
