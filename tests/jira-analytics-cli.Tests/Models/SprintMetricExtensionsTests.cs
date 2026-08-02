using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Contains unit tests for the <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions"/> class.
/// </summary>
public class SprintMetricExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.GetProgressPercentage"/> 
    /// returns the correct percentage calculation based on planned and completed story points.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.GetProgressPercentage"/> 
    /// throws a <see cref="DivideByZeroException"/> when the planned points are zero.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.GetProgressPercentage"/> 
    /// throws an <see cref="ArgumentNullException"/> when the metric is null.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.IsSprintComplete"/> 
    /// returns true when the sprint's end date is in the past.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.IsSprintComplete"/> 
    /// returns false when the sprint's end date is in the future.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.IsSprintComplete"/> 
    /// throws an <see cref="ArgumentNullException"/> when the metric is null.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.GetAverageDailyProgress"/> 
    /// returns the correct average daily progress calculation.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.GetAverageDailyProgress"/> 
    /// throws a <see cref="DivideByZeroException"/> when the sprint duration is zero.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.SprintMetricExtensions.GetAverageDailyProgress"/> 
    /// throws an <see cref="ArgumentNullException"/> when the metric is null.
    /// </summary>
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
