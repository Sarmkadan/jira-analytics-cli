# SnapshotStore

File-backed implementation of `ISnapshotStore`. It persists `BurndownSnapshot` instances across CLI invocations so that burndown history can be loaded and analysed in later runs. Each sprint gets its own JSON-lines file (`sprint-<SprintId>.jsonl`) inside a single data directory.

## API

### `public SnapshotStore(ILogger<SnapshotStore> logger, string? dataDirectory = null)`

Constructs a store and resolves the directory where snapshot files are kept. The directory is not created here; it is created on the first `AppendAsync`.

- **Parameters**:
  - `logger` – Logger used to report skipped (corrupt, empty or invalid) lines during `LoadAsync`.
  - `dataDirectory` – Directory for snapshot files. When null or whitespace, the `JIRA_SNAPSHOT_DIR` environment variable is used if set; otherwise a `snapshots` directory under the current working directory.
- **Throws**: `ArgumentNullException` when `logger` is null. The directory path itself is not validated.

### `public Task AppendAsync(BurndownSnapshot snapshot, CancellationToken cancellationToken = default)`

Validates the snapshot, serializes it as a single JSON line and appends it to `sprint-<SprintId>.jsonl`. Creates the data directory if it does not exist. Existing lines are never rewritten.

- **Parameters**:
  - `snapshot` – The snapshot to persist. Its `SprintId` selects the target file.
  - `cancellationToken` – Observed while waiting for the write lock and passed to the file append.
- **Returns**: A task that completes once the line has been written.
- **Throws**:
  - `ArgumentNullException` when `snapshot` is null.
  - `ArgumentException` when `snapshot.Validate()` fails (for example, a non-positive `SprintId` or a default `Timestamp`). Validation runs before any directory or file is touched.
  - `OperationCanceledException` when the token is cancelled while waiting for the lock.

### `public Task<IReadOnlyList<BurndownSnapshot>> LoadAsync(int sprintId, CancellationToken cancellationToken = default)`

Reads `sprint-<sprintId>.jsonl` and returns the valid snapshots ordered by `Timestamp`, oldest first.

- **Parameters**:
  - `sprintId` – The sprint whose history should be loaded. Must be positive.
  - `cancellationToken` – Passed to the file read.
- **Returns**: The snapshots for the sprint. An empty array when no file exists yet.
- **Throws**: `ArgumentOutOfRangeException` when `sprintId` is zero or negative.

Line handling during load:

| Line content | Behaviour |
|------|-----------|
| Blank or whitespace | Skipped silently. |
| Not valid JSON (`JsonException`) | Skipped with a logged warning that includes the line number. |
| Deserializes to `null` | Skipped with a logged warning. |
| Fails `Validate()` (`ArgumentException`) | Skipped with a logged warning. |
| Valid | Added to the result. |

Only the caught categories above are tolerated; other I/O failures propagate to the caller.

## Usage

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;

ILogger<SnapshotStore> logger = loggerFactory.CreateLogger<SnapshotStore>();

// Uses JIRA_SNAPSHOT_DIR if set, otherwise ./snapshots
ISnapshotStore store = new SnapshotStore(logger);

// Persist the current state of sprint 42
await store.AppendAsync(new BurndownSnapshot
{
    Timestamp = DateTime.UtcNow,
    SprintId = 42,
    RemainingStoryPoints = 120,
    CompletedStoryPoints = 80,
    TotalStoryPoints = 200,
    RemainingIssueCount = 15,
    CompletedIssueCount = 5,
    TotalIssueCount = 20,
    ScopeChanges = 10
});

// Read the full history back, oldest first
IReadOnlyList<BurndownSnapshot> history = await store.LoadAsync(42);
```

Explicit directory, for example in tests:

```csharp
var store = new SnapshotStore(logger, Path.Combine(Path.GetTempPath(), "burndown-tests"));
```

## Notes

- **Storage layout.** Files are named `sprint-<SprintId>.jsonl`. Lines are written with `System.Text.Json` default serialization and `Environment.NewLine` as the terminator. The file holds lines in append order; ordering by `Timestamp` happens in `LoadAsync`, not on disk.
- **Append-only writes.** Because nothing is rewritten, an interrupted run can damage at most its own last line. `LoadAsync` skips such a line, so one partial write does not make the whole history unreadable.
- **Write lock scope.** `AppendAsync` uses a `static` `SemaphoreSlim(1, 1)`, so writes are serialized across all `SnapshotStore` instances in the same process, including writes to different sprints. The lock does not coordinate separate OS processes; two CLI processes appending at the same moment rely on the operating system's append semantics.
- **Reads are not locked.** `LoadAsync` can run concurrently with an append and may observe a partially written final line. That line is then skipped as corrupt.
- **Validation contract.** Both methods rely on `BurndownSnapshot.Validate()` throwing `ArgumentException` (or a subclass such as `ArgumentOutOfRangeException`). Any other exception type from validation would not be caught by `LoadAsync` and would fail the load.
- **Missing history is not an error.** A sprint with no file returns an empty list, which lets callers such as `BurndownCalculator` treat a first run as having no trend data.
- **Registration.** `ServiceFactory` registers `SnapshotStore` as a singleton behind `ISnapshotStore`. Consumers include `ReportService` (appends the current snapshot), `BurndownCalculator` (loads history) and the `burndown` command in `Program.cs`. Tests substitute `ISnapshotStore` with a mock.
