
## BurndownSnapshotExtensions

The `BurndownSnapshotExtensions` class provides extension methods for the `BurndownSnapshot` model that enhance burndown chart analysis and reporting. It includes methods for calculating velocity trends, detecting acceleration/deceleration, computing burn rates, creating delta comparisons, formatting status strings, detecting scope creep, and extracting time-series data for charting.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;
using System.Collections.Generic;

// Create a sample burndown snapshot
var snapshot = new BurndownSnapshot
{
    Timestamp = new DateTime(2026, 7, 10, 14, 30, 0),
    SprintId = "PROJ-2026-Q3-SPR1",
    TotalStoryPoints = 50,
    CompletedStoryPoints = 25,
    RemainingStoryPoints = 25,
    TotalIssueCount = 20,
    CompletedIssueCount = 10,
    RemainingIssueCount = 10,
    ScopeChanges = 2
};

// Create historical snapshots for trend analysis
var historicalSnapshots = new List<BurndownSnapshot>
{
    new BurndownSnapshot
    {
        Timestamp = new DateTime(2026, 7, 9, 14, 30, 0),
        SprintId = "PROJ-2026-Q3-SPR1",
        TotalStoryPoints = 50,
        CompletedStoryPoints = 20,
        RemainingStoryPoints = 30,
        TotalIssueCount = 20,
        CompletedIssueCount = 8,
        RemainingIssueCount = 12,
        ScopeChanges = 1
    },
    new BurndownSnapshot
    {
        Timestamp = new DateTime(2026, 7, 8, 14, 30, 0),
        SprintId = "PROJ-2026-Q3-SPR1",
        TotalStoryPoints = 50,
        CompletedStoryPoints = 15,
        RemainingStoryPoints = 35,
        TotalIssueCount = 20,
        CompletedIssueCount = 6,
        RemainingIssueCount = 14,
        ScopeChanges = 0
    }
};

// Calculate velocity trend over time
var velocityTrend = snapshot.CalculateVelocityTrend(historicalSnapshots);
Console.WriteLine($"Velocity Trend: {velocityTrend:F2} story points/day");

// Check if velocity is accelerating
var isAccelerating = snapshot.IsVelocityAccelerating(historicalSnapshots);
Console.WriteLine($"Is Accelerating: {isAccelerating}");

// Get burn rate for the sprint
var burnRate = snapshot.GetBurnRate(daysInSprint: 14);
Console.WriteLine($"Burn Rate: {burnRate:F2} story points/day");

// Create a delta snapshot for comparison
var previousSnapshot = historicalSnapshots[0];
var deltaSnapshot = snapshot.CreateDeltaSnapshot(previousSnapshot);
if (deltaSnapshot != null)
{
    Console.WriteLine($"Delta - Completed: {deltaSnapshot.CompletedStoryPoints}, " +
                     $"Remaining: {deltaSnapshot.RemainingStoryPoints}");
}

// Format as status string
var statusString = snapshot.ToStatusString();
Console.WriteLine(statusString);
// Output: Sprint PROJ-2026-Q3-SPR1 @ 2026-07-10 14:30 | 25/50 pts (50.0%) | 10/20 issues

// Check for scope creep
var hasScopeCreep = snapshot.HasScopeCreep(threshold: 3);
Console.WriteLine($"Has Scope Creep: {hasScopeCreep}");

// Extract time-series data for charting
var completedOverTime = historicalSnapshots.Append(snapshot).ToList()
    .GetCompletedStoryPointsOverTime();
var remainingOverTime = historicalSnapshots.Append(snapshot).ToList()
    .GetRemainingStoryPointsOverTime();

Console.WriteLine($"Completed over time: [{string.Join(", ", completedOverTime)}]");
Console.WriteLine($"Remaining over time: [{string.Join(", ", remainingOverTime)}]");
```
