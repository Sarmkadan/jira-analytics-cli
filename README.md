// ... rest of README content ...

## DateTimeExtensions

The `DateTimeExtensions` class provides utility extension methods for date and time calculations, formatting, and business logic operations. It includes methods for calculating business days, checking business hours, determining week numbers, validating date states, formatting durations, and finding the last business day of a month.


### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Current date and time
var now = DateTime.UtcNow;
var yesterday = now.AddDays(-1);
var tomorrow = now.AddDays(1);
var lastMonth = new DateTime(now.Year, now.Month - 1, 15);

// Check if date is in business hours
var isBusinessHour = now.IsBusinessHour();
Console.WriteLine($"Is business hour: {isBusinessHour}");

// Calculate business days between two dates
var businessDays = yesterday.GetBusinessDaysBetween(now);
Console.WriteLine($"Business days between yesterday and now: {businessDays}");

// Get week number (ISO 8601)
var weekNumber = now.GetWeekNumber();
Console.WriteLine($"Current week number: {weekNumber}");

// Check if date is in past or future
Console.WriteLine($"Yesterday is past: {yesterday.IsPast()}");
Console.WriteLine($"Tomorrow is future: {tomorrow.IsFuture()}");

// Format duration in human-readable format
var duration = TimeSpan.FromHours(2.5);
Console.WriteLine($"Duration: {duration.ToHumanReadableDuration()}");

// Get last business day of month
var lastBusinessDay = now.GetLastBusinessDayOfMonth();
Console.WriteLine($"Last business day of month: {lastBusinessDay:yyyy-MM-dd}");
```

## StringExtensions

The `StringExtensions` class provides a set of extension methods for string manipulation and formatting. It offers utilities for truncating strings, removing whitespace, converting to slug format, parsing boolean values, repeating strings, matching patterns, finding common prefixes, and escaping special characters for safe SQL use.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Sample data
var longString = "This is a very long string that needs to be truncated.";
var whitespaceString = "   This string has leading and trailing whitespace.   ";
var slugString = "This string will be converted to slug format.";
var boolString = "true";

// Truncate string with ellipsis
var truncated = longString.TruncateWithEllipsis(20);
Console.WriteLine(truncated); // Output: "This is a very long..."

// Remove whitespace from string
var cleaned = whitespaceString.RemoveWhitespace();
Console.WriteLine(cleaned); // Output: "This string has leading and trailing whitespace."

// Convert string to slug format
var slug = slugString.ToSlug();
Console.WriteLine(slug); // Output: "this-string-will-be-converted-to-slug-format"

// Parse boolean value from string
if (boolString.TryParseBool(out bool result))
{
    Console.WriteLine(result); // Output: true
}

// Repeat string
var repeated = "Hello".Repeat(3);
Console.WriteLine(repeated); // Output: "HelloHelloHello"

// Match pattern
var pattern = "Hello*";
if ("HelloWorld".MatchesPattern(pattern))
{
    Console.WriteLine("Match found!"); // Output: Match found!
}

// Find common prefix
var prefix = "Hello".GetCommonPrefix("HelloWorld");
Console.WriteLine(prefix); // Output: "Hello"

// Escape special characters for SQL
var sqlString = "Hello'World";
var escaped = sqlString.EscapeForSql();
Console.WriteLine(escaped); // Output: "Hello''World"
```

