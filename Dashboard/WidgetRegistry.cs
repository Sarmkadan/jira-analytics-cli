// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Frozen;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Dashboard;

/// <summary>
/// Describes the static metadata for a widget type: its display name, default size,
/// description, and the setting keys it recognises.
/// </summary>
/// <param name="Type">The <see cref="WidgetType"/> this definition describes.</param>
/// <param name="DisplayName">Short human-readable label shown in the widget picker.</param>
/// <param name="Description">One-sentence description of what the widget shows.</param>
/// <param name="DefaultSize">Suggested <see cref="WidgetSize"/> when dragged onto the canvas.</param>
/// <param name="SupportedSettings">
/// Setting keys accepted in <see cref="WidgetConfig.Settings"/>.
/// Consumers should document values per key in the widget's data handler.
/// </param>
public sealed record WidgetDefinition(
    WidgetType              Type,
    string                  DisplayName,
    string                  Description,
    WidgetSize              DefaultSize,
    IReadOnlyList<string>   SupportedSettings);

/// <summary>
/// Central catalogue of all available dashboard widget types.
/// Provides factory methods for creating pre-configured <see cref="WidgetConfig"/> instances
/// and look-up access to static widget metadata.
/// </summary>
public sealed class WidgetRegistry
{
    // FrozenDictionary gives O(1) look-up with no locking overhead for this read-only catalogue.
    private static readonly FrozenDictionary<WidgetType, WidgetDefinition> _definitions =
        BuildDefinitions().ToFrozenDictionary(d => d.Type);

    /// <summary>
    /// Gets the full read-only catalogue of widget definitions keyed by <see cref="WidgetType"/>.
    /// </summary>
    public static IReadOnlyDictionary<WidgetType, WidgetDefinition> All => _definitions;

    /// <summary>
    /// Returns the <see cref="WidgetDefinition"/> for the given <paramref name="type"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="type"/> is not registered in the catalogue.
    /// </exception>
    public static WidgetDefinition GetDefinition(WidgetType type) =>
        _definitions.TryGetValue(type, out var def)
            ? def
            : throw new ArgumentOutOfRangeException(nameof(type), $"Unknown widget type: {type}");

    /// <summary>
    /// Returns an ordered list of all registered <see cref="WidgetType"/> values.
    /// </summary>
    public static IReadOnlyList<WidgetType> GetSupportedTypes() =>
        [.. _definitions.Keys.OrderBy(t => t.ToString())];

    /// <summary>
    /// Creates a ready-to-use <see cref="WidgetConfig"/> for the given <paramref name="type"/>,
    /// anchored at column 0, row 0, using the type's default size preset.
    /// </summary>
    /// <param name="type">Widget type to instantiate.</param>
    /// <param name="title">
    /// Optional title override. When omitted, the definition's <see cref="WidgetDefinition.DisplayName"/> is used.
    /// </param>
    /// <param name="column">Zero-based column for the initial grid position.</param>
    /// <param name="row">Zero-based row for the initial grid position.</param>
    public static WidgetConfig CreateWidget(WidgetType type, string? title = null, int column = 0, int row = 0)
    {
        var definition = GetDefinition(type);
        return new WidgetConfig
        {
            Type     = type,
            Title    = title ?? definition.DisplayName,
            Position = WidgetPosition.FromSize(definition.DefaultSize, column, row)
        };
    }

    /// <summary>
    /// Creates a <see cref="WidgetConfig"/> with an explicit <paramref name="size"/> override,
    /// ignoring the type's default size.
    /// </summary>
    /// <param name="type">Widget type to instantiate.</param>
    /// <param name="size">Grid size preset to apply.</param>
    /// <param name="title">Optional title override.</param>
    /// <param name="column">Zero-based column for the initial position.</param>
    /// <param name="row">Zero-based row for the initial position.</param>
    public static WidgetConfig CreateWidgetWithSize(
        WidgetType type,
        WidgetSize size,
        string?    title  = null,
        int        column = 0,
        int        row    = 0)
    {
        var definition = GetDefinition(type);
        return new WidgetConfig
        {
            Type     = type,
            Title    = title ?? definition.DisplayName,
            Position = WidgetPosition.FromSize(size, column, row)
        };
    }

    /// <summary>
    /// Determines whether a given setting key is recognised by the specified <paramref name="type"/>.
    /// Useful for validating user-supplied widget configurations.
    /// </summary>
    public static bool IsSettingSupported(WidgetType type, string settingKey) =>
        _definitions.TryGetValue(type, out var def) &&
        def.SupportedSettings.Contains(settingKey, StringComparer.OrdinalIgnoreCase);

    // -----------------------------------------------------------------------
    // Private catalogue initialisation
    // -----------------------------------------------------------------------

    private static IEnumerable<WidgetDefinition> BuildDefinitions() =>
    [
        new(WidgetType.VelocityChart,
            "Velocity Chart",
            "Bar chart of story-point velocity across the most recent sprints.",
            WidgetSize.Wide,
            ["sprintCount", "showAverage", "showTrendLine"]),

        new(WidgetType.BurndownChart,
            "Burndown Chart",
            "Ideal vs actual remaining story-point line chart for the active sprint.",
            WidgetSize.Large,
            ["sprintId", "showIdealLine"]),

        new(WidgetType.SprintHealth,
            "Sprint Health",
            "Completion rate, quality score, and risk level for the current sprint.",
            WidgetSize.Medium,
            ["sprintId", "showRiskScore"]),

        new(WidgetType.DeveloperLoad,
            "Developer Load",
            "Horizontal bar chart of open issue count distributed across the team.",
            WidgetSize.Wide,
            ["maxDevelopers", "groupByType"]),

        new(WidgetType.OverdueIssues,
            "Overdue Issues",
            "Count and average age of overdue issues, with a critical-priority callout.",
            WidgetSize.Medium,
            ["maxItems", "showCriticalOnly"]),

        new(WidgetType.QualityMetrics,
            "Quality Metrics",
            "Defect rate and top high-risk components based on bug frequency.",
            WidgetSize.Medium,
            ["sprintCount", "maxRiskAreas"]),

        new(WidgetType.IssuesSummary,
            "Issues Summary",
            "Donut/status breakdown of open, in-progress, done, and blocked issues.",
            WidgetSize.Small,
            ["includeSubtasks", "statusFilter"]),

        new(WidgetType.TrendIndicator,
            "Trend Indicator",
            "Single KPI metric with a directional percentage change arrow.",
            WidgetSize.Small,
            ["metric", "sprintCount", "unit"])
    ];
}
