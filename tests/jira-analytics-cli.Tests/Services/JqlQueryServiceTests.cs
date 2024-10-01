// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the JqlQueryService class.
/// </summary>
public class JqlQueryServiceTests
{
    private readonly Mock<IJiraApiService> _jiraServiceMock;
    private readonly Mock<ILogger<JqlQueryService>> _loggerMock;
    private readonly JqlQueryService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="JqlQueryServiceTests"/> class.
    /// </summary>
    public JqlQueryServiceTests()
    {
        _jiraServiceMock = new Mock<IJiraApiService>();
        _loggerMock      = new Mock<ILogger<JqlQueryService>>();
        _sut             = new JqlQueryService(_jiraServiceMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the ExecuteQueryAsync method maps the result correctly when Jira returns issues.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_WhenJiraReturnsIssues_MapsResultCorrectly()
    {
        var issues = new List<JiraIssue>
        {
            new() { Key = "PROJ-1", Id = "1", Summary = "First issue",  Status = "Open",      IssueType = "Task", Priority = "High",   CreatedDate = DateTime.UtcNow.AddDays(-5) },
            new() { Key = "PROJ-2", Id = "2", Summary = "Second issue", Status = "In Progress", IssueType = "Bug", Priority = "Medium", CreatedDate = DateTime.UtcNow.AddDays(-3) }
        };

        _jiraServiceMock
            .Setup(s => s.SearchByJqlAsync("project = PROJ", 50, 0))
            .ReturnsAsync(new JiraSearchResult { Total = 2, StartAt = 0, Issues = issues });

        var result = await _sut.ExecuteQueryAsync("project = PROJ");

        result.IsSuccess.Should().BeTrue();
        result.Total.Should().Be(2);
        result.Issues.Should().HaveCount(2);
        result.Issues[0].Key.Should().Be("PROJ-1");
        result.Issues[1].Key.Should().Be("PROJ-2");
        result.Jql.Should().Be("project = PROJ");
    }

    /// <summary>
    /// Verifies that the ExecuteQueryAsync method returns a success result with no issues when Jira returns an empty result.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_WhenJiraReturnsEmpty_ReturnsSuccessWithNoIssues()
    {
        _jiraServiceMock
            .Setup(s => s.SearchByJqlAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new JiraSearchResult { Total = 0, StartAt = 0, Issues = new List<JiraIssue>() });

        var result = await _sut.ExecuteQueryAsync("project = EMPTY AND sprint in openSprints()");

        result.IsSuccess.Should().BeTrue();
        result.Total.Should().Be(0);
        result.Issues.Should().BeEmpty();
        result.HasMore.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the ExecuteQueryAsync method returns a failure result when Jira throws an exception.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_WhenJiraThrows_ReturnsFailureResult()
    {
        _jiraServiceMock
            .Setup(s => s.SearchByJqlAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _sut.ExecuteQueryAsync("project = PROJ");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Connection refused");
        result.Issues.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that the ExecuteQueryAsync method throws an ArgumentException when the JQL is empty.
    /// </summary>
    /// <param name="jql">The empty JQL string.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteQueryAsync_WithEmptyJql_ThrowsArgumentException(string jql)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ExecuteQueryAsync(jql));
    }

    /// <summary>
    /// Verifies that the ExecuteQueryAsync method passes the startAt and maxResults parameters when a pagination request is made.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_WithPaginationRequest_PassesStartAtAndMaxResults()
    {
        _jiraServiceMock
            .Setup(s => s.SearchByJqlAsync("project = PROJ ORDER BY created", 25, 50))
            .ReturnsAsync(new JiraSearchResult { Total = 100, StartAt = 50, Issues = new List<JiraIssue>() });

        var request = new JqlQueryRequest { Jql = "project = PROJ ORDER BY created", MaxResults = 25, StartAt = 50 };
        var result  = await _sut.ExecuteQueryAsync(request);

        result.StartAt.Should().Be(50);
        result.MaxResults.Should().Be(25);
        _jiraServiceMock.Verify(s => s.SearchByJqlAsync("project = PROJ ORDER BY created", 25, 50), Times.Once);
    }

    /// <summary>
    /// Verifies that the FormatAsText method returns a string containing the issue keys when the result is successful.
    /// </summary>
    /// <param name="result">The successful JqlQueryResult.</param>
    /// <returns>The formatted string.</returns>
    [Fact]
    public void FormatAsText_WithSuccessfulResult_ContainsIssueKeys()
    {
        var result = new JqlQueryResult
        {
            Jql      = "project = PROJ",
            Total    = 1,
            StartAt  = 0,
            IsSuccess = true,
            Issues   = new List<JiraIssue>
            {
                new() { Key = "PROJ-42", Id = "42", Summary = "Test issue", Status = "Open", IssueType = "Task", Priority = "High", CreatedDate = DateTime.UtcNow }
            }
        };

        var text = JqlQueryService.FormatAsText(result);

        text.Should().Contain("PROJ-42");
        text.Should().Contain("Test issue");
        text.Should().Contain("project = PROJ");
    }

    /// <summary>
    /// Verifies that the FormatAsText method returns a string containing the error message when the result is failed.
    /// </summary>
    /// <param name="result">The failed JqlQueryResult.</param>
    /// <returns>The formatted string.</returns>
    [Fact]
    public void FormatAsText_WithFailedResult_ReturnsErrorMessage()
    {
        var result = new JqlQueryResult
        {
            Jql          = "invalid JQL @@",
            IsSuccess    = false,
            ErrorMessage = "Syntax error"
        };

        var text = JqlQueryService.FormatAsText(result);

        text.Should().Contain("failed");
        text.Should().Contain("Syntax error");
    }
}
