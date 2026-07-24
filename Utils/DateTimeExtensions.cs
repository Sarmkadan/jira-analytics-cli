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
	/// <param name="startDate">The start date</param>
	/// <param name="endDate">The end date</param>
	/// <returns>Number of business days between the dates</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when startDate is after endDate</exception>
	public static int GetBusinessDaysBetween(this DateTime startDate, DateTime endDate)
	{
		if (startDate > endDate)
			throw new ArgumentOutOfRangeException(nameof(startDate), "Start date must be before or equal to end date");

		var span = endDate - startDate;
		var businessDays = 0;

		for (int i = 0; i <= span.Days; i++)
		{
			var date = startDate.AddDays(i);
			if (date.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Wednesday or DayOfWeek.Thursday or DayOfWeek.Friday)
				businessDays++;
		}

		return businessDays;
	}

	/// <summary>
	/// Check if date is within business hours (9 AM to 5 PM)
	/// </summary>
	/// <param name="dateTime">The date and time to check</param>
	/// <returns>True if within business hours, false otherwise</returns>
	public static bool IsBusinessHour(this DateTime dateTime) => dateTime.Hour >= 9 && dateTime.Hour < 17;

	/// <summary>
	/// Get the week number in the year using ISO 8601 week numbering
	/// </summary>
	/// <param name="date">The date to get week number for</param>
	/// <returns>ISO 8601 week number (1-53)</returns>
	public static int GetWeekNumber(this DateTime date)
	{
		return System.Globalization.ISOWeek.GetWeekOfYear(date);
	}

	/// <summary>
	/// Check if date is in the past (compared to DateTime.UtcNow)
	/// </summary>
	/// <param name="dateTime">The date to check</param>
	/// <returns>True if the date is in the past</returns>
	public static bool IsPast(this DateTime dateTime)
	{
		return dateTime < DateTime.UtcNow;
	}

	/// <summary>
	/// Check if date is in the future (compared to DateTime.UtcNow)
	/// </summary>
	/// <param name="dateTime">The date to check</param>
	/// <returns>True if the date is in the future</returns>
	public static bool IsFuture(this DateTime dateTime)
	{
		return dateTime > DateTime.UtcNow;
	}

	/// <summary>
	/// Format duration in human-readable format
	/// </summary>
	/// <param name="timeSpan">The time span to format</param>
	/// <returns>Human-readable duration string</returns>
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
	/// Get last business day of month (Monday-Friday)
	/// </summary>
	/// <param name="date">The date to get last business day for</param>
	/// <returns>The last business day of the month</returns>
	public static DateTime GetLastBusinessDayOfMonth(this DateTime date)
	{
		var lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

		while (lastDay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
			lastDay = lastDay.AddDays(-1);

		return lastDay;
	}
}

/// <summary>
/// JSON converter that always reads and writes <see cref="DateTime"/> values as UTC.
/// The default <c>System.Text.Json</c> <see cref="DateTime"/> converter preserves whatever
/// offset appears in the source JSON, which for a value bearing a non-"Z" numeric offset (as
/// Jira's API produces) results in a <see cref="DateTimeKind.Local"/> value converted to the
/// host machine's time zone. Deserializing on machines in different time zones - or across a
/// DST transition on the same machine - then yields values that are no longer comparable via
/// plain <see cref="DateTime"/> subtraction, corrupting burn-rate and cycle-time duration
/// math. This converter removes that ambiguity by always normalizing to UTC on read and
/// always emitting an unambiguous "Z"-suffixed ISO-8601 string on write.
/// </summary>
public sealed class UtcDateTimeJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
{
	/// <summary>
	/// Reads a JSON string as an ISO-8601 timestamp and converts it to UTC.
	/// </summary>
	/// <param name="reader">The reader positioned at the timestamp token.</param>
	/// <param name="typeToConvert">The type being converted (always <see cref="DateTime"/>).</param>
	/// <param name="options">The active serializer options.</param>
	/// <returns>The parsed timestamp, normalized to <see cref="DateTimeKind.Utc"/>.</returns>
	/// <exception cref="System.Text.Json.JsonException">Thrown when the token cannot be parsed as a valid ISO-8601 timestamp.</exception>
	public override DateTime Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
	{
		var value = reader.GetString();
		return DateTimeOffset.TryParse(
				value,
				System.Globalization.CultureInfo.InvariantCulture,
				System.Globalization.DateTimeStyles.AssumeUniversal,
				out var parsed)
			? parsed.UtcDateTime
			: throw new System.Text.Json.JsonException($"Unable to parse '{value}' as an ISO-8601 timestamp.");
	}

	/// <summary>
	/// Writes a <see cref="DateTime"/> as an unambiguous "Z"-suffixed UTC ISO-8601 string,
	/// converting non-UTC values to UTC first.
	/// </summary>
	/// <param name="writer">The writer to emit the timestamp token to.</param>
	/// <param name="value">The timestamp to write.</param>
	/// <param name="options">The active serializer options.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is null.</exception>
	public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime value, System.Text.Json.JsonSerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(writer);

		var utc = value.Kind switch
		{
			DateTimeKind.Utc => value,
			DateTimeKind.Local => value.ToUniversalTime(),
			_ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
		};

		writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", System.Globalization.CultureInfo.InvariantCulture));
	}
}