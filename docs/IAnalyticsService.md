# IAnalyticsService

The `IAnalyticsService` interface defines the contract for retrieving aggregated performance metrics, trend analysis, and risk assessments derived from Jira data within the `jira-analytics-cli` project. It serves as the primary data access point for generating reports on sprint velocity, developer productivity, code quality, and project health, exposing a comprehensive set of read-only properties that encapsulate the results of underlying analytical computations.

## API

The following members are exposed by the `IAnalyticsService` interface:

### `Metrics`
*   **Type**: `List<SprintMetric>`
*   **Purpose**: Provides a collection of detailed metrics calculated for individual sprints.
*   **Returns**: A list of `SprintMetric` objects containing granular data per sprint.
*   **Throws**: Does not throw on access; the list may be empty if no sprint data is available.

### `AverageVelocity`
*   **Type**: `double`
*   **Purpose**: Represents the mean velocity across all analyzed sprints.
*   **Returns**: A double-precision floating-point number indicating the average story points or work units completed per sprint.
*   **Throws**: Does not throw.

### `TrendPercentage`
*   **Type**: `double`
*   **Purpose**: Indicates the percentage change in velocity compared to a previous baseline period.
*   **Returns**: A double value where positive numbers indicate improvement and negative numbers indicate decline.
*   **Throws**: Does not throw.

### `OverallHealth`
*   **Type**: `string`
*   **Purpose**: Provides a qualitative assessment of the project's current state based on aggregated metrics.
*   **Returns**: A string descriptor (e.g., "Healthy", "At Risk", "Critical").
*   **Throws**: Does not throw.

### `TopPerformers`
*   **Type**: `List<Developer>`
*   **Purpose**: Lists developers who have exceeded performance thresholds based on productivity and quality scores.
*   **Returns**: A list of `Developer` objects ranked by performance.
*   **Throws**: Does not throw; returns an empty list if no developers meet the criteria.

### `LowPerformers`
*   **Type**: `List<Developer>`
*   **Purpose**: Lists developers falling below defined performance thresholds requiring attention or support.
*   **Returns**: A list of `Developer` objects.
*   **Throws**: Does not throw; returns an empty list if no developers meet the criteria.

### `AverageProductivity`
*   **Type**: `double`
*   **Purpose**: Calculates the mean productivity score across the entire team.
*   **Returns**: A double value representing the average productivity index.
*   **Throws**: Does not throw.

### `WorkloadDistribution`
*   **Type**: `Dictionary<string, int>`
*   **Purpose**: Maps developer names or IDs to their assigned work item counts to visualize load balancing.
*   **Returns**: A dictionary where keys are identifiers and values are the count of assigned items.
*   **Throws**: Does not throw.

### `AverageQualityScore`
*   **Type**: `double`
*   **Purpose**: Represents the mean quality score derived from code reviews, test coverage, or defect density.
*   **Returns**: A double value indicating the average quality metric.
*   **Throws**: Does not throw.

### `TotalDefects`
*   **Type**: `int`
*   **Purpose**: Counts the total number of defects identified within the analysis period.
*   **Returns**: An integer representing the aggregate defect count.
*   **Throws**: Does not throw.

### `DefectRate`
*   **Type**: `double`
*   **Purpose**: Calculates the ratio of defects to total work items or story points.
*   **Returns**: A double value representing the defect density.
*   **Throws**: Does not throw.

### `HighRiskAreas`
*   **Type**: `List<string>`
*   **Purpose**: Identifies specific modules, components, or processes flagged as high-risk based on historical data.
*   **Returns**: A list of strings describing the risk areas.
*   **Throws**: Does not throw.

### `Velocities`
*   **Type**: `List<(string SprintName, double Velocity)>`
*   **Purpose**: Provides a historical record of velocity paired with specific sprint names.
*   **Returns**: A list of tuples containing the sprint name and its corresponding velocity.
*   **Throws**: Does not throw.

### `TrendSlope`
*   **Type**: `double`
*   **Purpose**: Represents the mathematical slope of the velocity trend line over time.
*   **Returns**: A double value indicating the rate of change per sprint.
*   **Throws**: Does not throw.

### `Trend`
*   **Type**: `string`
*   **Purpose**: Provides a textual description of the current velocity trend direction.
*   **Returns**: A string such as "Increasing", "Decreasing", or "Stable".
*   **Throws**: Does not throw.

### `TotalOverdueCount`
*   **Type**: `int`
*   **Purpose**: Counts the total number of issues currently past their due date.
*   **Returns**: An integer representing the count of overdue items.
*   **Throws**: Does not throw.

### `CriticalCount`
*   **Type**: `int`
*   **Purpose**: Counts the number of issues marked with critical priority.
*   **Returns**: An integer representing the count of critical issues.
*   **Throws**: Does not throw.

### `Issues`
*   **Type**: `List<JiraIssue>`
*   **Purpose**: Returns the raw list of Jira issues included in the current analysis scope.
*   **Returns**: A list of `JiraIssue` objects.
*   **Throws**: Does not throw.

### `AverageDaysOverdue`
*   **Type**: `double`
*   **Purpose**: Calculates the average number of days that overdue issues have been past their deadline.
*   **Returns**: A double value representing the mean delay duration.
*   **Throws**: Does not throw.

## Usage

### Example 1: Generating a Project Health Summary
This example demonstrates how to access high-level health indicators and trend data to generate a console summary.

```csharp
public void PrintHealthSummary(IAnalyticsService analytics)
{
    Console.WriteLine($"Project Health Status: {analytics.OverallHealth}");
    Console.WriteLine($"Current Trend: {analytics.Trend} ({analytics.TrendPercentage:F2}%)");
    Console.WriteLine($"Average Velocity: {analytics.AverageVelocity:F1} points/sprint");
    
    if (analytics.CriticalCount > 0)
    {
        Console.WriteLine($"WARNING: {analytics.CriticalCount} critical issues detected.");
    }

    if (analytics.HighRiskAreas.Any())
    {
        Console.WriteLine("High Risk Areas Identified:");
        foreach (var area in analytics.HighRiskAreas)
        {
            Console.WriteLine($" - {area}");
        }
    }
}
```

### Example 2: Analyzing Developer Workload and Performance
This example illustrates retrieving developer-specific metrics to assess workload distribution and identify performance outliers.

```csharp
public void AssessTeamPerformance(IAnalyticsService analytics)
{
    Console.WriteLine($"Team Average Productivity: {analytics.AverageProductivity:F2}");
    Console.WriteLine($"Average Quality Score: {analytics.AverageQualityScore:F2}");
    
    Console.WriteLine("\nTop Performers:");
    foreach (var dev in analytics.TopPerformers)
    {
        Console.WriteLine($" - {dev.Name}");
    }

    Console.WriteLine("\nWorkload Distribution:");
    foreach (var entry in analytics.WorkloadDistribution)
    {
        Console.WriteLine($" {entry.Key}: {entry.Value} items");
    }

    if (analytics.LowPerformers.Count > 0)
    {
        Console.WriteLine("\nAttention Required:");
        foreach (var dev in analytics.LowPerformers)
        {
            Console.WriteLine($" - {dev.Name} (Below threshold)");
        }
    }
}
```

## Notes

*   **Read-Only Nature**: All exposed members are properties returning data structures or primitives. The interface implies a read-only snapshot of data calculated at a specific point in time; modifying the returned collections (e.g., adding to `Metrics` or `TopPerformers`) will not update the underlying service state and may only affect the local reference depending on the concrete implementation's list instantiation strategy.
*   **Empty Collections**: Consumers should expect empty lists (`Metrics`, `TopPerformers`, `HighRiskAreas`, etc.) rather than `null` when no data matches the criteria, though defensive null-checking is recommended if the concrete implementation varies.
*   **Division by Zero**: Properties such as `DefectRate`, `AverageDaysOverdue`, and `TrendPercentage` involve division. Implementations should handle scenarios where the denominator is zero (e.g., no issues, no overdue items) by returning `0.0` or `double.NaN` rather than throwing exceptions, ensuring the property getters remain safe to call.
*   **Thread Safety**: As this interface exposes mutable collection types (`List<T>`, `Dictionary<TKey, TValue>`) directly, it is not inherently thread-safe for write operations if the underlying implementation allows modification of the internal state. However, given the analytical nature of the service, these collections are typically populated once during a refresh cycle and treated as immutable thereafter. If multiple threads access these properties concurrently while a background refresh is occurring, external synchronization or defensive copying of the returned collections is required.
*   **Data Consistency**: The values across properties (e.g., `TotalDefects` vs `DefectRate`) are consistent only within the scope of the last calculation trigger. There is no guarantee of atomicity between property accesses if the service is actively recalculating metrics in the background.
