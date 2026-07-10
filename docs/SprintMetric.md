# SprintMetric

Represents a summary of a single sprint’s planning and execution data, used by the Jira analytics CLI to calculate agile performance indicators such as velocity, completion rate, and quality scores.

## API

### Fields

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `SprintId` | Unique identifier of the sprint within Jira. | none | `int` | none |
| `SprintName` | Human‑readable name of the sprint (e.g., “Sprint 23”). | none | `string` | none |
| `StartDate` | Date and time when the sprint began. | none | `DateTime` | none |
| `EndDate` | Date and time when the sprint ended. | none | `DateTime` | none |
| `PlannedStoryPoints` | Total story points the team planned to complete at sprint start. | none | `int` | none |
| `CompletedStoryPoints` | Story points actually completed by the sprint’s end. | none | `int` | none |
| `CommittedStoryPoints` | Story points the team committed to during sprint planning. | none | `int` | none |
| `CompletedIssueCount` | Number of issues moved to “Done” state. | none | `int` | none |
| `TotalIssueCount` | Total number of issues that were part of the sprint (including incomplete). | none | `int` | none |
| `DefectsCount` | Count of issues classified as defects that were completed. | none | `int` | none |
| `AverageCycleTime` | Average elapsed time (in days) from work start to completion for completed issues. | none | `double` | none |
| `OverdueIssueCount` | Number of issues not completed by `EndDate`. | none | `int` | none |
| `TeamSize` | Number of team members considered for productivity calculations. | none | `int` | none |
| `ScopeChangeCount` | Number of scope changes (issues added or removed) that occurred during the sprint. | none | `int` | none |

### Methods

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `GetVelocity` | Returns the sprint’s velocity, defined as `CompletedStoryPoints`. | none | `double` | none |
| `GetCompletionRate` | Returns the ratio of completed to planned story points (`CompletedStoryPoints / PlannedStoryPoints`). Value is 0 when no work was planned. | none | `double` | `DivideByZeroException` if `PlannedStoryPoints == 0` |
| `GetCommitmentAccuracy` | Returns the ratio of completed to committed story points (`CompletedStoryPoints / CommittedStoryPoints`). | none | `double` | `DivideByZeroException` if `CommittedStoryPoints == 0` |
| `GetQualityScore` | Returns the proportion of completed issues that are not defects (`(CompletedIssueCount - DefectsCount) / CompletedIssueCount`). | none | `double` | `DivideByZeroException` if `CompletedIssueCount == 0` |
| `GetProductivityPerTeamMember` | Returns completed story points per team member (`CompletedStoryPoints / TeamSize`). | none | `double` | `DivideByZeroException` if `TeamSize == 0` |
| `GetDailyBurndownRate` | Returns the average decrease in remaining story points per day (`(PlannedStoryPoints - CompletedStoryPoints) / (EndDate - StartDate).TotalDays`). | none | `double` | `DivideByZeroException` if the sprint duration (`EndDate - StartDate`) is zero |

## Usage

```csharp
using JiraAnalyticsCli.Models;

// Example 1: Populating a SprintMetric and retrieving derived metrics
var sprint = new SprintMetric
{
    SprintId = 42,
    SprintName = "Sprint 12",
    StartDate = new DateTime(2024, 3, 1),
    EndDate   = new DateTime(2024, 3, 14),
    PlannedStoryPoints = 120,
    CompletedStoryPoints = 105,
    CommittedStoryPoints = 110,
    CompletedIssueCount = 30,
    TotalIssueCount = 35,
    DefectsCount = 3,
    AverageCycleTime = 2.5,
    OverdueIssueCount = 2,
    TeamSize = 5,
    ScopeChangeCount = 4
};

double velocity = sprint.GetVelocity;                     // 105
double completionRate = sprint.GetCompletionRate;        // 0.875
double commitmentAccuracy = sprint.GetCommitmentAccuracy; // 0.9545...
double qualityScore = sprint.GetQualityScore;            // 0.9
double productivityPerMember = sprint.GetProductivityPerTeamMember; // 21
double dailyBurndown = sprint.GetDailyBurndownRate;      // (120-105)/13 ≈ 1.154
```

```csharp
using System;
using JiraAnalyticsCli.Models;

// Example 2: Safely processing a collection of sprints, guarding against zero divisors
foreach (var sprint in sprintRepository.GetAllSprints())
{
    try
    {
        double rate = sprint.GetCompletionRate;
        Console.WriteLine($"Sprint {sprint.SprintId} completion rate: {rate:P}");
    }
    catch (DivideByZeroException)
    {
        Console.WriteLine($"Sprint {sprint.SprintId} has zero planned story points; completion rate undefined.");
    }

    // Similar guards can be applied to other derived metrics as needed.
}
```

## Notes

- **Edge Cases**  
  - Negative values for story‑point fields or issue counts are technically allowed by the type but indicate data errors; consumers should validate input before relying on derived metrics.  
  - If `StartDate` is after `EndDate`, the duration used by `GetDailyBurndownRate` becomes negative, producing a negative rate; this reflects malformed sprint data.  
  - When any denominator field (`PlannedStoryPoints`, `CommittedStoryPoints`, `CompletedIssueCount`, `TeamSize`, or sprint duration) is zero, the corresponding `Get*` method throws `DivideByZeroException`. Callers should handle this exception or pre‑check the denominator.  
  - `AverageCycleTime` is expected to be non‑negative; a negative value would be nonsensical and should be treated as invalid.

- **Thread‑Safety**  
  - All members are public fields or parameterless methods that read those fields. The type itself does not provide any synchronization.  
  - Safe concurrent use is guaranteed only when the instance is **immutable after construction** (i.e., no thread modifies its fields while another reads).  
  - If multiple threads may write to the same `SprintMetric` instance without external locking, race conditions can lead to inconsistent state and undefined behavior for the derived metrics. In such scenarios, synchronize access (e.g., with `lock`) or prefer immutable data transfer objects.
