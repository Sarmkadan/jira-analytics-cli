// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JiraAnalyticsCli.Tests.Configuration;

/// <summary>
/// Unit tests for <see cref="AppConfigurationProvider"/> and the environment-variable rules it applies to numeric settings,
/// plus checks that the API token never appears in exception messages or <c>ToString</c> output.
/// </summary>
public class AppConfigurationProviderTests
{
    private const string ValidToken = "secret-token";

    /// <summary>Builds an environment lookup from name/value pairs; unknown names return null.</summary>
    private static Func<string, string?> Env(params (string Name, string Value)[] pairs)
    {
        var map = pairs.ToDictionary(p => p.Name, p => p.Value);
        return name => map.TryGetValue(name, out var value) ? value : null;
    }

    private static JiraOptions ValidOptions(string baseUrl = "https://jira.example.com") =>
        new() { BaseUrl = baseUrl, ApiToken = ValidToken };

    private static ICliConfig Load(JiraOptions options, params (string Name, string Value)[] env) =>
        new AppConfigurationProvider(NullLogger<AppConfigurationProvider>.Instance, options, Env(env)).LoadConfiguration();

    private static Action LoadAction(JiraOptions options, params (string Name, string Value)[] env) =>
        () => Load(options, env);

    // -------------------------------------------------------------------------
    // Numeric environment variables: valid values are applied
    // -------------------------------------------------------------------------

    [Fact]
    public void Load_NumericEnvVarsWithinRange_AreApplied()
    {
        var config = Load(
            ValidOptions(),
            ("JIRA_API_MAX_PAGE_SIZE", "500"),
            ("JIRA_API_MAX_PAGES", "20"),
            ("JIRA_API_MAX_RESULTS_PER_REQUEST", "250"));

        config.JiraApiMaxPageSize.Should().Be(500);
        config.JiraApiMaxPages.Should().Be(20);
        config.JiraApiMaxResultsPerRequest.Should().Be(250);
    }

    [Fact]
    public void Load_EmptyNumericEnvVar_IsTreatedAsUnset()
    {
        var config = Load(ValidOptions(), ("JIRA_API_MAX_PAGE_SIZE", "   "));

        config.JiraApiMaxPageSize.Should().Be(200);
    }

    // -------------------------------------------------------------------------
    // Non-numeric values are rejected instead of silently falling back to the default
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("lots")]
    [InlineData("10 pages")]
    [InlineData("1.5")]
    [InlineData("1e3")]
    public void Load_NonNumericEnvVar_ThrowsConfigurationExceptionNamingVariable(string value)
    {
        var act = LoadAction(ValidOptions(), ("JIRA_API_MAX_PAGE_SIZE", value));

        act.Should().Throw<ConfigurationException>()
            .Which.PropertyName.Should().Be("JIRA_API_MAX_PAGE_SIZE");
    }

    // -------------------------------------------------------------------------
    // Negative, zero and oversized values are rejected
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("JIRA_API_MAX_PAGE_SIZE", "-1")]
    [InlineData("JIRA_API_MAX_PAGES", "0")]
    [InlineData("JIRA_API_TIMEOUT_SECONDS", "0")]
    [InlineData("JIRA_API_TIMEOUT_SECONDS", "-30")]
    [InlineData("JIRA_API_TIMEOUT_SECONDS", "301")]
    [InlineData("JIRA_API_MAX_PAGE_SIZE", "100000")]
    [InlineData("JIRA_API_MAX_PAGES", "2147483647")]
    [InlineData("CACHE_EXPIRATION_MINUTES", "-1")]
    [InlineData("DEFAULT_SPRINT_COUNT", "0")]
    public void Load_OutOfRangeEnvVar_ThrowsConfigurationException(string name, string value)
    {
        var act = LoadAction(ValidOptions(), (name, value));

        act.Should().Throw<ConfigurationException>()
            .Which.PropertyName.Should().Be(name);
    }

    // -------------------------------------------------------------------------
    // Base URL must be an absolute http(s) URL
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("ftp://files.example.com")]
    [InlineData("file:///etc/passwd")]
    [InlineData("gopher://jira.example.com")]
    [InlineData("jira.example.com")]
    public void Load_NonHttpBaseUrl_ThrowsInvalidOperation(string baseUrl)
    {
        var act = LoadAction(ValidOptions(baseUrl));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JiraBaseUrl must be an absolute http or https URL");
    }

    [Fact]
    public void Load_HttpBaseUrl_IsAccepted()
    {
        var config = Load(ValidOptions("http://jira.internal.example.com"));

        config.JiraBaseUrl.Should().Be("http://jira.internal.example.com");
    }

    // -------------------------------------------------------------------------
    // The API token never appears in exception messages or ToString output
    // -------------------------------------------------------------------------

    [Fact]
    public void Load_ThrownExceptionMessage_DoesNotContainToken()
    {
        var act = LoadAction(ValidOptions(), ("JIRA_API_TOKEN", ValidToken), ("JIRA_API_MAX_PAGES", "-5"));

        act.Should().Throw<ConfigurationException>()
            .Which.Message.Should().NotContain(ValidToken);
    }

    [Fact]
    public void Load_TokenPastedIntoNumericVariable_IsNotEchoedInMessage()
    {
        var act = LoadAction(ValidOptions(), ("JIRA_API_MAX_PAGE_SIZE", ValidToken));

        act.Should().Throw<ConfigurationException>()
            .Which.Message.Should().NotContain(ValidToken);
    }

    [Fact]
    public void Load_ApiTokenNotInToString()
    {
        var config = (CliConfig)Load(ValidOptions());

        config.ToString().Should().NotContain(ValidToken).And.Contain("<redacted>");
    }

    [Fact]
    public void JiraOptions_ToString_DoesNotContainToken()
    {
        var options = ValidOptions();

        options.ToString().Should().NotContain(ValidToken).And.Contain("<redacted>");
    }

    // -------------------------------------------------------------------------
    // Constructor arguments must not be null
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new AppConfigurationProvider(null!, ValidOptions());

        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_NullJiraOptions_ThrowsArgumentNullException()
    {
        var act = () => new AppConfigurationProvider(NullLogger<AppConfigurationProvider>.Instance, null!);

        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("jiraOptions");
    }

    [Fact]
    public void Constructor_NullEnvLookup_ThrowsArgumentNullException()
    {
        var act = () => new AppConfigurationProvider(NullLogger<AppConfigurationProvider>.Instance, ValidOptions(), null!);

        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("getEnv");
    }
}
