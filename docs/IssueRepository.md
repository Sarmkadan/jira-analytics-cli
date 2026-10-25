# IssueRepository

In-memory repository for `JiraIssue` data, implementing `IIssueRepository`. Issues are keyed by `JiraIssue.Key` in a `ConcurrentDictionary<string, JiraIssue>`, and the repository offers lookup by key, project, and sprint, filtered views for overdue and high-priority issues, and upsert-style saves.

Namespace: `JiraAnalyticsCli.Repositories`

Nothing is persisted. Data lives only for the lifetime of the instance. `ServiceFactory` registers `IIssueRepository` as a singleton, so the whole process shares one cache.

All methods return `ValueTask` because the work is synchronous. Query methods are fail-soft: an exception is logged and a `null` or empty result is returned. Save methods log and rethrow.

## API

### `public IssueRepository(ILogger<IssueRepository> logger)`

Creates an empty repository. `logger` receives debug, information, and error messages for each operation.

- **Parameters**
  - `logger`: Logger used for diagnostics.
- **Exceptions**
  - None are thrown by the constructor itself. `logger` is stored without a null check.

### `public ValueTask<JiraIssue?> GetByKeyAsync(string issueKey)`

Returns the issue stored under `issueKey`. Keys are matched case-sensitively and ordinally, so `"proj-1"` does not match `"PROJ-1"`.

- **Parameters**
  - `issueKey`: The issue key to look up (e.g., `"PROJ-123"`).
- **Return value**
  - The stored issue, or `null` if no issue has that key.
- **Exceptions**
  - None. A `null` key is passed to the dictionary, which throws. The exception is caught, logged as an error, and `null` is returned.

### `public ValueTask<List<JiraIssue>> GetByProjectAsync(string projectKey)`

Returns all issues whose `ProjectKey` equals `projectKey`.

- **Parameters**
  - `projectKey`: The project key to filter by (e.g., `"PROJ"`). Compared case-sensitively.
- **Return value**
  - A new `List<JiraIssue>` with the matching issues. The list is empty if there are none.
- **Exceptions**
  - None. The method performs no argument validation. Any exception is logged and an empty list is returned.

### `public ValueTask<List<JiraIssue>> GetBySprintAsync(int sprintId)`

Returns all issues whose `SprintId` equals `sprintId`.

- **Parameters**
  - `sprintId`: The numeric sprint identifier.
- **Return value**
  - A new `List<JiraIssue>` with the matching issues. Issues with no sprint (`SprintId == null`) never match. The list is empty if there are none.
- **Exceptions**
  - None. Any exception is logged and an empty list is returned.

### `public ValueTask<List<JiraIssue>> GetOverdueAsync(string projectKey)`

Returns the issues in `projectKey` that are overdue, as defined by `JiraIssue.IsOverdue()`. That check compares `DueDate` against `DateTime.UtcNow`, and issues without a due date are excluded.

- **Parameters**
  - `projectKey`: The project key to filter by.
- **Return value**
  - A new `List<JiraIssue>` ordered by `DueDate` descending (latest due date first). The list is empty if none are overdue.
- **Exceptions**
  - None. Any exception is logged and an empty list is returned.

### `public ValueTask<List<JiraIssue>> GetHighPriorityAsync(string projectKey)`

Returns the issues in `projectKey` whose priority is high, as defined by `JiraIssue.IsHighPriority()`. The high-priority values are `"Critical"`, `"Blocker"`, and `"High"`.

- **Parameters**
  - `projectKey`: The project key to filter by.
- **Return value**
  - A new `List<JiraIssue>` ordered by the `Priority` string, descending. This is alphabetical order, not severity order. The list is empty if there are none.
- **Exceptions**
  - None. Any exception is logged and an empty list is returned.

### `public ValueTask SaveAsync(JiraIssue issue)`

Validates `issue` with `JiraIssue.Validate()` and stores it. An existing entry with the same `Key` is replaced.

- **Parameters**
  - `issue`: The issue to store.
- **Return value**
  - A completed `ValueTask`.
- **Exceptions**
  - `ArgumentNullException` if `issue` is `null`.
  - `ArgumentException` if validation fails (empty key, empty summary, unset `CreatedDate`, or `DueDate` earlier than `CreatedDate`).
  - Both are logged before being rethrown. Nothing is stored on failure.

### `public ValueTask SaveRangeAsync(List<JiraIssue> issues)`

Validates and stores each issue in `issues`, replacing entries with matching keys.

- **Parameters**
  - `issues`: The issues to store.
- **Return value**
  - A completed `ValueTask`.
- **Exceptions**
  - `ArgumentException` if `issues` is `null` or empty.
  - `ArgumentException` from `JiraIssue.Validate()` if any issue is invalid.
  - The operation is not atomic. Issues are validated and stored one at a time, so issues before the failing one remain stored. The error is logged and rethrown.

### `public int GetCount()`

Returns the number of issues currently stored.

- **Return value**
  - The count of stored issues.
- **Exceptions**
  - None.

### `public void Clear()`

Removes all issues from the repository and logs an information message.

- **Return value**
  - None.
- **Exceptions**
  - None.

## Usage

```csharp
using JiraAnalyticsCli.Repositories;

var repository = new IssueRepository(logger);

await repository.SaveRangeAsync(issuesFromJira);

var issue = await repository.GetByKeyAsync("PROJ-123");
var overdue = await repository.GetOverdueAsync("PROJ");
var highPriority = await repository.GetHighPriorityAsync("PROJ");

Console.WriteLine($"Cached issues: {repository.GetCount()}");
```

## See also

- `IIssueRepository`: the interface this class implements.
- `IssueRepositoryExtensions`: additional query helpers.
