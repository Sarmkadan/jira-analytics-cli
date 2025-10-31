// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// DateTime utility extensions for calculations and formatting
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Calculate business days between two dates (excluding weekends)
    /// </summary>
    public static int GetBusinessDaysBetween(this DateTime startDate, DateTime endDate)
    {
        var span = endDate - startDate;
        var businessDays = 0;

        for (int i = 0; i <= span.Days; i++)
        {
            var date = startDate.AddDays(i);
            if (date.DayOfWeek > DayOfWeek.Monday && date.DayOfWeek < DayOfWeek.Friday)
                businessDays++;
        }

        return businessDays;
    }

    /// <summary>
    /// Check if date is within business hours (9 AM to 5 PM)
    /// </summary>
    public static bool IsBusinessHour(this DateTime dateTime)
    {
        return dateTime.Hour >= 9 && dateTime.Hour < 17;
    }

    /// <summary>
    /// Get the week number in the year
    /// </summary>
    public static int GetWeekNumber(this DateTime date)
    {
        var jan1 = new DateTime(date.Year, 1, 1);
        var days = (date - jan1).Days;
        return (days / 7) + 1;
    }

    /// <summary>
    /// Check if date is in the past
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Check if date is in the future
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Format duration in human-readable format
    /// </summary>
    public static string ToHumanReadableDuration(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")}";

        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")}";

        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")}";

        return $"{(int)timeSpan.TotalSeconds} second{((int)timeSpan.TotalSeconds != 1 ? "s" : "")}";
    }

    /// <summary>
    /// Get last business day of month
    /// </summary>
    public static DateTime GetLastBusinessDayOfMonth(this DateTime date)
    {
        var lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

        while (lastDay.DayOfWeek == DayOfWeek.Saturday || lastDay.DayOfWeek == DayOfWeek.Sunday)
            lastDay = lastDay.AddDays(-1);

        return lastDay;
    }
}
