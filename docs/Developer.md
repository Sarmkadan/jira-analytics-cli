# Developer

Represents a developer working on Jira issues, tracking their assignments, sprint metrics, and productivity indicators.

## API

### `public string Key`
Identifier for the developer within Jira (e.g., username or employee ID). Immutable after initialization.

### `public string Name`
Full name of the developer. Immutable after initialization.

### `public string? Email`
Email address of the developer. Optional; may be null if not provided.

### `public string DisplayName`
Human-readable name for display purposes, derived from `Name` or system defaults.

### `public bool Active`
Indicates whether the developer is currently active. Defaults to `true`.

### `public DateTime? JoinDate`
Date when the developer joined the team. Optional; may be null for contractors or historical data.

### `public List<JiraIssue> AssignedIssues`
List of Jira issues currently assigned to the developer. Mutable; may be modified to reflect changes in assignments.

### `public List<SprintMetric> SprintMetrics`
Historical metrics for sprints the developer participated in. Mutable; updated as new sprint data is processed.

### `public int GetTotalAssignedIssues()`
Returns the total number of issues assigned to the developer.

**Returns:** `int` – Count of issues in `AssignedIssues`.

### `public int GetCompletedIssues()`
Returns the number of issues marked as completed.

**Returns:** `int` – Count of issues in `AssignedIssues` with status indicating completion.

### `public int GetInProgressIssues()`
Returns the number of issues currently in progress.

**Returns:** `int` – Count of issues in `AssignedIssues` with status indicating active work.

### `public int GetTotalStoryPoints()`
Returns the sum of story points for all assigned issues.

**Returns:** `int` – Total story points across `AssignedIssues`.

### `public int GetCompletedStoryPoints()`
Returns the sum of story points for completed issues.

**Returns:** `int` – Total story points for completed issues in `AssignedIssues`.

### `public int GetOverdueIssueCount()`
Returns the number of assigned issues past their due date.

**Returns:** `int` – Count of overdue issues in `AssignedIssues`.

### `public double GetCompletionRate()`
Calculates the ratio of completed issues to total assigned issues.

**Returns:** `double` – Completion rate as a value between 0.0 and 1.0. Returns `0.0` if no issues are assigned.

### `public double GetAverageIssuesPerDay()`
Calculates the average number of issues completed per day over the developer's active period.

**Returns:** `double` – Average issues per day. Returns `0.0` if `JoinDate` is null or no issues are completed.

### `public double GetAvailableHours()`
Estimates the developer's available working hours based on sprint metrics and historical data.

**Returns:** `double` – Available hours for the current period. Returns `0.0` if no sprint data is available.

### `public double GetLoadFactor()`
Calculates the ratio of assigned work (story points) to available capacity.

**Returns:** `double` – Load factor; values > 1.0 indicate over-allocation. Returns `0.0` if no data is available.

### `public double GetAverageStoryPointsPerIssue()`
Calculates the average story points per issue for completed work.

**Returns:** `double` – Average story points. Returns `0.0` if no issues are completed.

### `public double GetProductivity()`
Calculates a productivity score based on completed story points and time active.

**Returns:** `double` – Productivity score; higher values indicate higher output. Returns `0.0` if no data is available.

## Usage
