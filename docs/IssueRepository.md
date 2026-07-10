# IssueRepository

Centralizes access to Jira issues stored in a local cache, providing asynchronous retrieval and persistence operations for issue data.

## API

### `public IssueRepository`

Constructs a new `IssueRepository` instance. The repository is initialized with an empty in-memory collection and requires explicit calls to `SaveAsync` or `SaveRangeAsync` to persist changes to a backing store.

### `public ValueTask<JiraIssue?> GetByKeyAsync(string key)`

Retrieves a single issue by its unique key (e.g., "PROJ-123"). The key is case-sensitive.

- **Parameters**
  - `key`: The issue key to search for.
- **Return value**
  - A `ValueTask<JiraIssue?>` resolving to the found issue or `null` if no match exists.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `ArgumentException` if `key` is empty or whitespace.

### `public ValueTask<List<JiraIssue>> GetByProjectAsync(string projectKey)`

Retrieves all issues belonging to the specified project.

- **Parameters**
  - `projectKey`: The project key to filter by (e.g., "PROJ").
- **Return value**
  - A `ValueTask<List<JiraIssue>>` containing all matching issues. The list is empty if no issues exist for the project.
- **Exceptions**
  - Throws `ArgumentNullException` if `projectKey` is `null`.
  - Throws `ArgumentException` if `projectKey` is empty or whitespace.

### `public ValueTask<List<JiraIssue>> GetBySprintAsync(string sprintId)`

Retrieves all issues assigned to the specified sprint.

- **Parameters**
  - `sprintId`: The sprint identifier to filter by.
- **Return value**
  - A `ValueTask<List<JiraIssue>>` containing all matching issues. The list is empty if no issues are assigned to the sprint.
- **Exceptions**
  - Throws `ArgumentNullException` if `sprintId` is `null`.
  - Throws `ArgumentException` if `sprintId` is empty or whitespace.

### `public ValueTask<List<JiraIssue>> GetOverdueAsync()`

Retrieves all issues that are past their due date.

- **Return value**
  - A `ValueTask<List<JiraIssue>>` containing all overdue issues. The list is empty if no issues are overdue.
- **Exceptions**
  - None.

### `public ValueTask<List<JiraIssue>> GetHighPriorityAsync()`

Retrieves all issues marked as high priority.

- **Return value**
  - A `ValueTask<List<JiraIssue>>` containing all high-priority issues. The list is empty if no high-priority issues exist.
- **Exceptions**
  - None.

### `public ValueTask SaveAsync(JiraIssue issue)`

Persists a single issue to the backing store, replacing any existing entry with the same key.

- **Parameters**
  - `issue`: The issue to save.
- **Return value**
  - A `ValueTask` representing the asynchronous save operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `issue` is `null`.
  - Throws `InvalidOperationException` if the issue key is malformed or invalid.

### `public ValueTask SaveRangeAsync(IEnumerable<JiraIssue> issues)`

Persists a collection of issues to the backing store, replacing any existing entries with matching keys.

- **Parameters**
  - `issues`: The issues to save.
- **Return value**
  - A `ValueTask` representing the asynchronous save operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `issues` is `null`.
  - Throws `InvalidOperationException` if any issue key is malformed or invalid.

### `public int GetCount()`

Returns the total number of issues currently cached in memory.

- **Return value**
  - The count of cached issues.
- **Exceptions**
  - None.

### `public void Clear()`

Removes all issues from the in-memory cache. Does not affect the backing store.

- **Parameters**
  - None.
- **Return value**
  - None.
- **Exceptions**
  - None.

## Usage
