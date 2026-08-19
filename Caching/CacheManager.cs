// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Caching;

/// <summary>
/// Manages multiple cache stores and coordinates cache strategies.
/// Provides facade for simplified cache access and management.
/// </summary>
public class CacheManager
{
    private readonly Dictionary<string, InMemoryCache> _stores = new();
    private readonly ILogger<CacheManager> _logger;
    private readonly TimeSpan _defaultExpiration;

    public CacheManager(ILogger<CacheManager> logger, TimeSpan? defaultExpiration = null)
    {
        _logger = logger;
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(15);
    }

    /// <summary>
    /// Gets or creates cache store with specified name.
    /// Allows different caching strategies for different data types.
    /// </summary>
    public InMemoryCache GetStore(string storeName = "default")
    {
        _logger.LogInformation("Getting cache store: {StoreName}", storeName);
        if (!_stores.ContainsKey(storeName))
        {
            _stores[storeName] = new InMemoryCache(new Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryCache>());
            _logger.LogDebug("Created cache store: {StoreName}", storeName);
        }

        _logger.LogInformation("Returning cache store: {StoreName}", storeName);
        return _stores[storeName];
    }

    /// <summary>
    /// Sets value in default cache store with standard expiration.
    /// </summary>
    public void SetDefault<T>(string key, T value)
    {
        _logger.LogInformation("Setting default cache value for key: {Key}", key);
        var policy = CachePolicy.WithAbsoluteExpiration(key, _defaultExpiration);
        GetStore().Set(key, value, policy);
        _logger.LogInformation("Default cache value set for key: {Key}", key);
    }

    /// <summary>
    /// Gets value from default cache store.
    /// </summary>
    public T? GetDefault<T>(string key, T? defaultValue = default)
    {
        _logger.LogInformation("Getting default cache value for key: {Key}", key);
        var value = GetStore().Get(key, defaultValue);
        if (value is null)
        {
            _logger.LogWarning("Default cache miss for key: {Key}, returning default value", key);
        }
        else
        {
            _logger.LogInformation("Default cache hit for key: {Key}", key);
        }
        return value;
    }

    /// <summary>
    /// Sets value with custom policy in named store.
    /// </summary>
    public void Set<T>(string storeName, string key, T value, CachePolicy policy)
    {
        _logger.LogInformation("Setting cache value in store: {StoreName}, key: {Key}", storeName, key);
        GetStore(storeName).Set(key, value, policy);
        _logger.LogInformation("Cache value set in store: {StoreName}, key: {Key}", storeName, key);
    }

    /// <summary>
    /// Gets value from named store with default value.
    /// </summary>
    public T? Get<T>(string storeName, string key, T? defaultValue = default)
    {
        _logger.LogInformation("Getting cache value from store: {StoreName}, key: {Key}", storeName, key);
        var value = GetStore(storeName).Get(key, defaultValue);
        if (value is null)
        {
            _logger.LogWarning("Cache miss for key: {Key} in store: {StoreName}, returning default value", key, storeName);
        }
        else
        {
            _logger.LogInformation("Cache hit for key: {Key} in store: {StoreName}", key, storeName);
        }
        return value;
    }

    /// <summary>
    /// Checks if value exists in cache and is not expired.
    /// </summary>
    public bool Contains(string key, string storeName = "default")
    {
        _logger.LogInformation("Checking if cache contains key: {Key} in store: {StoreName}", key, storeName);
        var contains = GetStore(storeName).Contains(key);
        _logger.LogInformation("Cache contains key {Key}: {Contains}", key, contains);
        return contains;
    }

    /// <summary>
    /// Removes value from cache.
    /// </summary>
    public void Remove(string key, string storeName = "default")
    {
        _logger.LogInformation("Removing cache entry: {Key} from store: {StoreName}", key, storeName);
        GetStore(storeName).Remove(key);
    }

    /// <summary>
    /// Clears specified store completely.
    /// </summary>
    public void ClearStore(string storeName = "default")
    {
        _logger.LogInformation("Clearing cache store: {StoreName}", storeName);
        GetStore(storeName).Clear();
    }

    /// <summary>
    /// Clears all cache stores.
    /// </summary>
    public void ClearAll()
    {
        _logger.LogInformation("Starting to clear all cache stores");
        foreach (var store in _stores.Values)
        {
            store.Clear();
        }

        _logger.LogInformation("All cache stores cleared");
    }

    /// <summary>
    /// Gets statistics across all cache stores.
    /// </summary>
    public Dictionary<string, InMemoryCache.CacheStatistics> GetGlobalStatistics()
    {
        _logger.LogInformation("Getting global cache statistics");
        var stats = new Dictionary<string, InMemoryCache.CacheStatistics>();

        foreach (var (name, store) in _stores)
        {
            stats[name] = store.GetStatistics();
        }

        _logger.LogInformation("Retrieved global cache statistics for {Count} stores", stats.Count);
        return stats;
    }

    /// <summary>
    /// Performs cleanup of expired entries across all stores.
    /// </summary>
    public int CleanupAll()
    {
        _logger.LogInformation("Starting cleanup of all cache stores");
        var totalRemoved = 0;

        foreach (var store in _stores.Values)
        {
            totalRemoved += store.CleanupExpired();
        }

        _logger.LogInformation("Cleanup finished, total expired entries removed: {TotalRemoved}", totalRemoved);
        return totalRemoved;
    }
}
