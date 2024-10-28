# MetricsRepository

The `MetricsRepository` class provides asynchronous access to persisted sprint metrics and burndown data for the Jira Analytics CLI. It encapsulates all read and write operations against the underlying storage, allowing callers to retrieve, save, and clear metric collections without dealing with low‑level data‑access details.

## API

### MetricsRepository()
Initializes a new instance of the repository. The constructor configures the storage connection based on the application’s settings; no parameters are required.

### GetByProjectAsync
```csharp
public async Task<List<SprintMetric>> GetByProjectAsync(string projectKey, CancellationToken cancellationToken = default)
```
**Purpose:** Returns all sprint metrics associated with the given project.  
**Parameters:**  
- `projectKey` – The identifier of the Jira project (e.g., "PROJ").  
- `cancellationToken` – Optional token to cancel the operation.  
**Return Value:** A list of `SprintMetric` objects; an empty list if no metrics exist for the project.  
**Exceptions:**  
- `ArgumentNullException` if `projectKey` is null.  
- `OperationCanceledException` if the cancellation token is triggered.  
- `IOException` or derived storage exceptions if the underlying data store cannot be accessed.

### GetBySprintAsync
```csharp
public async Task<List<SprintMetric>> GetBySprintAsync(int sprintId, CancellationToken cancellationToken = default)
```
**Purpose:** Retrieves all metrics recorded for a specific sprint.  
**Parameters:**  
- `sprintId` – The numeric ID of the sprint within Jira.  
- `cancellationToken` – Optional cancellation token.  
**Return Value:** List of `SprintMetric` entries for the sprint; empty list when none are found.  
**Exceptions:**  
- `ArgumentOutOfRangeException` if `sprintId` is less than or equal to zero.  
- `OperationCanceledException` on token cancellation.  
- `IOException` for storage‑related failures.

### GetBurndownDataAsync
```csharp
public async Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId, CancellationToken cancellationToken = default)
```
**Purpose:** Obtains the chronological burndown snapshots for a sprint.  
**Parameters:**  
- `sprintId` – The sprint identifier.  
- `cancellationToken` – Optional cancellation token.  
**Return Value:** Ordered list of `BurndownSnapshot` objects; empty list if no snapshots exist.  
**Exceptions:**  
- Same as `GetBySprintAsync` for invalid `sprintId` or cancellation.  
- `IOException` on storage access errors.

### GetLatestForProjectAsync
```csharp
public async Task<SprintMetric?> GetLatestForProjectAsync(string projectKey, CancellationToken cancellationToken = default)
```
**Purpose:** Returns the most recent metric entry for a project, or null if none exist.  
**Parameters:**  
- `projectKey` – Project identifier.  
- `cancellationToken` – Optional cancellation token.  
**Return Value:** The latest `SprintMetric` or null.  
**Exceptions:**  
- `ArgumentNullException` for a null `projectKey`.  
- `OperationCanceledException` on cancellation.  
- `IOException` for storage issues.

### SaveMetricAsync
```csharp
public async Task SaveMetricAsync(SprintMetric metric, CancellationToken cancellationToken = default)
```
**Purpose:** Persists a single sprint metric.  
**Parameters:**  
- `metric` – The `SprintMetric` instance to store.  
- `cancellationToken` – Optional cancellation token.  
**Return Value:** Completes when the metric has been saved.  
**Exceptions:**  
- `ArgumentNullException` if `metric` is null.  
- `OperationCanceledException` on cancellation.  
- `IOException` if the write operation fails.

### SaveBurndownAsync
```csharp
public async Task SaveBurndownAsync(BurndownSnapshot snapshot, CancellationToken cancellationToken = default)
```
**Purpose:** Stores a single burndown snapshot.  
**Parameters:**  
- `snapshot` – The `BurndownSnapshot` to persist.  
- `cancellationToken` – Optional cancellation token.  
**Return Value:** Completes when the snapshot is saved.  
**Exceptions:**  
- `ArgumentNullException` for a null `snapshot`.  
- `OperationCanceledException` on cancellation.  
- `IOException` for storage errors.

### SaveBurndownRangeAsync
```csharp
public async Task SaveBurndownRangeAsync(IEnumerable<BurndownSnapshot> snapshots, CancellationToken cancellationToken = default)
```
**Purpose:** Persists a collection of burndown snapshots in a single operation.  
**Parameters:**  
- `snapshots` – Sequence of `BurndownSnapshot` objects to store.  
- `cancellationToken` – Optional cancellation token.  
**Return Value:** Completes when all snapshots have been saved.  
**Exceptions:**  
- `ArgumentNullException` if `snapshots` is null.  
- `OperationCanceledException` on cancellation.  
- `IOException` if any write fails; partial writes may have occurred before the exception.

### GetMetricCount
```csharp
public int GetMetricCount(string projectKey)
```
**Purpose:** Returns the total number of stored metrics for a project.  
**Parameters:**  
- `projectKey` – Project identifier.  
**Return Value:** Integer count of metrics; zero if none exist.  
**Exceptions:**  
- `ArgumentNullException` for a null `projectKey`.  
- `IOException` on storage read failure.

### GetBurndownCount
```csharp
public int GetBurndownCount(int sprintId)
```
**Purpose:** Returns the number of burndown snapshots stored for a sprint.  
**Parameters:**  
- `sprintId` – Sprint identifier.  
**Return Value:** Integer count of snapshots; zero if none exist.  
**Exceptions:**  
- `ArgumentOutOfRangeException` for non‑positive `sprintId`.  
- `IOException` on storage read failure.

### Clear()
```csharp
public void Clear()
```
**Purpose:** Removes all persisted metrics and burndown data from the repository.  
**Parameters:** None.  
**Return Value:** None.  
**Exceptions:**  
- `IOException` if the underlying store cannot be cleared.

## Usage

### Example 1: Retrieving and displaying the latest metric for a project
```csharp
using var repo = new MetricsRepository();

string projectKey = "ABC";
var latest = await repo.GetLatestForProjectAsync(projectKey);
if (latest is not null)
{
    Console.WriteLine($"Latest metric for {projectKey}: {latest.CompletedStoryPoints} points completed.");
}
else
{
    Console.WriteLine($"No metrics found for project {projectKey}.");
}
```

### Example 2: Saving a batch of burndown snapshots for a sprint
```csharp
using var repo = new MetricsRepository();

int sprintId = 42;
var snapshots = GetBurndownSnapshotsFromSource(); // hypothetical helper
await repo.SaveBurndownRangeAsync(snapshots);

int count = repo.GetBurndownCount(sprintId);
Console.WriteLine($"Saved {count} burndown snapshots for sprint {sprintId}.");
```

## Notes
- The repository does not enforce any locking; concurrent calls to mutating methods (`SaveMetricAsync`, `SaveBurndownAsync`, `SaveBurndownRangeAsync`, `Clear`) may result in race conditions. Callers should synchronize access if multiple threads share the same instance.  
- Read‑only methods (`GetByProjectAsync`, `GetBySprintAsync`, `GetBurndownDataAsync`, `GetLatestForProjectAsync`, `GetMetricCount`, `GetBurndownCount`) are safe to invoke concurrently with each other, but not concurrently with mutating operations on the same data without external synchronization.  
- All asynchronous methods accept a `CancellationToken`; passing a token that is already cancelled will cause the method to throw `OperationCanceledException` before performing any I/O.  
- The `Clear` method removes **all** data indiscriminately; use it only in test scenarios or when intentional data wiping is required.  
- If the underlying storage throws an `IOException`, the repository does not retry automatically; callers may implement retry logic based on their reliability requirements.  
- The `GetMetricCount` and `GetBurndownCount` methods return instantaneous counts; rapid successive calls may observe different values if other threads are modifying the repository concurrently.
