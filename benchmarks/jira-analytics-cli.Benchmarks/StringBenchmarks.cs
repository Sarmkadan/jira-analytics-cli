// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using JiraAnalyticsCli.Utils;

namespace JiraAnalyticsCli.Benchmarks;

/// <summary>
/// Benchmarks for string manipulation hot paths used throughout the CLI.
/// Tests realistic inputs that mirror production Jira key, summary, and slug data.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class StringBenchmarks
{
    private const string JiraKeyWithSpaces     = "  PROJ-1234  sprint-planning  alpha  ";
    private const string LongIssueSummary      = "Implement OAuth2 token refresh with sliding-window expiry and per-client rate limits for the API gateway";
    private const string RawSlugInput          = "Hello World! This is a Jira Sprint -- alpha release v2.0";
    private const string SprintLabelA          = "MYPROJECT-SPRINT-2024-Q1-WEEK-03";
    private const string SprintLabelB          = "MYPROJECT-SPRINT-2024-Q2-WEEK-01";
    private const string WildcardPattern       = "*PROJ-12*";

    [Benchmark(Description = "RemoveWhitespace — ArrayPool fast path")]
    public string RemoveWhitespace() => JiraKeyWithSpaces.RemoveWhitespace();

    [Benchmark(Description = "TruncateWithEllipsis — Span concat")]
    public string TruncateWithEllipsis() => LongIssueSummary.TruncateWithEllipsis(50);

    [Benchmark(Description = "ToSlug — GeneratedRegex pipeline")]
    public string ToSlug() => RawSlugInput.ToSlug();

    [Benchmark(Description = "GetCommonPrefix — Span scan")]
    public string GetCommonPrefix() => SprintLabelA.GetCommonPrefix(SprintLabelB);

    [Benchmark(Description = "MatchesPattern — cached compiled Regex")]
    public bool MatchesPattern() => JiraKeyWithSpaces.MatchesPattern(WildcardPattern);
}
