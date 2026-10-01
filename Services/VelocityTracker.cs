// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Tracks and analyses team velocity across sprints, providing trend detection and forecasting.
/// </summary>
public class VelocityTracker
{
    private readonly IMetricsRepository _metricsRepository;
    private readonly ILogger<VelocityTracker> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="VelocityTracker"/>.
    /// </summary>
    /// <param name="metricsRepository">Repository for persisted sprint metrics.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public VelocityTracker(IMetricsRepository metricsRepository, ILogger<VelocityTracker> logger)
    {
        _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Computes the rolling average velocity over the last N sprints.
    /// </summary>
    /// <param name="metrics">Sprint metrics ordered chronologically.</param>
    /// <param name="windowSize">Number of sprints in the rolling window.</param>
    /// <returns>Rolling average of completed story points.</returns>
    public double GetRollingAverageVelocity(IReadOnlyList<SprintMetric> metrics, int windowSize = 3)
    {
        if (metrics.Count == 0)
            return 0.0;

        var window = metrics
            .OrderByDescending(m => m.EndDate)
            .Take(windowSize)
            .ToList();

        double avg = window.Average(m => m.CompletedStoryPoints);
        _logger.LogDebug("Rolling velocity ({Window} sprints): {Velocity:F1} SP", window.Count, avg);
        return avg;
    }

    /// <summary>
    /// Detects whether velocity is trending up, down or stable using linear regression slope.
    /// </summary>
    /// <param name="metrics">Sprint metrics ordered chronologically.</param>
    /// <returns>A <see cref="VelocityTrend"/> describing the velocity trend.</returns>
    public VelocityTrend DetectTrend(IReadOnlyList<SprintMetric> metrics)
    {
        if (metrics.Count < 2)
        {
            return new VelocityTrend("Stable", Slope: 0, SampleSize: metrics.Count);
        }

        // Project sprint metrics into ITimeSeriesPoint for reuse of TrendAnalysis math.
        var points = metrics
            .OrderBy(m => m.EndDate)
            .Select(m => (ITimeSeriesPoint)new TimeSeriesPoint(m.EndDate.UtcDateTime, m.CompletedStoryPoints))
            .ToList();

        double slope = TrendAnalysis.CalculateSlope(points);

        string direction = slope switch
        {
            > 0.5 => "Increasing",
            < -0.5 => "Decreasing",
            _ => "Stable"
        };

        _logger.LogInformation("Velocity trend: {Direction} (slope={Slope:F2})", direction, slope);
        return new VelocityTrend(direction, slope, points.Count);
    }

    /// <summary>
    /// Forecasts how many story points the team will complete in the next sprint.
    /// Uses exponential weighted moving average giving recent sprints more weight.
    /// </summary>
    /// <param name="metrics">Sprint metrics ordered chronologically.</param>
    /// <param name="alpha">Smoothing factor between 0 and 1; higher values weight recent sprints more.</param>
    /// <returns>Forecasted velocity in story points.</returns>
    public double ForecastNextSprint(IReadOnlyList<SprintMetric> metrics, double alpha = 0.3)
    {
        if (metrics.Count == 0)
            return 0.0;

        var ordered = metrics.OrderBy(m => m.EndDate).ToList();
        double ewma = ordered[0].CompletedStoryPoints;

        for (int i = 1; i < ordered.Count; i++)
        {
            ewma = alpha * ordered[i].CompletedStoryPoints + (1 - alpha) * ewma;
        }

        _logger.LogDebug("EWMA forecast: {Forecast:F1} SP (alpha={Alpha})", ewma, alpha);
        return ewma;
    }

    /// <summary>
    /// Calculates commitment accuracy: the ratio of completed to planned story points.
    /// </summary>
    /// <param name="metric">A single sprint metric.</param>
    /// <returns>Accuracy percentage (0-100+), or 0 if nothing was planned.</returns>
    public double GetCommitmentAccuracy(SprintMetric metric)
    {
        if (metric.PlannedStoryPoints <= 0)
            return 0.0;

        return (double)metric.CompletedStoryPoints / metric.PlannedStoryPoints * 100.0;
    }
}

/// <summary>
/// Describes a velocity trend detected across sprints.
/// </summary>
/// <param name="Direction">Trend direction: Increasing, Decreasing or Stable.</param>
/// <param name="Slope">Linear regression slope in story points per sprint.</param>
/// <param name="SampleSize">Number of sprints analysed.</param>
public sealed record VelocityTrend(string Direction, double Slope, int SampleSize);
