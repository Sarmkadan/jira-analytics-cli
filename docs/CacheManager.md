# CacheManager

Central cache manager for the jira-analytics-cli project. Provides typed in-memory caching with per-type defaults and global statistics. Supports CRUD operations on named caches, cleanup policies, and thread-safe access to cached values.

## API

### `public CacheManager`

Constructor that initializes the cache manager with default settings. All caches are created lazily on first access.

### `public InMemoryCache GetStore()`

Returns the underlying default in-memory cache instance used for all operations when no explicit cache name is provided.

- **Returns**: The default `InMemoryCache` instance.
- **Throws**: None.

### `public void SetDefault<T>(T value)`

Sets the default value for type `T`. This value is returned by `GetDefault<T>` when no cached value exists for `T`.

- **Type Parameter**: `T` – The type of value to set as default.
- **Parameter**: `value` – The default value to store.
- **Throws**: None.

### `public T? GetDefault<T>()`

Retrieves the default value previously set for type `T` via `SetDefault<T>`. Returns `null` if no default has been set.

- **Type Parameter**: `T` – The type of value to retrieve.
- **Returns**: The default value of type `T`, or `null` if not set.
- **Throws**: None.

### `public void Set<T>(string key, T value)`

Stores a value of type `T` in the default cache under the specified key.

- **Type Parameter**: `T` – The type of value to store.
- **Parameter**: `key` – The unique key under which the value is stored.
- **Parameter**: `value` – The value to cache.
- **Throws**: None.

### `public T? Get<T>(string key)`

Retrieves a value of type `T` from the default cache using the specified key. Returns `null` if the key does not exist or the cached value is of an incompatible type.

- **Type Parameter**: `T` – The expected type of the cached value.
- **Parameter**: `key` – The key of the cached value to retrieve.
- **Returns**: The cached value of type `T`, or `null` if not found or type mismatch.
- **Throws**: None.

### `public bool Contains(string key)`

Checks whether the default cache contains an entry under the specified key.

- **Parameter**: `key` – The key to check.
- **Returns**: `true` if the key exists in the cache; otherwise, `false`.
- **Throws**: None.

### `public void Remove(string key)`

Removes the entry with the specified key from the default cache.

- **Parameter**: `key` – The key of the entry to remove.
- **Throws**: None.

### `public void ClearStore()`

Clears all entries from the default cache. Does not affect type defaults or other named caches.

- **Throws**: None.

### `public void ClearAll()`

Clears all entries from the default cache and resets all type defaults.

- **Throws**: None.

### `public Dictionary<string, InMemoryCache.CacheStatistics> GetGlobalStatistics()`

Returns a dictionary of cache statistics for all named caches managed by this instance. The default cache is included under the key `"default"`.

- **Returns**: A dictionary mapping cache names to their respective `CacheStatistics`.
- **Throws**: None.

### `public int CleanupAll()`

Triggers cleanup on all named caches, removing expired or invalid entries. Returns the total number of entries removed across all caches.

- **Returns**: The total number of entries removed during cleanup.
- **Throws**: None.

## Usage
