# AnalyticsService

The `AnalyticsService` class provides comprehensive analytics capabilities for Jira project data, including sprint analysis, team performance evaluation, quality metrics, velocity trends, overdue issue tracking, and cycle time analysis. It implements the `IAnalyticsService` interface and depends on `IJiraApiService` for data retrieval, `IMetricsRepository` for persistence, and `ILogger<AnalyticsService>` for logging.

## Responsibilities

- Analyze sprint performance metrics (velocity, completion rates, defects, overdue issues)
- Evaluate team performance and productivity distribution
- Calculate quality metrics including defect rates and high-risk component identification
- Analyze velocity trends over time to identify improving/declining/stable patterns
- Track and analyze overdue issues with priority classification
- Compute cycle time statistics for resolved issues including averages, medians, and percentiles

## Public API

### Constructor

```csharp
public AnalyticsService(IJiraApiService jiraService, IMetricsRepository metricsRepository, ILogger<AnalyticsService> logger)
```

Initializes a new instance of the AnalyticsService with required dependencies.

**Parameters:**
- `jiraService`: Client used to fetch sprints, issues and team data from Jira
- `metricsRepository`: Repository used to persist computed metrics  
- `logger`: Logger used to record analysis progress and failures

**Exceptions:**
- Throws `ArgumentNullException` if any parameter is null

### AnalyzeSprints

```csharp
public async Task<SprintAnalysisResult> AnalyzeSprints(string projectKey, int sprintCount)
```

Analyzes the most recently closed sprints for a project, computing per-sprint metrics (velocity, completion, defects, overdue issues) and aggregate trend/health.

**Parameters:**
- `projectKey`: The project identifier (required)
- `sprintCount`: Number of most recent closed sprints to analyze (must be positive)

**Returns:**
- `SprintAnalysisResult` containing per-sprint metrics and aggregate figures
- Returns an empty result (not thrown) if the analysis fails

**Exceptions:**
- Throws `ArgumentException` if `projectKey` is null or empty
- Logs errors internally but does not throw exceptions for service failures

### AnalyzeTeam

```csharp
public async Task<TeamAnalysisResult> AnalyzeTeam(string projectKey)
```

Analyzes team performance for a project by assigning issues to team members and computing per-developer productivity and workload distribution.

**Parameters:**
- `projectKey`: The project identifier (required)

**Returns:**
- `TeamAnalysisResult` with top/low performers, average productivity and workload distribution
- Returns an empty result (not thrown) if the analysis fails

**Exceptions:**
- Throws `ArgumentException` if `projectKey` is null or empty
- Logs errors internally but does not throw exceptions for service failures

### AnalyzeQuality

```csharp
public async Task<QualityMetricsResult> AnalyzeQuality(string projectKey)
```

Analyzes quality metrics for a project across all closed sprints, computing defect count, defect rate and the components with the highest bug concentration.

**Parameters:**
- `projectKey`: The project identifier (required)

**Returns:**
- `QualityMetricsResult` with defect statistics and high-risk areas
- Returns an empty result (not thrown) if the analysis fails

**Exceptions:**
- Throws `ArgumentException` if `projectKey` is null or empty
- Logs errors internally but does not throw exceptions for service failures

### AnalyzeVelocityTrend

```csharp
public async Task<VelocityTrendResult> AnalyzeVelocityTrend(string projectKey, int sprintCount)
```

Analyzes velocity trends over the most recent closed sprints, comparing the average velocity of the earlier half against the later half of the range.

**Parameters:**
- `projectKey`: The project identifier (required)
- `sprintCount`: Number of most recent closed sprints to include (must be at least 2 for meaningful trend)

**Returns:**
- `VelocityTrendResult` with per-sprint velocities, trend slope and trend classification
- Returns an empty result (not thrown) if the analysis fails

**Exceptions:**
- Throws `ArgumentException` if `projectKey` is null or empty
- Logs errors internally but does not throw exceptions for service failures

### AnalyzeOverdueIssues

```csharp
public async Task<OverdueIssuesResult> AnalyzeOverdueIssues(string projectKey)
```

Analyzes overdue issues in a project, computing the total and critical overdue counts along with the average number of days past their due date.

**Parameters:**
- `projectKey`: The project identifier (required)

**Returns:**
- `OverdueIssuesResult` with overdue issues and derived statistics
- Returns an empty result (not thrown) if the analysis fails

**Exceptions:**
- Throws `ArgumentException` if `projectKey` is null or empty
- Logs errors internally but does not throw exceptions for service failures

### AnalyzeCycleTime

```csharp
public async Task<CycleTimeResult> AnalyzeCycleTime(string projectKey)
```

Analyzes cycle time metrics for a project's resolved issues, computing the average, median and percentile (P50/P75/P90) cycle times along with a per-issue breakdown.

**Parameters:**
- `projectKey`: The project identifier (required)

**Returns:**
- `CycleTimeResult` with aggregate and per-issue cycle time statistics
- Returns a partial result (not thrown) if the analysis fails

**Exceptions:**
- Throws `ArgumentException` if `projectKey` is null or empty
- Logs errors internally but does not throw exceptions for service failures

## Result Objects

### SprintAnalysisResult

Contains the results of sprint analysis:
- `Metrics`: List of `SprintMetric` objects for each analyzed sprint
- `AverageVelocity`: Average story points completed per sprint
- `TrendPercentage`: Percentage change in velocity from oldest to newest sprint
- `OverallHealth`: Health assessment ("Excellent", "Healthy", "At Risk", "Critical", or "Unknown")

### TeamAnalysisResult

Contains the results of team analysis:
- `TopPerformers`: Top 3 developers by productivity
- `LowPerformers`: Bottom 3 developers by productivity
- `AverageProductivity`: Average story points completed per developer
- `WorkloadDistribution`: Dictionary mapping developer names to their assigned issue counts

### QualityMetricsResult

Contains the results of quality analysis:
- `AverageQualityScore`: Average health score across sprints (1-4 scale)
- `TotalDefects`: Total number of bug-type issues found
- `DefectRate`: Percentage of issues that are bugs
- `HighRiskAreas`: Top 5 components with the highest bug concentration

### VelocityTrendResult

Contains the results of velocity trend analysis:
- `Velocities`: List of tuples containing sprint name and velocity
- `TrendSlope`: Percentage change in velocity between first and second half of analyzed sprints
- `Trend`: Classification ("Increasing", "Decreasing", or "Stable")

### OverdueIssuesResult

Contains the results of overdue issue analysis:
- `TotalOverdueCount`: Total number of overdue issues
- `CriticalCount`: Number of overdue issues with high priority
- `Issues`: List of overdue `JiraIssue` objects
- `AverageDaysOverdue`: Average number of days issues are overdue

### CycleTimeResult

Contains the results of cycle time analysis:
- `ProjectKey`: The analyzed project key
- `AverageCycleTime`: Average cycle time in days
- `MedianCycleTime`: Median cycle time in days
- `P50CycleTime`: 50th percentile cycle time in days
- `P75CycleTime`: 75th percentile cycle time in days
- `P90CycleTime`: 90th percentile cycle time in days
- `IssueCycleTimes`: List of per-issue cycle time details

## Usage Examples

### Basic Sprint Analysis

```csharp
var analytics = new AnalyticsService(jiraService, metricsRepository, logger);

// Analyze the last 5 sprints for project PROJ
SprintAnalysisResult result = await analytics.AnalyzeSprints("PROJ", 5);

Console.WriteLine($"Analyzed {result.Metrics.Count} sprints");
Console.WriteLine($"Average velocity: {result.AverageVelocity:F2} story points/sprint");
Console.WriteLine($"Velocity trend: {result.TrendPercentage:F2}%");
Console.WriteLine($"Overall health: {result.OverallHealth}");

foreach (var metric in result.Metrics)
{
    Console.WriteLine($"Sprint '{metric.SprintName}': {metric.GetVelocity():F2} velocity, {metric.GetHealthStatus()} health");
}
```

### Team Performance Analysis

```csharp
var analytics = new AnalyticsService(jiraService, metricsRepository, logger);

// Analyze team performance for project PROJ
TeamAnalysisResult teamResult = await analytics.AnalyzeTeam("PROJ");

Console.WriteLine($"Team size: {teamResult.TopPerformers.Count + teamResult.LowPerformers.Count + 
                   (teamResult.WorkloadDistribution.Count - teamResult.TopPerformers.Count - teamResult.LowPerformers.Count)} members");
Console.WriteLine($"Average productivity: {teamResult.AverageProductivity:F2} story points/developer");
Console.WriteLine($"Top performers: {string.Join(", ", teamResult.TopPerformers.Select(d => d.DisplayName))}");
Console.WriteLine($"Workload distribution:");
foreach (var kvp in teamResult.WorkloadDistribution.OrderByDescending(kvp => kvp.Value))
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value} issues");
}
```

### Quality Metrics Analysis

```csharp
var analytics = new AnalyticsService(jiraService, metricsRepository, logger);

// Analyze quality for project PROJ
QualityMetricsResult qualityResult = await analytics.AnalyzeQuality("PROJ");

Console.WriteLine($"Total defects found: {qualityResult.TotalDefects}");
Console.WriteLine($"Defect rate: {qualityResult.DefectRate:F2}%");
Console.WriteLine($"Average quality score: {qualityResult.AverageQualityScore:F2}/4.0");

if (qualityResult.HighRiskAreas.Any())
{
    Console.WriteLine("High-risk components (most bugs):");
    foreach (var area in qualityResult.HighRiskAreas)
    {
        Console.WriteLine($"  {area}");
    }
}
```

### Velocity Trend Analysis

```csharp
var analytics = new AnalyticsService(jiraService, metricsRepository, logger);

// Analyze velocity trend for the last 8 sprints
VelocityTrendResult trendResult = await analytics.AnalyzeVelocityTrend("PROJ", 8);

Console.WriteLine($"Velocity trend: {trendResult.Trend} ({trendResult.TrendSlope:F2}% slope)");
Console.WriteLine($"Analyzed {trendResult.Velocities.Count} sprints:");

foreach (var (sprintName, velocity) in trendResult.Velocities)
{
    Console.WriteLine($"  {sprintName}: {velocity:F2} velocity");
}
```

### Overdue Issues Analysis

```csharp
var analytics = new AnalyticsService(jiraService, metricsRepository, logger);

// Analyze overdue issues for project PROJ
OverdueIssuesResult overdueResult = await analytics.AnalyzeOverdueIssues("PROJ");

Console.WriteLine($"Total overdue issues: {overdueResult.TotalOverdueCount}");
Console.WriteLine($"Critical overdue issues: {overdueResult.CriticalCount}");
Console.WriteLine($"Average days overdue: {overdueResult.AverageDaysOverdue:F1} days");

if (overdueResult.Issues.Any())
{
    Console.WriteLine("Overdue issues:");
    foreach (var issue in overdueResult.Issues
        .Where(i => i.DueDate.HasValue)
        .OrderByDescending(i => (DateTime.UtcNow - i.DueDate!.Value).TotalDays))
    {
        var daysOverdue = (DateTime.UtcNow - issue.DueDate!.Value).TotalDays;
        Console.WriteLine($"  {issue.Key}: {issue.Summary} ({daysOverdue:F0} days overdue)");
    }
}
```

### Cycle Time Analysis

```csharp
var analytics = new AnalyticsService(jiraService, metricsRepository, logger);

// Analyze cycle time for project PROJ
CycleTimeResult cycleResult = await analytics.AnalyzeCycleTime("PROJ");

Console.WriteLine($"Cycle time analysis for {cycleResult.ProjectKey}:");
Console.WriteLine($"  Average: {cycleResult.AverageCycleTime:F2} days");
Console.WriteLine($"  Median: {cycleResult.MedianCycleTime:F2} days");
Console.WriteLine($"  P50: {cycleResult.P50CycleTime:F2} days");
Console.WriteLine($"  P75: {cycleResult.P75CycleTime:F2} days");
Console.WriteLine($"  P90: {cycleResult.P90CycleTime:F2} days");
Console.WriteLine($"  Analyzed {cycleResult.IssueCycleTimes.Count} resolved issues");

if (cycleResult.IssueCycleTimes.Any())
{
    Console.WriteLine("Longest cycle times:");
    foreach (var ict in cycleResult.IssueCycleTimes.Take(5))
    {
        Console.WriteLine($"  {ict.IssueKey}: {ict.Summary} ({ict.CycleTimeDays:F1} days)");
    }
}
```

## Thread Safety

Instance methods on `AnalyticsService` are not guaranteed to be thread-safe. Each method initiates independent HTTP calls and internal state may be mutated during execution. Consumers should avoid concurrent calls on the same instance without external synchronization, or create separate instances per parallel operation.

## Error Handling

All methods catch exceptions internally, log them using the injected logger, and return empty/partial result objects rather than propagating exceptions. This ensures that analytics failures don't break calling code, though callers should check result properties (like empty collections or zero values) to detect when analysis failed.

## Dependencies

The service depends on three injected interfaces:
- `IJiraApiService`: For retrieving sprints, issues, and team data from Jira
- `IMetricsRepository`: For persisting computed metrics (though current implementation doesn't appear to use this)
- `ILogger<AnalyticsService>`: For logging analysis progress and failures

These dependencies should be registered with the application's dependency injection container.