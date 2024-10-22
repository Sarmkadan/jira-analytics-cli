# AnalyticsService

`AnalyticsService` provides a centralized entry point for performing various analytical computations against Jira project data. It exposes asynchronous methods that each return a specialized result object covering sprint performance, team throughput, quality metrics, velocity trends, and overdue issue tracking.

## API

### `public AnalyticsService()`

Initializes a new instance of the `AnalyticsService` class. The default constructor requires no arguments; dependencies such as Jira client adapters or configuration are resolved internally at construction time.

### `public async Task<SprintAnalysisResult> AnalyzeSprints(…)`

Computes sprint-level analytics for a specified set of sprints. The method gathers scope changes, completion percentages, and cycle-time distributions, returning them as a `SprintAnalysisResult`.

- **Parameters:** Accepts a sprint identifier or collection of sprint identifiers, along with optional filters for date ranges and board location.
- **Returns:** `SprintAnalysisResult` containing per-sprint metrics, scope creep indicators, and completion summaries.
- **Exceptions:** Throws `ArgumentException` when the sprint list is empty or identifiers are malformed. Throws `JiraDataException` (or a derived connectivity exception) when the underlying Jira API is unreachable or returns an error response.

### `public async Task<TeamAnalysisResult> AnalyzeTeam(…)`

Evaluates team-level performance over a given time window. The analysis includes throughput, work-in-progress averages, and assignment distribution across team members.

- **Parameters:** Accepts a team identifier (name or ID) and a date range. Optional parameters allow filtering by issue type or project.
- **Returns:** `TeamAnalysisResult` with throughput counts, average cycle time per assignee, and WIP statistics.
- **Exceptions:** Throws `ArgumentException` when the team identifier is null or empty. Throws `InvalidOperationException` when the date range is inverted (end before start). Connectivity failures surface as `JiraDataException`.

### `public async Task<QualityMetricsResult> AnalyzeQuality(…)`

Calculates quality-related metrics such as defect density, re-open rates, and escaped-defect counts for a given project or board.

- **Parameters:** Accepts a project or board identifier and an optional release-version filter. A lookback period can be specified to bound the data set.
- **Returns:** `QualityMetricsResult` containing defect counts, re-open ratios, and trend indicators compared to previous periods.
- **Exceptions:** Throws `ArgumentException` when the project or board identifier is missing. Throws `JiraDataException` when the remote query fails.

### `public async Task<VelocityTrendResult> AnalyzeVelocityTrend(…)`

Produces a velocity trend over multiple sprints or iterations, including rolling averages and forecast projections based on historical completion data.

- **Parameters:** Accepts a board identifier and a sprint count or explicit sprint list. Optional parameters control whether partial-credit story points are included.
- **Returns:** `VelocityTrendResult` with per-sprint velocity values, a moving average series, and a simple linear projection for the next one or two sprints.
- **Exceptions:** Throws `ArgumentException` when fewer than two sprints are provided. Throws `JiraDataException` on data retrieval errors.

### `public async Task<OverdueIssuesResult> AnalyzeOverdueIssues(…)`

Identifies and categorizes issues that have exceeded their due dates or SLA targets. The result groups overdue items by severity, assignee, and age.

- **Parameters:** Accepts a project or filter identifier and an optional threshold that defines the minimum number of business days past due for inclusion.
- **Returns:** `OverdueIssuesResult` with a list of overdue issues, aging buckets, and summary counts by priority.
- **Exceptions:** Throws `ArgumentException` when the project or filter identifier is invalid. Throws `JiraDataException` when the underlying data source cannot be queried.

## Usage

### Example 1: Sprint Analysis for a Single Board

```csharp
var analytics = new AnalyticsService();

// Analyze the last three sprints on board "DEV-BOARD"
SprintAnalysisResult sprintResult = await analytics.AnalyzeSprints(
    boardId: "DEV-BOARD",
    sprintCount: 3,
    includeScopeChangeDetails: true
);

Console.WriteLine($"Average completion: {sprintResult.AverageCompletionPercent:P}");
foreach (var sprint in sprintResult.Sprints)
{
    Console.WriteLine($"{sprint.Name}: {sprint.CompletedIssues} completed, " +
                      $"scope change {sprint.ScopeChangePercent:P}");
}
```

### Example 2: Velocity Trend and Overdue Issues Combined

```csharp
var analytics = new AnalyticsService();

// Retrieve velocity trend for forecasting
VelocityTrendResult velocity = await analytics.AnalyzeVelocityTrend(
    boardId: "TEAM-A-BOARD",
    sprintCount: 5
);

Console.WriteLine($"Next sprint projection: {velocity.ProjectedNextSprintPoints} points");

// Identify overdue issues that are at least 3 business days late
OverdueIssuesResult overdue = await analytics.AnalyzeOverdueIssues(
    projectKey: "PROJ",
    minimumBusinessDaysPastDue: 3
);

Console.WriteLine($"Critical overdue items: {overdue.CriticalCount}");
foreach (var bucket in overdue.AgingBuckets)
{
    Console.WriteLine($"{bucket.AgeRange}: {bucket.Count} issues");
}
```

## Notes

- **Thread safety:** Instance methods on `AnalyticsService` are not guaranteed to be thread-safe. Each method initiates independent HTTP calls and internal state may be mutated during execution. Consumers should avoid concurrent calls on the same instance without external synchronization, or create separate instances per parallel operation.
- **Empty result sets:** When the requested date range or sprint list yields no matching issues, the returned result objects contain empty collections and zero-valued metrics rather than null. Callers should check collection counts before iterating.
- **Partial data:** If a board or project contains archived or deleted sprints that are referenced but not fully retrievable, methods such as `AnalyzeSprints` and `AnalyzeVelocityTrend` may return results with missing data points and a populated `Warnings` collection on the result object.
- **Date-range edge cases:** Methods accepting date ranges treat a null end date as “up to the current moment.” An end date exactly equal to the start date includes only data timestamped within that single calendar day, which may produce zero results if timestamps include time components.
- **Connectivity resilience:** All methods throw `JiraDataException` (or a subclass) when the remote Jira instance is unreachable, returns HTTP errors, or times out. No automatic retry logic is built into the service; callers must implement their own retry policies if needed.
