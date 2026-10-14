// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text.RegularExpressions;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Rules for <see cref="VelocityOptions"/>, shared by the record constructor, the loader and the tracker.
/// </summary>
public static class VelocityOptionsValidator
{
    /// <summary>Smallest allowed <see cref="VelocityOptions.WindowSize"/>.</summary>
    public const int MinWindowSize = 1;

    /// <summary>Largest allowed <see cref="VelocityOptions.WindowSize"/>.</summary>
    public const int MaxWindowSize = 20;

    /// <summary>Format of <see cref="VelocityOptions.StoryPointsField"/>. <c>\z</c> rejects a trailing newline.</summary>
    private static readonly Regex StoryPointsFieldPattern = new(@"^customfield_[0-9]+\z", RegexOptions.CultureInvariant);

    /// <summary>Returns true when <paramref name="windowSize"/> is within <see cref="MinWindowSize"/> and <see cref="MaxWindowSize"/>.</summary>
    public static bool IsValidWindowSize(int windowSize) => windowSize is >= MinWindowSize and <= MaxWindowSize;

    /// <summary>Returns true when <paramref name="storyPointsField"/> looks like <c>customfield_</c> followed by digits.</summary>
    public static bool IsValidStoryPointsField(string? storyPointsField) =>
        storyPointsField is not null && StoryPointsFieldPattern.IsMatch(storyPointsField);

    /// <summary>
    /// Checks the values against every rule.
    /// </summary>
    /// <param name="windowSize">Candidate window size.</param>
    /// <param name="storyPointsField">Candidate story-points field id.</param>
    /// <returns>One message per violated rule, each naming the invalid value and the allowed range; empty when valid.</returns>
    public static IReadOnlyList<string> Validate(int windowSize, string? storyPointsField)
    {
        var problems = new List<string>();

        if (!IsValidWindowSize(windowSize))
            problems.Add($"WindowSize must be between {MinWindowSize} and {MaxWindowSize} (got {windowSize.ToString(CultureInfo.InvariantCulture)}).");

        if (!IsValidStoryPointsField(storyPointsField))
            problems.Add($"StoryPointsField must match customfield_<digits>, for example customfield_10016 (got '{storyPointsField}').");

        return problems;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> listing every violated rule, if there is any.
    /// </summary>
    /// <param name="windowSize">Candidate window size.</param>
    /// <param name="storyPointsField">Candidate story-points field id.</param>
    /// <exception cref="ArgumentException">At least one value is outside its allowed range or format.</exception>
    public static void ThrowIfInvalid(int windowSize, string? storyPointsField)
    {
        var problems = Validate(windowSize, storyPointsField);
        if (problems.Count > 0)
            throw new ArgumentException(string.Join(" ", problems));
    }
}
