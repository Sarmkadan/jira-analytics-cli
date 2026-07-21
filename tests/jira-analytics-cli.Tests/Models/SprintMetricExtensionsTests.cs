using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

public class SprintMetricExtensionsTests
{
    [Fact]
    public void GetProgressPercentage_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var metric = new SprintMetric { PlannedStoryPoints = 100, CompletedStoryPoints = 50 };

        // Act
        var result = metric.GetProgressPercentage();

        // Assert
        result.Should().Be(50.0);
    }

    [Fact]
    public void GetProgressPercentage_ShouldThrowDivideByZeroException_WhenPlannedPointsIsZero()
    {
        // Arrange
        var metric = new SprintMetric { PlannedStoryPoints = 0, CompletedStoryPoints = 50 };

        // Act
        Action act = () => metric.GetProgressPercentage();

        // Assert
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void GetProgressPercentage_ShouldThrowArgumentNullException_WhenMetricIsNull()
    {
        // Arrange
        SprintMetric? metric = null;

        // Act
        Action act = () => metric!.GetProgressPercentage();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsSprintComplete_ShouldReturnTrue_WhenEndDateIsInPast()
    {
        // Arrange
        var metric = new SprintMetric { EndDate = DateTime.UtcNow.AddDays(-1) };

        // Act
        var result = metric.IsSprintComplete();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSprintComplete_ShouldReturnFalse_WhenEndDateIsInFuture()
    {
        // Arrange
        var metric = new SprintMetric { EndDate = DateTime.UtcNow.AddDays(1) };

        // Act
        var result = metric.IsSprintComplete();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSprintComplete_ShouldThrowArgumentNullException_WhenMetricIsNull()
    {
        // Arrange
        SprintMetric? metric = null;

        // Act
        Action act = () => metric!.IsSprintComplete();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAverageDailyProgress_ShouldReturnCorrectAverage()
    {
        // Arrange
        var metric = new SprintMetric
        {
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 11), // 10 days
            CompletedStoryPoints = 50
        };

        // Act
        var result = metric.GetAverageDailyProgress();

        // Assert
        result.Should().Be(5.0);
    }

    [Fact]
    public void GetAverageDailyProgress_ShouldThrowDivideByZeroException_WhenDurationIsZero()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1);
        var metric = new SprintMetric
        {
            StartDate = date,
            EndDate = date,
            CompletedStoryPoints = 50
        };

        // Act
        Action act = () => metric.GetAverageDailyProgress();

        // Assert
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void GetAverageDailyProgress_ShouldThrowArgumentNullException_WhenMetricIsNull()
    {
        // Arrange
        SprintMetric? metric = null;

        // Act
        Action act = () => metric!.GetAverageDailyProgress();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
