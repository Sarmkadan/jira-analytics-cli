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

namespace JiraAnalyticsCli.Tests.Services;

public class JqlQueryServiceTests
{
    private readonly Mock<IJiraApiService> _jiraServiceMock;
    private readonly Mock<ILogger<JqlQueryService>> _loggerMock;
    private readonly JqlQueryService _sut;

    public JqlQueryServiceTests()
    {
        _jiraServiceMock = new Mock<IJiraApiService>();
        _loggerMock      = new Mock<ILogger<JqlQueryService>>();
        _sut             = new JqlQueryService(_jiraServiceMock.Object, _loggerMock.Object);
    }

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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteQueryAsync_WithEmptyJql_ThrowsArgumentException(string jql)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ExecuteQueryAsync(jql));
    }

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
