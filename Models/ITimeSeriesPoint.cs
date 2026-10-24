// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Represents a single observation in a numeric time series, used as the common
/// input shape for trend/delta calculations shared across the various metric types
/// (burndown snapshots, sprint metrics, cycle time results, etc).
/// </summary>
public interface ITimeSeriesPoint
{
    /// <summary>Gets the point in time the observation was recorded at.</summary>
    public DateTime Timestamp { get; }

    /// <summary>Gets the numeric value observed at <see cref="Timestamp"/>.</summary>
    public double Value { get; }
}