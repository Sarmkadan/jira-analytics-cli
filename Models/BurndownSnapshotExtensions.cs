// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Extension methods for <see cref="BurndownSnapshot"/> that provide additional functionality
/// for analysis, comparison, and reporting on burndown data.
/// </summary>
public static class BurndownSnapshotExtensions
{
    /// <summary>
    /// Calculates the velocity trend over time by comparing the current burndown
    /// to historical snapshots from previous days.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="historicalSnapshots">Historical snapshots sorted by timestamp (newest first)</param>
    /// <returns>Velocity trend in story points per day, or 0 if insufficient data</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot or historicalSnapshots is null</exception>
    public static double CalculateVelocityTrend(
        this BurndownSnapshot snapshot,
        IReadOnlyList<BurndownSnapshot> historicalSnapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(historicalSnapshots);

        if (historicalSnapshots.Count < 2)
            return 0;

        // Find snapshots from previous days
        var previousDaySnapshots = historicalSnapshots
            .Where(h => h.Timestamp.Date < snapshot.Timestamp.Date)
            .ToList();

        if (previousDaySnapshots.Count < 1)
            return 0;

        // Calculate total story points burned between historical and current
        var oldestHistorical = previousDaySnapshots.MinBy(h => h.Timestamp)?.Timestamp ?? DateTime.MinValue;
        var daysBetween = (snapshot.Timestamp - oldestHistorical).TotalDays;

        if (daysBetween <= 0)
            return 0;

        var maxHistoricalCompleted = previousDaySnapshots.Max(h => h.CompletedStoryPoints);
        var totalStoryPointsBurned = snapshot.CompletedStoryPoints - maxHistoricalCompleted;
        return totalStoryPointsBurned / daysBetween;
    }

    /// <summary>
    /// Determines if the current burndown is accelerating or decelerating
    /// based on comparing recent velocity to overall velocity.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="historicalSnapshots">Historical snapshots sorted by timestamp (newest first)</param>
    /// <returns>True if velocity is accelerating (improving), false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot or historicalSnapshots is null</exception>
    public static bool IsVelocityAccelerating(
        this BurndownSnapshot snapshot,
        IReadOnlyList<BurndownSnapshot> historicalSnapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(historicalSnapshots);

        var velocityTrend = snapshot.CalculateVelocityTrend(historicalSnapshots);
        var oldestTimestamp = historicalSnapshots.LastOrDefault()?.Timestamp ?? snapshot.Timestamp.AddDays(-1);
        var daysBetween = (snapshot.Timestamp - oldestTimestamp).TotalDays;
        var overallVelocity = daysBetween > 0 ? snapshot.CompletedStoryPoints / daysBetween : 0;

        // If we're burning story points significantly faster now than overall, velocity is accelerating
        return velocityTrend > overallVelocity * 1.1; // 10% improvement threshold
    }

    /// <summary>
    /// Gets the burn rate in story points per day based on the current snapshot.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="daysInSprint">Total days in the sprint (default 14 for 2-week sprint)</param>
    /// <returns>Burn rate in story points per day</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when daysInSprint is not positive</exception>
    public static double GetBurnRate(
        this BurndownSnapshot snapshot,
        int daysInSprint = 14)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(daysInSprint, 0);

        if (daysInSprint <= 0)
            return 0;

        var startDate = snapshot.Timestamp.AddDays(-daysInSprint);
        var daysElapsed = (snapshot.Timestamp - startDate).TotalDays;

        if (daysElapsed <= 0)
            return 0;

        return snapshot.CompletedStoryPoints / daysElapsed;
    }

    /// <summary>
    /// Creates a comparison snapshot showing the difference between this snapshot
    /// and a previous snapshot.
    /// </summary>
    /// <param name="current">The current snapshot</param>
    /// <param name="previous">The previous snapshot to compare against</param>
    /// <returns>A new <see cref="BurndownSnapshot"/> representing the delta, or null if comparison not possible (e.g., timestamps are not in chronological order)</returns>
    /// <exception cref="ArgumentNullException">Thrown when current or previous is null</exception>
    public static BurndownSnapshot? CreateDeltaSnapshot(
        this BurndownSnapshot current,
        BurndownSnapshot previous)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(previous);

        // Only create delta if timestamps are comparable
        if (current.Timestamp <= previous.Timestamp)
            return null;

        return new BurndownSnapshot
        {
            Timestamp = current.Timestamp,
            SprintId = current.SprintId,
            RemainingStoryPoints = current.RemainingStoryPoints - previous.RemainingStoryPoints,
            CompletedStoryPoints = current.CompletedStoryPoints - previous.CompletedStoryPoints,
            TotalStoryPoints = current.TotalStoryPoints - previous.TotalStoryPoints,
            RemainingIssueCount = current.RemainingIssueCount - previous.RemainingIssueCount,
            CompletedIssueCount = current.CompletedIssueCount - previous.CompletedIssueCount,
            TotalIssueCount = current.TotalIssueCount - previous.TotalIssueCount,
            ScopeChanges = current.ScopeChanges - previous.ScopeChanges
        };
    }

    /// <summary>
    /// Formats the burndown data as a compact status string suitable for logging
    /// or dashboard display.
    /// </summary>
    /// <param name="snapshot">The snapshot to format</param>
    /// <param name="includeProjected">Whether to include projected completion info</param>
    /// <returns>Formatted status string</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null</exception>
    public static string ToStatusString(
        this BurndownSnapshot snapshot,
        bool includeProjected = true)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var parts = new List<string>
        {
            $"Sprint {snapshot.SprintId} @ {snapshot.Timestamp:yyyy-MM-dd HH:mm}",
            $"{snapshot.CompletedStoryPoints}/{snapshot.TotalStoryPoints} pts ({snapshot.GetBurndownPercentage():F1}%)",
            $"{snapshot.CompletedIssueCount}/{snapshot.TotalIssueCount} issues"
        };

        if (includeProjected && snapshot.TotalStoryPoints > 0)
        {
            var projectedDate = snapshot.Timestamp.AddDays(7);
            var projected = snapshot.GetProjectedCompletionPercentage(projectedDate);
            parts.Add($"Projected: {projected:F1}% in 7 days");
        }

        if (snapshot.ScopeChanges != 0)
        {
            parts.Add($"Scope change: {(snapshot.ScopeChanges > 0 ? "+" : "")}{snapshot.ScopeChanges}");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Determines if the current burndown indicates potential scope creep
    /// based on the number of scope changes.
    /// </summary>
    /// <param name="snapshot">The current snapshot</param>
    /// <param name="threshold">Minimum scope changes to consider as creep (default 3)</param>
    /// <returns>True if scope creep is detected</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when threshold is negative</exception>
    public static bool HasScopeCreep(
        this BurndownSnapshot snapshot,
        int threshold = 3)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfNegative(threshold);

        return snapshot.ScopeChanges >= threshold;
    }

    /// <summary>
    /// Gets a list of completed story points over time as a sequence of values.
    /// Useful for generating charts and visualizations.
    /// </summary>
    /// <param name="snapshots">List of snapshots sorted by timestamp (oldest first)</param>
    /// <returns>Sequence of completed story points values</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshots is null</exception>
    public static IEnumerable<int> GetCompletedStoryPointsOverTime(
        this IReadOnlyList<BurndownSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        return snapshots.Select(s => s.CompletedStoryPoints).ToList();
    }

    /// <summary>
    /// Gets a list of remaining story points over time as a sequence of values.
    /// Useful for generating charts and visualizations.
    /// </summary>
    /// <param name="snapshots">List of snapshots sorted by timestamp (oldest first)</param>
    /// <returns>Sequence of remaining story points values</returns>
    /// <exception cref="ArgumentNullException">Thrown when snapshots is null</exception>
    public static IEnumerable<int> GetRemainingStoryPointsOverTime(
        this IReadOnlyList<BurndownSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        return snapshots.Select(s => s.RemainingStoryPoints).ToList();
    }
}