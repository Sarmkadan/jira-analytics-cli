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

## ReportServiceValidation

The `ReportServiceValidation` class provides extension methods for validating `ReportService` instances and their parameters used in report generation. It includes methods for validating service instances, checking validity, ensuring validity with exceptions, and validating various report parameters like project keys, sprint IDs, output paths, and formats.


### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using System;
using System.Collections.Generic;

// Create a ReportService instance (typically injected via DI)
var reportService = new ReportService(
    // dependencies would be injected here
);

// Validate the ReportService instance
var validationErrors = reportService.Validate();
Console.WriteLine($"Validation errors count: {validationErrors.Count}");

// Check if the ReportService instance is valid
var isValid = reportService.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Ensure the ReportService instance is valid (throws if invalid)
try
{
    reportService.EnsureValid();
    Console.WriteLine("ReportService instance is valid");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a project key for report generation
var projectKeyErrors = ReportServiceValidation.ValidateProjectKey("PROJ-2026");
if (projectKeyErrors.Count == 0)
{
    Console.WriteLine("Project key is valid");
}

// Validate a sprint ID for burndown chart generation
var sprintIdErrors = ReportServiceValidation.ValidateSprintId(123);
if (sprintIdErrors.Count == 0)
{
    Console.WriteLine("Sprint ID is valid");
}

// Validate an output path for report generation
var outputPathErrors = ReportServiceValidation.ValidateOutputPath("./reports/sprint-2026-q3.html");
if (outputPathErrors.Count == 0)
{
    Console.WriteLine("Output path is valid");
}

// Validate a format for report generation
var formatErrors = ReportServiceValidation.ValidateFormat("html");
if (formatErrors.Count == 0)
{
    Console.WriteLine("Format is valid");
}
```

## AnalyticsServiceValidation

`AnalyticsServiceValidation` offers a set of static helper methods that validate an `AnalyticsService` instance and the parameters of its public API. The methods ensure that inputs such as project keys, sprint counts, dates, numeric values, and collections meet basic business rules before the service performs any calculations.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using JiraAnalyticsCli.Services;

// Assume an AnalyticsService instance is available (dependencies would normally be injected)
var analyticsService = new AnalyticsService(
    // constructor arguments omitted for brevity
);

// Validate the service instance itself
IReadOnlyList<string> instanceProblems = analyticsService.Validate();
Console.WriteLine($"Instance validation problems: {instanceProblems.Count}");

// Quick validity check
bool isValid = analyticsService.IsValid();
Console.WriteLine($"Service is valid: {isValid}");

// Throw if the instance is not valid
analyticsService.EnsureValid();

// Validate a project key
IReadOnlyList<string> keyProblems = AnalyticsServiceValidation.ValidateProjectKey("PROJ-2026");
Console.WriteLine($"Project key problems: {keyProblems.Count}");

// Validate sprint count
IReadOnlyList<string> sprintProblems = AnalyticsServiceValidation.ValidateSprintCount(5);
Console.WriteLine($"Sprint count problems: {sprintProblems.Count}");

// Validate a date parameter
IReadOnlyList<string> dateProblems = AnalyticsServiceValidation.ValidateDate(DateTime.UtcNow, nameof(dateProblems));
Console.WriteLine($"Date problems: {dateProblems.Count}");

// Validate a numeric value
IReadOnlyList<string> numberProblems = AnalyticsServiceValidation.ValidateNumber(42.7, "velocity");
Console.WriteLine($"Number problems: {numberProblems.Count}");

// Validate a collection
var ids = new List<int> { 1, 2, 3 };
IReadOnlyList<string> collectionProblems = AnalyticsServiceValidation.ValidateCollection(ids, "ids");
Console.WriteLine($"Collection problems: {collectionProblems.Count}");
```

## SprintRepositoryJsonExtensions

The `SprintRepositoryJsonExtensions` class provides extension methods for serializing and deserializing `SprintRepository` instances to and from JSON format. It includes methods for converting a repository to a JSON string, parsing JSON back into a repository, safe parsing with error handling, retrieving all sprints for serialization, and loading sprints into a repository.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using System;
using System.Collections.Generic;

// Create a sample sprint repository with sprints
var repository = new SprintRepository(null!);
var sprints = new List<Sprint>
{
    new Sprint
    {
        Id = 1,
        Name = "Sprint 1",
        Goal = "Complete initial features",
        StartDate = new DateTime(2026, 7, 1),
        EndDate = new DateTime(2026, 7, 14),
        State = "Active",
        CompletedIssues = 15,
        TotalIssues = 20,
        CompletedStoryPoints = 45,
        TotalStoryPoints = 50
    },
    new Sprint
    {
        Id = 2,
        Name = "Sprint 2",
        Goal = "Implement advanced features",
        StartDate = new DateTime(2026, 7, 15),
        EndDate = new DateTime(2026, 7, 28),
        State = "Planned",
        CompletedIssues = 0,
        TotalIssues = 25,
        CompletedStoryPoints = 0,
        TotalStoryPoints = 60
    }
};
repository.LoadSprints(sprints);

// Serialize the repository to JSON (compact format)
var json = repository.ToJson();
Console.WriteLine(json);
// Output: [{"id":1,"name":"Sprint 1","goal":"Complete initial features",...}]

// Serialize with indentation for readability
var prettyJson = repository.ToJson(indented: true);
Console.WriteLine(prettyJson);

// Deserialize JSON back to a repository
var jsonData = """[
    {"id":1,"name":"Sprint 1","goal":"Complete initial features","startDate":"2026-07-01T00:00:00","endDate":"2026-07-14T00:00:00","state":"Active","completedIssues":15,"totalIssues":20,"completedStoryPoints":45,"totalStoryPoints":50},
    {"id":2,"name":"Sprint 2","goal":"Implement advanced features","startDate":"2026-07-15T00:00:00","endDate":"2026-07-28T00:00:00","state":"Planned","completedIssues":0,"totalIssues":25,"completedStoryPoints":0,"totalStoryPoints":60}
]""";
var deserializedRepo = SprintRepositoryJsonExtensions.FromJson(jsonData);
Console.WriteLine($"Deserialized repository has {deserializedRepo?.GetAllSprints().Count} sprints");

// Safe deserialization with error handling
if (SprintRepositoryJsonExtensions.TryFromJson(jsonData, out var safeRepo))
{
    Console.WriteLine("Successfully deserialized using TryFromJson");
}
else
{
    Console.WriteLine("Failed to deserialize JSON");
}

// Get all sprints from a repository
var allSprints = repository.GetAllSprints();
Console.WriteLine($"Repository contains {allSprints.Count} sprints");

// Load sprints into an empty repository
var emptyRepo = new SprintRepository(null!);
emptyRepo.LoadSprints(allSprints);
Console.WriteLine($"Empty repository now has {emptyRepo.GetAllSprints().Count} sprints");
```
