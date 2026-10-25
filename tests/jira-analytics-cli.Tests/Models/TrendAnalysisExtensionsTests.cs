using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Contains unit tests for the <see cref="TrendAnalysisExtensions"/> class.
/// </summary>
public class TrendAnalysisExtensionsTests
{
    private static readonly DateTime BaseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static IReadOnlyList<ITimeSeriesPoint> Series(params double[] values) =>
        values
            .Select((value, i) => (ITimeSeriesPoint)new TimeSeriesPoint(BaseDate.AddDays(i), value))
            .ToList();

    [Fact]
    public void GetTrendDirection_ShouldReturnRising_WhenValuesGrowOverTime()
    {
        var direction = Series(1, 2, 3, 4, 5).GetTrendDirection();

        direction.Should().Be(TrendDirection.Rising);
    }

    [Fact]
    public void GetTrendDirection_ShouldReturnFalling_WhenValuesShrinkOverTime()
    {
        var direction = Series(10, 8, 6, 4, 2).GetTrendDirection();

        direction.Should().Be(TrendDirection.Falling);
    }

    [Fact]
    public void GetTrendDirection_ShouldReturnFlat_WhenValuesAreConstant()
    {
        var direction = Series(7, 7, 7, 7).GetTrendDirection();

        direction.Should().Be(TrendDirection.Flat);
    }

    [Fact]
    public void GetTrendDirection_ShouldReturnFlat_WhenSeriesIsEmpty()
    {
        var direction = Series().GetTrendDirection();

        direction.Should().Be(TrendDirection.Flat);
    }

    [Fact]
    public void GetTrendDirection_ShouldReturnFlat_WhenSeriesHasSinglePoint()
    {
        var direction = Series(42).GetTrendDirection();

        direction.Should().Be(TrendDirection.Flat);
    }

    [Fact]
    public void GetTrendDirection_ShouldIgnoreInputOrder()
    {
        var shuffled = new List<ITimeSeriesPoint>
        {
            new TimeSeriesPoint(BaseDate.AddDays(3), 4),
            new TimeSeriesPoint(BaseDate, 1),
            new TimeSeriesPoint(BaseDate.AddDays(4), 5),
            new TimeSeriesPoint(BaseDate.AddDays(1), 2),
            new TimeSeriesPoint(BaseDate.AddDays(2), 3)
        };

        var direction = shuffled.GetTrendDirection();

        direction.Should().Be(TrendDirection.Rising);
    }

    [Fact]
    public void GetTrendDirection_ShouldReturnFlat_WhenSlopeIsWithinTolerance()
    {
        // Slope is exactly 2 per day, so a tolerance of 2 must still count as flat.
        var direction = Series(0, 2, 4, 6).GetTrendDirection(flatTolerance: 2);

        direction.Should().Be(TrendDirection.Flat);
    }

    [Fact]
    public void GetTrendDirection_ShouldReturnRising_WhenSlopeExceedsTolerance()
    {
        var direction = Series(0, 2, 4, 6).GetTrendDirection(flatTolerance: 1);

        direction.Should().Be(TrendDirection.Rising);
    }

    [Fact]
    public void GetTrendDirection_ShouldReturnFalling_WhenNegativeSlopeExceedsTolerance()
    {
        var direction = Series(6, 4, 2, 0).GetTrendDirection(flatTolerance: 1);

        direction.Should().Be(TrendDirection.Falling);
    }

    [Fact]
    public void GetTrendDirection_ShouldThrowArgumentNullException_WhenPointsIsNull()
    {
        IReadOnlyList<ITimeSeriesPoint> points = null!;

        var act = () => points.GetTrendDirection();

        act.Should().Throw<ArgumentNullException>().WithParameterName("points");
    }

    [Fact]
    public void GetTrendDirection_ShouldThrowArgumentOutOfRangeException_WhenToleranceIsNegative()
    {
        var points = Series(1, 2, 3);

        var act = () => points.GetTrendDirection(flatTolerance: -0.5);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("flatTolerance");
    }
}
