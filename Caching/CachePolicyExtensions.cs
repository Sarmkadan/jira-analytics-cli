// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for CachePolicy providing fluent API for common caching scenarios
// =============================================================================

using System;

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
    /// <param name="policy">The cache policy to configure. Cannot be null.</param>
    /// <param name="condition">Function that returns true if cache entry should be used. Cannot be null.</param>
    /// <returns>The configured cache policy for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> or <paramref name="condition"/> is null.</exception>
    public static CachePolicy WithCondition(this CachePolicy policy, Func<bool> condition)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(condition);

        policy.Condition = condition;
        return policy;
    }

    /// <summary>
    /// Creates a cache policy with a maximum size limit in bytes.
    /// Useful for enforcing memory constraints on cached data.
    /// </summary>
    /// <param name="policy">The cache policy to configure. Cannot be null.</param>
    /// <param name="maxSizeBytes">Maximum size in bytes (e.g., 1024 * 1024 for 1MB). Must be non-negative.</param>
    /// <returns>The configured cache policy for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSizeBytes"/> is negative.</exception>
    public static CachePolicy WithMaxSize(this CachePolicy policy, int maxSizeBytes)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentOutOfRangeException.ThrowIfNegative(maxSizeBytes);

        policy.MaxSize = maxSizeBytes;
        return policy;
    }

    /// <summary>
    /// Creates a cache policy with both absolute and sliding expiration.
    /// The entry expires when either condition is met (whichever comes first).
    /// </summary>
    /// <param name="key">Cache entry key. Cannot be null or empty.</param>
    /// <param name="absoluteExpiration">Time from creation when entry expires absolutely.</param>
    /// <param name="slidingExpiration">Time from last access when entry expires.</param>
    /// <returns>New cache policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
    public static CachePolicy WithCombinedExpiration(this string key, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

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
    /// <param name="key">Cache entry key. Cannot be null or empty.</param>
    /// <param name="condition">Function that determines if cache should be used. Cannot be null.</param>
    /// <returns>New cache policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="condition"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
    public static CachePolicy WithCondition(this string key, Func<bool> condition)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(condition);

        return new CachePolicy(key) { Condition = condition };
    }

    /// <summary>
    /// Creates a cache policy with a maximum size limit.
    /// </summary>
    /// <param name="key">Cache entry key. Cannot be null or empty.</param>
    /// <param name="maxSizeBytes">Maximum size in bytes. Must be non-negative.</param>
    /// <returns>New cache policy instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSizeBytes"/> is negative.</exception>
    public static CachePolicy WithMaxSize(this string key, int maxSizeBytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentOutOfRangeException.ThrowIfNegative(maxSizeBytes);

        return new CachePolicy(key) { MaxSize = maxSizeBytes };
    }

    /// <summary>
    /// Checks if a cache policy has any expiration configured (absolute or sliding).
    /// </summary>
    /// <param name="policy">The cache policy to check. Cannot be null.</param>
    /// <returns>True if policy has expiration configured; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
    public static bool HasExpiration(this CachePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return policy.AbsoluteExpiration.HasValue || policy.SlidingExpiration.HasValue;
    }

    /// <summary>
    /// Gets the effective expiration time for the cache policy.
    /// Returns the soonest expiration time between absolute and sliding.
    /// </summary>
    /// <param name="policy">The cache policy. Cannot be null.</param>
    /// <param name="createdAt">When the cache entry was created.</param>
    /// <param name="lastAccessedAt">When the cache entry was last accessed.</param>
    /// <returns>TimeSpan representing the effective expiration, or null if no expiration is configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
    public static TimeSpan? GetEffectiveExpiration(this CachePolicy policy, DateTime createdAt, DateTime lastAccessedAt)
    {
        ArgumentNullException.ThrowIfNull(policy);

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