# JiraProject

Represents a Jira project within the analytics CLI, aggregating project metadata, associated sprints, team composition, and historical sprint metrics. It serves as the central entity for querying project status, sprint progress, team performance, and issue tracking across the project lifecycle.

## API

### Properties

#### `Key : string`
The unique project key assigned in Jira (e.g., `"PROJ"`). This identifier is used in issue keys and API calls.

#### `Id : string`
The internal Jira project identifier, typically a numeric string. Distinct from the human-readable `Key`.

#### `Name : string`
The display name of the project as configured in Jira.

#### `Description : string?`
The project description text, or `null` if no description has been set.

#### `ProjectType : string`
Indicates the project template type (e.g., `"software"`, `"business"`, `"service_desk"`). Determines available features and workflows.

#### `Lead : string?`
The username or display name of the project lead, or `null` if no lead is assigned.

#### `CreatedDate : DateTime`
The UTC timestamp when the project was created in Jira.

#### `Url : string?`
The full browser-accessible URL to the project in the Jira instance, or `null` if the URL is unavailable.

#### `Sprints : List<Sprint>`
All sprints associated with this project, regardless of state (active, future, closed). Order is not guaranteed; use dedicated methods for filtered retrieval.

#### `TeamMembers : List<Developer>`
The complete roster of developers assigned to this project. Membership is derived from issue assignments and sprint participation.

#### `MetricsHistory : List<SprintMetric>`
A chronological collection of sprint-level metrics recorded over time. Each entry corresponds to a completed sprint's quantitative data (velocity, scope change, completion rate, etc.).

### Methods

#### `GetTotalSprintCount() : int`
Returns the total number of sprints in the `Sprints` collection, including future, active, and closed sprints.

- **Returns:** Non-negative integer.
- **Throws:** Nothing.

#### `GetCompletedSprintCount() : int`
Returns the count of sprints whose state is closed/completed.

- **Returns:** Non-negative integer, always ≤ `GetTotalSprintCount()`.
- **Throws:** Nothing.

#### `GetCurrentActiveSprint() : Sprint?`
Returns the sprint that is currently in progress, or `null` if no sprint is active at the time of invocation.

- **Returns:** The active `Sprint` instance, or `null`.
- **Throws:** `InvalidOperationException` if multiple sprints are unexpectedly marked active simultaneously.

#### `GetRecentSprints(int count = 5) : List<Sprint>`
Retrieves the most recently completed sprints, ordered by end date descending.

- **Parameters:**
  - `count` (default `5`): Maximum number of sprints to return. Must be ≥ 0.
- **Returns:** A list of up to `count` completed sprints. May be empty if no sprints have been completed.
- **Throws:** `ArgumentOutOfRangeException` if `count` is negative.

#### `GetAverageVelocity(double? recentSprintCount = null) : double`
Calculates the average velocity (story points completed per sprint) over a specified number of recent completed sprints.

- **Parameters:**
  - `recentSprintCount` (optional): Number of recent sprints to consider. If `null`, all completed sprints are used.
- **Returns:** The mean velocity as a `double`. Returns `0.0` if no completed sprints exist or the specified count resolves to zero.
- **Throws:** `ArgumentOutOfRangeException` if `recentSprintCount` is provided and negative.

#### `GetTotalTeamSize() : int`
Returns the number of distinct developers in the `TeamMembers` list.

- **Returns:** Non-negative integer.
- **Throws:** Nothing.

#### `GetTopPerformers(int count = 3) : List<Developer>`
Returns developers ranked by total story points completed across all closed sprints, in descending order.

- **Parameters:**
  - `count` (default `3`): Maximum number of developers to return. Must be ≥ 0.
- **Returns:** Ordered list of up to `count` developers. May be empty if no completed work exists.
- **Throws:** `ArgumentOutOfRangeException` if `count` is negative.

#### `GetAllOverdueIssues() : List<JiraIssue>`
Collects all issues within the project whose due date has passed and whose resolution is unset (i.e., not done/cancelled).

- **Returns:** List of overdue `JiraIssue` instances. May be empty.
- **Throws:** Nothing. Issues with a `null` due date are excluded silently.

#### `GetAllBlockedIssues() : List<JiraIssue>`
Collects all unresolved issues that are flagged as blocked or have an active impediment link/flag.

- **Returns:** List of blocked `JiraIssue` instances. May be empty.
- **Throws:** Nothing.

## Usage

### Example 1: Sprint Health Dashboard

```csharp
var project = analyticsService.GetProject("PROJ");

// Determine current sprint status
var activeSprint = project.GetCurrentActiveSprint();
if (activeSprint != null)
{
    Console.WriteLine($"Active sprint: {activeSprint.Name} " +
                      $"({activeSprint.CompletedIssues}/{activeSprint.TotalIssues} issues done)");
}
else
{
    Console.WriteLine("No active sprint.");
}

// Velocity trend over last 5 sprints
double velocity = project.GetAverageVelocity(recentSprintCount: 5);
Console.WriteLine($"Average velocity (last 5 sprints): {velocity:F1} points");

// Overdue and blocked items requiring attention
var overdue = project.GetAllOverdueIssues();
var blocked = project.GetAllBlockedIssues();
Console.WriteLine($"Attention needed: {overdue.Count} overdue, {blocked.Count} blocked");
```

### Example 2: Team Performance Report

```csharp
var project = analyticsService.GetProject("PROJ");

Console.WriteLine($"Project: {project.Name} ({project.Key})");
Console.WriteLine($"Lead: {project.Lead ?? "Unassigned"}");
Console.WriteLine($"Team size: {project.GetTotalTeamSize()}");
Console.WriteLine($"Total sprints: {project.GetTotalSprintCount()} " +
                  $"({project.GetCompletedSprintCount()} completed)");

// Top contributors
var topPerformers = project.GetTopPerformers(count: 5);
Console.WriteLine("\nTop performers:");
foreach (var dev in topPerformers)
{
    Console.WriteLine($"  {dev.DisplayName}: {dev.TotalStoryPoints} points " +
                      $"across {dev.CompletedSprints} sprints");
}

// Recent sprint outcomes
var recentSprints = project.GetRecentSprints(count: 3);
Console.WriteLine("\nRecent sprint outcomes:");
foreach (var sprint in recentSprints)
{
    Console.WriteLine($"  {sprint.Name}: " +
                      $"Committed {sprint.CommittedPoints} → " +
                      $"Completed {sprint.CompletedPoints} " +
                      $"({sprint.CompletionRate:P0})");
}
```

## Notes

- **Null handling:** Properties such as `Description`, `Lead`, and `Url` may be `null` when the corresponding Jira fields are not populated. Callers must guard against `null` before dereferencing.
- **Empty collections:** `Sprints`, `TeamMembers`, and `MetricsHistory` may be empty for newly created projects or those with no activity. Methods returning lists (`GetRecentSprints`, `GetTopPerformers`, `GetAllOverdueIssues`, `GetAllBlockedIssues`) return empty collections rather than `null`.
- **Active sprint ambiguity:** `GetCurrentActiveSprint` throws `InvalidOperationException` if the internal state contains more than one sprint marked active. This guards against data integrity issues in the underlying Jira data.
- **Velocity edge cases:** `GetAverageVelocity` returns `0.0` when no completed sprints are available or when the specified window contains zero sprints. It does not distinguish between "no data" and "zero velocity"; callers should check `GetCompletedSprintCount()` first if this distinction matters.
- **Thread safety:** This type is not designed for concurrent mutation. Properties and methods that enumerate collections (`Sprints`, `TeamMembers`, `MetricsHistory`) assume stable state during enumeration. External modification of the underlying collections while a method is executing may result in `InvalidOperationException` or inconsistent results. Synchronization is the caller's responsibility if instances are shared across threads.
- **Performance:** Methods that scan all issues (`GetAllOverdueIssues`, `GetAllBlockedIssues`) operate over the full project issue set and may be expensive for large projects. Consider caching results if called repeatedly within a short time window.
