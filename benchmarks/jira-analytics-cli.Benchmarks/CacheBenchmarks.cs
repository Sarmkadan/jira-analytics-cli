// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using JiraAnalyticsCli.Caching;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace JiraAnalyticsCli.Benchmarks;

/// <summary>
/// Benchmarks for in-memory cache operations.
/// Establishes baseline characteristics for Set, Get, and Contains — the three
/// paths that dominate cache overhead in the analytics pipeline.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class CacheBenchmarks
{
    private InMemoryCache _cache = null!;
    private CachePolicy  _policy = null!;
    private JiraIssue    _issue  = null!;

    private const string CacheKey = "sprint:42:issue:PROJ-9001";

    [GlobalSetup]
    public void Setup()
    {
        _cache  = new InMemoryCache(NullLogger<InMemoryCache>.Instance);
        _policy = CachePolicy.WithAbsoluteExpiration("bench", TimeSpan.FromMinutes(30));

        _issue = new JiraIssue
        {
            Key         = "PROJ-9001",
            Id          = "9001",
            Summary     = "Implement OAuth2 token refresh with sliding-window expiry",
            Status      = "In Progress",
            IssueType   = "Story",
            Priority    = "High",
            StoryPoints = 8,
            CreatedDate = DateTime.UtcNow.AddDays(-7),
            UpdatedDate = DateTime.UtcNow.AddHours(-2),
            ProjectKey  = "PROJ",
            SprintId    = 42,
            Labels      = new List<string> { "backend", "security" },
            Components  = new List<string> { "Auth" }
        };

        _cache.Set(CacheKey, _issue, _policy);
    }

    [Benchmark(Description = "CacheSet — JSON serialise + ConcurrentDictionary write")]
    public void CacheSet() => _cache.Set(CacheKey, _issue, _policy);

    [Benchmark(Description = "CacheGet (hit) — expiry check + JSON deserialise")]
    public JiraIssue? CacheGet() => _cache.Get<JiraIssue>(CacheKey);

    [Benchmark(Description = "CacheContains — expiry check only, no deserialise")]
    public bool CacheContains() => _cache.Contains(CacheKey);
}
