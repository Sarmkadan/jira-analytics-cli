# CycleTimeResult
The `CycleTimeResult` type represents the outcome of calculating cycle time metrics for a specific issue or project in Jira. It encapsulates various statistics, such as average, median, and percentile cycle times, along with detailed information about the issue and its lifecycle.

## API
The `CycleTimeResult` type exposes the following public members:
* `ProjectKey`: A string representing the key of the project to which the issue belongs.
* `AverageCycleTime`: A double value indicating the average cycle time for the issue or project.
* `MedianCycleTime`: A double value representing the median cycle time.
* `P50CycleTime`, `P75CycleTime`, `P90CycleTime`: Double values representing the 50th, 75th, and 90th percentile cycle times, respectively.
* `IssueCycleTimes`: A list of `IssueCycleTime` objects providing detailed cycle time information for individual issues.
* `IssueKey`: A string representing the key of the issue.
* `Summary`: A string summarizing the issue.
* `CycleTimeDays`: A double value indicating the cycle time in days.
* `CreatedDate`: A `DateTime` object representing the date the issue was created.
* `ResolutionDate`: A nullable `DateTime` object representing the date the issue was resolved, if applicable.

## Usage
Here are two examples of using the `CycleTimeResult` type in C#:
```csharp
// Example 1: Accessing cycle time metrics
CycleTimeResult result = CalculateCycleTime("PROJECT-123", "ISSUE-456");
Console.WriteLine($"Average cycle time: {result.AverageCycleTime} days");
Console.WriteLine($"Median cycle time: {result.MedianCycleTime} days");

// Example 2: Iterating over issue cycle times
CycleTimeResult result2 = CalculateCycleTime("PROJECT-789", "ISSUE-012");
foreach (IssueCycleTime issueCycleTime in result2.IssueCycleTimes)
{
    Console.WriteLine($"Issue {issueCycleTime.IssueKey}: {issueCycleTime.CycleTimeDays} days");
}
```

## Notes
When working with `CycleTimeResult`, consider the following edge cases:
* If an issue has not been resolved, `ResolutionDate` will be null.
* If the issue or project has no cycle time data, the corresponding metrics (e.g., `AverageCycleTime`) may be zero or null.
* The `IssueCycleTimes` list may be empty if no cycle time data is available for the issue or project.
Regarding thread safety, the `CycleTimeResult` type is designed to be immutable, making it safe to access and manipulate its members from multiple threads without fear of data corruption. However, the `IssueCycleTimes` list is a reference type, so modifications to the list itself (e.g., adding or removing elements) are not thread-safe.
