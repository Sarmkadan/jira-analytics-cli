// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Configuration;
using Xunit;

namespace JiraAnalyticsCli.Tests.Configuration;

/// <summary>
/// Tests for <see cref="VelocityOptions"/>, its validator and <see cref="VelocityOptionsLoader"/>:
/// the allowed ranges, precedence between CLI flags, environment variables and appsettings.json,
/// and collection of problems.
/// </summary>
public class VelocityOptionsLoaderTests
{
    private static Func<string, string?> Env(params (string Name, string Value)[] pairs)
    {
        var map = pairs.ToDictionary(p => p.Name, p => p.Value);
        return name => map.TryGetValue(name, out var value) ? value : null;
    }

    private static readonly Func<string, string?> NoEnv = _ => null;

    // -------------------------------------------------------------------------
    // The record itself
    // -------------------------------------------------------------------------

    [Fact]
    public void Defaults_AreValid()
    {
        var options = new VelocityOptions();

        options.WindowSize.Should().Be(3);
        options.ExcludeZeroPointSprints.Should().BeFalse();
        options.StoryPointsField.Should().Be("customfield_10016");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    [InlineData(-5)]
    public void Constructor_WindowSizeOutOfRange_ThrowsWithValueAndRange(int windowSize)
    {
        var act = () => new VelocityOptions(windowSize: windowSize);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*WindowSize must be between 1 and 20 (got {windowSize}).*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void Constructor_WindowSizeAtBounds_IsAccepted(int windowSize)
    {
        new VelocityOptions(windowSize: windowSize).WindowSize.Should().Be(windowSize);
    }

    [Theory]
    [InlineData("customfield_10016")]
    [InlineData("customfield_1")]
    [InlineData("customfield_99999999")]
    public void Constructor_StoryPointsFieldMatchingRegex_IsAccepted(string field)
    {
        new VelocityOptions(storyPointsField: field).StoryPointsField.Should().Be(field);
    }

    [Theory]
    [InlineData("story_points")]
    [InlineData("customfield_")]
    [InlineData("customfield_10016x")]
    [InlineData("CUSTOMFIELD_10016")]
    [InlineData("customfield_10016\n")]
    [InlineData("")]
    public void Constructor_StoryPointsFieldNotMatchingRegex_ThrowsWithValueAndFormat(string field)
    {
        var act = () => new VelocityOptions(storyPointsField: field);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*StoryPointsField must match customfield_<digits>*");
    }

    [Fact]
    public void Constructor_ReportsEveryViolation()
    {
        var act = () => new VelocityOptions(windowSize: 50, storyPointsField: "points");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*WindowSize must be between 1 and 20 (got 50).*StoryPointsField must match*");
    }

    // -------------------------------------------------------------------------
    // Loader: precedence CLI > env > appsettings.json > defaults
    // -------------------------------------------------------------------------

    [Fact]
    public void Load_CliFlagOverridesEnvAndJson()
    {
        var result = VelocityOptionsLoader.Load(
            new VelocityOptionsOverrides(WindowSize: "7"),
            Env(("VELOCITY_WINDOW_SIZE", "5")),
            """{ "velocityWindowSize": 4 }""");

        result.IsValid.Should().BeTrue();
        result.Options!.WindowSize.Should().Be(7);
    }

    [Fact]
    public void Load_EnvOverridesJson()
    {
        var result = VelocityOptionsLoader.Load(
            new VelocityOptionsOverrides(),
            Env(("VELOCITY_WINDOW_SIZE", "5")),
            """{ "velocityWindowSize": 4 }""");

        result.Options!.WindowSize.Should().Be(5);
    }

    [Fact]
    public void Load_JsonUsedWhenNoFlagOrEnv()
    {
        var result = VelocityOptionsLoader.Load(
            new VelocityOptionsOverrides(),
            NoEnv,
            """{ "velocityWindowSize": 4, "velocityExcludeZeroPoints": true, "velocityStoryPointsField": "customfield_20001" }""");

        result.Options.Should().Be(new VelocityOptions(4, true, "customfield_20001"));
    }

    [Fact]
    public void Load_NothingSet_UsesDefaults()
    {
        var result = VelocityOptionsLoader.Load(new VelocityOptionsOverrides(), NoEnv, null);

        result.Options.Should().Be(new VelocityOptions());
    }

    [Fact]
    public void Load_EmptyEnvVar_FallsThroughToJson()
    {
        var result = VelocityOptionsLoader.Load(
            new VelocityOptionsOverrides(),
            Env(("VELOCITY_WINDOW_SIZE", "   ")),
            """{ "velocityWindowSize": 6 }""");

        result.Options!.WindowSize.Should().Be(6);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("False", false)]
    public void Load_ExcludeZeroPointsFlag_ParsesBoolean(string text, bool expected)
    {
        var result = VelocityOptionsLoader.Load(
            new VelocityOptionsOverrides(ExcludeZeroPointSprints: text),
            NoEnv,
            null);

        result.Options!.ExcludeZeroPointSprints.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Loader: problems are collected, and Options is null when any exist
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("0")]
    [InlineData("21")]
    public void Load_WindowSizeOutOfRange_ReportsValueRangeAndSource(string value)
    {
        var result = VelocityOptionsLoader.Load(new VelocityOptionsOverrides(WindowSize: value), NoEnv, null);

        result.IsValid.Should().BeFalse();
        result.Options.Should().BeNull();
        result.Problems.Should().ContainSingle()
            .Which.Should().Contain("WindowSize must be between 1 and 20")
            .And.Contain($"got '{value}'")
            .And.Contain("--velocity-window")
            .And.Contain("VELOCITY_WINDOW_SIZE");
    }

    [Fact]
    public void Load_WindowSizeNotANumber_ReportsWholeNumberProblem()
    {
        var result = VelocityOptionsLoader.Load(new VelocityOptionsOverrides(WindowSize: "many"), NoEnv, null);

        result.Problems.Should().ContainSingle().Which.Should().Contain("must be a whole number").And.Contain("'many'");
    }

    [Fact]
    public void Load_ExcludeZeroPointsNotBoolean_ReportsProblem()
    {
        var result = VelocityOptionsLoader.Load(new VelocityOptionsOverrides(ExcludeZeroPointSprints: "yes"), NoEnv, null);

        result.Problems.Should().ContainSingle().Which.Should().Contain("must be true or false").And.Contain("'yes'");
    }

    [Fact]
    public void Load_StoryPointsFieldInvalid_ReportsProblemFromEnv()
    {
        var result = VelocityOptionsLoader.Load(
            new VelocityOptionsOverrides(),
            Env(("VELOCITY_STORY_POINTS_FIELD", "story_points")),
            null);

        result.Problems.Should().ContainSingle()
            .Which.Should().Contain("StoryPointsField must match customfield_<digits>").And.Contain("'story_points'");
    }

    [Fact]
    public void Load_MultipleProblems_AreAllReported()
    {
        var result = VelocityOptionsLoader.Load(
            new VelocityOptionsOverrides(WindowSize: "0", ExcludeZeroPointSprints: "maybe", StoryPointsField: "points"),
            NoEnv,
            null);

        result.Problems.Should().HaveCount(3);
        result.Options.Should().BeNull();
    }

    [Fact]
    public void Load_InvalidAppSettingsJson_ReportsProblem()
    {
        var result = VelocityOptionsLoader.Load(new VelocityOptionsOverrides(), NoEnv, "{ not json");

        result.Problems.Should().ContainSingle().Which.Should().Contain("appsettings.json is not valid JSON");
    }

    [Fact]
    public void Load_NullOverrides_Throws()
    {
        var act = () => VelocityOptionsLoader.Load(null!, NoEnv, null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FormatProblems_ListsEachProblem()
    {
        var text = VelocityOptionsLoader.FormatProblems(new[] { "WindowSize first", "StoryPointsField second" });

        text.Should().Contain("Invalid velocity configuration (2 problem(s))")
            .And.Contain("  - WindowSize first")
            .And.Contain("  - StoryPointsField second");
    }
}
