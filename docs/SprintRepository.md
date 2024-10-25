# SprintRepository

Provides data access operations for Jira sprints, including retrieval by identifier, project, active state, and recent closure, as well as persistence methods for single and batch saving.

## API

### `public SprintRepository`

Initializes a new instance of the `SprintRepository` with required dependencies for interacting with Jira sprint data.

### `public async Task<Sprint?> GetByIdAsync(int id)`

Retrieves a sprint by its unique identifier.

- **Parameters**
  - `id`: The numeric identifier of the sprint to retrieve.
- **Return value**
  - A `Task` resolving to the `Sprint` instance if found; otherwise `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `id` is less than or equal to zero.

### `public async Task<List<Sprint>> GetByProjectAsync(string projectKey)`

Retrieves all sprints associated with a given Jira project key.

- **Parameters**
  - `projectKey`: The uppercase project key (e.g., `"PROJ"`).
- **Return value**
  - A `Task` resolving to a `List<Sprint>` of sprints belonging to the project.
- **Exceptions**
  - Throws `ArgumentException` if `projectKey` is `null`, empty, or whitespace.

### `public async Task<List<Sprint>> GetActiveSprints()`

Retrieves all sprints currently marked as active.

- **Return value**
  - A `Task` resolving to a `List<Sprint>` of active sprints.
- **Exceptions**
  - None.

### `public async Task<List<Sprint>> GetRecentClosedSprints(int limit)`

Retrieves a limited number of recently closed sprints, ordered by closure date descending.

- **Parameters**
  - `limit`: Maximum number of sprints to return.
- **Return value**
  - A `Task` resolving to a `List<Sprint>` of the most recent closed sprints.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `limit` is less than zero.

### `public async Task SaveAsync(Sprint sprint)`

Persists a single sprint to the underlying data store.

- **Parameters**
  - `sprint`: The `Sprint` instance to save.
- **Return value**
  - A `Task` representing the asynchronous save operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `sprint` is `null`.
  - Throws `InvalidOperationException` if the sprint’s required fields are invalid.

### `public async Task SaveRangeAsync(IEnumerable<Sprint> sprints)`

Persists a collection of sprints to the underlying data store in a single transaction.

- **Parameters**
  - `sprints`: The collection of `Sprint` instances to save.
- **Return value**
  - A `Task` representing the asynchronous batch save operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `sprints` is `null`.
  - Throws `InvalidOperationException` if any sprint in the collection has invalid required fields.

### `public int GetCount()`

Returns the total number of sprints currently stored.

- **Return value**
  - The count of sprints as an `int`.
- **Exceptions**
  - None.

### `public void Clear()`

Removes all sprints from the in-memory cache or persistent store managed by this repository.

- **Return value**
  - None.
- **Exceptions**
  - None.

## Usage
