using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Contains unit tests for the <see cref="JiraAnalyticsCli.Models.CycleTimeResultExtensions"/> class.
/// </summary>
public class CycleTimeResultExtensionsTests
{
    private static IssueCycleTime Issue(string key, double days) => new()
    {
        IssueKey = key,
        Summary = $"Summary {key}",
        CycleTimeDays = days,
        CreatedDate = DateTimeOffset.UtcNow
    };

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.CycleTimeResultExtensions.GetIssuesExceedingCycleTime"/>
    /// returns only issues strictly above the threshold, ordered slowest first.
    /// </summary>
    [Fact]
    public void GetIssuesExceedingCycleTime_ShouldReturnIssuesAboveThresholdSlowestFirst()
    {
        // Arrange
        var result = new CycleTimeResult
        {
            IssueCycleTimes = new List<IssueCycleTime>
            {
                Issue("P-1", 2),
                Issue("P-2", 15),
                Issue("P-3", 8),
                Issue("P-4", 30),
                Issue("P-5", 10)
            }
        };

        // Act
        var exceeding = result.GetIssuesExceedingCycleTime(7);

        // Assert
        exceeding.Select(i => i.IssueKey).Should().Equal("P-4", "P-2", "P-5", "P-3");
    }

    /// <summary>
    /// Verifies that an issue whose cycle time equals the threshold exactly is not reported,
    /// because the comparison is strictly greater than the threshold.
    /// </summary>
    [Fact]
    public void GetIssuesExceedingCycleTime_ShouldExcludeIssueEqualToThreshold()
    {
        // Arrange
        var result = new CycleTimeResult
        {
            IssueCycleTimes = new List<IssueCycleTime> { Issue("P-1", 5), Issue("P-2", 5.01) }
        };

        // Act
        var exceeding = result.GetIssuesExceedingCycleTime(5);

        // Assert
        exceeding.Should().ContainSingle().Which.IssueKey.Should().Be("P-2");
    }

    /// <summary>
    /// Verifies that an empty result set, or one where no issue exceeds the threshold,
    /// yields an empty list rather than null.
    /// </summary>
    [Fact]
    public void GetIssuesExceedingCycleTime_ShouldReturnEmpty_WhenNothingExceedsThreshold()
    {
        // Arrange
        var empty = new CycleTimeResult();
        var fast = new CycleTimeResult
        {
            IssueCycleTimes = new List<IssueCycleTime> { Issue("P-1", 1), Issue("P-2", 3) }
        };

        // Act & Assert
        empty.GetIssuesExceedingCycleTime(0).Should().NotBeNull().And.BeEmpty();
        fast.GetIssuesExceedingCycleTime(3).Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.CycleTimeResultExtensions.GetIssuesExceedingCycleTime"/>
    /// throws <see cref="ArgumentNullException"/> when the cycle time result is null.
    /// </summary>
    [Fact]
    public void GetIssuesExceedingCycleTime_ShouldThrow_WhenResultIsNull()
    {
        // Arrange
        CycleTimeResult? result = null;

        // Act
        var act = () => result!.GetIssuesExceedingCycleTime(1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
