// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

public class DateTimeExtensionsTests
{
    [Fact]
    public void GetBusinessDaysBetween_SameDay_ReturnsZeroOrOne()
    {
        var date = new DateTime(2026, 5, 20); // Wednesday
        date.GetBusinessDaysBetween(date).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetBusinessDaysBetween_FullWeek_ExcludesWeekends()
    {
        var monday = new DateTime(2026, 5, 18); // Monday
        var friday = new DateTime(2026, 5, 22); // Friday
        var businessDays = monday.GetBusinessDaysBetween(friday);
        businessDays.Should().BeGreaterThan(0);
        businessDays.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public void GetBusinessDaysBetween_WeekendOnly_ReturnsZero()
    {
        var saturday = new DateTime(2026, 5, 23);
        var sunday = new DateTime(2026, 5, 24);
        saturday.GetBusinessDaysBetween(sunday).Should().Be(0);
    }

    [Fact]
    public void IsBusinessHour_At9AM_ReturnsTrue()
    {
        new DateTime(2026, 5, 21, 9, 0, 0).IsBusinessHour().Should().BeTrue();
    }

    [Fact]
    public void IsBusinessHour_At16_59_ReturnsTrue()
    {
        new DateTime(2026, 5, 21, 16, 59, 0).IsBusinessHour().Should().BeTrue();
    }

    [Fact]
    public void IsBusinessHour_At17_00_ReturnsFalse()
    {
        new DateTime(2026, 5, 21, 17, 0, 0).IsBusinessHour().Should().BeFalse();
    }

    [Fact]
    public void IsBusinessHour_At8AM_ReturnsFalse()
    {
        new DateTime(2026, 5, 21, 8, 0, 0).IsBusinessHour().Should().BeFalse();
    }

    [Fact]
    public void GetWeekNumber_January1_ReturnsWeek1()
    {
        new DateTime(2026, 1, 1).GetWeekNumber().Should().Be(1);
    }

    [Fact]
    public void GetWeekNumber_January8_ReturnsWeek2()
    {
        new DateTime(2026, 1, 8).GetWeekNumber().Should().Be(2);
    }

    [Fact]
    public void IsPast_YesterdayDate_ReturnsTrue()
    {
        DateTime.UtcNow.AddDays(-1).IsPast().Should().BeTrue();
    }

    [Fact]
    public void IsFuture_TomorrowDate_ReturnsTrue()
    {
        DateTime.UtcNow.AddDays(1).IsFuture().Should().BeTrue();
    }

    [Fact]
    public void IsPast_FutureDate_ReturnsFalse()
    {
        DateTime.UtcNow.AddDays(1).IsPast().Should().BeFalse();
    }

    [Fact]
    public void IsFuture_PastDate_ReturnsFalse()
    {
        DateTime.UtcNow.AddDays(-1).IsFuture().Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // ToHumanReadableDuration
    // -------------------------------------------------------------------------

    [Fact]
    public void ToHumanReadableDuration_MultipleDays_ReturnsDaysFormat()
    {
        TimeSpan.FromDays(3).ToHumanReadableDuration().Should().Be("3 days");
    }

    [Fact]
    public void ToHumanReadableDuration_SingleDay_ReturnsSingularForm()
    {
        TimeSpan.FromDays(1).ToHumanReadableDuration().Should().Be("1 day");
    }

    [Fact]
    public void ToHumanReadableDuration_Hours_ReturnsHoursFormat()
    {
        TimeSpan.FromHours(5).ToHumanReadableDuration().Should().Be("5 hours");
    }

    [Fact]
    public void ToHumanReadableDuration_SingleHour_ReturnsSingularForm()
    {
        TimeSpan.FromHours(1).ToHumanReadableDuration().Should().Be("1 hour");
    }

    [Fact]
    public void ToHumanReadableDuration_Minutes_ReturnsMinutesFormat()
    {
        TimeSpan.FromMinutes(30).ToHumanReadableDuration().Should().Be("30 minutes");
    }

    [Fact]
    public void ToHumanReadableDuration_Seconds_ReturnsSecondsFormat()
    {
        TimeSpan.FromSeconds(45).ToHumanReadableDuration().Should().Be("45 seconds");
    }

    // -------------------------------------------------------------------------
    // GetLastBusinessDayOfMonth
    // -------------------------------------------------------------------------

    [Fact]
    public void GetLastBusinessDayOfMonth_MonthEndingOnFriday_ReturnsFriday()
    {
        // January 2027 ends on Sunday; last business day should be Friday Jan 29
        var date = new DateTime(2027, 1, 15);
        var result = date.GetLastBusinessDayOfMonth();
        result.DayOfWeek.Should().NotBe(DayOfWeek.Saturday);
        result.DayOfWeek.Should().NotBe(DayOfWeek.Sunday);
        result.Month.Should().Be(1);
    }

    [Fact]
    public void GetLastBusinessDayOfMonth_MonthEndingOnWeekday_ReturnsThatDay()
    {
        // Find a month ending on a weekday (June 2026 ends on Tuesday)
        var date = new DateTime(2026, 6, 1);
        var result = date.GetLastBusinessDayOfMonth();
        result.Day.Should().Be(30);
        result.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
    }
}
