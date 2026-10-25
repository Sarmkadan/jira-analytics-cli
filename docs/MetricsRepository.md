# MetricsRepository

`MetricsRepository` is the in-memory implementation of `IMetricsRepository`. It keeps sprint metrics and burndown snapshots in two `ConcurrentDictionary` collections and exposes asynchronous read/write methods over them. It has no persistent storage: all data lives in the process and is lost when the process exits.

- **Namespace:** `JiraAnalyticsCli.Repositories`
- **Source:** `Repositories/MetricsRepository.cs`
- **Interface:** `IMetricsRepository` (`Repositories/IMetricsRepository.cs`)
- **Registration:** `ServiceFactory.cs` registers it as a singleton: `services.AddSingleton<IMetricsRepository, MetricsRepository>();`

## Storage layout

| Field | Type | Key | Holds |
|-------|------|-----|-------|
| `_metrics` | `ConcurrentDictionary<string, List<SprintMetric>>` | `SprintMetric.SprintName` | Sprint metrics, grouped by sprint name |
| `_burndownData` | `ConcurrentDictionary<int, List<BurndownSnapshot>>` | `BurndownSnapshot.SprintId` | Burndown snapshots, grouped by sprint ID |

Because the metrics dictionary is keyed by sprint name, the `projectKey` parameters below are matched against `SprintName`. `SprintMetric` has no project property, so a project key only finds metrics whose sprint name equals that key. See [Notes](#notes).

## Constructor

### MetricsRepository(ILogger<MetricsRepository> logger)
Creates an empty repository. The logger is used for debug, information, and error messages. There is no parameterless constructor.

**Parameters:**
- `logger` – Logger used for all diagnostic output.

## Interface members

These members are declared on `IMetricsRepository` and are the ones that consumers normally depend on.

### GetByProjectAsync
```csharp
public async Task<List<SprintMetric>> GetByProjectAsync(string projectKey)
```
**Purpose:** Returns all metrics stored under `projectKey`, ordered by `EndDate` descending (newest first).

**Parameters:**
- `projectKey` – Key looked up in the metrics dictionary (matched against `SprintName`).

**Return value:** A new list. Empty if the key is not present.

**Errors:** Exceptions are caught and logged. On error, an empty list is returned. No exception propagates to the caller.

### GetBySprintAsync
```csharp
public async Task<List<SprintMetric>> GetBySprintAsync(int sprintId)
```
**Purpose:** Returns every metric whose `SprintId` equals `sprintId`, across all project keys. The method flattens all stored metrics and filters them, so the order is the storage order.

**Parameters:**
- `sprintId` – The Jira sprint ID.

**Return value:** A new list. Empty if no metric matches.

**Errors:** Caught and logged; an empty list is returned.

### GetBurndownDataAsync
```csharp
public async Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId)
```
**Purpose:** Returns the burndown snapshots stored for a sprint, ordered by `Timestamp` ascending (oldest first).

**Parameters:**
- `sprintId` – The Jira sprint ID.

**Return value:** A new list. Empty if the sprint has no snapshots.

**Errors:** Caught and logged; an empty list is returned.

### GetLatestForProjectAsync
```csharp
public async Task<SprintMetric?> GetLatestForProjectAsync(string projectKey)
```
**Purpose:** Returns the metric with the greatest `EndDate` stored under `projectKey`.

**Parameters:**
- `projectKey` – Key looked up in the metrics dictionary (matched against `SprintName`).

**Return value:** The newest `SprintMetric`, or `null` if none is stored.

**Errors:** Caught and logged; `null` is returned.

### SaveMetricAsync
```csharp
public async Task SaveMetricAsync(SprintMetric metric)
```
**Purpose:** Validates a metric and appends it to the list stored under `metric.SprintName`. The list is created if it does not exist. No deduplication is performed, so saving the same metric twice stores it twice.

**Parameters:**
- `metric` – The metric to store.

**Errors:**
- `ArgumentNullException` if `metric` is `null`.
- `ArgumentException` from `SprintMetric.Validate()` if the metric is invalid (for example, a non-positive sprint ID, an empty sprint name, an end date not after the start date, negative planned points, or completed points exceeding planned points).

Errors are logged and then rethrown. Because the method is `async`, these exceptions are delivered through the returned `Task`.

### SaveBurndownAsync
```csharp
public async Task SaveBurndownAsync(BurndownSnapshot snapshot)
```
**Purpose:** Validates a burndown snapshot and appends it to the list stored under `snapshot.SprintId`.

**Parameters:**
- `snapshot` – The snapshot to store.

**Errors:**
- `ArgumentNullException` if `snapshot` is `null`.
- `ArgumentException` from `BurndownSnapshot.Validate()` if the snapshot is invalid (for example, a non-positive sprint ID, a missing timestamp, negative values, a total of zero or less, or completed plus remaining points not equal to the total).

Errors are logged and then rethrown.

### SaveBurndownRangeAsync
```csharp
public async Task SaveBurndownRangeAsync(List<BurndownSnapshot> snapshots)
```
**Purpose:** Validates and stores several snapshots. The input is grouped by `SprintId`, and each group is appended to its sprint's list.

**Parameters:**
- `snapshots` – The snapshots to store. Must contain at least one element.

**Errors:**
- `ArgumentException` if `snapshots` is `null` or empty.
- `ArgumentException` from `BurndownSnapshot.Validate()` if any snapshot is invalid.

Validation runs group by group. Groups that were processed before the failing one are already stored when the exception is thrown, so a failed call can leave the repository partially updated.

On success, an information message records the number of snapshots saved.

## Class-only members

These members are public on `MetricsRepository` but are not declared on `IMetricsRepository`. Code that depends on the interface cannot call them without a cast.

### GetMetricCount
```csharp
public int GetMetricCount()
```
**Purpose:** Returns the total number of stored `SprintMetric` entries across all project keys.

**Return value:** The count, or `0` if nothing is stored.

### GetBurndownCount
```csharp
public int GetBurndownCount()
```
**Purpose:** Returns the total number of stored `BurndownSnapshot` entries across all sprints.

**Return value:** The count, or `0` if nothing is stored.

### Clear
```csharp
public void Clear()
```
**Purpose:** Removes all metrics and burndown snapshots and logs an information message.

**Notes:** This removes all data, not just one project or sprint.

## Validation helpers

`Repositories/MetricsRepositoryValidation.cs` provides extension methods on `MetricsRepository` that check the data it holds:

| Method | Returns | Behavior |
|--------|---------|----------|
| `Validate(this MetricsRepository)` | `IReadOnlyList<string>` | Re-runs `Validate()` on every stored metric and snapshot and returns one message per failure. An empty list means the data is valid. |
| `IsValid(this MetricsRepository)` | `bool` | `true` when `Validate()` returns no problems. |
| `EnsureValid(this MetricsRepository)` | `void` | Throws `ArgumentException` listing all problems if any exist. |

All three throw `ArgumentNullException` when the repository argument is `null`.

These helpers read the private `_metrics` and `_burndownData` fields through reflection. If those field names change, the helpers silently see no data and report the repository as valid.

## Usage

Inject the interface rather than constructing the class directly. The DI container supplies the singleton, so all callers share the same data.

```csharp
public class SprintReportService(IMetricsRepository metrics)
{
    public async Task<string> DescribeLatestAsync(string projectKey)
    {
        var latest = await metrics.GetLatestForProjectAsync(projectKey);
        return latest is null
            ? $"No metrics stored for {projectKey}."
            : $"{latest.SprintName}: {latest.CompletedStoryPoints} points completed.";
    }
}
```

Saving a batch of snapshots:

```csharp
var snapshots = new List<BurndownSnapshot> { /* captured from Jira */ };
await metrics.SaveBurndownRangeAsync(snapshots);

var stored = await metrics.GetBurndownDataAsync(sprintId);
```

## Notes

- **Project keys are sprint names.** `GetByProjectAsync` and `GetLatestForProjectAsync` look up the dictionary by `SprintName`, not by a Jira project key. Callers must store metrics under the key they later query with, or those queries return nothing.
- **No cancellation support.** None of the methods accept a `CancellationToken`.
- **Errors are mostly swallowed on reads.** The read methods catch exceptions, log them, and return an empty result or `null`. Write methods log and rethrow.
- **No duplicate checks.** Saving the same metric or snapshot again adds another copy.
- **Thread safety.** The dictionaries are concurrent, but the lists stored in them are plain `List<T>`. Appends through `AddOrUpdate` are not synchronized with reads of the same list. Concurrent writes to one sprint or project can race, and reads can observe a list while it is being modified. Callers that share an instance across threads should serialize access.
- **Data is not persisted.** Metrics and snapshots stored in one run are not available in the next.
- **Redundant `finally` blocks.** Each method has `finally { await Task.CompletedTask; }`, which does nothing useful. It is kept as-is here, and no behavior depends on it.
