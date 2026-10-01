// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Calculates burndown charts and ideal trend lines from sprint snapshot data.
/// </summary>
public class BurndownCalculator
{
    private readonly ISnapshotStore _snapshotStore;
    private readonly ILogger<BurndownCalculator> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="BurndownCalculator"/>.
    /// </summary>
    /// <param name="snapshotStore">Store providing point-in-time burndown snapshots.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public BurndownCalculator(ISnapshotStore snapshotStore, ILogger<BurndownCalculator> logger)
    {
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Computes the ideal (linear) burndown line for a sprint.
    /// </summary>
    /// <param name="totalPoints">Total story points committed at sprint start.</param>
    /// <param name="sprintStart">Sprint start timestamp.</param>
    /// <param name="sprintEnd">Sprint end timestamp.</param>
    /// <param name="intervalCount">Number of evenly spaced data points to generate.</param>
    /// <returns>A list of <see cref="TimeSeriesPoint"/> representing the ideal line.</returns>
    public List<TimeSeriesPoint> ComputeIdealBurndown(int totalPoints, DateTimeOffset sprintStart, DateTimeOffset sprintEnd, int intervalCount = 10)
    {
        if (intervalCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(intervalCount), "Interval count must be positive.");

        var result = new List<TimeSeriesPoint>(intervalCount + 1);
        var totalDuration = sprintEnd - sprintStart;

        for (int i = 0; i <= intervalCount; i++)
        {
            double fraction = (double)i / intervalCount;
            var timestamp = sprintStart + totalDuration * fraction;
            double remaining = totalPoints * (1.0 - fraction);
            result.Add(new TimeSeriesPoint(timestamp.UtcDateTime, remaining));
        }

        _logger.LogDebug("Computed ideal burndown with {Count} points for {Points} SP", result.Count, totalPoints);
        return result;
    }

    /// <summary>
    /// Builds the actual burndown series from stored snapshots for a sprint.
    /// </summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <returns>Ordered time series of remaining story points.</returns>
    public async Task<List<TimeSeriesPoint>> GetActualBurndownAsync(int sprintId)
    {
        var snapshots = await _snapshotStore.LoadAsync(sprintId);

        if (snapshots.Count == 0)
        {
            _logger.LogWarning("No snapshots found for sprint {SprintId}", sprintId);
            return [];
        }

        return snapshots
            .OrderBy(s => s.Timestamp)
            .Select(s => new TimeSeriesPoint(s.Timestamp.UtcDateTime, s.RemainingStoryPoints))
            .ToList();
    }

    /// <summary>
    /// Calculates the burn rate (story points per day) from a sequence of snapshots.
    /// </summary>
    /// <param name="snapshots">Chronologically ordered snapshots.</param>
    /// <returns>Average burn rate in story points per day, or zero if insufficient data.</returns>
    public double CalculateBurnRate(IReadOnlyList<BurndownSnapshot> snapshots)
    {
        if (snapshots.Count < 2)
            return 0.0;

        var first = snapshots[0];
        var last = snapshots[^1];
        double elapsedDays = (last.Timestamp - first.Timestamp).TotalDays;

        if (elapsedDays <= 0)
            return 0.0;

        double pointsBurned = first.RemainingStoryPoints - last.RemainingStoryPoints;
        double rate = pointsBurned / elapsedDays;

        _logger.LogDebug("Burn rate: {Rate:F2} SP/day over {Days:F1} days", rate, elapsedDays);
        return rate;
    }

    /// <summary>
    /// Projects the sprint completion date based on current burn rate.
    /// </summary>
    /// <param name="snapshots">Chronologically ordered snapshots.</param>
    /// <returns>Projected completion date, or null if the team is not making progress.</returns>
    public DateTimeOffset? ProjectCompletionDate(IReadOnlyList<BurndownSnapshot> snapshots)
    {
        double burnRate = CalculateBurnRate(snapshots);
        if (burnRate <= 0)
        {
            _logger.LogWarning("Non-positive burn rate ({Rate:F2}); cannot project completion", burnRate);
            return null;
        }

        var latest = snapshots[^1];
        double daysRemaining = latest.RemainingStoryPoints / burnRate;
        return latest.Timestamp.AddDays(daysRemaining);
    }
}
