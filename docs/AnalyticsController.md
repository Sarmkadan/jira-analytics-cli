# AnalyticsController

`AnalyticsController` serves as the primary entry point for all analytics-related HTTP endpoints in the jira-analytics-cli project. It orchestrates project-level analysis, sprint evaluations, team dashboard generation, and system health checks by delegating to dedicated service classes. The controller exposes asynchronous actions that return `IActionResult` or typed domain objects, enabling both RESTful API consumption and internal service composition.

## API

### public AnalyticsController

Constructor. Initializes a new instance of the controller with the required service dependencies. The exact parameter list is determined by the dependency injection container, but the controller expects at minimum the services exposed as public properties (see below). No default constructor is available; instantiation without proper service injection will fail at runtime.

### public async Task<IActionResult> GetProjectAnalytics

Retrieves analytics for a specified project. Accepts project identifier information via route or query parameters (the exact binding depends on the configured ASP.NET Core model binding). Returns an `IActionResult` that wraps the project analytics data, typically as a JSON payload with an HTTP 200 status on success. Throws `ArgumentException` when the project identifier is missing or invalid. May propagate exceptions from underlying data sources if the project cannot be found or the analytics computation fails.

### public ScheduledReportService

Publicly exposed service instance for scheduled report generation. This property provides direct access to the `ScheduledReportService` used internally, allowing middleware, filters, or derived classes to interact with scheduled reporting capabilities without re-injection. The service is initialized during controller construction and remains immutable for the lifetime of the controller instance.

### public TeamDashboardService

Publicly exposed service instance for team dashboard construction. Provides access to the `TeamDashboardService` that aggregates team-level metrics, velocity data, and performance indicators. Available for consumption by action filters, view components, or testing infrastructure that require direct service access.

### public async Task<TeamDashboard> GenerateTeamDashboard

Generates a comprehensive team dashboard for a given team and optional time range. Accepts parameters identifying the team (such as team ID or name) and an optional date range filter. Returns a fully populated `TeamDashboard` object containing metrics, charts, and summary statistics. Throws `ArgumentNullException` when the team identifier is null or empty. Throws `InvalidOperationException` when the specified team cannot be found in Jira or when the underlying data retrieval fails. The returned `TeamDashboard` is a concrete object, not wrapped in `IActionResult`, making this method suitable for internal composition as well as direct API responses.

### public JiraHealthCheck

Publicly exposed `JiraHealthCheck` instance. This property holds the health check service that validates connectivity to the Jira instance, credential validity, and essential service availability. Exposed for use by the ASP.NET Core health checks middleware or for manual invocation in diagnostic scenarios.

### public async Task<HealthCheckResult> CheckHealthAsync

Performs a comprehensive health check against the Jira instance and all dependent services. Accepts an optional `CancellationToken` to support timeout scenarios. Returns a `HealthCheckResult` indicating overall health status (`Healthy`, `Degraded`, or `Unhealthy`) along with per-service diagnostic descriptions. Throws `OperationCanceledException` when the cancellation token is signaled. Does not throw on service failures; instead, degraded or unhealthy statuses are captured in the returned result. Designed to be called both by the ASP.NET Core health check endpoint infrastructure and programmatically.

### public CachedAnalyticsService

Publicly exposed `CachedAnalyticsService` instance. Provides access to the caching layer that wraps raw analytics computations. This service manages cache invalidation, time-to-live policies, and fallback to direct computation on cache misses. Exposed for scenarios where callers need to bypass the controller’s standard caching behavior or inspect cache state.

### public async Task<ProjectAnalysis> AnalyzeSprints

Analyzes all sprints within a specified project, computing velocity trends, completion rates, scope change metrics, and bottleneck indicators. Accepts a project identifier and an optional sprint range filter. Returns a `ProjectAnalysis` object containing per-sprint breakdowns and aggregate project-level insights. Throws `ArgumentException` when the project identifier is invalid. Throws `InvalidOperationException` when the project contains no sprints or when the Jira API returns malformed sprint data. The returned `ProjectAnalysis` is a domain object suitable for serialization or further processing.

## Usage

### Example 1: Retrieving Project Analytics via an API Endpoint

```csharp
// In an ASP.NET Core controller action that delegates to AnalyticsController
[HttpGet("projects/{projectKey}/analytics")]
public async Task<IActionResult> GetProjectAnalyticsEndpoint(
    [FromServices] AnalyticsController analyticsController,
    string projectKey)
{
    // The controller's GetProjectAnalytics method is invoked directly.
    // Model binding for the project key is handled by the framework.
    IActionResult result = await analyticsController.GetProjectAnalytics(projectKey);
    return result; // Typically returns JSON with HTTP 200
}
```

### Example 2: Generating a Team Dashboard for Internal Composition

```csharp
// In a reporting service that composes multiple analytics outputs
public async Task<ExecutiveReport> BuildExecutiveReport(
    AnalyticsController analyticsController,
    string teamId,
    DateTime startDate,
    DateTime endDate)
{
    // Generate the team dashboard using the dedicated method
    TeamDashboard dashboard = await analyticsController.GenerateTeamDashboard(
        teamId, startDate, endDate);

    // Access the cached analytics service for supplementary data
    CachedAnalyticsService cache = analyticsController.CachedAnalyticsService;

    // Compose the executive report from dashboard and cached data
    return new ExecutiveReport
    {
        TeamMetrics = dashboard.Metrics,
        GeneratedAt = DateTime.UtcNow,
        CacheStatus = cache.GetCacheState()
    };
}
```

## Notes

- **Thread Safety**: The controller follows the ASP.NET Core per-request instantiation model. Each HTTP request receives a new controller instance, so instance methods and property accesses are safe without additional synchronization. However, the exposed service properties (`ScheduledReportService`, `TeamDashboardService`, `JiraHealthCheck`, `CachedAnalyticsService`) may internally share state across requests (e.g., cached data, HTTP client pools). Consult each service’s documentation for its specific thread-safety guarantees.
- **Edge Cases**: `AnalyzeSprints` returns an empty sprint collection within the `ProjectAnalysis` object when the project exists but has no sprints, rather than throwing. `CheckHealthAsync` may return `Degraded` when the Jira instance is reachable but exhibits high latency or partial API failures; it returns `Unhealthy` only when connectivity is entirely lost or authentication fails. `GenerateTeamDashboard` treats a null end date as “up to the current date” and a null start date as “from the team’s first recorded activity.”
- **Exception Propagation**: Methods that accept project or team identifiers validate them synchronously and throw `ArgumentException` variants before any asynchronous work begins. Exceptions from the Jira API (network failures, rate limiting, authentication expiry) are surfaced as `InvalidOperationException` or `HttpRequestException` wrapped in `AggregateException` when multiple parallel calls fail.
- **Caching Behavior**: The `CachedAnalyticsService` exposed by the controller operates with a default sliding expiration. Repeated calls to `GetProjectAnalytics` or `AnalyzeSprints` with identical parameters within the cache window return cached results. The cache is invalidated when the underlying Jira data is known to have changed (e.g., sprint completion events), though callers should not assume immediate consistency.
