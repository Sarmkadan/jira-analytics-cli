# CachePolicyExtensions

Provides extension methods for configuring and inspecting `CachePolicy` instances, enabling fine-grained control over cache behavior such as expiration, size limits, and conditional caching.

## API

### `WithCondition(CachePolicy, Func<CacheEntry, bool>)`
Configures a `CachePolicy` to only cache entries that satisfy a specified condition.

- **Parameters**
  - `policy`: The `CachePolicy` instance to configure.
  - `condition`: A function that evaluates a cache entry and returns `true` if the entry should be cached.
- **Return Value**
  Returns a new `CachePolicy` with the condition applied.
- **Throws**
  Throws `ArgumentNullException` if `policy` or `condition` is `null`.

---

### `WithMaxSize(CachePolicy, int)`
Configures a `CachePolicy` to enforce a maximum number of entries in the cache.

- **Parameters**
  - `policy`: The `CachePolicy` instance to configure.
  - `maxSize`: The maximum number of entries allowed in the cache. Must be a positive integer.
- **Return Value**
  Returns a new `CachePolicy` with the size limit applied.
- **Throws**
  Throws `ArgumentNullException` if `policy` is `null`.
  Throws `ArgumentOutOfRangeException` if `maxSize` is not a positive integer.

---
### `WithCombinedExpiration(CachePolicy, TimeSpan, TimeSpan)`
Configures a `CachePolicy` to use a combined expiration strategy where entries expire based on either absolute or sliding expiration, whichever occurs first.

- **Parameters**
  - `policy`: The `CachePolicy` instance to configure.
  - `absoluteExpiration`: The maximum duration an entry can remain in the cache regardless of access.
  - `slidingExpiration`: The duration an entry remains in the cache if accessed within this period.
- **Return Value**
  Returns a new `CachePolicy` with the combined expiration applied.
- **Throws**
  Throws `ArgumentNullException` if `policy` is `null`.
  Throws `ArgumentOutOfRangeException` if either `absoluteExpiration` or `slidingExpiration` is not positive.

---
### `HasExpiration(CachePolicy)`
Determines whether the given `CachePolicy` has any expiration constraints configured.

- **Parameters**
  - `policy`: The `CachePolicy` instance to inspect.
- **Return Value**
  Returns `true` if the policy has either absolute or sliding expiration configured; otherwise, `false`.
- **Throws**
  Throws `ArgumentNullException` if `policy` is `null`.

---
### `GetEffectiveExpiration(CachePolicy)`
Retrieves the effective expiration time for the given `CachePolicy`, considering both absolute and sliding expiration settings.

- **Parameters**
  - `policy`: The `CachePolicy` instance to inspect.
- **Return Value**
  Returns a `TimeSpan?` representing the effective expiration duration. If no expiration is configured, returns `null`.
- **Throws**
  Throws `ArgumentNullException` if `policy` is `null`.

## Usage

### Example 1: Configuring a Cache Policy with Size Limit and Conditional Caching
