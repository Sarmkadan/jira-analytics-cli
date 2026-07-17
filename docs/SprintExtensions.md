# SprintExtensions

Utility class providing analytical extensions for Jira sprints, enabling calculation of key metrics such as completion, health, cycle time, and risk indicators. Designed to support reporting and decision-making workflows within the `jira-analytics-cli` toolset.

## API

### `public static double GetCompletionPercentage(IEnumerable<JiraIssue> issues)`

Calculates the percentage of completed issues in a sprint based on issue status. Only issues with statuses categorized as "Done" are considered complete.

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - A `double` between 0.0 and 100.0 representing the completion percentage.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---

### `public static double GetHealthScore(IEnumerable<JiraIssue> issues)`

Computes a normalized health score for a sprint based on issue completion, priority distribution, and time-to-completion trends. Higher scores indicate better health.

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - A `double` between 0.0 and 100.0 representing the health score.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---

### `public static double GetAverageCycleTime(IEnumerable<JiraIssue> issues)`

Calculates the average time (in days) taken to complete issues in the sprint, based on the difference between creation and resolution dates.

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - A `double` representing the average cycle time in days. Returns `0.0` if no issues are completed or if all lack resolution dates.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---
### `public static double GetBurnRate(IEnumerable<JiraIssue> issues)`

Determines the rate at which story points are consumed relative to the sprint's planned capacity. A value greater than 1.0 indicates over-burning.

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - A `double` representing the burn rate. Returns `0.0` if no issues have story points assigned.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---
### `public static string GetStatusSummary(IEnumerable<JiraIssue> issues)`

Generates a human-readable summary of issue status distribution in the sprint, including counts for "To Do", "In Progress", and "Done".

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - A `string` in the format: `"To Do: {n}, In Progress: {m}, Done: {k}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---
### `public static IReadOnlyList<JiraIssue> GetHighPriorityIssues(IEnumerable<JiraIssue> issues)`

Filters and returns a read-only list of issues marked as high priority (e.g., "High" or "Critical").

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - An `IReadOnlyList<JiraIssue>` containing high-priority issues. Returns an empty list if none are found.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---
### `public static IReadOnlyList<JiraIssue> GetAtRiskIssues(IEnumerable<JiraIssue> issues)`

Identifies issues that are overdue or blocked and likely at risk of not being completed on time.

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - An `IReadOnlyList<JiraIssue>` of at-risk issues. Returns an empty list if none are identified.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---
### `public static string GetProgressTrend(IEnumerable<JiraIssue> issues)`

Analyzes issue status changes over time to determine whether the sprint is accelerating, decelerating, or stable in progress.

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - A `string` indicating the trend: `"Accelerating"`, `"Decelerating"`, `"Stable"`, or `"Unknown"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

---
### `public static string GetGoalStatus(IEnumerable<JiraIssue> issues)`

Evaluates whether the sprint is on track to meet its defined goals based on completion rate, burn rate, and issue health.

- **Parameters**
  - `issues`: Collection of `JiraIssue` objects representing sprint issues.
- **Return Value**
  - A `string` indicating goal status: `"On Track"`, `"At Risk"`, `"Behind"`, or `"Unknown"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.

## Usage
