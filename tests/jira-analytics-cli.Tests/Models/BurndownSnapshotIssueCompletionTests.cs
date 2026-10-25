// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Tests for <see cref="BurndownSnapshotExtensions.GetIssueCompletionPercentage"/>.
/// </summary>
public class BurndownSnapshotIssueCompletionTests
{
    private static BurndownSnapshot CreateSnapshot(
        int completedStoryPoints = 50,
        int totalStoryPoints = 100,
        int completedIssueCount = 10,
        int totalIssueCount = 20)
    {
        return new BurndownSnapshot
        {
            SprintId = 1,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-1),
            CompletedStoryPoints = completedStoryPoints,
            TotalStoryPoints = totalStoryPoints,
            RemainingStoryPoints = totalStoryPoints - completedStoryPoints,
            CompletedIssueCount = completedIssueCount,
            TotalIssueCount = totalIssueCount,
            RemainingIssueCount = totalIssueCount - completedIssueCount,
            ScopeChanges = 0
        };
    }

    [Fact]
    public void GetIssueCompletionPercentage_ShouldReturnCompletedOverTotalIssues()
    {
        // Arrange
        var snapshot = CreateSnapshot(completedIssueCount: 5, totalIssueCount: 20);

        // Act
        var result = snapshot.GetIssueCompletionPercentage();

        // Assert
        result.Should().BeApproximately(25.0, 0.0001);
    }

    [Fact]
    public void GetIssueCompletionPercentage_ShouldReturnHundred_WhenAllIssuesCompleted()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            completedStoryPoints: 100,
            totalStoryPoints: 100,
            completedIssueCount: 20,
            totalIssueCount: 20);

        // Act
        var result = snapshot.GetIssueCompletionPercentage();

        // Assert
        result.Should().BeApproximately(100.0, 0.0001);
    }

    [Fact]
    public void GetIssueCompletionPercentage_ShouldReturnZero_WhenNoIssuesCompleted()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            completedStoryPoints: 0,
            totalStoryPoints: 100,
            completedIssueCount: 0,
            totalIssueCount: 20);

        // Act
        var result = snapshot.GetIssueCompletionPercentage();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetIssueCompletionPercentage_ShouldDifferFromStoryPointPercentage()
    {
        // Arrange - 1 of 4 issues done, but 75 of 100 story points done
        var snapshot = CreateSnapshot(
            completedStoryPoints: 75,
            totalStoryPoints: 100,
            completedIssueCount: 1,
            totalIssueCount: 4);

        // Act
        var issuePercentage = snapshot.GetIssueCompletionPercentage();
        var pointPercentage = snapshot.GetBurndownPercentage();

        // Assert
        issuePercentage.Should().BeApproximately(25.0, 0.0001);
        pointPercentage.Should().BeApproximately(75.0, 0.0001);
    }

    [Fact]
    public void GetIssueCompletionPercentage_ShouldThrowArgumentNullException_WhenSnapshotIsNull()
    {
        // Arrange
        BurndownSnapshot? snapshot = null;

        // Act
        Action act = () => snapshot!.GetIssueCompletionPercentage();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetIssueCompletionPercentage_ShouldThrowArgumentException_WhenSnapshotIsInvalid()
    {
        // Arrange - completed issues exceed total, which fails validation
        var snapshot = CreateSnapshot();
        snapshot.CompletedIssueCount = 25;
        snapshot.RemainingIssueCount = -5;

        // Act
        Action act = () => snapshot.GetIssueCompletionPercentage();

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
