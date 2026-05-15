// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Manages the full lifecycle of custom dashboards: layout persistence, live data resolution,
/// drag-drop widget manipulation, and snapshot rendering.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Creates and persists a new named dashboard layout for <paramref name="projectKey"/>.
    /// </summary>
    /// <param name="name">Human-readable layout name (max 120 characters).</param>
    /// <param name="projectKey">Jira project key to associate with this layout.</param>
    /// <param name="layout">Initial canvas geometry and widget configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created <see cref="SavedLayout"/> with its generated identifier.</returns>
    Task<SavedLayout> CreateLayoutAsync(
        string          name,
        string          projectKey,
        DashboardLayout layout,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a saved layout by its unique identifier.
    /// Returns <see langword="null"/> when no layout with that identifier exists.
    /// </summary>
    /// <param name="layoutId">The UUID of the saved layout.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<SavedLayout?> GetLayoutAsync(string layoutId, CancellationToken ct = default);

    /// <summary>
    /// Returns all saved layouts associated with <paramref name="projectKey"/>,
    /// ordered by <see cref="SavedLayout.UpdatedAt"/> descending.
    /// </summary>
    /// <param name="projectKey">Jira project key to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<SavedLayout>> GetLayoutsForProjectAsync(
        string            projectKey,
        CancellationToken ct = default);

    /// <summary>
    /// Replaces the canvas and widget configuration of an existing layout.
    /// </summary>
    /// <param name="layoutId">Identifier of the layout to update.</param>
    /// <param name="layout">New layout definition to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated <see cref="SavedLayout"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="layoutId"/> does not exist.</exception>
    Task<SavedLayout> UpdateLayoutAsync(
        string            layoutId,
        DashboardLayout   layout,
        CancellationToken ct = default);

    /// <summary>
    /// Permanently removes the saved layout with the specified <paramref name="layoutId"/>.
    /// No-ops when the identifier does not exist.
    /// </summary>
    /// <param name="layoutId">Identifier of the layout to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteLayoutAsync(string layoutId, CancellationToken ct = default);

    /// <summary>
    /// Fetches live analytics data for every visible widget in the layout and returns a
    /// fully-populated <see cref="DashboardSnapshot"/> ready for rendering or export.
    /// Widget data is resolved in parallel; individual widget failures are captured in
    /// <see cref="PopulatedWidget.ErrorMessage"/> without aborting the entire render.
    /// </summary>
    /// <param name="layoutId">Identifier of the layout to render.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="layoutId"/> does not exist.</exception>
    Task<DashboardSnapshot> RenderDashboardAsync(string layoutId, CancellationToken ct = default);

    /// <summary>
    /// Resolves analytics data for a single <paramref name="widget"/> in the context of
    /// <paramref name="projectKey"/>. Returns <see langword="null"/> when the widget type
    /// has no data source configured.
    /// </summary>
    /// <param name="widget">Widget configuration including type and type-specific settings.</param>
    /// <param name="projectKey">Jira project key used as the analytics scope.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<WidgetData?> GetWidgetDataAsync(
        WidgetConfig      widget,
        string            projectKey,
        CancellationToken ct = default);

    // -----------------------------------------------------------------------
    // Immutable layout mutation helpers — each returns a new layout instance
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns a new <see cref="DashboardLayout"/> with <paramref name="widget"/> appended,
    /// validating that it fits within the grid bounds and does not collide with existing widgets.
    /// </summary>
    /// <param name="layout">Source layout (not mutated).</param>
    /// <param name="widget">Widget to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the layout has reached its maximum widget capacity, the widget falls
    /// outside the grid bounds, or it overlaps an existing widget.
    /// </exception>
    DashboardLayout AddWidget(DashboardLayout layout, WidgetConfig widget);

    /// <summary>
    /// Returns a new <see cref="DashboardLayout"/> with the widget identified by
    /// <paramref name="widgetId"/> removed. No-ops when the widget is not found.
    /// </summary>
    /// <param name="layout">Source layout (not mutated).</param>
    /// <param name="widgetId">Identifier of the widget to remove.</param>
    DashboardLayout RemoveWidget(DashboardLayout layout, string widgetId);

    /// <summary>
    /// Returns a new <see cref="DashboardLayout"/> with the target widget relocated to
    /// <paramref name="newPosition"/>, validating grid bounds and collision avoidance.
    /// </summary>
    /// <param name="layout">Source layout (not mutated).</param>
    /// <param name="widgetId">Identifier of the widget to move.</param>
    /// <param name="newPosition">Desired top-left position (column/row) and dimensions.</param>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="widgetId"/> is not in the layout.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the new position is out of bounds or collides.</exception>
    DashboardLayout MoveWidget(DashboardLayout layout, string widgetId, WidgetPosition newPosition);

    /// <summary>
    /// Returns a new <see cref="DashboardLayout"/> with the target widget resized to
    /// <paramref name="width"/> columns and <paramref name="height"/> rows, while keeping its origin.
    /// </summary>
    /// <param name="layout">Source layout (not mutated).</param>
    /// <param name="widgetId">Identifier of the widget to resize.</param>
    /// <param name="width">New column span (must be ≥ 1).</param>
    /// <param name="height">New row span (must be ≥ 1).</param>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="widgetId"/> is not in the layout.</exception>
    DashboardLayout ResizeWidget(DashboardLayout layout, string widgetId, int width, int height);
}
