// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Configuration;
using Xunit;

namespace JiraAnalyticsCli.Tests.Configuration;

/// <summary>
/// Unit tests for <see cref="JiraOptionsValidator"/>. Each rule has its own tests, plus checks that
/// all violations are reported together and that messages say where to fix the setting.
/// </summary>
public class JiraOptionsValidatorTests
{
    private static JiraOptions ValidOptions() => new()
    {
        BaseUrl = "https://your-company.atlassian.net",
        ApiToken = "secret-token",
        TimeoutSeconds = 30,
        BoardId = null
    };

    [Fact]
    public void Validate_ValidOptions_ReturnsNoProblems()
    {
        JiraOptionsValidator.Validate(ValidOptions()).Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullOptions_Throws()
    {
        var act = () => JiraOptionsValidator.Validate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // BaseUrl: absolute https URI
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("https://your-company.atlassian.net")]
    [InlineData("https://jira.example.com:8443/jira")]
    public void Validate_BaseUrlAbsoluteHttps_IsAccepted(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        JiraOptionsValidator.Validate(options).Should().BeEmpty();
    }

    [Theory]
    [InlineData("http://your-company.atlassian.net")]
    [InlineData("ftp://your-company.atlassian.net")]
    [InlineData("your-company.atlassian.net")]
    [InlineData("/jira")]
    [InlineData("https://")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BaseUrlNotAbsoluteHttps_ReportsBaseUrl(string baseUrl)
    {
        var options = ValidOptions();
        options.BaseUrl = baseUrl;

        var problems = JiraOptionsValidator.Validate(options);

        problems.Should().ContainSingle()
            .Which.Should().StartWith("BaseUrl must be an absolute https URI");
    }

    [Fact]
    public void Validate_BaseUrlProblem_NamesWhereToSetIt()
    {
        var options = ValidOptions();
        options.BaseUrl = "http://insecure.example.com";

        var problem = JiraOptionsValidator.Validate(options).Should().ContainSingle().Subject;

        problem.Should().Contain("(got 'http://insecure.example.com')");
        problem.Should().Contain("--jira-url");
        problem.Should().Contain("JIRA_URL or JIRA_BASE_URL");
        problem.Should().Contain("\"jiraBaseUrl\" in appsettings.json");
    }

    // -------------------------------------------------------------------------
    // ApiToken: non-empty
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ApiTokenEmptyOrWhitespace_ReportsApiToken(string token)
    {
        var options = ValidOptions();
        options.ApiToken = token;

        var problems = JiraOptionsValidator.Validate(options);

        problems.Should().ContainSingle()
            .Which.Should().StartWith("ApiToken must not be empty");
    }

    [Fact]
    public void Validate_ApiTokenProblem_NamesWhereToSetIt()
    {
        var options = ValidOptions();
        options.ApiToken = string.Empty;

        var problem = JiraOptionsValidator.Validate(options).Should().ContainSingle().Subject;

        problem.Should().Contain("--jira-token");
        problem.Should().Contain("JIRA_API_TOKEN or JIRA_TOKEN");
        problem.Should().Contain("\"jiraApiToken\" in appsettings.json");
    }

    [Fact]
    public void Validate_ApiTokenProblem_DoesNotEchoTheValue()
    {
        var options = ValidOptions();
        options.ApiToken = "   ";

        var problem = JiraOptionsValidator.Validate(options).Should().ContainSingle().Subject;

        problem.Should().NotContain("(got");
    }

    // -------------------------------------------------------------------------
    // BoardId: positive when provided
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_BoardIdNotProvided_IsAccepted()
    {
        var options = ValidOptions();
        options.BoardId = null;

        JiraOptionsValidator.Validate(options).Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    public void Validate_BoardIdPositive_IsAccepted(int boardId)
    {
        var options = ValidOptions();
        options.BoardId = boardId;

        JiraOptionsValidator.Validate(options).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-7)]
    public void Validate_BoardIdNotPositive_ReportsBoardId(int boardId)
    {
        var options = ValidOptions();
        options.BoardId = boardId;

        var problem = JiraOptionsValidator.Validate(options).Should().ContainSingle().Subject;

        problem.Should().StartWith("BoardId must be a positive integer");
        problem.Should().Contain($"(got '{boardId}')");
        problem.Should().Contain("--board-id");
        problem.Should().Contain("JIRA_BOARD_ID");
        problem.Should().Contain("\"jiraBoardId\" in appsettings.json");
    }

    // -------------------------------------------------------------------------
    // TimeoutSeconds: 1..300
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(300)]
    public void Validate_TimeoutWithinRange_IsAccepted(int timeout)
    {
        var options = ValidOptions();
        options.TimeoutSeconds = timeout;

        JiraOptionsValidator.Validate(options).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(301)]
    [InlineData(3600)]
    public void Validate_TimeoutOutOfRange_ReportsTimeoutSeconds(int timeout)
    {
        var options = ValidOptions();
        options.TimeoutSeconds = timeout;

        var problem = JiraOptionsValidator.Validate(options).Should().ContainSingle().Subject;

        problem.Should().StartWith("TimeoutSeconds must be between 1 and 300 seconds");
        problem.Should().Contain($"(got '{timeout}')");
        problem.Should().Contain("--timeout-seconds");
        problem.Should().Contain("JIRA_API_TIMEOUT_SECONDS");
        problem.Should().Contain("\"jiraApiTimeoutSeconds\" in appsettings.json");
    }

    // -------------------------------------------------------------------------
    // Collecting all problems
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_MultipleViolations_ReportsAllOfThem()
    {
        var options = new JiraOptions
        {
            BaseUrl = "http://insecure.example.com",
            ApiToken = string.Empty,
            BoardId = 0,
            TimeoutSeconds = 0
        };

        var problems = JiraOptionsValidator.Validate(options);

        problems.Should().HaveCount(4);
        problems.Should().Contain(p => p.StartsWith("BaseUrl "));
        problems.Should().Contain(p => p.StartsWith("ApiToken "));
        problems.Should().Contain(p => p.StartsWith("BoardId "));
        problems.Should().Contain(p => p.StartsWith("TimeoutSeconds "));
    }
}
