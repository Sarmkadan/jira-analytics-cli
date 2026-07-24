// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Immutable, minimal implementation of <see cref="ITimeSeriesPoint"/> used by the
/// adapter extension methods to project domain-specific metric types into a shape
/// the shared <see cref="TrendAnalysis"/> helper understands.
/// </summary>
/// <param name="Timestamp">The point in time the observation was recorded at.</param>
/// <param name="Value">The numeric value observed at <paramref name="Timestamp"/>.</param>
public sealed record TimeSeriesPoint(DateTime Timestamp, double Value) : ITimeSeriesPoint;