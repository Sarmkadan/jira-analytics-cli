// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Caching;

/// <summary>
/// In-memory cache implementation for storing analytics and API data.
/// Thread-safe with automatic expiration and size management.
/// </summary>
public class InMemoryCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<InMemoryCache> _logger;
    private readonly object _mutationLock = new();
    private long _hits;
    private long _misses;
    private long _evictions;
    private long _entryCount;

    public InMemoryCache(ILogger<InMemoryCache> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this._logger = logger;
    }

    /// <summary>
    /// Sets value in cache with specified policy.
    /// Overwrites existing value if key already exists.
    /// </summary>
    public void Set<T>(string key, T value, CachePolicy policy)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(policy);

        var serialized = JsonSerializer.Serialize(value);
        var entry = new CacheEntry
        {
            Key = key,
            Value = serialized,
            ValueType = value?.GetType(),
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow,
            Policy = policy
        };

        lock (this._mutationLock)
        {
            if (this._cache.TryAdd(key, entry))
            {
                Interlocked.Increment(ref this._entryCount);
            }
            else
            {
                this._cache[key] = entry;
            }
        }

        this._logger.LogDebug("Cache set: {Key} ({Size} bytes)", key, serialized.Length);
    }

    /// <summary>
    /// Gets value from cache if it exists and is not expired.
    /// Returns default value if not found or expired.
    /// </summary>
    public T? Get<T>(string key, T? defaultValue = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (this._cache.TryGetValue(key, out var entry))
        {
            // Check expiration
            if (entry.Policy.IsExpired(entry.CreatedAt, entry.LastAccessedAt))
            {
                this.Evict(key);
                Interlocked.Increment(ref this._misses);
                this._logger.LogDebug("Cache expired: {Key}", key);
                return defaultValue;
            }

            // Update last accessed time for sliding expiration
            if (entry.Policy.SlidingExpiration.HasValue)
            {
                entry.LastAccessedAt = DateTime.UtcNow;
            }

            try
            {
                var value = JsonSerializer.Deserialize<T>(entry.Value);
                Interlocked.Increment(ref this._hits);
                this._logger.LogDebug("Cache hit: {Key}", key);
                return value;
            }
            catch (JsonException ex)
            {
                this._logger.LogError(ex, "Error deserializing cache entry: {Key}", key);
                this.Evict(key);
                Interlocked.Increment(ref this._misses);
                return defaultValue;
            }
        }

        Interlocked.Increment(ref this._misses);
        this._logger.LogDebug("Cache miss: {Key}", key);
        return defaultValue;
    }

    /// <summary>
    /// Checks if key exists in cache and is not expired.
    /// </summary>
    public bool Contains(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (this._cache.TryGetValue(key, out var entry))
        {
            if (entry.Policy.IsExpired(entry.CreatedAt, entry.LastAccessedAt))
            {
                this.Evict(key);
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes single entry from cache.
    /// </summary>
    public void Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        this.RemoveEntry(key);
        this._logger.LogDebug("Cache removed: {Key}", key);
    }

    /// <summary>
    /// Removes all entries matching key pattern (supports wildcards).
    /// </summary>
    public int RemoveByPattern(string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        var removed = 0;
        var regex = new System.Text.RegularExpressions.Regex(
            "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$");

        foreach (var key in this._cache.Keys)
        {
            if (regex.IsMatch(key))
            {
                if (this.RemoveEntry(key))
                {
                    removed++;
                }
            }
        }

        this._logger.LogDebug("Cache pattern removed: {Pattern} ({Count} entries)", pattern, removed);
        return removed;
    }

    /// <summary>
    /// Clears entire cache.
    /// </summary>
    public void Clear()
    {
        lock (this._mutationLock)
        {
            this._cache.Clear();
            Interlocked.Exchange(ref this._entryCount, 0);
        }

        this._logger.LogInformation("Cache cleared completely");
    }

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics(
            Interlocked.Read(ref this._hits),
            Interlocked.Read(ref this._misses),
            Interlocked.Read(ref this._evictions),
            (int)Interlocked.Read(ref this._entryCount));
    }

    /// <summary>
    /// Performs cleanup of expired entries.
    /// Usually called periodically by background task.
    /// </summary>
    public int CleanupExpired()
    {
        var removed = 0;
        var expiredKeys = new List<string>();

        foreach (var entry in this._cache.Values)
        {
            if (entry.Policy.IsExpired(entry.CreatedAt, entry.LastAccessedAt))
            {
                expiredKeys.Add(entry.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            if (this.Evict(key))
            {
                removed++;
            }
        }

        if (removed > 0)
        {
            this._logger.LogInformation("Cache cleanup removed {Count} expired entries", removed);
        }

        return removed;
    }

    private bool RemoveEntry(string key)
    {
        lock (this._mutationLock)
        {
            if (!this._cache.TryRemove(key, out _))
            {
                return false;
            }

            Interlocked.Decrement(ref this._entryCount);
            return true;
        }
    }

    private bool Evict(string key)
    {
        if (!this.RemoveEntry(key))
        {
            return false;
        }

        Interlocked.Increment(ref this._evictions);
        return true;
    }

    private class CacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public Type? ValueType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public CachePolicy Policy { get; set; } = new CachePolicy("unknown");
    }

    public record CacheStatistics(long Hits, long Misses, long Evictions, int EntryCount)
    {
        public double HitRate => this.Hits + this.Misses == 0 ? 0 : (double)this.Hits / (this.Hits + this.Misses);
    }
}
