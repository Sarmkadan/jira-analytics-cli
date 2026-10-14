// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Configuration;
using Xunit;

namespace JiraAnalyticsCli.Tests.Configuration;

/// <summary>
/// Unit tests for <see cref="JiraOptionsLoader"/>: precedence between CLI flags, environment variables
/// and appsettings.json, parsing of numeric values, and collection of problems.
/// </summary>
public class JiraOptionsLoaderTests
{
    private const string ValidToken = "secret-token";

    /// <summary>Builds an environment lookup from name/value pairs; unknown names return null.</summary>
    private static Func<string, string?> Env(params (string Name, string Value)[] pairs)
    {
        var map = pairs.ToDictionary(p => p.Name, p => p.Value);
        return name => map.TryGetValue(name, out var value) ? value : null;
    }

    private static readonly Func<string, string?> NoEnv = _ => null;

    // -------------------------------------------------------------------------
    // Precedence: CLI > env > appsettings.json > defaults
    // -------------------------------------------------------------------------

    [Fact]
    public void Load_CliFlagOverridesEnvAndJson()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(BaseUrl: "https://cli.example.com"),
            Env(("JIRA_URL", "https://env.example.com"), ("JIRA_API_TOKEN", ValidToken)),
            """{ "jiraBaseUrl": "https://json.example.com" }""");

        result.IsValid.Should().BeTrue();
        result.Options.BaseUrl.Should().Be("https://cli.example.com");
    }

    [Fact]
    public void Load_EnvOverridesJson()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken),
            Env(("JIRA_URL", "https://env.example.com")),
            """{ "jiraBaseUrl": "https://json.example.com" }""");

        result.Options.BaseUrl.Should().Be("https://env.example.com");
    }

    [Fact]
    public void Load_JsonUsedWhenCliAndEnvAbsent()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken),
            NoEnv,
            """{ "jiraBaseUrl": "https://json.example.com" }""");

        result.Options.BaseUrl.Should().Be("https://json.example.com");
    }

    [Fact]
    public void Load_DefaultsUsedWhenNothingProvided()
    {
        var result = JiraOptionsLoader.Load(new JiraOptionsOverrides(ApiToken: ValidToken), NoEnv, null);

        result.IsValid.Should().BeTrue();
        result.Options.BaseUrl.Should().Be("https://jira.atlassian.net");
        result.Options.TimeoutSeconds.Should().Be(30);
        result.Options.BoardId.Should().BeNull();
        result.Options.Email.Should().BeNull();
    }

    [Fact]
    public void Load_AlternateEnvNameIsUsedWhenPrimaryAbsent()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(),
            Env(("JIRA_BASE_URL", "https://alt.example.com"), ("JIRA_TOKEN", ValidToken)),
            null);

        result.IsValid.Should().BeTrue();
        result.Options.BaseUrl.Should().Be("https://alt.example.com");
        result.Options.ApiToken.Should().Be(ValidToken);
    }

    [Fact]
    public void Load_EmptyEnvVarFallsThroughToJson()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(),
            Env(("JIRA_URL", "   "), ("JIRA_API_TOKEN", ValidToken)),
            """{ "jiraBaseUrl": "https://json.example.com" }""");

        result.Options.BaseUrl.Should().Be("https://json.example.com");
    }

    [Fact]
    public void Load_JsonNullIsTreatedAsUnset()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(),
            NoEnv,
            """{ "jiraApiToken": null, "jiraBaseUrl": null }""");

        result.Options.BaseUrl.Should().Be("https://jira.atlassian.net");
        result.Problems.Should().ContainSingle().Which.Should().StartWith("ApiToken must not be empty");
    }

    [Fact]
    public void Load_EmptyCliFlagIsKeptAndReported()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: string.Empty),
            Env(("JIRA_API_TOKEN", ValidToken)),
            null);

        result.Options.ApiToken.Should().BeEmpty();
        result.Problems.Should().ContainSingle().Which.Should().StartWith("ApiToken must not be empty");
    }

    [Fact]
    public void Load_ProjectKeyAndEmailReadFromEnvAndJson()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken),
            Env(("JIRA_EMAIL", "dev@example.com")),
            """{ "defaultProject": "PROJ" }""");

        result.Options.Email.Should().Be("dev@example.com");
        result.Options.ProjectKey.Should().Be("PROJ");
    }

    [Fact]
    public void Load_TokenFromJsonWhenNoOtherSource()
    {
        var result = JiraOptionsLoader.Load(new JiraOptionsOverrides(), NoEnv, $$"""{ "jiraApiToken": "{{ValidToken}}" }""");

        result.IsValid.Should().BeTrue();
        result.Options.ApiToken.Should().Be(ValidToken);
    }

    // -------------------------------------------------------------------------
    // BoardId and TimeoutSeconds parsing
    // -------------------------------------------------------------------------

    [Fact]
    public void Load_BoardIdFromJsonNumber_IsParsed()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken),
            NoEnv,
            """{ "jiraBoardId": 42 }""");

        result.IsValid.Should().BeTrue();
        result.Options.BoardId.Should().Be(42);
    }

    [Fact]
    public void Load_BoardIdFromCliFlag_OverridesEnv()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken, BoardId: "7"),
            Env(("JIRA_BOARD_ID", "99")),
            null);

        result.Options.BoardId.Should().Be(7);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("4.5")]
    [InlineData("99999999999")]
    public void Load_BoardIdNotAWholeNumber_ReportsProblemNamingEnvVar(string boardId)
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken),
            Env(("JIRA_BOARD_ID", boardId)),
            null);

        result.Options.BoardId.Should().BeNull();
        var problem = result.Problems.Should().ContainSingle().Subject;
        problem.Should().StartWith("BoardId must be a whole number");
        problem.Should().Contain($"(got '{boardId}')");
        problem.Should().Contain("JIRA_BOARD_ID");
    }

    [Fact]
    public void Load_BoardIdZeroFromCli_ReportsPositiveRule()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken, BoardId: "0"),
            NoEnv,
            null);

        result.Problems.Should().ContainSingle().Which.Should().StartWith("BoardId must be a positive integer");
    }

    [Fact]
    public void Load_TimeoutFromCliFlag_IsParsed()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken, TimeoutSeconds: "120"),
            Env(("JIRA_API_TIMEOUT_SECONDS", "10")),
            null);

        result.Options.TimeoutSeconds.Should().Be(120);
    }

    [Fact]
    public void Load_TimeoutNotAWholeNumber_ReportsProblemNamingFlag()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken, TimeoutSeconds: "soon"),
            NoEnv,
            null);

        var problem = result.Problems.Should().ContainSingle().Subject;
        problem.Should().StartWith("TimeoutSeconds must be a whole number");
        problem.Should().Contain("--timeout-seconds");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("301")]
    public void Load_TimeoutOutOfRange_ReportsRangeRule(string timeout)
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(ApiToken: ValidToken, TimeoutSeconds: timeout),
            NoEnv,
            null);

        result.Problems.Should().ContainSingle().Which.Should().StartWith("TimeoutSeconds must be between 1 and 300 seconds");
    }

    // -------------------------------------------------------------------------
    // appsettings.json problems
    // -------------------------------------------------------------------------

    [Fact]
    public void Load_InvalidJson_ReportsFileAndStillResolvesOtherSources()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(),
            Env(("JIRA_API_TOKEN", ValidToken)),
            "{ not json");

        result.Options.ApiToken.Should().Be(ValidToken);
        result.Problems.Should().ContainSingle()
            .Which.Should().StartWith("appsettings.json is not valid JSON");
    }

    [Fact]
    public void Load_JsonRootNotObject_ReportsFile()
    {
        var result = JiraOptionsLoader.Load(new JiraOptionsOverrides(ApiToken: ValidToken), NoEnv, "[1, 2]");

        result.Problems.Should().ContainSingle()
            .Which.Should().StartWith("appsettings.json must contain a JSON object");
    }

    // -------------------------------------------------------------------------
    // Collecting all problems at once
    // -------------------------------------------------------------------------

    [Fact]
    public void Load_MultipleBadSettings_ReportsAllProblemsTogether()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(BaseUrl: "http://insecure.example.com", BoardId: "abc", TimeoutSeconds: "999"),
            NoEnv,
            null);

        result.IsValid.Should().BeFalse();
        result.Problems.Should().HaveCount(4);
        result.Problems.Should().Contain(p => p.StartsWith("BaseUrl must be an absolute https URI"));
        result.Problems.Should().Contain(p => p.StartsWith("ApiToken must not be empty"));
        result.Problems.Should().Contain(p => p.StartsWith("BoardId must be a whole number"));
        result.Problems.Should().Contain(p => p.StartsWith("TimeoutSeconds must be between 1 and 300 seconds"));
    }

    [Fact]
    public void Load_ApiTokenNeverAppearsInProblems()
    {
        var result = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(BaseUrl: "http://insecure.example.com", ApiToken: "   "),
            NoEnv,
            null);

        var tokenProblem = result.Problems.Should().ContainSingle(p => p.StartsWith("ApiToken ")).Subject;
        tokenProblem.Should().NotContain("(got");
    }

    [Fact]
    public void Load_NullOverrides_Throws()
    {
        var act = () => JiraOptionsLoader.Load(null!, NoEnv, null);

        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // FormatProblems
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatProblems_ListsCountAndEveryProblemInOneMessage()
    {
        var text = JiraOptionsLoader.FormatProblems(new[] { "BaseUrl first", "ApiToken second" });

        text.Should().StartWith("Invalid Jira configuration (2 problem(s)):");
        text.Should().Contain("  - BaseUrl first");
        text.Should().Contain("  - ApiToken second");
    }
}
