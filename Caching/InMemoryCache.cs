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
    private readonly object _lockObj = new();

    public InMemoryCache(ILogger<InMemoryCache> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sets value in cache with specified policy.
    /// Overwrites existing value if key already exists.
    /// </summary>
    public void Set<T>(string key, T value, CachePolicy policy)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be empty", nameof(key));

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

        _cache[key] = entry;
        _logger.LogDebug("Cache set: {Key} ({Size} bytes)", key, serialized.Length);
    }

    /// <summary>
    /// Gets value from cache if it exists and is not expired.
    /// Returns default value if not found or expired.
    /// </summary>
    public T? Get<T>(string key, T? defaultValue = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            // Check expiration
            if (entry.Policy.IsExpired(entry.CreatedAt, entry.LastAccessedAt))
            {
                _cache.TryRemove(key, out _);
                _logger.LogDebug("Cache expired: {Key}", key);
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
                _logger.LogDebug("Cache hit: {Key}", key);
                return value;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing cache entry: {Key}", key);
                _cache.TryRemove(key, out _);
                return defaultValue;
            }
        }

        _logger.LogDebug("Cache miss: {Key}", key);
        return defaultValue;
    }

    /// <summary>
    /// Checks if key exists in cache and is not expired.
    /// </summary>
    public bool Contains(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Policy.IsExpired(entry.CreatedAt, entry.LastAccessedAt))
            {
                _cache.TryRemove(key, out _);
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
        _cache.TryRemove(key, out _);
        _logger.LogDebug("Cache removed: {Key}", key);
    }

    /// <summary>
    /// Removes all entries matching key pattern (supports wildcards).
    /// </summary>
    public int RemoveByPattern(string pattern)
    {
        var removed = 0;
        var regex = new System.Text.RegularExpressions.Regex(
            "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$");

        foreach (var key in _cache.Keys)
        {
            if (regex.IsMatch(key))
            {
                _cache.TryRemove(key, out _);
                removed++;
            }
        }

        _logger.LogDebug("Cache pattern removed: {Pattern} ({Count} entries)", pattern, removed);
        return removed;
    }

    /// <summary>
    /// Clears entire cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _logger.LogInformation("Cache cleared completely");
    }

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        lock (_lockObj)
        {
            var totalSize = 0L;
            var expiredCount = 0;
            var now = DateTime.UtcNow;

            foreach (var entry in _cache.Values)
            {
                totalSize += entry.Value.Length;

                if (entry.Policy.IsExpired(entry.CreatedAt, entry.LastAccessedAt))
                {
                    expiredCount++;
                }
            }

            return new CacheStatistics
            {
                TotalEntries = _cache.Count,
                TotalSizeBytes = totalSize,
                ExpiredEntries = expiredCount,
                CheckedAt = now
            };
        }
    }

    /// <summary>
    /// Performs cleanup of expired entries.
    /// Usually called periodically by background task.
    /// </summary>
    public int CleanupExpired()
    {
        var removed = 0;
        var expiredKeys = new List<string>();

        foreach (var entry in _cache.Values)
        {
            if (entry.Policy.IsExpired(entry.CreatedAt, entry.LastAccessedAt))
            {
                expiredKeys.Add(entry.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
            removed++;
        }

        if (removed > 0)
        {
            _logger.LogInformation("Cache cleanup removed {Count} expired entries", removed);
        }

        return removed;
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

    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public long TotalSizeBytes { get; set; }
        public int ExpiredEntries { get; set; }
        public DateTime CheckedAt { get; set; }
    }
}
