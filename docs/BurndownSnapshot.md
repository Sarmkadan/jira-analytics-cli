# BurndownSnapshot

A data transfer object that captures the state of a sprint at a specific point in time, used by the Jira analytics CLI to compute burndown metrics and track progress.

## API

### Timestamp  
**Type:** `DateTime`  
**  
The date and time when the snapshot was taken.  
**Parameters:** none  
**Return value:** the stored moment; defaults to `DateTime.MinValue` if not explicitly set.  
**Throws:** none.

### SprintId  
**Type:** `int`  
**Purpose:** Identifier of the sprint to which the snapshot belongs.  
**Parameters:** none  
**Return value:** the sprint ID; expected to be a positive value.  
**Throws:** none.

### RemainingStoryPoints  
**Type:** `int`  
**Purpose:** Story points of work that remain unfinished in the sprint.  
**Parameters:** none  
**Return value:** non‑negative integer; should not exceed `TotalStoryPoints`.  
**Throws:** none.

### CompletedStoryPoints  
**Type:** `int`  
**Purpose:** Story points of work that have been finished in the sprint.  
**Parameters:** none  
**Return value:** non‑negative integer; should not exceed `TotalStoryPoints`.  
**Throws:** none.

### TotalStoryPoints  
**Type:** `int`  
**Purpose:** Sum of remaining and completed story points for the sprint.  
**Parameters:** none  
**Return value:** non‑negative integer; ideally equals `RemainingStoryPoints + CompletedStoryPoints`.  
**Throws:** none.

### RemainingIssueCount  
**Type:** `int`  
**Purpose:** Number of issues that are still open in the sprint.  
**Parameters:** none  
**Return value:** non‑negative integer; should not exceed `TotalIssueCount`.  
**Throws:** none.

### CompletedIssueCount  
**Type:** `int`  
**Purpose:** Number of issues that have been closed in the sprint.  
**Parameters:** none  
**Return value:** non‑negative integer; should not exceed `TotalIssueCount`.  
**Throws:** none.

### TotalIssueCount  
**Type:** `int`  
**Purpose:** Sum of remaining and completed issue counts for the sprint.  
**Parameters:** none  
**Return value:** non‑negative integer; ideally equals `RemainingIssueCount + CompletedIssueCount`.  
**Throws:** none.

### ScopeChanges  
**Type:** `int`  
**Purpose:** Net change in scope (added minus removed story points or issues) since the sprint started.  
**Parameters:** none  
**Return value:** can be positive, zero, or negative.  
**Throws:** none.

### GetBurndownPercentage  
**Type:** `double` (method)  
**Purpose:** Calculates the proportion of story points completed relative to the total.  
**Parameters:** none  
**Return value:** value between 0.0 and 1.0; returns 0.0 when `TotalStoryPoints` is 0.  
**Throws:** none.

### GetProjectedCompletionPercentage  
**Type:** `double` (method)  
**Purpose:** Estimates the likely completion percentage based on current burndown rate.  
**Parameters:** none  
**Return value:** value between 0.0 and 1.0; may exceed 1.0 if projected work overshoots the remaining effort.  
**Throws:** none.

### IsOnTrack  
**Type:** `bool` (method)  
**Purpose:** Determines whether the sprint is progressing as expected according to a simple linear burndown model.  
**Parameters:** none  
**Return value:** `true` if the remaining effort is less than or equal to the effort that should remain at this point in time; otherwise `false`.  
**Throws:** none.

### GetHoursUntilCompletion  
**Type:** `int` (method)  
**Purpose:** Projects the number of working hours left until the sprint is completed, based on the current burndown velocity.  
**Parameters:** none  
**Return value:** non‑negative integer; returns 0 when no work remains.  
**Throws:** `InvalidOperationException` if the burndown velocity cannot be determined (e.g., insufficient historical data).

### Validate  
**Type:** `void` (method)  
**Purpose:** Checks internal consistency of the snapshot’s fields.  
**Parameters:** none  
**Return value:** none.  
**Throws:**  
- `ArgumentOutOfRangeException` if any count is negative.  
- `InvalidOperationException` if `TotalStoryPoints` does not equal `RemainingStoryPoints + CompletedStoryPoints` or if `TotalIssueCount` does not equal `RemainingIssueCount + CompletedIssueCount`.

### ToString  
**Type:** `string` (override)  
**Purpose:** Provides a human‑readable representation of the snapshot, including timestamp and key metrics.  
**Parameters:** none  
**Return value:** formatted string.  
**Throws:** none.

## Usage

```csharp
using JiraAnalyticsCli.Models;

// Create a snapshot for sprint 42 taken now
var snap = new BurndownSnapshot
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
};

// Ensure the data is consistent before using derived metrics
snap.Validate(); // throws if any invariant is violated

double burndown = snap.GetBurndownPercentage; // 0.4
bool onTrack = snap.IsOnTrack;                // depends on projected rate
int hoursLeft = snap.GetHoursUntilCompletion; // e.g., 32
```

```csharp
// Example: processing a collection of snapshots to detect scope creep
IEnumerable<BurndownSnapshot> history = LoadSnapshots();

foreach (var snap in history)
{
    snap.Validate(); // defensive check

    if (snap.ScopeChanges > 0)
    {
        Console.WriteLine(
            $"Sprint {snap.SprintId} at {snap.Timestamp:u} added {snap.ScopeChanges} points of scope.");
    }

    // Simple alert when projected completion falls below 80%
    if (snap.GetProjectedCompletionPercentage < 0.8)
    {
        Console.WriteLine(
            $"Sprint {snap.SprintId} projected completion low: {snap.GetProjectedCompletionPercentage:P0}.");
    }
}
```

## Notes

- The class does not enforce immutability; its fields are public settable. Concurrent modification of the same instance from multiple threads without external synchronization can lead to inconsistent state and incorrect results from the calculation methods.
- `Validate` should be called after populating a snapshot or after deserialization to guarantee that the derived methods operate on a coherent dataset.
- When `TotalStoryPoints` or `TotalIssueCount` is zero, `GetBurndownPercentage` returns `0.0` to avoid division by zero; callers should treat this as “no work defined” rather than “0 % complete”.
- `GetHoursUntilCompletion` relies on a linear extrapolation of completed work over time; if the snapshot lacks sufficient temporal context (e.g., only one snapshot available), the method throws `InvalidOperationException`.
- `IsOnTrack` uses a simple heuristic comparing remaining effort to the effort that should remain based on elapsed sprint duration; it does not account for non‑linear work patterns or changes in team capacity.
