// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Persists <see cref="BurndownSnapshot"/> instances across CLI invocations so that
/// historical trend analysis can be performed between separate runs of the tool.
/// </summary>
public interface ISnapshotStore
{
    /// <summary>
    /// Appends a snapshot to the persisted series for its sprint.
    /// </summary>
    /// <param name="snapshot">The snapshot to persist.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task that completes once the snapshot has been written.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshot"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="snapshot"/> fails validation.</exception>
    Task AppendAsync(BurndownSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the persisted snapshot series for a sprint, ordered chronologically (oldest first).
    /// Corrupt or partially written lines (e.g. from an interrupted previous run) are skipped
    /// with a logged warning rather than failing the whole load.
    /// </summary>
    /// <param name="sprintId">The sprint identifier whose snapshot history should be loaded.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The persisted snapshots for the sprint, oldest first. Empty when none exist yet.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sprintId"/> is not positive.</exception>
    Task<IReadOnlyList<BurndownSnapshot>> LoadAsync(int sprintId, CancellationToken cancellationToken = default);
}
