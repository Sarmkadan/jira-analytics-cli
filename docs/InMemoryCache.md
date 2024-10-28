# InMemoryCache

A lightweight, in-process caching abstraction that stores key-value pairs with optional expiration and policy-based eviction. Designed for CLI applications requiring fast, temporary storage of structured data with minimal overhead.

## API

### `public InMemoryCache`
Initializes a new instance of the in-memory cache with default settings. The cache starts empty and is thread-safe by design.

### `public void Set<T>(string key, T value, CachePolicy? policy = null)`
Stores a value in the cache under the specified key. If the key already exists, its value and policy are overwritten. The optional `policy` parameter defines expiration and eviction behavior; if omitted, a default policy is applied.

- **Parameters**
  - `key`: Unique identifier for the cached item. Must not be null.
  - `value`: The data to cache. Can be null if `T` is a nullable type.
  - `policy`: Optional cache policy governing expiration and eviction. If null, a default policy with no expiration is used.

- **Exceptions**
  - Throws `ArgumentNullException` if `key` is null.
  - Throws `ArgumentException` if `key` is empty or whitespace.

### `public T? Get<T>(string key)`
Retrieves a cached value by key. Returns null if the key does not exist or the value is expired.

- **Parameters**
  - `key`: The key of the item to retrieve. Must not be null.

- **Return Value**
  - The cached value of type `T`, or null if not found or expired.

- **Exceptions**
  - Throws `ArgumentNullException` if `key` is null.

### `public bool Contains(string key)`
Checks whether a key exists in the cache and has not expired.

- **Parameters**
  - `key`: The key to check. Must not be null.

- **Return Value**
  - `true` if the key exists and is valid; otherwise, `false`.

- **Exceptions**
  - Throws `ArgumentNullException` if `key` is null.

### `public void Remove(string key)`
Removes a single item from the cache by key. No effect if the key does not exist.

- **Parameters**
  - `key`: The key of the item to remove. Must not be null.

- **Exceptions**
  - Throws `ArgumentNullException` if `key` is null.

### `public int RemoveByPattern(string pattern)`
Removes all cache entries whose keys match the given regex pattern. Returns the number of items removed.

- **Parameters**
  - `pattern`: A regular expression string used to match keys. Must not be null.

- **Return Value**
  - The count of items removed.

- **Exceptions**
  - Throws `ArgumentNullException` if `pattern` is null.
  - Throws `ArgumentException` if `pattern` is not a valid regular expression.

### `public void Clear()`
Removes all entries from the cache. The cache remains usable after this operation.

### `public CacheStatistics GetStatistics()`
Returns a snapshot of current cache statistics, including entry counts, size, and expiration status.

- **Return Value**
  - A `CacheStatistics` struct containing metrics such as `TotalEntries`, `TotalSizeBytes`, `ExpiredEntries`, and timestamps.

### `public int CleanupExpired()`
Scans the cache and removes all expired entries. Returns the number of items removed.

- **Return Value**
  - The count of expired entries removed.

### `public string Key`
Gets the key associated with a cache entry. Read-only property available on cached items.

### `public string Value`
Gets the serialized string representation of the cached value. Read-only property available on cached items.

### `public Type? ValueType`
Gets the runtime type of the cached value. Returns null if the value is null or not set. Read-only property available on cached items.

### `public DateTime CreatedAt`
Gets the timestamp when the cache entry was created. Read-only property available on cached items.

### `public DateTime LastAccessedAt`
Gets the timestamp of the last access (read or write) to the cache entry. Read-only property available on cached items.

### `public CachePolicy Policy`
Gets the policy governing expiration and eviction for the cache entry. Read-only property available on cached items.

### `public int TotalEntries`
Gets the total number of entries currently stored in the cache. Read-only property available on cache statistics.

### `public long TotalSizeBytes`
Gets the estimated total size in bytes of all cached values. Read-only property available on cache statistics.

### `public int ExpiredEntries`
Gets the number of entries that have expired but not yet been removed. Read-only property available on cache statistics.

### `public DateTime CheckedAt`
Gets the timestamp when the statistics were last updated. Read-only property available on cache statistics.

## Usage
