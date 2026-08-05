// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JiraAnalyticsCli.Tests.Repositories;

/// <summary>
/// Tests for the IssueRepository class.
/// </summary>
public class IssueRepositoryTests
{
    private readonly Mock<ILogger<IssueRepository>> _loggerMock;
    private readonly IssueRepository _sut;

    public IssueRepositoryTests()
    {
        _loggerMock = new Mock<ILogger<IssueRepository>>();
        _sut = new IssueRepository(_loggerMock.Object);
    }

    [Fact]
    public async Task SaveAsync_ValidIssue_SavesIssueSuccessfully()
    {
        const string testName = nameof(SaveAsync_ValidIssue_SavesIssueSuccessfully);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        try
        {
            var issue = new JiraIssue
            {
                Key = "PROJ-1",
                Id = "1",
                Summary = "Test issue",
                CreatedDate = DateTime.UtcNow
            };

            await _sut.SaveAsync(issue);

            _sut.GetCount().Should().Be(1);
            var savedIssue = await _sut.GetByKeyAsync("PROJ-1");
            savedIssue.Should().NotBeNull();
            savedIssue!.Key.Should().Be("PROJ-1");

            _loggerMock.Object.LogInformation("Finished {TestName} successfully", testName);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in {TestName}", testName);
            throw;
        }
    }

    [Fact]
    public async Task SaveAsync_NullIssue_ThrowsArgumentNullException()
    {
        const string testName = nameof(SaveAsync_NullIssue_ThrowsArgumentNullException);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        try
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.SaveAsync(null!));

            _loggerMock.Object.LogInformation("Finished {TestName} successfully", testName);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in {TestName}", testName);
            throw;
        }
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsCorrectIssuesForProject()
    {
        const string testName = nameof(GetByProjectAsync_ReturnsCorrectIssuesForProject);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        try
        {
            var issue1 = new JiraIssue { Key = "PROJ-1", Id = "1", Summary = "S1", ProjectKey = "PROJ", CreatedDate = DateTime.UtcNow };
            var issue2 = new JiraIssue { Key = "OTHER-1", Id = "2", Summary = "S2", ProjectKey = "OTHER", CreatedDate = DateTime.UtcNow };
            
            await _sut.SaveAsync(issue1);
            await _sut.SaveAsync(issue2);

            var result = await _sut.GetByProjectAsync("PROJ");

            result.Should().HaveCount(1);
            result.First().Key.Should().Be("PROJ-1");

            _loggerMock.Object.LogInformation("Finished {TestName} successfully", testName);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in {TestName}", testName);
            throw;
        }
    }

    [Fact]
    public async Task Clear_RemovesAllIssues()
    {
        const string testName = nameof(Clear_RemovesAllIssues);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        try
        {
            var issue = new JiraIssue { Key = "PROJ-1", Id = "1", Summary = "S1", CreatedDate = DateTime.UtcNow };
            await _sut.SaveAsync(issue);
            _sut.GetCount().Should().Be(1);

            _sut.Clear();

            _sut.GetCount().Should().Be(0);

            _loggerMock.Object.LogInformation("Finished {TestName} successfully", testName);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in {TestName}", testName);
            throw;
        }
    }
}
