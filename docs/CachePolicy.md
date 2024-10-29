# CachePolicy

A configuration class for defining cache entry behavior in the `jira-analytics-cli` project, including expiration policies, size limits, and conditional invalidation logic.

## API

### `public string Key`
The unique identifier for the cache entry. Used to retrieve or remove the cached value. Must be non-null and non-empty when the cache entry is created.

### `public TimeSpan? AbsoluteExpiration`
The duration after which the cache entry expires unconditionally. If `null`, the entry does not expire based on absolute time. Mutually exclusive with `SlidingExpiration` when set via constructor.

### `public TimeSpan? SlidingExpiration`
The duration after which the cache entry expires if not accessed. Resets on each access. If `null`, the entry does not expire based on sliding time. Mutually exclusive with `AbsoluteExpiration` when set via constructor.

### `public Func<bool>? Condition`
A predicate that determines whether the cache entry is valid. If the function returns `false`, the entry is treated as expired. Optional; if `null`, the entry is always considered valid unless expiration policies dictate otherwise.

### `public int MaxSize`
The maximum number of entries allowed in the cache. When exceeded, older entries are evicted based on least-recently-used (LRU) order. Must be a positive integer.

### `public CachePolicy()`
Constructs a new `CachePolicy` with default values: no expiration, no size limit, and no condition.

### `public static CachePolicy WithAbsoluteExpiration(TimeSpan absoluteExpiration)`
Creates a `CachePolicy` with an absolute expiration time. The `absoluteExpiration` parameter must be a positive `TimeSpan`.

**Returns:** A new `CachePolicy` instance with `AbsoluteExpiration` set to the provided value and `SlidingExpiration` set to `null`.

**Throws:** `ArgumentOutOfRangeException` if `absoluteExpiration` is zero or negative.

### `public static CachePolicy WithSlidingExpiration(TimeSpan slidingExpiration)`
Creates a `CachePolicy` with a sliding expiration time. The `slidingExpiration` parameter must be a positive `TimeSpan`.

**Returns:** A new `CachePolicy` instance with `SlidingExpiration` set to the provided value and `AbsoluteExpiration` set to `null`.

**Throws:** `ArgumentOutOfRangeException` if `slidingExpiration` is zero or negative.

### `public static CachePolicy WithCombinedExpiration(TimeSpan absoluteExpiration, TimeSpan slidingExpiration)`
Creates a `CachePolicy` with both absolute and sliding expiration times. Both parameters must be positive `TimeSpan` values.

**Returns:** A new `CachePolicy` instance with both `AbsoluteExpiration` and `SlidingExpiration` set to the provided values.

**Throws:** `ArgumentOutOfRangeException` if either `absoluteExpiration` or `slidingExpiration` is zero or negative.

### `public CachePolicy WithCondition(Func<bool> condition)`
Configures a conditional validation rule for the cache entry. The `condition` parameter must be non-null.

**Returns:** A new `CachePolicy` instance with `Condition` set to the provided value.

**Throws:** `ArgumentNullException` if `condition` is `null`.

### `public CachePolicy WithMaxSize(int maxSize)`
Sets the maximum number of entries allowed in the cache. The `maxSize` parameter must be a positive integer.

**Returns:** A new `CachePolicy` instance with `MaxSize` set to the provided value.

**Throws:** `ArgumentOutOfRangeException` if `maxSize` is zero or negative.

### `public bool IsExpired(DateTimeOffset lastAccessed, DateTimeOffset created)`
Determines whether the cache entry has expired based on the provided timestamps and the policy's expiration rules.

**Parameters:**
- `lastAccessed`: The timestamp of the last access to the entry.
- `created`: The timestamp when the entry was created.

**Returns:** `true` if the entry is expired; otherwise, `false`.

## Usage

### Example 1: Basic Cache with Absolute Expiration
