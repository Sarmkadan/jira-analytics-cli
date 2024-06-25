// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Performance;

/// <summary>
/// Collects performance metrics for operations including timing, memory, and throughput.
/// Provides aggregate statistics and performance trend analysis.
/// </summary>
public class MetricsCollector
{
    private readonly ConcurrentDictionary<string, OperationMetrics> _metrics = new();
    private readonly ILogger<MetricsCollector> _logger;

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Records execution time for an operation.
    /// Aggregates multiple executions for statistical analysis.
    /// </summary>
    public void RecordOperation(string operationName, long durationMs, long? bytesProcessed = null)
    {
        var metrics = _metrics.AddOrUpdate(
            operationName,
            new OperationMetrics { OperationName = operationName },
            (_, existing) => existing);

        metrics.ExecutionCount++;
        metrics.TotalDurationMs += durationMs;
        metrics.MinDurationMs = Math.Min(metrics.MinDurationMs ?? durationMs, durationMs);
        metrics.MaxDurationMs = Math.Max(metrics.MaxDurationMs ?? durationMs, durationMs);
        metrics.LastExecutedAt = DateTime.UtcNow;

        if (bytesProcessed.HasValue)
        {
            metrics.TotalBytesProcessed += bytesProcessed.Value;
        }
    }

    /// <summary>
    /// Gets aggregate metrics for specific operation.
    /// </summary>
    public OperationMetrics? GetMetrics(string operationName)
    {
        return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
    }

    /// <summary>
    /// Gets metrics for all tracked operations.
    /// </summary>
    public IEnumerable<OperationMetrics> GetAllMetrics()
    {
        return _metrics.Values;
    }

    /// <summary>
    /// Gets slowest operations ranked by average duration.
    /// </summary>
    public IEnumerable<OperationMetrics> GetSlowestOperations(int topCount = 10)
    {
        return _metrics.Values
            .OrderByDescending(m => m.AverageDurationMs)
            .Take(topCount);
    }

    /// <summary>
    /// Gets most-called operations ranked by execution count.
    /// </summary>
    public IEnumerable<OperationMetrics> GetMostFrequentOperations(int topCount = 10)
    {
        return _metrics.Values
            .OrderByDescending(m => m.ExecutionCount)
            .Take(topCount);
    }

    /// <summary>
    /// Gets operations with highest memory throughput.
    /// </summary>
    public IEnumerable<OperationMetrics> GetHighestThroughputOperations(int topCount = 10)
    {
        return _metrics.Values
            .Where(m => m.TotalBytesProcessed > 0)
            .OrderByDescending(m => m.ThroughputMbPerSecond)
            .Take(topCount);
    }

    /// <summary>
    /// Clears all recorded metrics.
    /// </summary>
    public void Clear()
    {
        _metrics.Clear();
        _logger.LogInformation("Performance metrics cleared");
    }

    /// <summary>
    /// Exports metrics summary as formatted report.
    /// </summary>
    public string ExportSummary()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Performance Metrics Summary ===");
        report.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
        report.AppendLine($"Tracked Operations: {_metrics.Count}");
        report.AppendLine();

        foreach (var metric in _metrics.Values.OrderByDescending(m => m.AverageDurationMs).Take(20))
        {
            report.AppendLine($"{metric.OperationName}:");
            report.AppendLine($"  Executions: {metric.ExecutionCount}");
            report.AppendLine($"  Avg Duration: {metric.AverageDurationMs:F2}ms");
            report.AppendLine($"  Min/Max Duration: {metric.MinDurationMs}ms / {metric.MaxDurationMs}ms");

            if (metric.TotalBytesProcessed > 0)
            {
                report.AppendLine($"  Throughput: {metric.ThroughputMbPerSecond:F2} MB/s");
            }

            report.AppendLine();
        }

        return report.ToString();
    }

    public class OperationMetrics
    {
        public string OperationName { get; set; } = string.Empty;
        public long ExecutionCount { get; set; }
        public long TotalDurationMs { get; set; }
        public long? MinDurationMs { get; set; }
        public long? MaxDurationMs { get; set; }
        public long TotalBytesProcessed { get; set; }
        public DateTime? LastExecutedAt { get; set; }

        public double AverageDurationMs => ExecutionCount > 0 ? TotalDurationMs / (double)ExecutionCount : 0;

        public double ThroughputMbPerSecond
        {
            get
            {
                if (TotalDurationMs <= 0 || TotalBytesProcessed <= 0)
                    return 0;

                var seconds = TotalDurationMs / 1000.0;
                var megabytes = TotalBytesProcessed / 1024.0 / 1024.0;
                return megabytes / seconds;
            }
        }
    }
}
