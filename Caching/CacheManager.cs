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
        if (!_stores.ContainsKey(storeName))
        {
            _stores[storeName] = new InMemoryCache(_logger);
            _logger.LogDebug("Created cache store: {StoreName}", storeName);
        }

        return _stores[storeName];
    }

    /// <summary>
    /// Sets value in default cache store with standard expiration.
    /// </summary>
    public void SetDefault<T>(string key, T value)
    {
        var policy = CachePolicy.WithAbsoluteExpiration(key, _defaultExpiration);
        GetStore().Set(key, value, policy);
    }

    /// <summary>
    /// Gets value from default cache store.
    /// </summary>
    public T? GetDefault<T>(string key, T? defaultValue = default)
    {
        return GetStore().Get(key, defaultValue);
    }

    /// <summary>
    /// Sets value with custom policy in named store.
    /// </summary>
    public void Set<T>(string storeName, string key, T value, CachePolicy policy)
    {
        GetStore(storeName).Set(key, value, policy);
    }

    /// <summary>
    /// Gets value from named store with default value.
    /// </summary>
    public T? Get<T>(string storeName, string key, T? defaultValue = default)
    {
        return GetStore(storeName).Get(key, defaultValue);
    }

    /// <summary>
    /// Checks if value exists in cache and is not expired.
    /// </summary>
    public bool Contains(string key, string storeName = "default")
    {
        return GetStore(storeName).Contains(key);
    }

    /// <summary>
    /// Removes value from cache.
    /// </summary>
    public void Remove(string key, string storeName = "default")
    {
        GetStore(storeName).Remove(key);
    }

    /// <summary>
    /// Clears specified store completely.
    /// </summary>
    public void ClearStore(string storeName = "default")
    {
        GetStore(storeName).Clear();
    }

    /// <summary>
    /// Clears all cache stores.
    /// </summary>
    public void ClearAll()
    {
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
        var stats = new Dictionary<string, InMemoryCache.CacheStatistics>();

        foreach (var (name, store) in _stores)
        {
            stats[name] = store.GetStatistics();
        }

        return stats;
    }

    /// <summary>
    /// Performs cleanup of expired entries across all stores.
    /// </summary>
    public int CleanupAll()
    {
        var totalRemoved = 0;

        foreach (var store in _stores.Values)
        {
            totalRemoved += store.CleanupExpired();
        }

        return totalRemoved;
    }
}
