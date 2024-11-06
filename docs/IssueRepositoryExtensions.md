# IssueRepositoryExtensions

Extension methods for querying `JiraIssue` collections via `IIssueRepository`, providing common filtering and sorting operations used in analytics scenarios.

## API

### `GetByAssigneeAsync`

Queries issues assigned to the specified user.

- **Parameters**
  - `repository` (`IIssueRepository`): The repository to query.
  - `assignee` (`string`): The user key or name to filter by.
- **Return value**
  Returns a `ValueTask<List<JiraIssue>>` containing all issues assigned to the given user. The list is empty if no matches are found.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` or `assignee` is `null`.

### `GetUnassignedAsync`

Queries issues that have no assignee.

- **Parameters**
  - `repository` (`IIssueRepository`): The repository to query.
- **Return value**
  Returns a `ValueTask<List<JiraIssue>>` containing all unassigned issues. The list is empty if no unassigned issues exist.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` is `null`.

### `GetDueWithinAsync`

Queries issues whose due date falls within the specified time window.

- **Parameters**
  - `repository` (`IIssueRepository`): The repository to query.
  - `from` (`DateTime`): The start of the due date range (inclusive).
  - `to` (`DateTime`): The end of the due date range (inclusive).
- **Return value**
  Returns a `ValueTask<List<JiraIssue>>` containing issues with due dates between `from` and `to`. The list is empty if no issues match the range.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` is `null`.
  Throws `ArgumentOutOfRangeException` if `from` is after `to`.

### `GetTopPriorityAsync`

Queries issues ordered by descending priority, returning the highest-priority issues.

- **Parameters**
  - `repository` (`IIssueRepository`): The repository to query.
  - `count` (`int`): The maximum number of issues to return.
- **Return value**
  Returns a `ValueTask<List<JiraIssue>>` containing up to `count` issues, ordered by descending priority. The list is empty if no issues exist or `count` is zero or negative.
- **Exceptions**
  Throws `ArgumentNullException` if `repository` is `null`.
  Throws `ArgumentOutOfRangeException` if `count` is less than zero.

## Usage
