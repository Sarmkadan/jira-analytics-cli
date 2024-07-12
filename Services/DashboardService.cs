// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using JiraAnalyticsCli.Caching;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Dashboard;
using JiraAnalyticsCli.Events;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;
#pragma warning disable CA1859 // use concrete type — intentional interface return

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Core dashboard engine. Manages layout persistence, resolves widget analytics data
/// from the existing service layer, and builds fully-populated
/// <see cref="DashboardSnapshot"/> instances ready for rendering or export.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly IAnalyticsService         _analytics;
    private readonly ISprintComparisonService  _comparison;
    private readonly CacheManager              _cache;
    private readonly EventBus                  _eventBus;
    private readonly DashboardLayoutManager    _layoutManager;
    private readonly DashboardOptions          _options;
    private readonly ILogger<DashboardService> _logger;

    // In-memory layout store — keyed by SavedLayout.Id (UUID string).
    // A real production implementation would swap this for a file or database repository.
    private readonly ConcurrentDictionary<string, SavedLayout> _store = new();

    /// <summary>
    /// Initialises the service with all required dependencies and validated options.
    /// </summary>
    public DashboardService(
        IAnalyticsService         analytics,
        ISprintComparisonService  comparison,
        CacheManager              cache,
        EventBus                  eventBus,
        DashboardLayoutManager    layoutManager,
        DashboardOptions          options,
        ILogger<DashboardService> logger)
    {
        _analytics     = analytics;
        _comparison    = comparison;
        _cache         = cache;
        _eventBus      = eventBus;
        _layoutManager = layoutManager;
        _options       = options;
        _logger        = logger;

        options.Validate();
    }

    // -----------------------------------------------------------------------
    // Layout CRUD
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public Task<SavedLayout> CreateLayoutAsync(
        string name, string projectKey, DashboardLayout layout, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectKey);

        var errors = _layoutManager.ValidateLayout(layout);
        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"Layout validation failed: {string.Join("; ", errors)}");

        var existing = _store.Values.Count(s => s.ProjectKey == projectKey);
        if (existing >= _options.MaxSavedLayoutsPerProject)
            throw new InvalidOperationException(
                $"Project '{projectKey}' has reached the maximum of {_options.MaxSavedLayoutsPerProject} saved layouts.");

        var saved = new SavedLayout
        {
            Name       = name,
            ProjectKey = projectKey,
            Layout     = layout
        };

        _store[saved.Id] = saved;
        _logger.LogInformation("Dashboard layout created: {Layout}", saved);

        _eventBus.Publish(new DashboardLayoutSavedEvent
        {
            ProjectKey  = projectKey,
            LayoutId    = saved.Id,
            LayoutName  = name,
            Operation   = "Created",
            WidgetCount = layout.Widgets.Count
        });

        return Task.FromResult(saved);
    }

    /// <inheritdoc />
    public Task<SavedLayout?> GetLayoutAsync(string layoutId, CancellationToken ct = default)
    {
        _store.TryGetValue(layoutId, out var layout);
        return Task.FromResult(layout);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SavedLayout>> GetLayoutsForProjectAsync(
        string projectKey, CancellationToken ct = default)
    {
        IReadOnlyList<SavedLayout> results = _store.Values
            .Where(s => string.Equals(s.ProjectKey, projectKey, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.UpdatedAt)
            .ToList();

        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task<SavedLayout> UpdateLayoutAsync(
        string layoutId, DashboardLayout layout, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(layoutId, out var existing))
            throw new KeyNotFoundException($"No saved layout with id '{layoutId}'.");

        var errors = _layoutManager.ValidateLayout(layout);
        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"Layout validation failed: {string.Join("; ", errors)}");

        var updated = new SavedLayout
        {
            Id         = existing.Id,
            Name       = existing.Name,
            ProjectKey = existing.ProjectKey,
            Description = existing.Description,
            CreatedAt  = existing.CreatedAt,
            UpdatedAt  = DateTime.UtcNow,
            Layout     = layout,
            Tags       = existing.Tags
        };

        _store[layoutId] = updated;
        _logger.LogInformation("Dashboard layout updated: {Layout}", updated);

        _eventBus.Publish(new DashboardLayoutSavedEvent
        {
            ProjectKey  = updated.ProjectKey,
            LayoutId    = updated.Id,
            LayoutName  = updated.Name,
            Operation   = "Updated",
            WidgetCount = layout.Widgets.Count
        });

        return Task.FromResult(updated);
    }

    /// <inheritdoc />
    public Task DeleteLayoutAsync(string layoutId, CancellationToken ct = default)
    {
        if (_store.TryRemove(layoutId, out var removed))
            _logger.LogInformation("Dashboard layout deleted: [{Id}] {Name}", removed.Id[..8], removed.Name);

        return Task.CompletedTask;
    }

    // -----------------------------------------------------------------------
    // Rendering
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<DashboardSnapshot> RenderDashboardAsync(string layoutId, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(layoutId, out var saved))
            throw new KeyNotFoundException($"No saved layout with id '{layoutId}'.");

        _logger.LogInformation(
            "Rendering dashboard '{Name}' ({Id}) for project {Project} — {Count} widget(s).",
            saved.Name, saved.Id[..8], saved.ProjectKey, saved.Layout.Widgets.Count);

        var visibleWidgets = saved.Layout.Widgets.Where(w => w.IsVisible).ToList();

        // Resolve all widget data concurrently — individual failures are isolated.
        var populated = await Task.WhenAll(
            visibleWidgets.Select(w => ResolveWidgetSafeAsync(w, saved.ProjectKey, ct)));

        var snapshot = new DashboardSnapshot
        {
            LayoutId    = saved.Id,
            LayoutName  = saved.Name,
            ProjectKey  = saved.ProjectKey,
            Widgets     = [.. populated]
        };

        _logger.LogInformation(
            "Dashboard rendered: {Success} succeeded, {Failed} failed.",
            snapshot.SuccessfulWidgets.Count(), snapshot.FailedWidgets.Count());

        return snapshot;
    }

    /// <inheritdoc />
    public async Task<WidgetData?> GetWidgetDataAsync(
        WidgetConfig widget, string projectKey, CancellationToken ct = default)
    {
        var sprintCount = widget.Settings.TryGetValue("sprintCount", out var sc)
            && int.TryParse(sc, out var n) ? n : _options.DefaultSprintCount;

        return widget.Type switch
        {
            WidgetType.VelocityChart  => await BuildVelocityDataAsync(projectKey, sprintCount, ct),
            WidgetType.BurndownChart  => await BuildBurndownDataAsync(projectKey, ct),
            WidgetType.SprintHealth   => await BuildSprintHealthDataAsync(projectKey, ct),
            WidgetType.DeveloperLoad  => await BuildDeveloperLoadDataAsync(projectKey, ct),
            WidgetType.OverdueIssues  => await BuildOverdueDataAsync(projectKey, ct),
            WidgetType.QualityMetrics => await BuildQualityDataAsync(projectKey, sprintCount, ct),
            WidgetType.IssuesSummary  => await BuildIssuesSummaryDataAsync(projectKey, ct),
            WidgetType.TrendIndicator => await BuildTrendIndicatorDataAsync(projectKey, sprintCount, widget, ct),
            _                         => null
        };
    }

    // -----------------------------------------------------------------------
    // Immutable layout mutation helpers
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public DashboardLayout AddWidget(DashboardLayout layout, WidgetConfig widget)
    {
        if (layout.Widgets.Count >= _options.MaxWidgetsPerDashboard)
            throw new InvalidOperationException(
                $"Cannot add widget: layout has reached the limit of {_options.MaxWidgetsPerDashboard} widgets.");

        if (!_layoutManager.CanPlace(layout, widget))
        {
            if (_options.EnableAutoArrange)
            {
                var slot = _layoutManager.FindNextAvailablePosition(
                    layout, widget.Position.Width, widget.Position.Height);

                if (slot is null)
                    throw new InvalidOperationException(
                        "No available slot for the widget in the current grid.");

                widget = CopyWidget(widget, slot);
            }
            else
            {
                throw new InvalidOperationException(
                    "Widget position is out of grid bounds or collides with an existing widget.");
            }
        }

        return CloneLayout(layout, [.. layout.Widgets, widget]);
    }

    /// <inheritdoc />
    public DashboardLayout RemoveWidget(DashboardLayout layout, string widgetId) =>
        CloneLayout(layout, layout.Widgets.Where(w => w.Id != widgetId).ToList());

    /// <inheritdoc />
    public DashboardLayout MoveWidget(DashboardLayout layout, string widgetId, WidgetPosition newPosition)
    {
        var target = layout.Widgets.FirstOrDefault(w => w.Id == widgetId)
            ?? throw new KeyNotFoundException($"Widget '{widgetId}' not found in layout.");

        var moved          = CopyWidget(target, newPosition);
        var withoutTarget  = CloneLayout(layout, layout.Widgets.Where(w => w.Id != widgetId).ToList());

        if (!_layoutManager.CanPlace(withoutTarget, moved))
            throw new InvalidOperationException(
                "Target position is out of grid bounds or collides with another widget.");

        return CloneLayout(layout, layout.Widgets.Select(w => w.Id == widgetId ? moved : w).ToList());
    }

    /// <inheritdoc />
    public DashboardLayout ResizeWidget(DashboardLayout layout, string widgetId, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width,  1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        var target = layout.Widgets.FirstOrDefault(w => w.Id == widgetId)
            ?? throw new KeyNotFoundException($"Widget '{widgetId}' not found in layout.");

        var newPosition = new WidgetPosition(target.Position.Column, target.Position.Row, width, height);
        return MoveWidget(layout, widgetId, newPosition);
    }

    // -----------------------------------------------------------------------
    // Private widget data builders — each maps to one WidgetType
    // -----------------------------------------------------------------------

    private async Task<WidgetData> BuildVelocityDataAsync(
        string projectKey, int sprintCount, CancellationToken ct)
    {
        var cacheKey = $"dashboard:velocity:{projectKey}:{sprintCount}";

        if (_options.EnableWidgetDataCaching && _cache.GetDefault<VelocityWidgetData>(cacheKey) is { } cached)
            return cached;

        var trend = await _analytics.AnalyzeVelocityTrend(projectKey, sprintCount).ConfigureAwait(false);
        var data  = new VelocityWidgetData(
            DataPoints: trend.Velocities.Select(v => (v.SprintName, v.Velocity)).ToList(),
            Average:    trend.Velocities.Any() ? trend.Velocities.Average(v => v.Velocity) : 0,
            Trend:      trend.Trend);

        if (_options.EnableWidgetDataCaching)
            _cache.Set("dashboard", cacheKey, data,
                CachePolicy.WithAbsoluteExpiration(cacheKey, _options.WidgetCacheTtl));

        return data;
    }

    private async Task<WidgetData> BuildBurndownDataAsync(string projectKey, CancellationToken ct)
    {
        var analysis = await _analytics.AnalyzeSprints(projectKey, 1).ConfigureAwait(false);
        var latest   = analysis.Metrics.FirstOrDefault();

        if (latest is null)
            return new BurndownWidgetData("No active sprint", [], 0);

        var duration   = (latest.EndDate - latest.StartDate).TotalDays;
        var idealSlope = duration > 0 ? latest.PlannedStoryPoints / duration : 0;

        return new BurndownWidgetData(latest.SprintName, [], idealSlope);
    }

    private async Task<WidgetData> BuildSprintHealthDataAsync(string projectKey, CancellationToken ct)
    {
        var analysis = await _analytics.AnalyzeSprints(projectKey, 1).ConfigureAwait(false);
        var latest   = analysis.Metrics.FirstOrDefault();

        if (latest is null)
            return new SprintHealthWidgetData("No data", 0, 100, 0, "Unknown");

        return new SprintHealthWidgetData(
            CurrentSprintName: latest.SprintName,
            CompletionRate:    latest.GetCompletionRate(),
            QualityScore:      latest.GetQualityScore(),
            RiskScore:         latest.GetRiskScore(),
            HealthStatus:      latest.GetHealthStatus());
    }

    private async Task<WidgetData> BuildDeveloperLoadDataAsync(string projectKey, CancellationToken ct)
    {
        var team = await _analytics.AnalyzeTeam(projectKey).ConfigureAwait(false);
        var load = team.WorkloadDistribution;

        var top = load.Any()
            ? load.MaxBy(kv => kv.Value).Key
            : "N/A";

        var avg = load.Any() ? load.Values.Average() : 0;

        return new DeveloperLoadWidgetData(load, top, avg);
    }

    private async Task<WidgetData> BuildOverdueDataAsync(string projectKey, CancellationToken ct)
    {
        var overdue = await _analytics.AnalyzeOverdueIssues(projectKey).ConfigureAwait(false);
        var keys    = overdue.Issues.Take(20).Select(i => i.Key).ToList();

        return new OverdueIssuesWidgetData(
            Total:              overdue.TotalOverdueCount,
            Critical:           overdue.CriticalCount,
            AverageDaysOverdue: overdue.AverageDaysOverdue,
            IssueKeys:          keys);
    }

    private async Task<WidgetData> BuildQualityDataAsync(
        string projectKey, int sprintCount, CancellationToken ct)
    {
        var quality = await _analytics.AnalyzeQuality(projectKey).ConfigureAwait(false);

        return new QualityMetricsWidgetData(
            TotalDefects:        quality.TotalDefects,
            DefectRate:          quality.DefectRate,
            HighRiskAreas:       quality.HighRiskAreas,
            OverallQualityScore: quality.AverageQualityScore);
    }

    private async Task<WidgetData> BuildIssuesSummaryDataAsync(string projectKey, CancellationToken ct)
    {
        var overdue  = await _analytics.AnalyzeOverdueIssues(projectKey).ConfigureAwait(false);
        var issues   = overdue.Issues;

        var open      = issues.Count(i => i.Status is "To Do" or "Open" or "Backlog");
        var inProgress = issues.Count(i => i.IsInProgress());
        var done      = issues.Count(i => i.Status is "Done" or "Closed" or "Resolved");
        var blocked   = issues.Count(i => i.Status is "Blocked" or "Impediment");

        return new IssuesSummaryWidgetData(open, inProgress, done, blocked, issues.Count);
    }

    private async Task<WidgetData> BuildTrendIndicatorDataAsync(
        string projectKey, int sprintCount, WidgetConfig widget, CancellationToken ct)
    {
        var metric = widget.Settings.GetValueOrDefault("metric", "velocity");
        var unit   = widget.Settings.GetValueOrDefault("unit",   "pts/day");

        var trend = await _analytics.AnalyzeVelocityTrend(projectKey, sprintCount).ConfigureAwait(false);
        var vels  = trend.Velocities.Select(v => v.Velocity).ToList();

        var current  = vels.LastOrDefault();
        var previous = vels.Count >= 2 ? vels[^2] : current;
        var direction = current > previous ? "Up" : current < previous ? "Down" : "Flat";

        return new TrendIndicatorWidgetData(
            Label:         metric,
            CurrentValue:  current,
            PreviousValue: previous,
            Unit:          unit,
            Direction:     direction);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<PopulatedWidget> ResolveWidgetSafeAsync(
        WidgetConfig widget, string projectKey, CancellationToken ct)
    {
        try
        {
            var data = await GetWidgetDataAsync(widget, projectKey, ct).ConfigureAwait(false);
            return new PopulatedWidget(widget, data, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to resolve data for widget '{Title}' ({Type}).", widget.Title, widget.Type);
            return new PopulatedWidget(widget, null, DateTime.UtcNow, ex.Message);
        }
    }

    // -----------------------------------------------------------------------
    // Private layout / widget copy helpers
    // (DashboardLayout and WidgetConfig are classes, not records, so `with`
    //  expressions are unavailable — these helpers centralise shallow copies.)
    // -----------------------------------------------------------------------

    private static DashboardLayout CloneLayout(DashboardLayout source, List<WidgetConfig> widgets) =>
        new()
        {
            Columns         = source.Columns,
            Rows            = source.Rows,
            BackgroundColor = source.BackgroundColor,
            AccentColor     = source.AccentColor,
            Widgets         = widgets
        };

    private static WidgetConfig CopyWidget(WidgetConfig source, WidgetPosition newPosition) =>
        new()
        {
            Id                     = source.Id,
            Type                   = source.Type,
            Title                  = source.Title,
            Position               = newPosition,
            Settings               = source.Settings,
            RefreshIntervalSeconds = source.RefreshIntervalSeconds,
            IsVisible              = source.IsVisible
        };
}
