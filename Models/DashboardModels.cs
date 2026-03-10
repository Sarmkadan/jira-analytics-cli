// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Identifies the analytics data type rendered by a dashboard widget.
/// </summary>
public enum WidgetType
{
    /// <summary>Sprint-over-sprint velocity bar chart.</summary>
    VelocityChart,
    /// <summary>Ideal vs actual burndown line chart for the active sprint.</summary>
    BurndownChart,
    /// <summary>Current sprint health scorecard.</summary>
    SprintHealth,
    /// <summary>Per-developer issue load distribution.</summary>
    DeveloperLoad,
    /// <summary>Overdue issue count and severity breakdown.</summary>
    OverdueIssues,
    /// <summary>Defect rate and high-risk component list.</summary>
    QualityMetrics,
    /// <summary>Aggregate issue count by workflow status.</summary>
    IssuesSummary,
    /// <summary>Single KPI with percentage change arrow.</summary>
    TrendIndicator
}

/// <summary>
/// Predefined widget dimension presets mapped to the 12-column grid.
/// </summary>
public enum WidgetSize
{
    /// <summary>2 columns × 2 rows.</summary>
    Small,
    /// <summary>4 columns × 3 rows.</summary>
    Medium,
    /// <summary>6 columns × 4 rows.</summary>
    Large,
    /// <summary>8 columns × 3 rows.</summary>
    Wide,
    /// <summary>12 columns × 4 rows — spans the full dashboard width.</summary>
    Full
}

/// <summary>
/// Immutable position and dimension descriptor for a widget on the dashboard grid.
/// </summary>
/// <param name="Column">Zero-based column index of the widget's top-left corner.</param>
/// <param name="Row">Zero-based row index of the widget's top-left corner.</param>
/// <param name="Width">Number of columns the widget occupies.</param>
/// <param name="Height">Number of rows the widget occupies.</param>
public record WidgetPosition(
    [property: JsonPropertyName("column")] int Column,
    [property: JsonPropertyName("row")]    int Row,
    [property: JsonPropertyName("width")]  int Width,
    [property: JsonPropertyName("height")] int Height)
{
    /// <summary>Gets the column index of the rightmost cell occupied by this widget (inclusive).</summary>
    public int RightEdge => Column + Width - 1;

    /// <summary>Gets the row index of the bottommost cell occupied by this widget (inclusive).</summary>
    public int BottomEdge => Row + Height - 1;

    /// <summary>
    /// Determines whether this widget's bounding box overlaps with <paramref name="other"/>.
    /// Used for collision detection during drag-drop placement.
    /// </summary>
    public bool Overlaps(WidgetPosition other) =>
        Column <= other.RightEdge  && RightEdge  >= other.Column &&
        Row    <= other.BottomEdge && BottomEdge >= other.Row;

    /// <summary>
    /// Produces a <see cref="WidgetPosition"/> preset for the given <paramref name="size"/>
    /// anchored at (<paramref name="column"/>, <paramref name="row"/>).
    /// </summary>
    public static WidgetPosition FromSize(WidgetSize size, int column = 0, int row = 0) => size switch
    {
        WidgetSize.Small  => new(column, row, 2, 2),
        WidgetSize.Medium => new(column, row, 4, 3),
        WidgetSize.Large  => new(column, row, 6, 4),
        WidgetSize.Wide   => new(column, row, 8, 3),
        WidgetSize.Full   => new(column, row, 12, 4),
        _                 => new(column, row, 4, 3)
    };
}

/// <summary>
/// Configuration for a single dashboard widget, including its grid position and type-specific settings.
/// </summary>
public class WidgetConfig
{
    /// <summary>Gets or sets the unique widget identifier (UUID).</summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the analytics widget type.</summary>
    [Required]
    [JsonPropertyName("type")]
    public WidgetType Type { get; set; }

    /// <summary>Gets or sets the human-readable title shown in the widget header.</summary>
    [Required]
    [MaxLength(80)]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the widget's grid position and dimensions.</summary>
    [JsonPropertyName("position")]
    public WidgetPosition Position { get; set; } = new(0, 0, 4, 3);

    /// <summary>
    /// Gets or sets type-specific settings as key-value pairs
    /// (e.g. <c>sprintCount</c>, <c>developerKey</c>, <c>maxItems</c>).
    /// </summary>
    [JsonPropertyName("settings")]
    public Dictionary<string, string> Settings { get; set; } = new();

    /// <summary>Gets or sets how often the widget data is auto-refreshed, in seconds.</summary>
    [JsonPropertyName("refreshIntervalSeconds")]
    public int RefreshIntervalSeconds { get; set; } = 300;

    /// <summary>Gets or sets whether the widget is rendered in the dashboard view.</summary>
    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Defines the grid geometry and widget placements for a dashboard canvas.
/// </summary>
public class DashboardLayout
{
    /// <summary>Gets or sets the number of horizontal grid columns (default 12).</summary>
    [Range(1, 24)]
    [JsonPropertyName("columns")]
    public int Columns { get; set; } = 12;

    /// <summary>Gets or sets the number of vertical grid rows (default 8).</summary>
    [Range(1, 32)]
    [JsonPropertyName("rows")]
    public int Rows { get; set; } = 8;

    /// <summary>Gets or sets the ordered list of widget configurations on this canvas.</summary>
    [JsonPropertyName("widgets")]
    public List<WidgetConfig> Widgets { get; set; } = [];

    /// <summary>Gets or sets the canvas background colour as a CSS hex string.</summary>
    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "#1e1e2e";

    /// <summary>Gets or sets the accent colour used for widget borders and highlights.</summary>
    [JsonPropertyName("accentColor")]
    public string AccentColor { get; set; } = "#89b4fa";
}

/// <summary>
/// A named, persisted dashboard layout associated with a Jira project.
/// </summary>
public class SavedLayout
{
    /// <summary>Gets or sets the unique layout identifier (UUID).</summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the user-assigned layout name.</summary>
    [Required]
    [MaxLength(120)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the Jira project key this layout belongs to.</summary>
    [Required]
    [MaxLength(10)]
    [JsonPropertyName("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional plain-text description for this layout.</summary>
    [MaxLength(500)]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when this layout was first created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp of the most recent update.</summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the layout canvas and widget configuration.</summary>
    [JsonPropertyName("layout")]
    public DashboardLayout Layout { get; set; } = new();

    /// <summary>Gets or sets user-defined tags for filtering and organisation.</summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() =>
        $"[{Id[..8]}] {Name} ({ProjectKey}) — {Layout.Widgets.Count} widget(s), updated {UpdatedAt:yyyy-MM-dd}";
}

/// <summary>
/// A widget with its configuration and resolved analytics data, ready for rendering.
/// </summary>
/// <param name="Config">Widget configuration and grid position.</param>
/// <param name="Data">Resolved analytics payload; <see langword="null"/> when fetch failed.</param>
/// <param name="RenderedAt">UTC timestamp when data was resolved.</param>
/// <param name="ErrorMessage">Non-null description of what went wrong when <paramref name="Data"/> is <see langword="null"/>.</param>
public record PopulatedWidget(
    WidgetConfig Config,
    WidgetData?  Data,
    DateTime     RenderedAt,
    string?      ErrorMessage = null)
{
    /// <summary>Gets whether the widget has valid data available for display.</summary>
    public bool HasData => Data is not null && ErrorMessage is null;
}

/// <summary>
/// A fully-populated snapshot of a dashboard, ready for console rendering or file export.
/// </summary>
public class DashboardSnapshot
{
    /// <summary>Gets or sets the identifier of the source <see cref="SavedLayout"/>.</summary>
    [JsonPropertyName("layoutId")]
    public string LayoutId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the source layout.</summary>
    [JsonPropertyName("layoutName")]
    public string LayoutName { get; set; } = string.Empty;

    /// <summary>Gets or sets the Jira project key.</summary>
    [JsonPropertyName("projectKey")]
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when this snapshot was generated.</summary>
    [JsonPropertyName("renderedAt")]
    public DateTime RenderedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the list of populated widgets in layout order.</summary>
    [JsonPropertyName("widgets")]
    public List<PopulatedWidget> Widgets { get; set; } = [];

    /// <summary>Gets the subset of widgets that resolved data successfully.</summary>
    [JsonIgnore]
    public IEnumerable<PopulatedWidget> SuccessfulWidgets => Widgets.Where(w => w.HasData);

    /// <summary>Gets the subset of widgets that encountered a data-fetch error.</summary>
    [JsonIgnore]
    public IEnumerable<PopulatedWidget> FailedWidgets => Widgets.Where(w => !w.HasData);
}

// ---------------------------------------------------------------------------
// Widget data payloads — one concrete record per WidgetType
// ---------------------------------------------------------------------------

/// <summary>
/// Abstract base for all widget analytics payloads.
/// Carries the UTC fetch timestamp for staleness checks.
/// </summary>
public abstract record WidgetData
{
    /// <summary>Gets the UTC timestamp when this payload was fetched from the analytics layer.</summary>
    public DateTime FetchedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>Sprint-over-sprint velocity series for <see cref="WidgetType.VelocityChart"/>.</summary>
/// <param name="DataPoints">Ordered (sprint name, velocity) tuples, oldest first.</param>
/// <param name="Average">Mean velocity across the data series.</param>
/// <param name="Trend">Trend label: <c>Increasing</c>, <c>Decreasing</c>, or <c>Stable</c>.</param>
public record VelocityWidgetData(
    IReadOnlyList<(string Sprint, double Velocity)> DataPoints,
    double Average,
    string Trend) : WidgetData;

/// <summary>Burndown actuals and ideal line for <see cref="WidgetType.BurndownChart"/>.</summary>
/// <param name="SprintName">Display name of the sprint being charted.</param>
/// <param name="Snapshots">Day-by-day remaining story points.</param>
/// <param name="IdealSlope">Expected daily burn rate (points/day) for comparison.</param>
public record BurndownWidgetData(
    string SprintName,
    IReadOnlyList<BurndownSnapshot> Snapshots,
    double IdealSlope) : WidgetData;

/// <summary>Health scorecard for the active sprint, used by <see cref="WidgetType.SprintHealth"/>.</summary>
/// <param name="CurrentSprintName">Display name of the sprint being evaluated.</param>
/// <param name="CompletionRate">Percentage of planned story points completed.</param>
/// <param name="QualityScore">Quality score 0-100 based on defect rate.</param>
/// <param name="RiskScore">Risk score 0-100 derived from overdue, scope change, and quality factors.</param>
/// <param name="HealthStatus">Overall status: <c>Excellent</c>, <c>Healthy</c>, <c>At Risk</c>, or <c>Critical</c>.</param>
public record SprintHealthWidgetData(
    string CurrentSprintName,
    double CompletionRate,
    double QualityScore,
    double RiskScore,
    string HealthStatus) : WidgetData;

/// <summary>Developer workload distribution for <see cref="WidgetType.DeveloperLoad"/>.</summary>
/// <param name="LoadByDeveloper">Map of developer display name → assigned issue count.</param>
/// <param name="TopContributor">Name of the developer with the highest load.</param>
/// <param name="AverageLoad">Mean issue count per developer.</param>
public record DeveloperLoadWidgetData(
    IReadOnlyDictionary<string, int> LoadByDeveloper,
    string TopContributor,
    double AverageLoad) : WidgetData;

/// <summary>Overdue issue summary for <see cref="WidgetType.OverdueIssues"/>.</summary>
/// <param name="Total">Total count of overdue issues.</param>
/// <param name="Critical">Count of high-priority overdue issues.</param>
/// <param name="AverageDaysOverdue">Mean age past due date in days.</param>
/// <param name="IssueKeys">Jira issue keys of the overdue items (up to 20).</param>
public record OverdueIssuesWidgetData(
    int Total,
    int Critical,
    double AverageDaysOverdue,
    IReadOnlyList<string> IssueKeys) : WidgetData;

/// <summary>Quality metric aggregation for <see cref="WidgetType.QualityMetrics"/>.</summary>
/// <param name="TotalDefects">Absolute defect count across analysed sprints.</param>
/// <param name="DefectRate">Defects as a percentage of total issues.</param>
/// <param name="HighRiskAreas">Component names ordered by defect frequency.</param>
/// <param name="OverallQualityScore">Composite quality score 0-100.</param>
public record QualityMetricsWidgetData(
    int TotalDefects,
    double DefectRate,
    IReadOnlyList<string> HighRiskAreas,
    double OverallQualityScore) : WidgetData;

/// <summary>Issue count by workflow status for <see cref="WidgetType.IssuesSummary"/>.</summary>
/// <param name="Open">Issues in To Do / Open state.</param>
/// <param name="InProgress">Issues currently being worked on.</param>
/// <param name="Done">Resolved or closed issues.</param>
/// <param name="Blocked">Issues flagged as blocked.</param>
/// <param name="Total">Sum of all statuses.</param>
public record IssuesSummaryWidgetData(
    int Open,
    int InProgress,
    int Done,
    int Blocked,
    int Total) : WidgetData;

/// <summary>Single KPI with directional trend arrow for <see cref="WidgetType.TrendIndicator"/>.</summary>
/// <param name="Label">Short metric label (e.g. <c>Avg Velocity</c>).</param>
/// <param name="CurrentValue">Most recent measured value.</param>
/// <param name="PreviousValue">Baseline value for comparison.</param>
/// <param name="Unit">Display unit string (e.g. <c>pts/day</c>, <c>%</c>).</param>
/// <param name="Direction">Change direction: <c>Up</c>, <c>Down</c>, or <c>Flat</c>.</param>
public record TrendIndicatorWidgetData(
    string Label,
    double CurrentValue,
    double PreviousValue,
    string Unit,
    string Direction) : WidgetData
{
    /// <summary>Gets the percentage change from <see cref="PreviousValue"/> to <see cref="CurrentValue"/>.</summary>
    public double ChangePercent =>
        PreviousValue == 0 ? 0 : (CurrentValue - PreviousValue) / Math.Abs(PreviousValue) * 100;
}
