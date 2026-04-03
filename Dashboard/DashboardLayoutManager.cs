// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Dashboard;

/// <summary>
/// Handles grid-based layout validation, collision detection, auto-arrangement, and
/// empty-cell discovery for the dashboard canvas. All mutation methods return new
/// layout instances and leave the originals unmodified.
/// </summary>
public sealed class DashboardLayoutManager(ILogger<DashboardLayoutManager> logger)
{
    private readonly ILogger<DashboardLayoutManager> _logger = logger;

    // -----------------------------------------------------------------------
    // Validation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Validates that all widgets in <paramref name="layout"/> fit within the declared
    /// grid dimensions and that no two widgets overlap.
    /// </summary>
    /// <returns>
    /// A list of validation error messages; empty when the layout is valid.
    /// </returns>
    public IReadOnlyList<string> ValidateLayout(DashboardLayout layout)
    {
        var errors = new List<string>();

        foreach (var widget in layout.Widgets)
        {
            var pos = widget.Position;

            if (pos.Column < 0 || pos.Row < 0)
                errors.Add($"Widget '{widget.Title}' ({widget.Id[..8]}) has a negative origin ({pos.Column},{pos.Row}).");

            if (pos.RightEdge >= layout.Columns)
                errors.Add($"Widget '{widget.Title}' extends beyond column bound " +
                           $"({pos.RightEdge} ≥ {layout.Columns}).");

            if (pos.BottomEdge >= layout.Rows)
                errors.Add($"Widget '{widget.Title}' extends beyond row bound " +
                           $"({pos.BottomEdge} ≥ {layout.Rows}).");

            if (pos.Width < 1 || pos.Height < 1)
                errors.Add($"Widget '{widget.Title}' has non-positive dimensions ({pos.Width}×{pos.Height}).");
        }

        // Pairwise overlap check — O(n²) but n is capped at MaxWidgetsPerDashboard (≤100)
        var widgets = layout.Widgets;
        for (var i = 0; i < widgets.Count; i++)
        {
            for (var j = i + 1; j < widgets.Count; j++)
            {
                if (widgets[i].Position.Overlaps(widgets[j].Position))
                    errors.Add($"Widgets '{widgets[i].Title}' and '{widgets[j].Title}' overlap.");
            }
        }

        if (errors.Count > 0)
            _logger.LogWarning("Layout validation found {ErrorCount} issue(s).", errors.Count);

        return errors;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the given <paramref name="widget"/> can be placed
    /// at its current <see cref="WidgetConfig.Position"/> without violating grid bounds or
    /// colliding with any widget already in <paramref name="layout"/>.
    /// </summary>
    public bool CanPlace(DashboardLayout layout, WidgetConfig widget)
    {
        var pos = widget.Position;

        if (pos.Column < 0 || pos.Row < 0) return false;
        if (pos.RightEdge  >= layout.Columns) return false;
        if (pos.BottomEdge >= layout.Rows)    return false;
        if (pos.Width < 1  || pos.Height < 1) return false;

        return layout.Widgets
            .Where(w => w.Id != widget.Id)
            .All(w => !w.Position.Overlaps(pos));
    }

    // -----------------------------------------------------------------------
    // Collision detection
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns pairs of widgets whose bounding boxes overlap in <paramref name="layout"/>.
    /// The result is empty when the layout is collision-free.
    /// </summary>
    public IReadOnlyList<(WidgetConfig A, WidgetConfig B)> DetectCollisions(DashboardLayout layout)
    {
        var collisions = new List<(WidgetConfig, WidgetConfig)>();
        var widgets    = layout.Widgets;

        for (var i = 0; i < widgets.Count; i++)
            for (var j = i + 1; j < widgets.Count; j++)
                if (widgets[i].Position.Overlaps(widgets[j].Position))
                    collisions.Add((widgets[i], widgets[j]));

        return collisions;
    }

    // -----------------------------------------------------------------------
    // Empty-cell discovery
    // -----------------------------------------------------------------------

    /// <summary>
    /// Finds the first top-left grid cell at which a widget of
    /// <paramref name="width"/> × <paramref name="height"/> can be placed without
    /// colliding with existing widgets or exceeding the canvas bounds.
    /// Returns <see langword="null"/> when no fitting slot exists.
    /// </summary>
    /// <remarks>
    /// Scans row-by-row, left-to-right. This matches the natural reading order and
    /// produces compact layouts that fill from the top-left corner.
    /// </remarks>
    public WidgetPosition? FindNextAvailablePosition(DashboardLayout layout, int width, int height)
    {
        for (var row = 0; row + height <= layout.Rows; row++)
        {
            for (var col = 0; col + width <= layout.Columns; col++)
            {
                var candidate = new WidgetPosition(col, row, width, height);
                var occupied  = layout.Widgets.Any(w => w.Position.Overlaps(candidate));
                if (!occupied)
                    return candidate;
            }
        }

        _logger.LogDebug(
            "No available {Width}×{Height} slot found in {Cols}×{Rows} grid with {Count} widget(s).",
            width, height, layout.Columns, layout.Rows, layout.Widgets.Count);

        return null;
    }

    // -----------------------------------------------------------------------
    // Auto-arrange
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns a new <see cref="DashboardLayout"/> where all widgets have been
    /// repositioned to eliminate gaps and collisions, scanning left-to-right then
    /// top-to-bottom. Widgets retain their dimensions; only their origins change.
    /// </summary>
    /// <param name="layout">Source layout (not mutated).</param>
    public DashboardLayout AutoArrange(DashboardLayout layout)
    {
        _logger.LogInformation(
            "Auto-arranging {Count} widget(s) on a {Cols}×{Rows} grid.",
            layout.Widgets.Count, layout.Columns, layout.Rows);

        // Sort by current top-then-left position to preserve approximate visual order.
        var ordered  = layout.Widgets
            .OrderBy(w => w.Position.Row)
            .ThenBy(w => w.Position.Column)
            .ToList();

        var arranged = new DashboardLayout
        {
            Columns         = layout.Columns,
            Rows            = layout.Rows,
            BackgroundColor = layout.BackgroundColor,
            AccentColor     = layout.AccentColor
        };

        foreach (var widget in ordered)
        {
            var slot = FindNextAvailablePosition(arranged, widget.Position.Width, widget.Position.Height);

            // If the widget genuinely doesn't fit, keep its current position and log a warning.
            // This avoids data loss at the cost of a remaining collision.
            var newPosition = slot ?? widget.Position;

            if (slot is null)
                _logger.LogWarning(
                    "Widget '{Title}' ({Id}) could not be placed during auto-arrange; position unchanged.",
                    widget.Title, widget.Id[..8]);

            arranged.Widgets.Add(new WidgetConfig
            {
                Id                     = widget.Id,
                Type                   = widget.Type,
                Title                  = widget.Title,
                Position               = newPosition,
                Settings               = widget.Settings,
                RefreshIntervalSeconds = widget.RefreshIntervalSeconds,
                IsVisible              = widget.IsVisible
            });
        }

        return arranged;
    }

    // -----------------------------------------------------------------------
    // Grid occupancy map
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds a boolean occupancy grid for the layout, where <c>true</c> indicates
    /// that a cell is occupied by at least one widget. Useful for visual debugging and
    /// export renderers that need per-cell state.
    /// </summary>
    /// <returns>
    /// A 2-D array indexed as <c>[row, column]</c> with dimensions
    /// <c>[layout.Rows, layout.Columns]</c>.
    /// </returns>
    public bool[,] BuildOccupancyGrid(DashboardLayout layout)
    {
        var grid = new bool[layout.Rows, layout.Columns];

        foreach (var widget in layout.Widgets)
        {
            var pos = widget.Position;
            for (var r = pos.Row; r <= Math.Min(pos.BottomEdge, layout.Rows - 1); r++)
                for (var c = pos.Column; c <= Math.Min(pos.RightEdge, layout.Columns - 1); c++)
                    grid[r, c] = true;
        }

        return grid;
    }

    /// <summary>
    /// Computes the percentage of grid cells occupied by widgets.
    /// A value of <c>1.0</c> means the canvas is completely full.
    /// </summary>
    public double GetGridUtilization(DashboardLayout layout)
    {
        var total    = layout.Columns * layout.Rows;
        if (total == 0) return 0;

        var occupancy = BuildOccupancyGrid(layout);
        var occupied  = 0;

        for (var r = 0; r < layout.Rows; r++)
            for (var c = 0; c < layout.Columns; c++)
                if (occupancy[r, c]) occupied++;

        return (double)occupied / total;
    }
}
