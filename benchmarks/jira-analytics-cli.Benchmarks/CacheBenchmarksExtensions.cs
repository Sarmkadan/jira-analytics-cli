using System;
using System.Collections.Generic;
using System.Diagnostics;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Benchmarks
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="CacheBenchmarks"/>
    /// in benchmark scenarios.
    /// </summary>
    public static class CacheBenchmarksExtensions
    {
        /// <summary>
        /// Populates the cache with a collection of <see cref="JiraIssue"/> instances.
        /// The benchmark's <c>Setup</c> method is called first to ensure the cache is ready.
        /// </summary>
        /// <param name="bench">The benchmark instance.</param>
        /// <param name="issues">The issues to add to the cache.</param>
        public static void WarmCache(this CacheBenchmarks bench, IEnumerable<JiraIssue> issues)
        {
            // Ensure any required initialization is performed.
            bench.Setup();

            foreach (var issue in issues)
            {
                // Use the issue key as the cache key; the real CacheSet signature
                // expects a string key and the value to store.
                bench.CacheSet(issue.Key, issue);
            }
        }

        /// <summary>
        /// Checks whether a specific key is present in the cache and that the retrieved
        /// value is not <c>null</c>. Returns <c>true</c> only when both conditions are met.
        /// </summary>
        /// <param name="bench">The benchmark instance.</param>
        /// <param name="key">The cache key to test.</param>
        /// <returns>True if the key exists and a non‑null value is returned.</returns>
        public static bool VerifyCacheHit(this CacheBenchmarks bench, string key)
        {
            if (!bench.CacheContains(key))
                return false;

            var value = bench.CacheGet(key);
            return value != null;
        }

        /// <summary>
        /// Measures the time required to retrieve a cached <see cref="JiraIssue"/> for the
        /// given key. The result is a <see cref="TimeSpan"/> representing the elapsed time.
        /// </summary>
        /// <param name="bench">The benchmark instance.</param>
        /// <param name="key">The cache key to retrieve.</param>
        /// <returns>The duration of the cache lookup.</returns>
        public static TimeSpan MeasureCacheRetrievalTime(this CacheBenchmarks bench, string key)
        {
            var stopwatch = Stopwatch.StartNew();
            // The return value is intentionally ignored; the purpose is timing.
            _ = bench.CacheGet(key);
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
