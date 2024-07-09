// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Configuration options for the dashboard builder subsystem.
/// Bind from <c>appsettings.json</c> under the <c>"Dashboard"</c> section,
/// or supply directly when registering services via
/// <see cref="ServiceCollectionExtensions.AddDashboardServices"/>.
/// </summary>
public sealed class DashboardOptions
{
    /// <summary>
    /// Gets or sets the number of horizontal columns in the default dashboard grid.
    /// Widgets whose right edge exceeds this value are rejected during validation.
    /// Defaults to <c>12</c>, matching a standard Bootstrap-style grid.
    /// </summary>
    [Range(4, 24)]
    public int DefaultGridColumns { get; init; } = 12;

    /// <summary>
    /// Gets or sets the number of vertical rows in the default dashboard grid.
    /// Defaults to <c>8</c>.
    /// </summary>
    [Range(2, 32)]
    public int DefaultGridRows { get; init; } = 8;

    /// <summary>
    /// Gets or sets the maximum number of widgets allowed on a single dashboard layout.
    /// Adding a widget beyond this limit throws <see cref="InvalidOperationException"/>.
    /// Defaults to <c>20</c>.
    /// </summary>
    [Range(1, 100)]
    public int MaxWidgetsPerDashboard { get; init; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of saved layouts stored per Jira project.
    /// Defaults to <c>50</c>.
    /// </summary>
    [Range(1, 500)]
    public int MaxSavedLayoutsPerProject { get; init; } = 50;

    /// <summary>
    /// Gets or sets whether the layout manager should automatically rearrange widgets
    /// to eliminate gaps and collisions when a widget is added or moved.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableAutoArrange { get; init; } = true;

    /// <summary>
    /// Gets or sets the default interval at which widget data is automatically refreshed.
    /// Individual widgets may override this via <c>WidgetConfig.RefreshIntervalSeconds</c>.
    /// Defaults to <c>5 minutes</c>.
    /// </summary>
    public TimeSpan DefaultDataRefreshInterval { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets whether resolved widget data is written back to the shared
    /// <c>CacheManager</c> to avoid redundant analytics calls on the next render.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableWidgetDataCaching { get; init; } = true;

    /// <summary>
    /// Gets or sets the TTL applied to cached widget data entries.
    /// Defaults to <c>3 minutes</c>.
    /// </summary>
    public TimeSpan WidgetCacheTtl { get; init; } = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Gets or sets the default number of sprints fetched when a widget setting does
    /// not specify an explicit <c>sprintCount</c>. Defaults to <c>5</c>.
    /// </summary>
    [Range(1, 52)]
    public int DefaultSprintCount { get; init; } = 5;

    /// <summary>
    /// Gets or sets the accent colour applied to widget borders and chart series
    /// when no project-specific theme is configured. Defaults to <c>#89b4fa</c>.
    /// </summary>
    public string DefaultAccentColor { get; init; } = "#89b4fa";

    /// <summary>
    /// Gets or sets the canvas background colour used for new layouts.
    /// Defaults to <c>#1e1e2e</c>.
    /// </summary>
    public string DefaultBackgroundColor { get; init; } = "#1e1e2e";

    /// <summary>
    /// Validates that all option values are within their allowed ranges.
    /// Throws <see cref="ArgumentOutOfRangeException"/> on the first violation found.
    /// </summary>
    public void Validate()
    {
        if (DefaultGridColumns is < 4 or > 24)
            throw new ArgumentOutOfRangeException(nameof(DefaultGridColumns), "Must be between 4 and 24.");

        if (DefaultGridRows is < 2 or > 32)
            throw new ArgumentOutOfRangeException(nameof(DefaultGridRows), "Must be between 2 and 32.");

        if (MaxWidgetsPerDashboard is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(MaxWidgetsPerDashboard), "Must be between 1 and 100.");

        if (DefaultDataRefreshInterval < TimeSpan.FromSeconds(10))
            throw new ArgumentOutOfRangeException(nameof(DefaultDataRefreshInterval),
                "Refresh interval must be at least 10 seconds.");

        if (WidgetCacheTtl < TimeSpan.FromSeconds(10))
            throw new ArgumentOutOfRangeException(nameof(WidgetCacheTtl),
                "Cache TTL must be at least 10 seconds.");
    }
}
