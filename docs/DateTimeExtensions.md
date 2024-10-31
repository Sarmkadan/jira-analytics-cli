# DateTimeExtensions

Utility class providing extension methods for `DateTime` and related calculations commonly needed in analytics and business-day logic. It centralises operations such as business-day counting, week-number derivation, human-readable duration formatting, and temporal comparisons.

## API

### GetBusinessDaysBetween

```csharp
public static int GetBusinessDaysBetween(this DateTime start, DateTime end)
```

Returns the number of business days (Monday through Friday) between two dates, inclusive of both `start` and `end`. The calculation iterates through the date range and counts only weekdays. No holidays are excluded; the method uses a simple weekday filter.

**Parameters**
- `start` — The beginning date of the range.
- `end` — The ending date of the range. Must be equal to or later than `start`.

**Return value**
A non-negative `int` representing the count of business days.

**Exceptions**
- `ArgumentException` — Thrown when `end` is earlier than `start`.

---

### IsBusinessHour

```csharp
public static bool IsBusinessHour(this DateTime dateTime)
```

Determines whether the given `DateTime` falls within standard business hours (9:00–17:00, Monday through Friday). Both the start and end of the hour range are inclusive.

**Parameters**
- `dateTime` — The `DateTime` value to evaluate.

**Return value**
`true` if the time is on a weekday and the hour component is between 9 and 17 inclusive; otherwise `false`.

**Exceptions**
None.

---

### GetWeekNumber

```csharp
public static int GetWeekNumber(this DateTime dateTime)
```

Computes the ISO 8601 week number for the given date. The calculation follows the rule that the first week of the year is the one containing the first Thursday.

**Parameters**
- `dateTime` — The `DateTime` value for which to compute the week number.

**Return value**
An `int` between 1 and 53 inclusive.

**Exceptions**
None.

---

### IsPast

```csharp
public static bool IsPast(this DateTime dateTime)
```

Indicates whether the given `DateTime` is strictly earlier than the current system time (`DateTime.Now`).

**Parameters**
- `dateTime` — The `DateTime` value to compare.

**Return value**
`true` if `dateTime` is before `DateTime.Now`; otherwise `false`.

**Exceptions**
None.

---

### IsFuture

```csharp
public static bool IsFuture(this DateTime dateTime)
```

Indicates whether the given `DateTime` is strictly later than the current system time (`DateTime.Now`).

**Parameters**
- `dateTime` — The `DateTime` value to compare.

**Return value**
`true` if `dateTime` is after `DateTime.Now`; otherwise `false`.

**Exceptions**
None.

---

### ToHumanReadableDuration

```csharp
public static string ToHumanReadableDuration(this TimeSpan timeSpan)
```

Converts a `TimeSpan` into a concise, human-readable string. The output format uses the largest non-zero units (days, hours, minutes, seconds) and omits zero-valued components.

**Parameters**
- `timeSpan` — The `TimeSpan` instance to format.

**Return value**
A `string` such as `"2d 3h 15m"` or `"45s"`.

**Exceptions**
None.

---

### GetLastBusinessDayOfMonth

```csharp
public static DateTime GetLastBusinessDayOfMonth(this DateTime dateTime)
```

Returns the last weekday (Monday–Friday) of the month containing the given date. If the last calendar day of the month falls on a weekend, the method walks backward until a weekday is found.

**Parameters**
- `dateTime` — Any `DateTime` value within the target month.

**Return value**
A `DateTime` representing the final business day of that month, with the time component set to midnight (`00:00:00`).

**Exceptions**
None.

---

## Usage

### Example 1: SLA deadline check

```csharp
DateTime ticketCreated = new DateTime(2025, 6, 12, 14, 30, 0);
DateTime now = DateTime.Now;

if (ticketCreated.IsPast())
{
    int businessDaysElapsed = ticketCreated.GetBusinessDaysBetween(now);
    Console.WriteLine($"Ticket is {businessDaysElapsed} business days old.");
}

if (!now.IsBusinessHour())
{
    Console.WriteLine("Current time is outside business hours — response may be delayed.");
}
```

### Example 2: Reporting period boundaries

```csharp
DateTime reportDate = new DateTime(2025, 3, 15);
DateTime lastBusinessDay = reportDate.GetLastBusinessDayOfMonth();
int weekNumber = reportDate.GetWeekNumber();

TimeSpan processingTime = TimeSpan.FromMinutes(245);
string duration = processingTime.ToHumanReadableDuration();

Console.WriteLine($"Report for week {weekNumber} must be finalised by {lastBusinessDay:yyyy-MM-dd}.");
Console.WriteLine($"Typical processing time: {duration}.");
```

---

## Notes

- **Business-day calculations** do not account for public holidays. Consumers requiring holiday-aware logic must layer additional filtering on top of `GetBusinessDaysBetween` or `GetLastBusinessDayOfMonth`.
- **`IsPast` and `IsFuture`** use `DateTime.Now`, which captures the local system time. This makes them unsuitable for scenarios requiring UTC-based comparisons or distributed consistency. For those cases, compare against `DateTime.UtcNow` externally before calling the extensions.
- **`GetWeekNumber`** follows ISO 8601 rules. Years with 53 weeks are handled correctly; the return value can be 53 for dates in late December or early January of certain years.
- **`ToHumanReadableDuration`** operates on `TimeSpan` values. Negative durations produce a string with a leading minus sign on the largest unit (e.g., `"-1d 2h"`). Zero `TimeSpan` returns `"0s"`.
- **`GetLastBusinessDayOfMonth`** returns a `DateTime` with the time component zeroed to midnight. If the entire month somehow contains no weekdays (impossible under the Gregorian calendar), the method would throw; in practice this never occurs.
- **Thread safety**: All methods are static and operate on immutable input types (`DateTime`, `TimeSpan`). They hold no mutable state and are safe for concurrent invocation from multiple threads without external synchronisation.
