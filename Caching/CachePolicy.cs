// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Caching;

/// <summary>
/// Defines caching behavior and expiration policies for cached data.
/// Supports absolute expiration, sliding windows, and conditional caching.
/// </summary>
public class CachePolicy
{
    public string Key { get; set; }
    public TimeSpan? AbsoluteExpiration { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public Func<bool>? Condition { get; set; }
    public int MaxSize { get; set; } = 1024 * 1024; // 1MB default

    public CachePolicy(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Creates policy with absolute expiration time from now.
    /// Data expires regardless of access after specified duration.
    /// </summary>
    public static CachePolicy WithAbsoluteExpiration(string key, TimeSpan expiration)
    {
        return new CachePolicy(key) { AbsoluteExpiration = expiration };
    }

    /// <summary>
    /// Creates policy with sliding window expiration.
    /// Expiration resets on each access, useful for active data.
    /// </summary>
    public static CachePolicy WithSlidingExpiration(string key, TimeSpan expiration)
    {
        return new CachePolicy(key) { SlidingExpiration = expiration };
    }

    /// <summary>
    /// Creates policy with both absolute and sliding expiration.
    /// Whichever expires first wins (usually sliding).
    /// </summary>
    public static CachePolicy WithCombinedExpiration(string key, TimeSpan absolute, TimeSpan sliding)
    {
        return new CachePolicy(key)
        {
            AbsoluteExpiration = absolute,
            SlidingExpiration = sliding
        };
    }

    /// <summary>
    /// Creates policy with conditional caching.
    /// Cache entry is only used if condition returns true.
    /// </summary>
    public CachePolicy WithCondition(Func<bool> condition)
    {
        Condition = condition;
        return this;
    }

    /// <summary>
    /// Sets maximum size for this cache entry in bytes.
    /// </summary>
    public CachePolicy WithMaxSize(int maxSizeBytes)
    {
        MaxSize = maxSizeBytes;
        return this;
    }

    /// <summary>
    /// Determines if cache entry should be considered expired.
    /// Takes into account absolute and sliding expirations.
    /// </summary>
    public bool IsExpired(DateTime createdAt, DateTime lastAccessedAt)
    {
        var now = DateTime.UtcNow;

        // Check absolute expiration
        if (AbsoluteExpiration.HasValue)
        {
            var absoluteExpireTime = createdAt.Add(AbsoluteExpiration.Value);
            if (now > absoluteExpireTime)
                return true;
        }

        // Check sliding expiration
        if (SlidingExpiration.HasValue)
        {
            var slidingExpireTime = lastAccessedAt.Add(SlidingExpiration.Value);
            if (now > slidingExpireTime)
                return true;
        }

        // Check condition
        if (Condition != null && !Condition())
            return true;

        return false;
    }
}
