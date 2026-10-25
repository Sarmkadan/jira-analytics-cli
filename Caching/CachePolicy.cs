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
    /// <summary>
    /// Gets or sets the cache key this policy applies to.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the maximum lifetime of an entry measured from its creation time, or <c>null</c> to disable.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the idle timeout measured from the last access, or <c>null</c> to disable.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets an optional predicate; the entry is treated as expired when it returns <c>false</c>.
    /// </summary>
    public Func<bool>? Condition { get; set; }

    /// <summary>
    /// Gets or sets the maximum size of a cache entry in bytes. Defaults to 1 MB.
    /// </summary>
    public int MaxSize { get; set; } = 1024 * 1024; // 1MB default

    /// <summary>
    /// Initializes a new policy for the specified cache key.
    /// </summary>
    /// <param name="key">The cache key this policy applies to.</param>
    public CachePolicy(string key)
    {
        this.Key = key;
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
        this.Condition = condition;
        return this;
    }

    /// <summary>
    /// Sets maximum size for this cache entry in bytes.
    /// </summary>
    public CachePolicy WithMaxSize(int maxSizeBytes)
    {
        this.MaxSize = maxSizeBytes;
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
        if (this.AbsoluteExpiration.HasValue)
        {
            var absoluteExpireTime = createdAt.Add(this.AbsoluteExpiration.Value);
            if (now > absoluteExpireTime)
            {
                return true;
            }
        }

        // Check sliding expiration
        if (this.SlidingExpiration.HasValue)
        {
            var slidingExpireTime = lastAccessedAt.Add(this.SlidingExpiration.Value);
            if (now > slidingExpireTime)
            {
                return true;
            }
        }

        // Check condition
        if (this.Condition != null && !this.Condition())
        {
            return true;
        }

        return false;
    }
}
