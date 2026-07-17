// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

/// <summary>
/// Extension methods for DateTimeExtensionsTests to provide additional test utilities.
/// </summary>
namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Provides extension methods for testing DateTimeExtensions functionality.
/// </summary>
public static class DateTimeExtensionsTestsExtensions
{
    /// <summary>
    /// Creates a date at the start of a business day (9:00 AM).
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime at 9:00 AM on the specified date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when date is null.</exception>
    public static DateTime AtBusinessStart(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        return date.Date.AddHours(9);
    }

    /// <summary>
    /// Creates a date at the end of a business day (5:00 PM).
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime at 17:00 on the specified date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when date is null.</exception>
    public static DateTime AtBusinessEnd(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        return date.Date.AddHours(17);
    }

    /// <summary>
    /// Creates a date at the start of a weekend (Saturday 00:00).
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime at midnight on Saturday.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="date"/> is null.</exception>
    public static DateTime AtWeekendStart(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        var daysToSaturday = ((int)DayOfWeek.Saturday - (int)date.DayOfWeek + 7) % 7;
        return date.Date.AddDays(daysToSaturday);
    }

    /// <summary>
    /// Creates a date at the start of the next business week (Monday 9:00 AM).
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime at 9:00 AM on the next Monday.</returns>
    /// <exception cref="ArgumentNullException">Thrown when date is null.</exception>
    public static DateTime AtNextBusinessWeekStart(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        var daysToMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
        return date.Date.AddDays(daysToMonday).AddHours(9);
    }

    /// <summary>
    /// Creates a date at the end of the previous business week (Friday 5:00 PM).
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime at 17:00 on the previous Friday.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="date"/> is null.</exception>
    public static DateTime AtPreviousBusinessWeekEnd(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        var daysToFriday = ((int)date.DayOfWeek - (int)DayOfWeek.Friday + 7) % 7;
        return date.Date.AddDays(-daysToFriday).AddHours(17);
    }

    /// <summary>
    /// Creates a date representing a public holiday (fixed date holidays only).
    /// </summary>
    /// <param name="year">The year for the holiday.</param>
    /// <returns>A DateTime for New Year's Day of the specified year.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when year is less than 2000.</exception>
    public static DateTime NewYearsDay(int year)
    {
        if (year < 2000)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be 2000 or later.");
        }

        return new DateTime(year, 1, 1);
    }

    /// <summary>
    /// Creates a date representing a public holiday (fixed date holidays only).
    /// </summary>
    /// <param name="year">The year for the holiday.</param>
    /// <returns>A DateTime for Christmas Day of the specified year.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when year is less than 2000.</exception>
    public static DateTime ChristmasDay(int year)
    {
        if (year < 2000)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be 2000 or later.");
        }

        return new DateTime(year, 12, 25);
    }

    /// <summary>
    /// Creates a date representing a public holiday (fixed date holidays only).
    /// </summary>
    /// <param name="year">The year for the holiday.</param>
    /// <returns>A DateTime for Independence Day (July 4th) of the specified year.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when year is less than 2000.</exception>
    public static DateTime IndependenceDay(int year)
    {
        if (year < 2000)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be 2000 or later.");
        }

        return new DateTime(year, 7, 4);
    }

    /// <summary>
    /// Creates a collection of consecutive business days between two dates.
    /// </summary>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <returns>An enumerable of DateTime values representing business days.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="startDate"/> or <paramref name="endDate"/> is null.</exception>
    public static IEnumerable<DateTime> GetBusinessDays(this DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(startDate);
        ArgumentNullException.ThrowIfNull(endDate);

        if (startDate > endDate)
        {
            yield break;
        }

        var current = startDate.Date;
        var end = endDate.Date;

        while (current <= end)
        {
            if (current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                yield return current;
            }

            current = current.AddDays(1);
        }
    }

    /// <summary>
    /// Creates a collection of consecutive business hours between two dates.
    /// </summary>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <returns>An enumerable of DateTime values representing business hours.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="startDate"/> or <paramref name="endDate"/> is null.</exception>
    public static IEnumerable<DateTime> GetBusinessHours(this DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(startDate);
        ArgumentNullException.ThrowIfNull(endDate);

        if (startDate > endDate)
        {
            yield break;
        }

        var current = startDate.Date.AddHours(9); // Start at 9 AM
        var end = endDate.Date.AddHours(17); // End at 5 PM

        while (current <= end)
        {
            yield return current;
            current = current.AddHours(1);
        }
    }

    /// <summary>
    /// Creates a date representing the first day of the month.
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime representing the first day of the month.</returns>
    /// <exception cref="ArgumentNullException">Thrown when date is null.</exception>
    public static DateTime FirstDayOfMonth(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Creates a date representing the last day of the month.
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime representing the last day of the month.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="date"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the year/month combination is invalid.</exception>
    public static DateTime LastDayOfMonth(this DateTime date)
    {
        ArgumentNullException.ThrowIfNull(date);
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    /// <summary>
    /// Creates a date with the time component set to midnight.
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime with time set to 00:00:00.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="date"/> is null.</exception>
    public static DateTime AtMidnight(this DateTime date) => date.Date;

    /// <summary>
    /// Creates a date with the time component set to noon.
    /// </summary>
    /// <param name="date">The base date.</param>
    /// <returns>A DateTime with time set to 12:00:00.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="date"/> is null.</exception>
    public static DateTime AtNoon(this DateTime date) => date.Date.AddHours(12);
}