// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Performance measurement and optimization utilities.
/// Provides timing analysis, memory tracking, and performance profiling.
/// </summary>
public static class PerformanceHelpers
{
    /// <summary>
    /// Measures execution time of an async operation.
    /// Returns both the result and elapsed time for performance tracking.
    /// </summary>
    public static async Task<(T Result, TimeSpan Elapsed)> MeasureAsync<T>(
        Func<Task<T>> operation, ILogger? logger = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await operation();
            stopwatch.Stop();
            logger?.LogDebug($"Operation completed in {stopwatch.ElapsedMilliseconds}ms");

            return (result, stopwatch.Elapsed);
        }
        catch
        {
            stopwatch.Stop();
            throw;
        }
    }

    /// <summary>
    /// Measures execution time of a synchronous operation.
    /// </summary>
    public static (T Result, TimeSpan Elapsed) Measure<T>(Func<T> operation, ILogger? logger = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = operation();
            stopwatch.Stop();
            logger?.LogDebug($"Operation completed in {stopwatch.ElapsedMilliseconds}ms");

            return (result, stopwatch.Elapsed);
        }
        catch
        {
            stopwatch.Stop();
            throw;
        }
    }

    /// <summary>
    /// Measures execution time with automatic logging if threshold exceeded.
    /// Useful for identifying slow operations in production.
    /// </summary>
    public static async Task MeasureWithThresholdAsync(
        Func<Task> operation, TimeSpan threshold, ILogger logger, string operationName)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await operation();
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.Elapsed > threshold)
            {
                logger.LogWarning(
                    "Slow operation detected: {Operation} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    threshold.TotalMilliseconds);
            }
        }
    }

    /// <summary>
    /// Gets current memory usage in megabytes.
    /// Useful for monitoring memory consumption during processing.
    /// </summary>
    public static double GetCurrentMemoryMb()
    {
        return GC.GetTotalMemory(false) / 1024.0 / 1024.0;
    }

    /// <summary>
    /// Gets peak memory usage since application start.
    /// </summary>
    public static double GetPeakMemoryMb()
    {
        var process = Process.GetCurrentProcess();
        return process.PeakWorkingSet64 / 1024.0 / 1024.0;
    }

    /// <summary>
    /// Checks if memory usage is above specified percentage of available.
    /// Returns true if memory usage is concerning.
    /// </summary>
    public static bool IsHighMemoryUsage(double thresholdPercent = 85.0)
    {
        var totalMemory = GC.GetTotalMemory(false);
        var process = Process.GetCurrentProcess();
        var availableMemory = GC.GetGCMemoryInfo().TotalCommittedSize;

        var percentUsed = (totalMemory / (double)availableMemory) * 100;
        return percentUsed > thresholdPercent;
    }

    /// <summary>
    /// Performs explicit garbage collection and returns freed memory amount.
    /// Useful before large operations to ensure available heap space.
    /// </summary>
    public static double ForceGarbageCollection()
    {
        var before = GetCurrentMemoryMb();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var after = GetCurrentMemoryMb();

        return before - after; // Return freed memory
    }

    /// <summary>
    /// Creates a performance timer that logs timing on disposal (using statement).
    /// Convenient for scope-based performance tracking.
    /// </summary>
    public static IDisposable CreateScopedTimer(string operationName, ILogger logger)
    {
        return new PerformanceTimer(operationName, logger);
    }

    private class PerformanceTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private readonly ILogger _logger;

        public PerformanceTimer(string operationName, ILogger logger)
        {
            _operationName = operationName;
            _logger = logger;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.LogInformation(
                "{Operation} completed in {ElapsedMs}ms",
                _operationName,
                _stopwatch.ElapsedMilliseconds);
        }
    }
}
