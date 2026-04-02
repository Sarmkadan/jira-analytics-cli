// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using JiraAnalyticsCli.Formatters;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace JiraAnalyticsCli.Benchmarks;

/// <summary>
/// Benchmarks for CSV formatting hot paths.
/// Tests both serialisation (Format) and deserialisation (Parse) with varying dataset sizes
/// to expose the reflection and allocation costs that dominate at scale.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class CsvFormatterBenchmarks
{
    private CsvFormatter _formatter = null!;
    private List<SprintMetric> _metrics = null!;
    private string _csvData = null!;

    [Params(10, 100)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _formatter = new CsvFormatter(NullLogger<CsvFormatter>.Instance);

        _metrics = Enumerable.Range(1, ItemCount).Select(i => new SprintMetric
        {
            SprintId             = i,
            SprintName           = $"Sprint {i:D3}",
            StartDate            = DateTime.UtcNow.AddDays(-14 * i),
            EndDate              = DateTime.UtcNow.AddDays(-14 * i + 14),
            PlannedStoryPoints   = 40 + (i % 20),
            CompletedStoryPoints = 30 + (i % 15),
            CommittedStoryPoints = 42 + (i % 10),
            CompletedIssueCount  = 12 + (i % 8),
            TotalIssueCount      = 15 + (i % 10),
            DefectsCount         = i % 3,
            AverageCycleTime     = 2.5 + (i % 5) * 0.3,
            OverdueIssueCount    = i % 2,
            TeamSize             = 6,
            ScopeChangeCount     = i % 3
        }).ToList();

        _csvData = _formatter.Format(_metrics);
    }

    [Benchmark(Description = "Format — reflection-cached, pooled StringBuilder")]
    public string Format() => _formatter.Format(_metrics);

    [Benchmark(Description = "Parse — reflection-cached, dictionary header lookup")]
    public List<SprintMetric> Parse() => _formatter.Parse<SprintMetric>(_csvData);
}
