// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JiraAnalyticsCli.Tests.Repositories;

/// <summary>
/// Tests for the <see cref="IssueRepositoryExtensions"/> helper methods.
/// </summary>
public class IssueRepositoryExtensionsTests
{
    private readonly IssueRepository _sut;

    public IssueRepositoryExtensionsTests()
    {
        var loggerMock = new Mock<ILogger<IssueRepository>>();
        this._sut = new IssueRepository(loggerMock.Object);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyIssuesWithMatchingStatusInProject()
    {
        await this._sut.SaveAsync(new JiraIssue { Key = "PROJ-1", Id = "1", Summary = "Test issue", ProjectKey = "PROJ", Status = "In Progress", CreatedDate = DateTime.UtcNow });
        await this._sut.SaveAsync(new JiraIssue { Key = "PROJ-2", Id = "2", Summary = "Test issue", ProjectKey = "PROJ", Status = "Done", CreatedDate = DateTime.UtcNow });
        await this._sut.SaveAsync(new JiraIssue { Key = "PROJ-3", Id = "3", Summary = "Test issue", ProjectKey = "PROJ", Status = "In Progress", CreatedDate = DateTime.UtcNow });
        await this._sut.SaveAsync(new JiraIssue { Key = "OTHER-1", Id = "4", Summary = "Test issue", ProjectKey = "OTHER", Status = "In Progress", CreatedDate = DateTime.UtcNow });

        var result = await this._sut.GetByStatusAsync("PROJ", "In Progress");

        result.Should().HaveCount(2);
        result.Select(i => i.Key).Should().BeEquivalentTo("PROJ-1", "PROJ-3");
    }

    [Fact]
    public async Task GetByStatusAsync_MatchesStatusCaseInsensitively()
    {
        await this._sut.SaveAsync(new JiraIssue { Key = "PROJ-1", Id = "1", Summary = "Test issue", ProjectKey = "PROJ", Status = "In Progress", CreatedDate = DateTime.UtcNow });

        var result = await this._sut.GetByStatusAsync("PROJ", "in progress");

        result.Should().ContainSingle().Which.Key.Should().Be("PROJ-1");
    }

    [Fact]
    public async Task GetByStatusAsync_NoMatchingIssues_ReturnsEmptyList()
    {
        await this._sut.SaveAsync(new JiraIssue { Key = "PROJ-1", Id = "1", Summary = "Test issue", ProjectKey = "PROJ", Status = "Done", CreatedDate = DateTime.UtcNow });

        var result = await this._sut.GetByStatusAsync("PROJ", "Blocked");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStatusAsync_UnknownProject_ReturnsEmptyList()
    {
        await this._sut.SaveAsync(new JiraIssue { Key = "PROJ-1", Id = "1", Summary = "Test issue", ProjectKey = "PROJ", Status = "Done", CreatedDate = DateTime.UtcNow });

        var result = await this._sut.GetByStatusAsync("MISSING", "Done");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStatusAsync_NullRepository_ThrowsArgumentNullException()
    {
        IssueRepository repository = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repository.GetByStatusAsync("PROJ", "Done"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetByStatusAsync_NullOrEmptyProjectKey_ThrowsArgumentException(string? projectKey)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await this._sut.GetByStatusAsync(projectKey!, "Done"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetByStatusAsync_NullOrEmptyStatus_ThrowsArgumentException(string? status)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await this._sut.GetByStatusAsync("PROJ", status!));
    }
}
