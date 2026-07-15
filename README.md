// ... rest of README content ...
## CachePolicyExtensions

The `CachePolicyExtensions` class provides a set of extension methods for working with `CachePolicy` objects. These methods allow you to create and modify cache policies with specific conditions, maximum sizes, and expiration times.

### Usage Example

```csharp
using JiraAnalyticsCli.Caching;

// Create a test instance of CachePolicy
var policy = CachePolicyExtensions.WithMaxSize(new CachePolicy(), 100);

// Add a condition to the policy
policy = CachePolicyExtensions.WithCondition(policy, issue => issue.IsOverdue());

// Set a combined expiration time
policy = CachePolicyExtensions.WithCombinedExpiration(policy, TimeSpan.FromHours(2));

// Check if the policy has an expiration
bool hasExpiration = CachePolicyExtensions.HasExpiration(policy);

// Get the effective expiration time
TimeSpan? effectiveExpiration = CachePolicyExtensions.GetEffectiveExpiration(policy);

Console.WriteLine($"Has expiration: {hasExpiration}");
Console.WriteLine($"Effective expiration: {effectiveExpiration}");
```

## DateTimeExtensionsTests

The `DateTimeExtensionsTests` class provides unit tests for the `DateTimeExtensions` utility methods that handle date and time calculations, business hour detection, week numbers, and human-readable duration formatting.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Calculate business days between two dates (excludes weekends)
var startDate = new DateTime(2026, 6, 1); // Monday
var endDate = new DateTime(2026, 6, 12); // Friday (2 weeks later)
int businessDays = startDate.GetBusinessDaysBetween(endDate);
Console.WriteLine($"Business days: {businessDays}"); // Output: Business days: 10

// Check if a date/time is within business hours (9:00 AM to 5:00 PM)
var businessTime = new DateTime(2026, 6, 1, 14, 30, 0); // 2:30 PM
bool isBusinessHour = businessTime.IsBusinessHour();
Console.WriteLine($"Is business hour: {isBusinessHour}"); // Output: Is business hour: True

// Get the ISO 8601 week number for a date
var weekNumber = new DateTime(2026, 1, 15).GetWeekNumber();
Console.WriteLine($"Week number: {weekNumber}"); // Output: Week number: 3

// Check if a date is in the past or future
var yesterday = DateTime.UtcNow.AddDays(-1);
Console.WriteLine($"Is past: {yesterday.IsPast()}"); // Output: Is past: True
var tomorrow = DateTime.UtcNow.AddDays(1);
Console.WriteLine($"Is future: {tomorrow.IsFuture()}"); // Output: Is future: True

// Format a TimeSpan as human-readable duration
var duration = TimeSpan.FromDays(3).Add(TimeSpan.FromHours(2));
string readableDuration = duration.ToHumanReadableDuration();
Console.WriteLine($"Duration: {readableDuration}"); // Output: Duration: 3 days 2 hours

// Get the last business day of a month
var lastBusinessDay = new DateTime(2026, 6, 15).GetLastBusinessDayOfMonth();
Console.WriteLine($"Last business day: {lastBusinessDay:yyyy-MM-dd}"); // Output: Last business day: 2026-06-30
```

# ... rest of README content ...
