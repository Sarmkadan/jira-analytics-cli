# JiraIssueExtensions

Static utility class providing extension-like helper methods for analyzing and evaluating Jira issue data. These methods operate on issue metadata such as creation dates, due dates, priority levels, and component assignments to compute derived metrics useful for workflow analytics and reporting.

## API

### `GetAgeInDays(JiraIssue issue)`
Calculates the number of days elapsed since the issue was created.

- **Parameters**
  - `issue`: The Jira issue containing the `CreatedDate` field.
- **Return value**
  - Returns the integer number of days between the issue creation date and the current UTC date. Returns `0` if the creation date is in the future.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null`.
  - Throws `InvalidOperationException` if the `CreatedDate` field is missing or unparseable.

---

### `IsBlocked(JiraIssue issue)`
Determines whether the issue is currently blocked based on its status and resolution.

- **Parameters**
  - `issue`: The Jira issue containing status and resolution fields.
- **Return value**
  - Returns `true` if the issue status indicates a blocked state (e.g., "Blocked", "Waiting for Support") or if a resolution such as "Cannot Reproduce" or "Won't Fix" is set; otherwise returns `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null`.

---

### `GetDaysUntilDue(JiraIssue issue)`
Computes the number of days remaining until the issue’s due date.

- **Parameters**
  - `issue`: The Jira issue containing the `DueDate` field.
- **Return value**
  - Returns the integer number of days from the current UTC date to the due date. Returns a negative value if the due date has passed. Returns `0` if the due date is today.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null`.
  - Throws `InvalidOperationException` if the `DueDate` field is missing or unparseable.

---
### `GetStagnationDays(JiraIssue issue)`
Calculates the number of days the issue has remained in its current status without state change.

- **Parameters**
  - `issue`: The Jira issue containing status change history or last status change timestamp.
- **Return value**
  - Returns the integer number of days since the last status change. Returns `0` if the status was changed today.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null`.
  - Throws `InvalidOperationException` if status change history is unavailable or unparseable.

---
### `HasComponent(JiraIssue issue, string componentName)`
Checks whether the issue is assigned to a specific component.

- **Parameters**
  - `issue`: The Jira issue containing component information.
  - `componentName`: The name of the component to check for.
- **Return value**
  - Returns `true` if the issue has a component matching `componentName` (case-insensitive); otherwise returns `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null` or if `componentName` is `null` or empty.

---
### `GetPriorityLevel(JiraIssue issue)`
Maps the issue’s priority to a numeric level for comparison.

- **Parameters**
  - `issue`: The Jira issue containing the `Priority` field.
- **Return value**
  - Returns an integer representing the priority level, where lower values indicate higher priority (e.g., "Highest" → `1`, "High" → `2`, ..., "Lowest" → `5`). Returns `0` if priority is undefined.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null`.

---
### `GetEstimatedCompletionPercentage(JiraIssue issue)`
Estimates the percentage of work completed based on issue fields such as `TimeSpent` and `Estimate`.

- **Parameters**
  - `issue`: The Jira issue containing time tracking fields.
- **Return value**
  - Returns an integer between `0` and `100` representing the estimated completion percentage. Returns `0` if no estimate or time spent is available.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null`.
  - Throws `InvalidOperationException` if time tracking fields are malformed or inconsistent.

## Usage
