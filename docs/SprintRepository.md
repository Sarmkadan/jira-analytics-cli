# SprintRepository

In-memory repository for `Sprint` data, implementing `ISprintRepository`. Sprints are keyed by `Sprint.Id` in a `ConcurrentDictionary<int, Sprint>`. The repository offers lookup by ID, by project key, and by state (active or recently closed), plus upsert-style saves.

Namespace: `JiraAnalyticsCli.Repositories`

Nothing is persisted. Data lives only for the lifetime of the instance. `ServiceFactory` registers `ISprintRepository` as a singleton, so the whole process shares one store.

All methods are `async` but do no I/O. They return completed tasks. Query methods are fail-soft: an exception is logged and a `null` or empty result is returned. Save methods log and rethrow.

## API

### `public SprintRepository(ILogger<SprintRepository> logger)`

Creates an empty repository. `logger` receives debug, information, and error messages for each operation.

- **Parameters**
  - `logger`: Logger used for diagnostics.
- **Exceptions**
  - None are thrown by the constructor itself. `logger` is stored without a null check.

### `public async Task<Sprint?> GetByIdAsync(int sprintId)`

Returns the sprint stored under `sprintId`.

- **Parameters**
  - `sprintId`: The Jira sprint identifier.
- **Return value**
  - The stored sprint, or `null` if no sprint has that ID.
- **Exceptions**
  - None. The ID is not validated, so zero or negative values simply do not match. Any unexpected exception is logged and `null` is returned.

### `public async Task<List<Sprint>> GetByProjectAsync(string projectKey)`

Returns all sprints whose `ProjectKey` equals `projectKey`, ordered by `EndDate` descending (latest end date first). Sprints with no `EndDate` sort last.

- **Parameters**
  - `projectKey`: The Jira project key (e.g., `"PROJ"`). Compared case-sensitively.
- **Return value**
  - A new `List<Sprint>` with the matching sprints. The list is empty if there are none.
- **Exceptions**
  - None. The key is not validated. Any exception is logged and an empty list is returned.

### `public async Task<List<Sprint>> GetActiveSprints()`

Returns all sprints for which `Sprint.IsActive()` is `true`, across all projects. A sprint is active when its `State` is `"Active"`, or when its start date is in the past and its end date is either unset or in the future (compared against `DateTime.UtcNow`).

- **Return value**
  - A new `List<Sprint>` with the active sprints, in no guaranteed order. The list is empty if there are none.
- **Exceptions**
  - None. Any exception is logged and an empty list is returned.

### `public async Task<List<Sprint>> GetRecentClosedSprints(int count)`

Returns up to `count` sprints for which `Sprint.IsClosed()` is `true`, across all projects, ordered by `CompleteDate` descending. A sprint is closed when its `State` is `"Closed"` or it has a `CompleteDate`. Closed sprints with no `CompleteDate` sort last.

- **Parameters**
  - `count`: Maximum number of sprints to return. Values less than or equal to zero produce an empty list, because LINQ `Take` yields nothing for them.
- **Return value**
  - A new `List<Sprint>` with at most `count` sprints. The list is empty if there are none.
- **Exceptions**
  - None. The method performs no argument validation. Any exception is logged and an empty list is returned.

### `public async Task SaveAsync(Sprint sprint)`

Validates `sprint` with `Sprint.Validate()` and stores it. An existing entry with the same `Id` is replaced.

- **Parameters**
  - `sprint`: The sprint to store.
- **Return value**
  - A completed `Task`.
- **Exceptions**
  - `ArgumentNullException` if `sprint` is `null`.
  - `ArgumentException` if validation fails: `Id` is not positive, `Name` is empty or whitespace, or `EndDate` is not after `StartDate` (when both are set).
  - Both are logged before being rethrown. Nothing is stored on failure.

### `public async Task SaveRangeAsync(List<Sprint> sprints)`

Validates and stores each sprint in `sprints`, replacing entries with matching IDs.

- **Parameters**
  - `sprints`: The sprints to store.
- **Return value**
  - A completed `Task`.
- **Exceptions**
  - `ArgumentException` if `sprints` is `null` or empty.
  - `ArgumentException` from `Sprint.Validate()` if any sprint is invalid.
  - The operation is not atomic. Sprints are validated and stored one at a time, so sprints before the failing one remain stored. The error is logged and rethrown.

### `public int GetCount()`

Returns the number of sprints currently stored.

- **Return value**
  - The count of stored sprints.
- **Exceptions**
  - None.

### `public void Clear()`

Removes all sprints from the repository and logs an information message.

- **Return value**
  - None.
- **Exceptions**
  - None.
