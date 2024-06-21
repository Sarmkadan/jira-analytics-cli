// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Caching;

namespace JiraAnalyticsCli.Performance;

/// <summary>
/// Provides system diagnostics including health checks, resource monitoring, and reporting.
/// Useful for debugging performance issues and monitoring application health.
/// </summary>
public class DiagnosticsService
{
    private readonly ILogger<DiagnosticsService> _logger;
    private readonly CacheManager? _cacheManager;
    private readonly MetricsCollector? _metricsCollector;

    public DiagnosticsService(
        ILogger<DiagnosticsService> logger,
        CacheManager? cacheManager = null,
        MetricsCollector? metricsCollector = null)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _metricsCollector = metricsCollector;
    }

    /// <summary>
    /// Generates comprehensive health report of the application.
    /// </summary>
    public DiagnosticsReport GenerateHealthReport()
    {
        var report = new DiagnosticsReport
        {
            ReportGeneratedAt = DateTime.UtcNow,
            EnvironmentInfo = GetEnvironmentInfo(),
            SystemResources = GetSystemResources(),
            CacheStatistics = GetCacheStatistics(),
            PerformanceMetrics = GetPerformanceMetrics()
        };

        _logger.LogInformation("Health report generated");
        return report;
    }

    /// <summary>
    /// Checks if system resources are in healthy state.
    /// Returns overall health status and list of any issues found.
    /// </summary>
    public HealthCheckResult PerformHealthCheck()
    {
        var result = new HealthCheckResult
        {
            CheckedAt = DateTime.UtcNow,
            Issues = new List<HealthIssue>()
        };

        // Check memory usage
        var memoryPercent = GetMemoryUsagePercent();
        if (memoryPercent > 85)
        {
            result.Issues.Add(new HealthIssue
            {
                Severity = "Warning",
                Component = "Memory",
                Message = $"Memory usage is high: {memoryPercent:F1}%"
            });
        }

        // Check garbage collection
        var gcCount = GC.CollectionCount(2);
        result.GarbageCollectionInfo = new GarbageCollectionInfo
        {
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalAllocatedBytes = GC.GetTotalAllocatedBytes()
        };

        // Check cache health if available
        if (_cacheManager != null)
        {
            var stats = _cacheManager.GetGlobalStatistics();
            var totalSize = stats.Values.Sum(s => s.TotalSizeBytes);

            if (totalSize > 100 * 1024 * 1024) // 100 MB
            {
                result.Issues.Add(new HealthIssue
                {
                    Severity = "Warning",
                    Component = "Cache",
                    Message = $"Cache size is large: {totalSize / 1024 / 1024}MB"
                });
            }
        }

        result.Status = result.Issues.Count == 0 ? "Healthy" : "Warning";
        _logger.LogInformation("Health check completed: {Status}", result.Status);

        return result;
    }

    /// <summary>
    /// Gets detailed environment information.
    /// </summary>
    private EnvironmentInfo GetEnvironmentInfo()
    {
        return new EnvironmentInfo
        {
            OsVersion = Environment.OSVersion.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            DotNetVersion = Environment.Version.ToString(),
            ApplicationStartTime = Process.GetCurrentProcess().StartTime,
            CurrentWorkingDirectory = Environment.CurrentDirectory,
            MachineName = Environment.MachineName,
            UserName = Environment.UserName
        };
    }

    /// <summary>
    /// Gets current system resource metrics.
    /// </summary>
    private SystemResources GetSystemResources()
    {
        var process = Process.GetCurrentProcess();

        return new SystemResources
        {
            MemoryUsageMb = process.WorkingSet64 / 1024.0 / 1024.0,
            MemoryPeakMb = process.PeakWorkingSet64 / 1024.0 / 1024.0,
            VirtualMemoryMb = process.VirtualMemorySize64 / 1024.0 / 1024.0,
            ProcessorUsagePercent = GetProcessorUsage(),
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            UptimeSeconds = (DateTime.UtcNow - process.StartTime).TotalSeconds
        };
    }

    /// <summary>
    /// Gets cache statistics if cache manager available.
    /// </summary>
    private Dictionary<string, object>? GetCacheStatistics()
    {
        if (_cacheManager == null)
            return null;

        var stats = _cacheManager.GetGlobalStatistics();
        var result = new Dictionary<string, object>();

        foreach (var (storeName, stat) in stats)
        {
            result[storeName] = new
            {
                stat.TotalEntries,
                SizeInMb = stat.TotalSizeBytes / 1024.0 / 1024.0,
                stat.ExpiredEntries
            };
        }

        return result;
    }

    /// <summary>
    /// Gets performance metrics if metrics collector available.
    /// </summary>
    private Dictionary<string, object>? GetPerformanceMetrics()
    {
        if (_metricsCollector == null)
            return null;

        var slowest = _metricsCollector.GetSlowestOperations(5)
            .Select(m => new
            {
                m.OperationName,
                m.AverageDurationMs,
                m.ExecutionCount
            })
            .ToList();

        return new Dictionary<string, object>
        {
            { "SlowedOperations", slowest }
        };
    }

    private double GetMemoryUsagePercent()
    {
        var process = Process.GetCurrentProcess();
        var totalMemory = GC.GetTotalMemory(false);
        var physicalMemory = process.WorkingSet64;

        // Estimate percentage based on process working set
        return (physicalMemory / (1024.0 * 1024.0 * 1024.0)) * 100;
    }

    private double GetProcessorUsage()
    {
        try
        {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(100);
            return cpuCounter.NextValue();
        }
        catch
        {
            return 0; // Not available on all systems
        }
    }

    public class DiagnosticsReport
    {
        public DateTime ReportGeneratedAt { get; set; }
        public EnvironmentInfo EnvironmentInfo { get; set; } = new();
        public SystemResources SystemResources { get; set; } = new();
        public Dictionary<string, object>? CacheStatistics { get; set; }
        public Dictionary<string, object>? PerformanceMetrics { get; set; }
    }

    public class EnvironmentInfo
    {
        public string OsVersion { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public string DotNetVersion { get; set; } = string.Empty;
        public DateTime ApplicationStartTime { get; set; }
        public string CurrentWorkingDirectory { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class SystemResources
    {
        public double MemoryUsageMb { get; set; }
        public double MemoryPeakMb { get; set; }
        public double VirtualMemoryMb { get; set; }
        public double ProcessorUsagePercent { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public double UptimeSeconds { get; set; }
    }

    public class HealthCheckResult
    {
        public DateTime CheckedAt { get; set; }
        public string Status { get; set; } = "Unknown";
        public List<HealthIssue> Issues { get; set; } = new();
        public GarbageCollectionInfo GarbageCollectionInfo { get; set; } = new();
    }

    public class HealthIssue
    {
        public string Severity { get; set; } = string.Empty; // Info, Warning, Error
        public string Component { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class GarbageCollectionInfo
    {
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public long TotalAllocatedBytes { get; set; }
    }
}
