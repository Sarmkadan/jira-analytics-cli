// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for CachePolicy providing fluent API for common caching scenarios
// =============================================================================

namespace JiraAnalyticsCli.Caching;

/// <summary>
/// Extension methods for CachePolicy to enable fluent configuration patterns
/// and common caching scenarios.
/// </summary>
public static class CachePolicyExtensions
{
    /// <summary>
    /// Creates a cache policy with a condition that checks if the cache entry
    /// should be used based on a predicate function.
    /// </summary>
    /// <param name="policy">The cache policy to configure</param>
    /// <param name="condition">Function that returns true if cache entry should be used</param>
    /// <returns>The configured cache policy for method chaining</returns>
    public static CachePolicy WithCondition(this CachePolicy policy, Func<bool> condition)
    {
        policy.Condition = condition;
        return policy;
    }

    /// <summary>
    /// Creates a cache policy with a maximum size limit in bytes.
    /// Useful for enforcing memory constraints on cached data.
    /// </summary>
    /// <param name="policy">The cache policy to configure</param>
    /// <param name="maxSizeBytes">Maximum size in bytes (e.g., 1024 * 1024 for 1MB)</param>
    /// <returns>The configured cache policy for method chaining</returns>
    public static CachePolicy WithMaxSize(this CachePolicy policy, int maxSizeBytes)
    {
        policy.MaxSize = maxSizeBytes;
        return policy;
    }

    /// <summary>
    /// Creates a cache policy with both absolute and sliding expiration.
    /// The entry expires when either condition is met (whichever comes first).
    /// </summary>
    /// <param name="key">Cache entry key</param>
    /// <param name="absoluteExpiration">Time from creation when entry expires absolutely</param>
    /// <param name="slidingExpiration">Time from last access when entry expires</param>
    /// <returns>New cache policy instance</returns>
    public static CachePolicy WithCombinedExpiration(this string key, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
    {
        return new CachePolicy(key)
        {
            AbsoluteExpiration = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };
    }

    /// <summary>
    /// Creates a cache policy with conditional caching based on a predicate.
    /// The cache entry will only be used if the predicate returns true.
    /// </summary>
    /// <param name="key">Cache entry key</param>
    /// <param name="condition">Function that determines if cache should be used</param>
    /// <returns>New cache policy instance</returns>
    public static CachePolicy WithCondition(this string key, Func<bool> condition)
    {
        return new CachePolicy(key) { Condition = condition };
    }

    /// <summary>
    /// Creates a cache policy with a maximum size limit.
    /// </summary>
    /// <param name="key">Cache entry key</param>
    /// <param name="maxSizeBytes">Maximum size in bytes</param>
    /// <returns>New cache policy instance</returns>
    public static CachePolicy WithMaxSize(this string key, int maxSizeBytes)
    {
        return new CachePolicy(key) { MaxSize = maxSizeBytes };
    }

    /// <summary>
    /// Checks if a cache policy has any expiration configured (absolute or sliding).
    /// </summary>
    /// <param name="policy">The cache policy to check</param>
    /// <returns>True if policy has expiration configured</returns>
    public static bool HasExpiration(this CachePolicy policy)
    {
        return policy.AbsoluteExpiration.HasValue || policy.SlidingExpiration.HasValue;
    }

    /// <summary>
    /// Gets the effective expiration time for the cache policy.
    /// Returns the soonest expiration time between absolute and sliding.
    /// </summary>
    /// <param name="policy">The cache policy</param>
    /// <param name="createdAt">When the cache entry was created</param>
    /// <param name="lastAccessedAt">When the cache entry was last accessed</param>
    /// <returns>TimeSpan representing the effective expiration, or null if no expiration</returns>
    public static TimeSpan? GetEffectiveExpiration(this CachePolicy policy, DateTime createdAt, DateTime lastAccessedAt)
    {
        TimeSpan? result = null;

        if (policy.AbsoluteExpiration.HasValue)
        {
            var absoluteExpireTime = createdAt.Add(policy.AbsoluteExpiration.Value);
            var remainingAbsolute = absoluteExpireTime - DateTime.UtcNow;
            if (remainingAbsolute > TimeSpan.Zero)
            {
                result = remainingAbsolute;
            }
        }

        if (policy.SlidingExpiration.HasValue)
        {
            var slidingExpireTime = lastAccessedAt.Add(policy.SlidingExpiration.Value);
            var remainingSliding = slidingExpireTime - DateTime.UtcNow;
            if (remainingSliding > TimeSpan.Zero)
            {
                if (!result.HasValue || remainingSliding < result.Value)
                {
                    result = remainingSliding;
                }
            }
        }

        return result;
    }
}