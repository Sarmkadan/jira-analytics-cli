# JiraIssue

The `JiraIssue` class serves as the primary data transfer object representing a single issue entity retrieved from the Jira API within the `jira-analytics-cli` project. It encapsulates core metadata, status information, and calculated analytics properties to facilitate reporting, filtering, and trend analysis without requiring direct interaction with the raw API response structure.

## API

### Properties

#### `Key`
```csharp
public string Key { get; set; }
```
The unique alphanumeric identifier for the issue (e.g., "PROJ-123"). This property is mandatory and never null.

#### `Id`
```csharp
public string Id { get; set; }
```
The internal numeric ID of the issue as assigned by the Jira database, stored as a string. This property is mandatory.

#### `Summary`
```csharp
public string Summary { get; set; }
```
The short title or headline of the issue. This property is mandatory.

#### `Description`
```csharp
public string? Description { get; set; }
```
The detailed textual description of the issue. This property may be `null` if no description was provided.

#### `Status`
```csharp
public string Status { get; set; }
```
The current workflow status of the issue (e.g., "To Do", "In Progress", "Done"). This property is mandatory.

#### `IssueType`
```csharp
public string IssueType { get; set; }
```
The classification of the issue (e.g., "Bug", "Story", "Task"). This property is mandatory.

#### `Assignee`
```csharp
public string? Assignee { get; set; }
```
The display name or key of the user currently assigned to the issue. This property may be `null` if the issue is unassigned.

#### `Priority`
```csharp
public string Priority { get; set; }
```
The priority level of the issue (e.g., "High", "Medium", "Low"). This property is mandatory.

#### `StoryPoints`
```csharp
public int? StoryPoints { get; set; }
```
The estimated effort value associated with the issue. This property may be `null` if story points are not enabled or defined for this issue.

#### `DueDate`
```csharp
public DateTime? DueDate { get; set; }
```
The target completion date for the issue. This property may be `null` if no due date is set.

#### `CreatedDate`
```csharp
public DateTime CreatedDate { get; set; }
```
The timestamp indicating when the issue was created. This property is mandatory.

#### `UpdatedDate`
```csharp
public DateTime UpdatedDate { get; set; }
```
The timestamp indicating when the issue was last modified. This property is mandatory.

#### `ResolutionDate`
```csharp
public DateTime? ResolutionDate { get; set; }
```
The timestamp indicating when the issue was resolved or closed. This property may be `null` if the issue is still open.

#### `Labels`
```csharp
public List<string> Labels { get; set; }
```
A collection of text labels tagged to the issue. The list is never null but may be empty.

#### `Components`
```csharp
public List<string> Components { get; set; }
```
A collection of system components associated with the issue. The list is never null but may be empty.

#### `ProjectKey`
```csharp
public string ProjectKey { get; set; }
```
The key of the project to which this issue belongs. This property is mandatory.

#### `SprintId`
```csharp
public int? SprintId { get; set; }
```
The numeric identifier of the active or future sprint containing this issue. This property may be `null` if the issue is not part of a sprint.

#### `IsOverdue`
```csharp
public bool IsOverdue { get; set; }
```
A calculated boolean flag indicating whether the current date exceeds the `DueDate` while the issue remains unresolved.

#### `IsHighPriority`
```csharp
public bool IsHighPriority { get; set; }
```
A calculated boolean flag indicating whether the `Priority` property matches high-severity criteria defined by the analytics logic.

### Methods

#### `GetDaysOpenWithoutProgress`
```csharp
public int GetDaysOpenWithoutProgress()
```
Calculates the number of full days elapsed since the last meaningful update to the issue.

*   **Return Value**: An integer representing the day count. Returns 0 if the issue was updated today.
*   **Logic**: Typically calculates the difference between `DateTime.Now` and `UpdatedDate`. If the issue is resolved, it may calculate based on `ResolutionDate` depending on implementation specifics.
*   **Exceptions**: Does not throw exceptions under normal operation; relies on valid `DateTime` values present in `UpdatedDate`.

## Usage

### Example 1: Filtering High-Priority Overdue Issues
The following example demonstrates iterating through a collection of issues to identify critical items requiring immediate attention based on calculated properties.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public class IssueAnalyzer
{
    public void PrintCriticalIssues(List<JiraIssue> issues)
    {
        var criticalIssues = issues
            .Where(i => i.IsHighPriority && i.IsOverdue)
            .OrderByDescending(i => i.GetDaysOpenWithoutProgress());

        foreach (var issue in criticalIssues)
        {
            Console.WriteLine($"[{issue.Key}] {issue.Summary}");
            Console.WriteLine($"  Status: {issue.Status} | Days Stale: {issue.GetDaysOpenWithoutProgress()}");
            Console.WriteLine($"  Assignee: {issue.Assignee ?? "Unassigned"}");
            Console.WriteLine("---");
        }
    }
}
```

### Example 2: Generating Sprint Velocity Data
This example illustrates aggregating story points for completed issues within a specific sprint, handling nullable fields safely.

```csharp
using System;
using System.Collections.Generic;

public class SprintReporter
{
    public int CalculateCompletedPoints(List<JiraIssue> issues, int sprintId)
    {
        int totalPoints = 0;

        foreach (var issue in issues)
        {
            if (issue.SprintId == sprintId && issue.ResolutionDate.HasValue)
            {
                if (issue.StoryPoints.HasValue)
                {
                    totalPoints += issue.StoryPoints.Value;
                }
            }
        }

        return totalPoints;
    }
}
```

## Notes

*   **Nullability**: Consumers must explicitly check for `null` on `Description`, `Assignee`, `DueDate`, `ResolutionDate`, and `StoryPoints` before accessing their values. Collection properties (`Labels`, `Components`) are guaranteed to be instantiated but may be empty.
*   **Time Zone Sensitivity**: All `DateTime` properties (`CreatedDate`, `UpdatedDate`, etc.) should be assumed to be in UTC or the server's local time depending on the CLI configuration. Comparisons involving `GetDaysOpenWithoutProgress` may yield different results if executed across different time zones relative to the Jira server.
*   **Calculated Properties**: `IsOverdue` and `IsHighPriority` are stateful booleans likely computed at the time of object hydration. They do not automatically update if the system clock changes or if the object remains in memory across a day boundary; re-hydration or manual recalculation is required for fresh data.
*   **Thread Safety**: The `JiraIssue` class is not thread-safe. Mutable properties (all listed properties) can be modified concurrently. If an instance is shared across multiple threads for reading while one thread performs updates or hydration, external synchronization (e.g., `lock`) is required.
*   **Mutability**: This is a mutable data object. There are no read-only guarantees on properties after initialization, allowing for local state adjustments (e.g., marking an issue as processed) before persistence or reporting.
