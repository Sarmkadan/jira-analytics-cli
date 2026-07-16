// ... rest of README content ...

## CachePolicy

The `CachePolicy` class defines caching behavior and expiration policies for cached data, supporting absolute expiration, sliding windows, and conditional caching. This allows you to control how long data remains in the cache and under what conditions it should be considered stale.

### Usage Example

```csharp
using JiraAnalyticsCli.Caching;

// Create a cache policy with 30-minute absolute expiration
var policy = CachePolicy.WithAbsoluteExpiration("my_cache_key", TimeSpan.FromMinutes(30));

// Access data from cache with automatic refresh on subsequent calls
bool exists = policy.TryGetCacheData(my_cache_key, out string cachedData);

// Cache miss; cache data for future calls
cache[my_cache_key] = "fresh data";

// Apply a sliding expiration that refreshes on access
var slidingPolicy = CachePolicy.WithSlidingExpiration(my_cache_key, TimeSpan.FromHours(2));

// Update policy to combine conditions
var combined = CachePolicy.WithCombinedExpiration(my_cache_key, TimeSpan.FromDays(1), TimeSpan.FromHours(6));

// Evaluate expiration status
bool expires = combined.IsExpired(DateTime.UtcNow);
```

## CollectionExtensions

The `CollectionExtensions` class provides a set of extension methods for working with collections and enumerables. It offers convenient utilities for batching, grouping, filtering, and transforming collections without modifying the original data structures.


### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Sample data
var issues = new List<string> { "JIRA-1", "JIRA-2", "JIRA-3", "JIRA-4", "JIRA-5" };
var projects = new Dictionary<string, int> { { "ProjectA", 1 }, { "ProjectB", 2 }, { "ProjectC", 3 } };

// Batch items into groups of 2
var batched = issues.Batch(2);
foreach (var batch in batched)
{
    Console.WriteLine(string.Join(", ", batch));
    // Output: JIRA-1, JIRA-2
    //         JIRA-3, JIRA-4
    //         JIRA-5
}

// Find duplicate project IDs (groups with count > 1)
var duplicateProjects = projects.GroupByMultiple(kvp => kvp.Value);
foreach (var group in duplicateProjects)
{
    Console.WriteLine($"Duplicate project ID {group.Key} found {group.Count()} times");
}

// Get distinct issues by removing duplicates based on issue number
var distinctIssues = issues.DistinctBy(issue => issue.Substring(5));

// Safely get issue at index 10 (returns null if out of range)
var issue = issues.GetAtIndexOrDefault(10);

// Check if collection is empty
if (issues.IsEmpty())
{
    Console.WriteLine("No issues found");
}

// Check if collection has exactly one element
if (issues.HasExactlyOne())
{
    Console.WriteLine("Single issue found");
}

// Merge multiple collections
var merged = CollectionExtensions.Merge(
    new[] { "JIRA-1", "JIRA-2" },
    new[] { "JIRA-3", "JIRA-4" },
    new[] { "JIRA-5" }
);

// Convert dictionary to key-value tuples
var tuples = projects.ToTuples();
foreach (var (Key, Value) in tuples)
{
    Console.WriteLine($"{Key}: {Value}");
}
```

## Contributing

Feel free to submit pull requests to help enhance and improve the jira-analytics-cli library.

