# Sprint
The `Sprint` type represents a sprint in a Jira project, encapsulating its properties and providing methods to calculate various metrics. It is used to analyze and track the progress of a sprint, including its duration, planned and completed story points, velocity, and issue counts.

## API
### Properties
* `Id`: A unique identifier for the sprint.
* `Key`: The key of the sprint.
* `Name`: The name of the sprint.
* `State`: The current state of the sprint.
* `StartDate`: The start date of the sprint, or `null` if not set.
* `EndDate`: The end date of the sprint, or `null` if not set.
* `CompleteDate`: The completion date of the sprint, or `null` if not set.
* `Goal`: The goal of the sprint, or `null` if not set.
* `ProjectKey`: The key of the project that the sprint belongs to.
* `Issues`: A list of Jira issues associated with the sprint.
* `IsActive`: A boolean indicating whether the sprint is active.
* `IsClosed`: A boolean indicating whether the sprint is closed.

### Methods
* `GetDuration`: Returns the duration of the sprint in days.
* `GetPlannedStoryPoints`: Returns the total planned story points for the sprint.
* `GetCompletedStoryPoints`: Returns the total completed story points for the sprint.
* `GetVelocity`: Returns the velocity of the sprint, calculated as completed story points divided by duration.
* `GetCompletedIssueCount`: Returns the number of completed issues in the sprint.
* `GetTotalIssueCount`: Returns the total number of issues in the sprint.
* `GetOverdueIssues`: Returns a list of overdue issues in the sprint.
* `GetInProgressIssues`: Returns a list of in-progress issues in the sprint.

## Usage
```csharp
// Example 1: Creating a new sprint and calculating its velocity
Sprint sprint = new Sprint
{
    Id = 1,
    Key = "SPR-1",
    Name = "Sprint 1",
    StartDate = DateTime.Now,
    EndDate = DateTime.Now.AddDays(14),
    Issues = new List<JiraIssue>
    {
        new JiraIssue { StoryPoints = 3 },
        new JiraIssue { StoryPoints = 5 },
        new JiraIssue { StoryPoints = 2 }
    }
};
double velocity = sprint.GetVelocity;
Console.WriteLine($"Sprint velocity: {velocity}");
```

```csharp
// Example 2: Retrieving a list of overdue issues in a sprint
Sprint sprint = new Sprint
{
    Id = 1,
    Key = "SPR-1",
    Name = "Sprint 1",
    StartDate = DateTime.Now,
    EndDate = DateTime.Now.AddDays(14),
    Issues = new List<JiraIssue>
    {
        new JiraIssue { DueDate = DateTime.Now.AddDays(-1) },
        new JiraIssue { DueDate = DateTime.Now.AddDays(1) },
        new JiraIssue { DueDate = DateTime.Now.AddDays(7) }
    }
};
List<JiraIssue> overdueIssues = sprint.GetOverdueIssues;
Console.WriteLine($"Overdue issues: {overdueIssues.Count}");
```

## Notes
* The `GetDuration` method returns the duration in days, which may not account for non-working days or holidays.
* The `GetVelocity` method assumes that the sprint has a valid start and end date, and that the issues have valid story points.
* The `GetOverdueIssues` and `GetInProgressIssues` methods rely on the `DueDate` property of the `JiraIssue` class, which may not be set or up-to-date.
* The `Sprint` class is not thread-safe, and concurrent access to its properties and methods may result in inconsistent or unexpected behavior.
