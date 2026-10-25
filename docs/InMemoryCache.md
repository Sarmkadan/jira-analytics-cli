# InMemoryCache

Thread-safe, in-process key/value cache for the jira-analytics-cli project. Values are stored as JSON strings, and each entry is governed by a `CachePolicy` that controls absolute expiration, sliding expiration, and an optional condition. Hit, miss, and eviction counters are tracked for monitoring.

Namespace: `JiraAnalyticsCli.Caching`
Source: `Caching/InMemoryCache.cs`

## Constructor

### `public InMemoryCache(ILogger<InMemoryCache> logger)`

Creates an empty cache that writes diagnostics to the supplied logger.

- **Parameters**
  - `logger`: Logger used for debug, information, and error messages. Must not be null.
- **Exceptions**
  - Throws `ArgumentNullException` if `logger` is null.

## Storage Model

- Each value is serialized with `System.Text.Json.JsonSerializer` on write and deserialized on read. Stored values are therefore copies, and the cache does not keep references to the caller's objects.
- Keys are compared with the default `string` equality of `ConcurrentDictionary<string, …>`, which is case-sensitive.
- The cache does not enforce `CachePolicy.MaxSize`. That property is informational for this class.
- Expired entries are removed lazily when they are read through `Get` or `Contains`, or in bulk by `CleanupExpired`. Nothing runs in the background.

## Methods

### `public void Set<T>(string key, T value, CachePolicy policy)`

Stores `value` under `key` with the given expiration policy. If the key already exists, its value and policy are replaced and its creation time is reset.

- **Type Parameter**
  - `T`: The type of the value. It is serialized with `System.Text.Json`, so serialization errors (for example, `NotSupportedException` for unsupported types) propagate to the caller.
- **Parameters**
  - `key`: Unique identifier for the entry. Must not be null or empty.
  - `value`: The value to cache. Must not be null, even when `T` is a nullable type.
  - `policy`: The expiration policy for the entry. Must not be null.
- **Exceptions**
  - Throws `ArgumentException` if `key` is null or empty. Whitespace-only keys are accepted.
  - Throws `ArgumentNullException` if `value` or `policy` is null.

### `public T? Get<T>(string key, T? defaultValue = default)`

Retrieves the value stored under `key`. Returns `defaultValue` when the key is missing, the entry is expired, or the stored JSON cannot be deserialized into `T`.

- **Type Parameter**
  - `T`: The expected type of the cached value.
- **Parameters**
  - `key`: The key to look up. Must not be null or empty.
  - `defaultValue`: Returned instead of `null` when no valid entry is found. Defaults to `default(T)`.
- **Return Value**
  - The deserialized value, or `defaultValue`.
- **Side Effects**
  - Expired entries are evicted and counted as evictions.
  - A deserialization failure logs an error, evicts the entry, and counts as a miss.
  - For entries with a sliding expiration, a successful read resets `LastAccessedAt`.
  - Each call increments either the hit counter or the miss counter.
- **Exceptions**
  - Throws `ArgumentException` if `key` is null or empty.

### `public bool Contains(string key)`

Reports whether `key` exists and is not expired. Does not update the sliding-expiration timestamp and does not change hit or miss counters. An expired entry is evicted as a side effect.

- **Parameters**
  - `key`: The key to check. Must not be null or empty.
- **Return Value**
  - `true` if a valid entry exists; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentException` if `key` is null or empty.

### `public void Remove(string key)`

Removes a single entry. Does nothing if the key is not present. Removal is not counted as an eviction.

- **Parameters**
  - `key`: The key of the entry to remove. Must not be null or empty.
- **Exceptions**
  - Throws `ArgumentException` if `key` is null or empty.

### `public int RemoveByPattern(string pattern)`

Removes every entry whose key matches a glob-style pattern and returns the number removed. The pattern is not a regular expression: `*` matches any sequence of characters, `?` matches a single character, and all other characters match literally. The match is anchored to the whole key.

- **Parameters**
  - `pattern`: Glob pattern to match keys against. Must not be null or empty.
- **Return Value**
  - The number of entries removed. Removals made through this method are not counted as evictions.
- **Exceptions**
  - Throws `ArgumentException` if `pattern` is null or empty.

Examples:

| Pattern | Matches |
|---------|---------|
| `sprint:*` | `sprint:42`, `sprint:42:issues` |
| `issue:?` | `issue:7`, not `issue:42` |
| `metrics:team-a` | Only `metrics:team-a` |

### `public void Clear()`

Removes all entries and logs at information level. The hit, miss, and eviction counters are not reset. The cache remains usable afterward.

### `public CacheStatistics GetStatistics()`

Returns a snapshot of the cache counters.

- **Return Value**
  - A `CacheStatistics` record with the following members:
    - `Hits` (`long`): Successful `Get` calls.
    - `Misses` (`long`): `Get` calls that returned the default value, including expired or undeserializable entries.
    - `Evictions` (`long`): Entries removed because they expired or failed to deserialize.
    - `EntryCount` (`int`): Number of entries currently stored, including expired entries that have not yet been removed.
    - `HitRate` (`double`, computed): `Hits / (Hits + Misses)`, or `0` when there have been no lookups.

Counters are cumulative for the lifetime of the instance. `Remove`, `RemoveByPattern`, and `Clear` do not change `Evictions`.

### `public int CleanupExpired()`

Scans the cache, removes every expired entry, and returns the number removed. Each removal is counted as an eviction. Logs at information level only when at least one entry is removed.

- **Return Value**
  - The number of expired entries removed.

## Expiration Rules

An entry is treated as expired when `CachePolicy.IsExpired` returns `true`, which happens if any of the following is true:

- **Absolute expiration**: `CreatedAt + AbsoluteExpiration` is earlier than now (UTC).
- **Sliding expiration**: `LastAccessedAt + SlidingExpiration` is earlier than now (UTC). `LastAccessedAt` is refreshed by successful `Get` calls.
- **Condition**: `Condition` is set and returns `false`.

A policy with none of these set never expires.

## Thread Safety

- Reads and lookups go directly to the underlying `ConcurrentDictionary`.
- Inserts, removals, and `Clear` take an internal lock so that the entry count stays consistent with the dictionary.
- Counters are updated with `Interlocked` operations.

## Usage

```csharp
using JiraAnalyticsCli.Caching;
using Microsoft.Extensions.Logging.Abstractions;

var cache = new InMemoryCache(NullLogger<InMemoryCache>.Instance);

// Cache a value for 10 minutes from creation
var policy = CachePolicy.WithAbsoluteExpiration("sprint:42", TimeSpan.FromMinutes(10));
cache.Set("sprint:42", "Sprint 42", policy);

// Read it back, with a fallback when it is missing or expired
var sprint = cache.Get<string>("sprint:42", defaultValue: "unknown");

// Remove every sprint entry
var removed = cache.RemoveByPattern("sprint:*");

// Inspect counters
var stats = cache.GetStatistics();
Console.WriteLine($"Hit rate: {stats.HitRate:P0}, entries: {stats.EntryCount}");
```

Use `CachePolicy.WithSlidingExpiration` or `CachePolicy.WithCombinedExpiration` for idle-based or combined expiration, and `CachePolicy.WithCondition` for conditional entries.

## Related

- `CacheManager`: Holds named `InMemoryCache` stores and exposes their statistics.
- `CachePolicy`: Defines expiration rules for each entry.
