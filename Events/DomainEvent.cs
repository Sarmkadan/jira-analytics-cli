// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Events;

/// <summary>
/// Base class for all domain events in the analytics system.
/// Provides timestamp, correlation ID, and event metadata.
/// </summary>
public abstract class DomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets event type name for routing and identification.
    /// </summary>
    public virtual string EventType => GetType().Name;

    /// <summary>
    /// Adds metadata to event for tracing and diagnostics.
    /// </summary>
    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }
}

/// <summary>
/// Event raised when sprint analysis is completed.
/// </summary>
public class SprintAnalysisCompletedEvent : DomainEvent
{
    public string ProjectKey { get; set; } = string.Empty;
    public int SprintId { get; set; }
    public double Velocity { get; set; }
    public int IssuesCompleted { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}

/// <summary>
/// Event raised when metrics are synced from Jira.
/// </summary>
public class MetricsSyncedEvent : DomainEvent
{
    public string ProjectKey { get; set; } = string.Empty;
    public int MetricsCount { get; set; }
    public DateTime LastSyncTime { get; set; }
}

/// <summary>
/// Event raised when report is generated.
/// </summary>
public class ReportGeneratedEvent : DomainEvent
{
    public string ProjectKey { get; set; } = string.Empty;
    public string ReportFormat { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FilePath { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when an error occurs during processing.
/// </summary>
public class ProcessingErrorEvent : DomainEvent
{
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error"; // Info, Warning, Error, Critical
}

/// <summary>
/// Event raised when cache is cleared or updated.
/// </summary>
public class CacheUpdatedEvent : DomainEvent
{
    public string CacheKey { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty; // Set, Clear, Remove
    public long DataSizeBytes { get; set; }
}
