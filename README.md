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

## Contributing

Feel free to submit pull requests to help enhance and improve the jira-analytics-cli library.

