// ... rest of README content ...

## Developer

The `Developer` class represents a developer or team member with their work metrics, including assigned issues, sprint metrics, and various performance calculations. It provides comprehensive methods for tracking developer productivity, completion rates, and capacity planning.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;

// Create a developer instance
var developer = new Developer
{
    Key = "DEV-001",
    Name = "John Doe",
    Email = "john.doe@example.com",
    DisplayName = "John D.",
    Active = true,
    JoinDate = new DateTime(2025, 1, 15)
};

// Assign some issues
developer.AssignedIssues.Add(new JiraIssue
{
    Key = "PROJ-123",
    Summary = "Implement login page",
    Status = "Done",
    StoryPoints = 5,
    Priority = "High",
    DueDate = DateTime.UtcNow.AddDays(14)
});

developer.AssignedIssues.Add(new JiraIssue
{
    Key = "PROJ-456",
    Summary = "Fix authentication bug",
    Status = "In Progress",
    StoryPoints = 3,
    Priority = "Critical",
    DueDate = DateTime.UtcNow.AddDays(7)
});

// Calculate metrics
int totalIssues = developer.GetTotalAssignedIssues();
int completedIssues = developer.GetCompletedIssues();
int inProgressIssues = developer.GetInProgressIssues();
int totalStoryPoints = developer.GetTotalStoryPoints();
int completedStoryPoints = developer.GetCompletedStoryPoints();

double completionRate = developer.GetCompletionRate();
double avgIssuesPerDay = developer.GetAverageIssuesPerDay();
double productivity = developer.GetProductivity();

Console.WriteLine($"Developer: {developer.DisplayName}");
Console.WriteLine($"Total issues: {totalIssues}");
Console.WriteLine($"Completed: {completedIssues}");
Console.WriteLine($"In progress: {inProgressIssues}");
Console.WriteLine($"Completion rate: {completionRate:F1}%");
Console.WriteLine($"Productivity: {productivity:F2} story points/day");

// Calculate capacity for a sprint
var sprintStart = new DateTime(2026, 6, 1);
var sprintEnd = new DateTime(2026, 6, 14);
double availableHours = developer.GetAvailableHours(sprintStart, sprintEnd);
double loadFactor = developer.GetLoadFactor(sprintStart, sprintEnd);

Console.WriteLine($"Available hours: {availableHours}");
Console.WriteLine($"Load factor: {loadFactor:F2} story points/hour");
```

## JiraIssue

The `JiraIssue` class represents a single Jira work item, holding all relevant information such as key, summary, status, priority, and timeline data. It provides methods to identify issue status, including overdue or high-priority flags, and calculate aging metrics.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;
using System.Collections.Generic;

// Create a new Jira issue
var issue = new JiraIssue
{
    Key = "PROJ-101",
    Id = "10001",
    Summary = "Implement user authentication",
    Description = "Add OAuth2 support to the login service.",
    Status = "In Progress",
    IssueType = "Story",
    Assignee = "dev.user@example.com",
    Priority = "High",
    StoryPoints = 5,
    DueDate = DateTime.UtcNow.AddDays(7),
    CreatedDate = DateTime.UtcNow.AddDays(-2),
    UpdatedDate = DateTime.UtcNow,
    ResolutionDate = null,
    Labels = new List<string> { "auth", "security" },
    Components = new List<string> { "backend" },
    ProjectKey = "PROJ",
    SprintId = 1
};

// Check issue status and aging
bool isOverdue = issue.IsOverdue;
bool isHighPriority = issue.IsHighPriority;
int daysOpen = issue.GetDaysOpenWithoutProgress();

Console.WriteLine($"Issue: {issue.Key} - {issue.Summary}");
Console.WriteLine($"Status: {issue.Status}");
Console.WriteLine($"Overdue: {isOverdue}");
Console.WriteLine($"Days open without progress: {daysOpen}");
```

## CachePolicyExtensions

The `CachePolicyExtensions` class provides a set of extension methods for working with `CachePolicy` objects. These methods allow you to create and modify cache policies with specific conditions, maximum sizes, and expiration times.

### Usage Example

```csharp
using JiraAnalyticsCli.Caching;

// Create a test instance of CachePolicy
var policy = CachePolicyExtensions.WithMaxSize(new CachePolicy(), 100);

// Add a condition to the policy
policy = CachePolicyExtensions.WithCondition(policy, issue => issue.IsOverdue());

// Set a combined expiration time
policy = CachePolicyExtensions.WithCombinedExpiration(policy, TimeSpan.FromHours(2));

// Check if the policy has an expiration
bool hasExpiration = CachePolicyExtensions.HasExpiration(policy);

// Get the effective expiration time
TimeSpan? effectiveExpiration = CachePolicyExtensions.GetEffectiveExpiration(policy);

Console.WriteLine($"Has expiration: {hasExpiration}");
Console.WriteLine($"Effective expiration: {effectiveExpiration}");
```

## DateTimeExtensionsTests

The `DateTimeExtensionsTests` class provides unit tests for the `DateTimeExtensions` utility methods that handle date and time calculations, business hour detection, week numbers, and human-readable duration formatting.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Calculate business days between two dates (excludes weekends)
var startDate = new DateTime(2026, 6, 1); // Monday
var endDate = new DateTime(2026, 6, 12); // Friday (2 weeks later)
int businessDays = startDate.GetBusinessDaysBetween(endDate);
Console.WriteLine($"Business days: {businessDays}"); // Output: Business days: 10

// Check if a date/time is within business hours (9:00 AM to 5:00 PM)
var businessTime = new DateTime(2026, 6, 1, 14, 30, 0); // 2:30 PM
bool isBusinessHour = businessTime.IsBusinessHour();
Console.WriteLine($"Is business hour: {isBusinessHour}"); // Output: Is business hour: True

// Get the ISO 8601 week number for a date
var weekNumber = new DateTime(2026, 1, 15).GetWeekNumber();
Console.WriteLine($"Week number: {weekNumber}"); // Output: Week number: 3

// Check if a date is in the past or future
var yesterday = DateTime.UtcNow.AddDays(-1);
Console.WriteLine($"Is past: {yesterday.IsPast()}"); // Output: Is past: True
var tomorrow = DateTime.UtcNow.AddDays(1);
Console.WriteLine($"Is future: {tomorrow.IsFuture()}"); // Output: Is future: True

// Format a TimeSpan as human-readable duration
var duration = TimeSpan.FromDays(3).Add(TimeSpan.FromHours(2));
string readableDuration = duration.ToHumanReadableDuration();
Console.WriteLine($"Duration: {readableDuration}"); // Output: Duration: 3 days 2 hours

// Get the last business day of a month
var lastBusinessDay = new DateTime(2026, 6, 15).GetLastBusinessDayOfMonth();
Console.WriteLine($"Last business day: {lastBusinessDay:yyyy-MM-dd}"); // Output: Last business day: 2026-06-30
```

## JsonFormatterTests

The `JsonFormatterTests` class provides comprehensive unit tests for the `JsonFormatter`, ensuring correct JSON serialization, validation, metadata inclusion, and formatting (pretty-printing). It covers scenarios such as handling null properties, serializing objects, and validating JSON structures.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Formatters;

// Instantiate the test class
var tests = new JsonFormatterTests();

// These methods contain the test logic and would typically be 
// invoked by a test runner like xUnit.
tests.Format_ShouldSerializeObjectToJson();
tests.Format_ShouldHandleNullPropertiesByIgnoringThem();
tests.Validate_ShouldReturnTrueForValidJson();
tests.Validate_ShouldReturnFalseForInvalidJson();
tests.FormatWithMetadata_ShouldIncludeMetadata();
tests.Prettify_ShouldFormatMinifiedJson();
```

## CliConfig

The `CliConfig` class provides centralized configuration for the CLI application, including Jira connection settings, caching behavior, logging preferences, and export options. It serves as the primary configuration source for all CLI operations and validates required settings before use.

### Usage Example

```csharp
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;

// Create configuration with default values
var config = new CliConfig
{
    JiraBaseUrl = "https://jira.atlassian.net",
    JiraApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN") ?? "your-api-token-here",
    DefaultProject = "PROJ",
    CacheExpirationMinutes = 30,
    EnableDetailedLogging = true,
    DefaultSprintCount = 10,
    ExportFormat = "json"
};

// Validate configuration before use
config.Validate();

// Use configuration with JiraApiService
var httpClientFactory = new TestHttpClientFactory();
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var jiraService = new JiraApiService(httpClientFactory, config, loggerFactory.CreateLogger<JiraApiService>());

// Verify connection
bool isConnected = await jiraService.VerifyConnectionAsync();
Console.WriteLine($"Connection status: {(isConnected ? "Connected" : "Failed")}");
```

## BurndownSnapshot

The `BurndownSnapshot` class captures a point‑in‑time view of a sprint’s burndown metrics, including story points and issue counts. It provides methods to calculate the current burndown percentage, project completion, determine if the sprint is on track, and estimate remaining work hours.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;

// Create a snapshot for a sprint
var snapshot = new BurndownSnapshot
{
    Timestamp = DateTime.UtcNow,
    SprintId = 42,
    RemainingStoryPoints = 30,
    CompletedStoryPoints = 20,
    TotalStoryPoints = 50,
    RemainingIssueCount = 5,
    CompletedIssueCount = 10,
    TotalIssueCount = 15,
    ScopeChanges = 2
};

// Validate the snapshot data
snapshot.Validate();

// Calculate percentages and projections
double percent = snapshot.GetBurndownPercentage();
double projected = snapshot.GetProjectedCompletionPercentage(DateTime.UtcNow.AddDays(7));
bool onTrack = snapshot.IsOnTrack(DateTime.UtcNow.AddDays(14));
int hoursUntilDone = snapshot.GetHoursUntilCompletion(issuesPerHour: 2);

// Output information
Console.WriteLine(snapshot); // uses ToString()
Console.WriteLine($"Burndown: {percent:F1}%");
Console.WriteLine($"Projected completion: {projected:F1}%");
Console.WriteLine($"On track: {onTrack}");
Console.WriteLine($"Estimated hours to finish: {hoursUntilDone}");
```

## Sprint

The `Sprint` class represents a Jira sprint with its metadata, state, and associated issues. It provides comprehensive methods for tracking sprint progress, velocity, and identifying issues requiring attention such as overdue or blocked items.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;

// Create a sprint instance
var sprint = new Sprint
{
    Id = 1,
    Key = "SPR-2026-Q2-01",
    Name = "Q2 Platform Enhancement",
    State = "Active",
    StartDate = new DateTime(2026, 4, 1),
    EndDate = new DateTime(2026, 4, 14),
    CompleteDate = null,
    Goal = "Enhance authentication service with OAuth2 support and improved error handling",
    ProjectKey = "PLATFORM"
};

// Add issues to the sprint
sprint.Issues.Add(new JiraIssue
{
    Key = "PLAT-123",
    Summary = "Implement OAuth2 client credentials flow",
    Status = "In Progress",
    IssueType = "Story",
    StoryPoints = 8,
    Priority = "High",
    DueDate = new DateTime(2026, 4, 10),
    Assignee = "john.doe@example.com"
});

sprint.Issues.Add(new JiraIssue
{
    Key = "PLAT-124",
    Summary = "Add rate limiting to authentication endpoints",
    Status = "To Do",
    IssueType = "Task",
    StoryPoints = 3,
    Priority = "Medium",
    DueDate = new DateTime(2026, 4, 12),
    Assignee = "jane.smith@example.com"
});

sprint.Issues.Add(new JiraIssue
{
    Key = "PLAT-125",
    Summary = "Fix authentication timeout issues",
    Status = "Done",
    IssueType = "Bug",
    StoryPoints = 5,
    Priority = "Critical",
    DueDate = new DateTime(2026, 3, 28),
    CompletedDate = new DateTime(2026, 4, 2)
});

// Calculate sprint metrics
int plannedStoryPoints = sprint.GetPlannedStoryPoints();
int completedStoryPoints = sprint.GetCompletedStoryPoints();
double velocity = sprint.GetVelocity();
int totalIssues = sprint.GetTotalIssueCount();
int completedIssues = sprint.GetCompletedIssueCount();
int inProgressIssues = sprint.GetInProgressIssues().Count;
int overdueIssues = sprint.GetOverdueIssues().Count;
int blockedIssues = sprint.GetBlockedIssues().Count;
int durationDays = sprint.GetDuration();

bool isActive = sprint.IsActive();
bool isClosed = sprint.IsClosed();

// Output sprint information
Console.WriteLine(sprint);
Console.WriteLine($"Duration: {durationDays} days");
Console.WriteLine($"Planned story points: {plannedStoryPoints}");
Console.WriteLine($"Completed story points: {completedStoryPoints}");
Console.WriteLine($"Velocity: {velocity:F1}%");
Console.WriteLine($"Total issues: {totalIssues}");
Console.WriteLine($"Completed: {completedIssues}");
Console.WriteLine($"In progress: {inProgressIssues}");
Console.WriteLine($"Overdue: {overdueIssues}");
Console.WriteLine($"Blocked: {blockedIssues}");
Console.WriteLine($"Active: {isActive}");
Console.WriteLine($"Closed: {isClosed}");

// Get specific issue lists
var overdueIssuesList = sprint.GetOverdueIssues();
var inProgressIssuesList = sprint.GetInProgressIssues();
var blockedIssuesList = sprint.GetBlockedIssues();
```

## SprintMetric

The `SprintMetric` class represents aggregated metrics for a sprint including velocity, quality indicators, and risk assessment. It provides comprehensive methods for calculating key performance metrics such as velocity, completion rate, commitment accuracy, quality score, and productivity per team member.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;

// Create a sprint metric instance for a completed sprint
var sprintMetric = new SprintMetric
{
    SprintId = 42,
    SprintName = "Sprint 2026-Q2-04",
    StartDate = new DateTime(2026, 6, 1),
    EndDate = new DateTime(2026, 6, 14),
    PlannedStoryPoints = 50,
    CompletedStoryPoints = 45,
    CommittedStoryPoints = 48,
    CompletedIssueCount = 12,
    TotalIssueCount = 15,
    DefectsCount = 2,
    AverageCycleTime = 2.5,
    OverdueIssueCount = 1,
    TeamSize = 6,
    ScopeChangeCount = 3
};

// Calculate key performance metrics
double velocity = sprintMetric.GetVelocity();
double completionRate = sprintMetric.GetCompletionRate();
double commitmentAccuracy = sprintMetric.GetCommitmentAccuracy();
double qualityScore = sprintMetric.GetQualityScore();
double productivityPerMember = sprintMetric.GetProductivityPerTeamMember();
double dailyBurndownRate = sprintMetric.GetDailyBurndownRate();

// Calculate risk and health status
double riskScore = sprintMetric.GetRiskScore();
string healthStatus = sprintMetric.GetHealthStatus();

// Validate the metric data
sprintMetric.Validate();

// Output the results
Console.WriteLine(sprintMetric);
Console.WriteLine($"Velocity: {velocity:F2} story points/day");
Console.WriteLine($"Completion rate: {completionRate:F1}%");
Console.WriteLine($"Commitment accuracy: {commitmentAccuracy:F1}%");
Console.WriteLine($"Quality score: {qualityScore:F1}/100");
Console.WriteLine($"Productivity: {productivityPerMember:F2} story points/member");
Console.WriteLine($"Daily burndown: {dailyBurndownRate:F2} story points/day");
Console.WriteLine($"Risk score: {riskScore:F1}/100");
Console.WriteLine($"Health status: {healthStatus}");
```

## SprintRepository

The `SprintRepository` class is an in-memory repository for managing Jira sprint data with lifecycle management and filtering capabilities. It provides methods to retrieve, filter, and save sprint entities efficiently using concurrent collections for thread-safe operations.

The repository is particularly useful for caching sprint data retrieved from the Jira API to reduce network calls and improve performance, and for managing sprint lifecycle operations such as finding active, recent closed, or project-specific sprints.

### Usage Example

```csharp
using JiraAnalyticsCli.Repositories;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;
using System;

// Create repository instance (typically via dependency injection)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var repository = new SprintRepository(loggerFactory.CreateLogger<SprintRepository>());

// Save individual sprints
var sprint1 = new Sprint
{
    Id = 1,
    Key = "SPR-2026-Q2-01",
    Name = "Q2 Platform Enhancement",
    State = "Active",
    StartDate = new DateTime(2026, 4, 1),
    EndDate = new DateTime(2026, 4, 14),
    CompleteDate = null,
    Goal = "Enhance authentication service with OAuth2 support and improved error handling",
    ProjectKey = "PLATFORM"
};

var sprint2 = new Sprint
{
    Id = 2,
    Key = "SPR-2026-Q2-02",
    Name = "Q2 API Improvements",
    State = "Closed",
    StartDate = new DateTime(2026, 4, 15),
    EndDate = new DateTime(2026, 4, 28),
    CompleteDate = new DateTime(2026, 4, 29),
    Goal = "Improve API performance and add new endpoints",
    ProjectKey = "PLATFORM"
};

var sprint3 = new Sprint
{
    Id = 3,
    Key = "SPR-2026-Q2-03",
    Name = "Q2 Bug Fixes",
    State = "Active",
    StartDate = new DateTime(2026, 4, 29),
    EndDate = new DateTime(2026, 5, 12),
    CompleteDate = null,
    Goal = "Fix critical bugs and security issues",
    ProjectKey = "PLATFORM"
};

await repository.SaveAsync(sprint1);
await repository.SaveAsync(sprint2);
await repository.SaveAsync(sprint3);

// Save multiple sprints at once
var batchSprints = new List<Sprint>
{
    new Sprint
    {
        Id = 4,
        Key = "SPR-2026-Q3-01",
        Name = "Q3 Migration",
        State = "Planned",
        StartDate = new DateTime(2026, 7, 1),
        EndDate = new DateTime(2026, 7, 14),
        Goal = "Migrate to new infrastructure",
        ProjectKey = "PLATFORM"
    },
    new Sprint
    {
        Id = 5,
        Key = "SPR-2026-Q3-02",
        Name = "Q3 Testing",
        State = "Planned",
        StartDate = new DateTime(2026, 7, 15),
        EndDate = new DateTime(2026, 7, 28),
        Goal = "Comprehensive testing of new features",
        ProjectKey = "PLATFORM"
    }
};

await repository.SaveRangeAsync(batchSprints);

// Retrieve a sprint by ID
var retrievedSprint = await repository.GetByIdAsync(1);
if (retrievedSprint != null)
{
    Console.WriteLine($"Found sprint: {retrievedSprint.Key} - {retrievedSprint.Name}");
}

// Get all sprints for a project
var platformSprints = await repository.GetByProjectAsync("PLATFORM");
Console.WriteLine($"Total sprints in PLATFORM: {platformSprints.Count}");

// Get active sprints
var activeSprints = await repository.GetActiveSprints();
Console.WriteLine($"Active sprints: {activeSprints.Count}");

// Get recent closed sprints (last 5)
var recentClosedSprints = await repository.GetRecentClosedSprints(5);
Console.WriteLine($"Recent closed sprints: {recentClosedSprints.Count}");

// Get repository statistics
int totalSprints = repository.GetCount();
Console.WriteLine($"Total sprints in repository: {totalSprints}");

// Clear repository (useful for testing or cache invalidation)
repository.Clear();
Console.WriteLine($"Repository cleared. Count: {repository.GetCount()}");
```

## MetricsRepository

The `MetricsRepository` class is an in-memory repository for managing sprint metrics and burndown data with historical tracking capabilities. It provides methods to retrieve, filter, and save sprint performance metrics and burndown snapshots efficiently using concurrent collections for thread-safe operations. The repository is particularly useful for caching metrics retrieved from the Jira API to reduce network calls and improve performance, and for maintaining historical records of sprint performance.

### Usage Example

```csharp
using JiraAnalyticsCli.Repositories;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;
using System;

// Create repository instance (typically via dependency injection)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var repository = new MetricsRepository(loggerFactory.CreateLogger<MetricsRepository>());

// Save individual sprint metrics
var sprintMetric1 = new SprintMetric
{
    SprintId = 1,
    SprintName = "Sprint 2026-Q2-01",
    StartDate = new DateTime(2026, 6, 1),
    EndDate = new DateTime(2026, 6, 14),
    PlannedStoryPoints = 50,
    CompletedStoryPoints = 45,
    CommittedStoryPoints = 48,
    CompletedIssueCount = 12,
    TotalIssueCount = 15,
    DefectsCount = 2,
    AverageCycleTime = 2.5,
    OverdueIssueCount = 1,
    TeamSize = 6,
    ScopeChangeCount = 3
};

var sprintMetric2 = new SprintMetric
{
    SprintId = 2,
    SprintName = "Sprint 2026-Q2-02",
    StartDate = new DateTime(2026, 6, 15),
    EndDate = new DateTime(2026, 6, 28),
    PlannedStoryPoints = 40,
    CompletedStoryPoints = 38,
    CommittedStoryPoints = 40,
    CompletedIssueCount = 10,
    TotalIssueCount = 12,
    DefectsCount = 1,
    AverageCycleTime = 1.8,
    OverdueIssueCount = 0,
    TeamSize = 6,
    ScopeChangeCount = 1
};

await repository.SaveMetricAsync(sprintMetric1);
await repository.SaveMetricAsync(sprintMetric2);

// Save burndown snapshots for a sprint
var burndownSnapshots = new List<BurndownSnapshot>
{
    new BurndownSnapshot
    {
        Timestamp = new DateTime(2026, 6, 1),
        SprintId = 1,
        RemainingStoryPoints = 50,
        CompletedStoryPoints = 0,
        TotalStoryPoints = 50,
        RemainingIssueCount = 15,
        CompletedIssueCount = 0,
        TotalIssueCount = 15,
        ScopeChanges = 0
    },
    new BurndownSnapshot
    {
        Timestamp = new DateTime(2026, 6, 7),
        SprintId = 1,
        RemainingStoryPoints = 25,
        CompletedStoryPoints = 25,
        TotalStoryPoints = 50,
        RemainingIssueCount = 8,
        CompletedIssueCount = 7,
        TotalIssueCount = 15,
        ScopeChanges = 0
    },
    new BurndownSnapshot
    {
        Timestamp = new DateTime(2026, 6, 14),
        SprintId = 1,
        RemainingStoryPoints = 5,
        CompletedStoryPoints = 45,
        TotalStoryPoints = 50,
        RemainingIssueCount = 2,
        CompletedIssueCount = 13,
        TotalIssueCount = 15,
        ScopeChanges = 0
    }
};

await repository.SaveBurndownRangeAsync(burndownSnapshots);

// Retrieve metrics by project
var projectMetrics = await repository.GetByProjectAsync("PROJ");
Console.WriteLine($"Metrics for PROJ: {projectMetrics.Count} entries");
foreach (var metric in projectMetrics)
{
    Console.WriteLine($" - {metric.SprintName}: {metric.CompletedStoryPoints}/{metric.PlannedStoryPoints} points");
}

// Get metrics for a specific sprint
var sprint1Metrics = await repository.GetBySprintAsync(1);
Console.WriteLine($"Metrics for Sprint 1: {sprint1Metrics.Count} entries");

// Get burndown data for a sprint
var sprint1Burndown = await repository.GetBurndownDataAsync(1);
Console.WriteLine($"Burndown snapshots for Sprint 1: {sprint1Burndown.Count} entries");

// Get the latest metrics for a project
var latestMetric = await repository.GetLatestForProjectAsync("PROJ");
if (latestMetric != null)
{
    Console.WriteLine($"Latest metric: {latestMetric.SprintName}");
    Console.WriteLine($"  Velocity: {latestMetric.GetVelocity():F2} story points/day");
    Console.WriteLine($"  Completion rate: {latestMetric.GetCompletionRate():F1}%");
}

// Get repository statistics
int totalMetrics = repository.GetMetricCount();
int totalBurndowns = repository.GetBurndownCount();
Console.WriteLine($"Total metrics: {totalMetrics}");
Console.WriteLine($"Total burndown snapshots: {totalBurndowns}");

// Clear repository (useful for testing or cache invalidation)
repository.Clear();
Console.WriteLine($"Repository cleared. Metrics: {repository.GetMetricCount()}, Burndowns: {repository.GetBurndownCount()}");
```

## IssueRepository

The `IssueRepository` class is an in-memory repository for managing Jira issues with caching and search capabilities. It provides methods to retrieve, filter, and save issues efficiently using concurrent collections for thread-safe operations. The repository is particularly useful for caching Jira issues retrieved from the API to reduce network calls and improve performance.

### Usage Example

```csharp
using JiraAnalyticsCli.Repositories;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;
using System;

// Create repository instance (typically via dependency injection)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var repository = new IssueRepository(loggerFactory.CreateLogger<IssueRepository>());

// Save individual issues
var issue1 = new JiraIssue
{
    Key = "PROJ-123",
    Summary = "Implement user authentication",
    Status = "In Progress",
    Priority = "High",
    StoryPoints = 5,
    DueDate = DateTime.UtcNow.AddDays(7),
    ProjectKey = "PROJ",
    SprintId = 1
};

var issue2 = new JiraIssue
{
    Key = "PROJ-456",
    Summary = "Fix authentication bug",
    Status = "To Do",
    Priority = "Critical",
    StoryPoints = 3,
    DueDate = DateTime.UtcNow.AddDays(3),
    ProjectKey = "PROJ",
    SprintId = 1
};

var issue3 = new JiraIssue
{
    Key = "PROJ-789",
    Summary = "Update API documentation",
    Status = "Done",
    Priority = "Medium",
    StoryPoints = 2,
    DueDate = DateTime.UtcNow.AddDays(-5), // Overdue
    ProjectKey = "PROJ",
    SprintId = 2
};

await repository.SaveAsync(issue1);
await repository.SaveAsync(issue2);
await repository.SaveAsync(issue3);

// Save multiple issues at once
var batchIssues = new List<JiraIssue>
{
    new JiraIssue
    {
        Key = "PROJ-101",
        Summary = "Add rate limiting",
        Status = "In Progress",
        Priority = "High",
        StoryPoints = 8,
        DueDate = DateTime.UtcNow.AddDays(10),
        ProjectKey = "PROJ",
        SprintId = 2
    },
    new JiraIssue
    {
        Key = "PROJ-102",
        Summary = "Performance optimization",
        Status = "To Do",
        Priority = "Medium",
        StoryPoints = 5,
        DueDate = DateTime.UtcNow.AddDays(14),
        ProjectKey = "PROJ",
        SprintId = 2
    }
};

await repository.SaveRangeAsync(batchIssues);

// Retrieve issues by key
var retrievedIssue = await repository.GetByKeyAsync("PROJ-123");
if (retrievedIssue != null)
{
    Console.WriteLine($"Found issue: {retrievedIssue.Key} - {retrievedIssue.Summary}");
}

// Get all issues for a project
var projIssues = await repository.GetByProjectAsync("PROJ");
Console.WriteLine($"Total issues in PROJ: {projIssues.Count}");

// Get issues by sprint
var sprint1Issues = await repository.GetBySprintAsync(1);
Console.WriteLine($"Issues in Sprint 1: {sprint1Issues.Count}");

// Get overdue issues for a project
var overdueIssues = await repository.GetOverdueAsync("PROJ");
Console.WriteLine($"Overdue issues: {overdueIssues.Count}");
foreach (var overdue in overdueIssues)
{
    Console.WriteLine($"  - {overdue.Key}: {overdue.Summary} (Due: {overdue.DueDate:yyyy-MM-dd})");
}

// Get high priority issues for a project
var highPriorityIssues = await repository.GetHighPriorityAsync("PROJ");
Console.WriteLine($"High priority issues: {highPriorityIssues.Count}");

// Get repository statistics
int totalIssues = repository.GetCount();
Console.WriteLine($"Total issues in repository: {totalIssues}");

// Clear repository (useful for testing or cache invalidation)
repository.Clear();
Console.WriteLine($"Repository cleared. Count: {repository.GetCount()}");
```

## JiraProject

The `JiraProject` class represents a Jira project with comprehensive tracking capabilities for sprints, team members, and project metrics. It provides methods to calculate sprint statistics, team performance, and identify issues requiring attention such as overdue or blocked items.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;
using System.Linq;

// Create a Jira project instance
var project = new JiraProject
{
    Key = "PROJ",
    Id = "10000",
    Name = "Platform Services",
    Description = "Core platform infrastructure and services",
    ProjectType = "Software",
    Lead = "john.leaderson@example.com",
    CreatedDate = new DateTime(2025, 1, 10),
    Url = "https://jira.example.com/projects/PROJ"
};

// Add sprints to the project
project.Sprints.Add(new Sprint
{
    Id = 1,
    Name = "Sprint 1",
    Goal = "Implement authentication service",
    StartDate = new DateTime(2026, 6, 1),
    EndDate = new DateTime(2026, 6, 14),
    Status = "Closed"
});

project.Sprints.Add(new Sprint
{
    Id = 2,
    Name = "Sprint 2",
    Goal = "Enhance API documentation",
    StartDate = new DateTime(2026, 6, 15),
    EndDate = new DateTime(2026, 6, 28),
    Status = "Active"
});

project.Sprints.Add(new Sprint
{
    Id = 3,
    Name = "Sprint 3",
    Goal = "Performance optimization",
    StartDate = new DateTime(2026, 6, 29),
    EndDate = new DateTime(2026, 7, 12),
    Status = "Planned"
});

// Add team members
project.TeamMembers.Add(new Developer
{
    Key = "DEV-001",
    Name = "Alice Johnson",
    Email = "alice.johnson@example.com",
    DisplayName = "Alice J.",
    Active = true,
    JoinDate = new DateTime(2025, 3, 1)
});

project.TeamMembers.Add(new Developer
{
    Key = "DEV-002",
    Name = "Bob Smith",
    Email = "bob.smith@example.com",
    DisplayName = "Bob S.",
    Active = true,
    JoinDate = new DateTime(2025, 2, 15)
});

// Add some metrics history
project.MetricsHistory.Add(new SprintMetric
{
    SprintId = 1,
    Date = new DateTime(2026, 6, 14),
    CompletedStoryPoints = 45,
    TotalStoryPoints = 50,
    CompletedIssues = 12,
    TotalIssues = 15
});

// Calculate project statistics
int totalSprints = project.GetTotalSprintCount();
int completedSprints = project.GetCompletedSprintCount();
Sprint? currentSprint = project.GetCurrentActiveSprint();
double avgVelocity = project.GetAverageVelocity();
int teamSize = project.GetTotalTeamSize();

// Get recent sprints
var recentSprints = project.GetRecentSprints(2);

// Identify issues requiring attention
var overdueIssues = project.GetAllOverdueIssues();
var blockedIssues = project.GetAllBlockedIssues();

// Get top performers
var topPerformers = project.GetTopPerformers(3);

// Output project information
Console.WriteLine($"Project: {project.Name} ({project.Key})");
Console.WriteLine($"Type: {project.ProjectType}");
Console.WriteLine($"Total sprints: {totalSprints}");
Console.WriteLine($"Completed sprints: {completedSprints}");
Console.WriteLine($"Current active sprint: {currentSprint?.Name ?? "None"}");
Console.WriteLine($"Average velocity: {avgVelocity} story points/sprint");
Console.WriteLine($"Team size: {teamSize} developers");
Console.WriteLine($"Top performers: {string.Join(", ", topPerformers.Select(d => d.DisplayName))}");
Console.WriteLine($"Overdue issues: {overdueIssues.Count}");
Console.WriteLine($"Blocked issues: {blockedIssues.Count}");
```

# AnalyticsController

The `AnalyticsController` class provides API endpoints for generating and retrieving Jira analytics reports. It serves as a bridge between the jira-analytics-cli library services and web applications, allowing you to expose analytics functionality through RESTful endpoints.

The controller integrates with the core jira-analytics-cli services including project analysis, report generation, and team dashboards, making it easy to build web applications that provide Jira insights to your team.

### Usage Example

```csharp
using Microsoft.AspNetCore.Mvc;
using JiraAnalyticsCli.Services;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IReportService _reportService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        IReportService reportService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("project/{projectKey}")]
    public async Task<IActionResult> GetProjectAnalytics(string projectKey, [FromQuery] int sprints = 5)
    {
        try
        {
            _logger.LogInformation("Generating analytics for project {ProjectKey}", projectKey);

            var analysis = await _analyticsService.AnalyzeSprints(projectKey, sprints);
            var report = _reportService.GenerateReport(analysis);

            return Ok(new {
                Project = projectKey,
                SprintCount = sprints,
                Analysis = analysis,
                Report = report
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate analytics");
            return BadRequest(new { error = ex.Message });
        }
    }
}

// In Program.cs:
var builder = WebApplication.CreateBuilder(args);

// Register jira-analytics-cli services
builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
builder.Services.AddSingleton<IReportService, ReportService>();

// Register the controller
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
```

## ReportService

The `ReportService` generates formatted text and HTML reports from Jira analytics data. It provides methods to create comprehensive sprint performance summaries, burndown charts, and various report formats for team and project stakeholders.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

// Create the service (typically via dependency injection)
var reportService = new ReportService(
    jiraService: new JiraApiService(),
    exportService: new ExportService(),
    logger: new Logger<ReportService>(new LoggerFactory())
);

// Generate a text report from sprint analysis
var analysis = new SprintAnalysisResult
{
    Metrics = new List<SprintMetric>
    {
        new SprintMetric
        {
            SprintId = 1,
            SprintName = "Sprint 1",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 14),
            PlannedStoryPoints = 50,
            CompletedStoryPoints = 45,
            TotalIssueCount = 15,
            CompletedIssueCount = 12,
            DefectsCount = 2,
            AverageCycleTime = 2.5,
            OverdueIssueCount = 1,
            TeamSize = 6,
            ScopeChangeCount = 3
        }
    },
    AverageVelocity = 45.0,
    TrendPercentage = 5.2,
    OverallHealth = "Healthy"
};

var textReport = reportService.GenerateReport(analysis);
Console.WriteLine(textReport);

// Generate an HTML report
var teamAnalysis = new TeamAnalysisResult
{
    TopPerformers = new List<Developer>
    {
        new Developer
        {
            Key = "DEV-001",
            Name = "Alice Johnson",
            DisplayName = "Alice J.",
            Active = true,
            JoinDate = new DateTime(2025, 3, 1)
        }
    },
    AverageProductivity = 2.3
};

var htmlReport = reportService.GenerateHtmlReport(analysis, teamAnalysis);
File.WriteAllText("report.html", htmlReport);

// Generate a burndown chart
await reportService.GenerateBurndownChart("PROJ", 1, "./burndown.png");

// Generate a summary report
var summaryReport = reportService.GenerateSummaryReport(analysis);
Console.WriteLine(summaryReport);
```

## ScheduledReportService

The `ScheduledReportService` is a background service that periodically generates and exports Jira analytics reports based on a configured schedule. It's ideal for creating automated reporting workflows that run on a regular basis (e.g., weekly reports).


### Usage Example

```csharp
using Microsoft.Extensions.Hosting;
using JiraAnalyticsCli.Services;

public class ScheduledReportService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ScheduledReportService> _logger;
    private readonly TimeSpan _scheduleInterval;

    public ScheduledReportService(
        IServiceProvider services,
        ILogger<ScheduledReportService> logger,
        TimeSpan scheduleInterval)
    {
        _services = services;
        _logger = logger;
        _scheduleInterval = scheduleInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled Report Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();

                // Generate weekly report
                var analysis = await analyticsService.AnalyzeSprints("PROJECT_KEY", 5);
                var report = reportService.GenerateReport(analysis);

                // Save to file
                var filePath = $"./reports/weekly-{DateTime.Now:yyyy-MM-dd}.txt";
                await File.WriteAllTextAsync(filePath, report, stoppingToken);

                _logger.LogInformation("Weekly report generated: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating scheduled report");
            }

            await Task.Delay(_scheduleInterval, stoppingToken);
        }
    }
}

// Register the background service
builder.Services.AddHostedService<ScheduledReportService>();
```

## ExportService

The `ExportService` provides functionality to export Jira analytics data in various formats including images (PNG/JPG/SVG), JSON, and CSV. It supports exporting sprint analytics, burndown charts, and team metrics, making it easy to share insights with stakeholders.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;

// Create the service (typically via dependency injection)
var exportService = new ExportService(
    jiraService: new JiraApiService(),
    analyticsService: new AnalyticsService(
        jiraService: new JiraApiService(),
        metricsRepository: new MetricsRepository(),
        logger: new Logger<AnalyticsService>(new LoggerFactory())
    ),
    logger: new Logger<ExportService>(new LoggerFactory())
);

// Export sprint analytics as a PNG image
await exportService.ExportAnalytics("PROJ", "png", "./sprint_analytics.png");

// Export sprint analytics as JSON
await exportService.ExportAnalytics("PROJ", "json", "./sprint_analytics.json");

// Export sprint analytics as CSV
await exportService.ExportAnalytics("PROJ", "csv", "./sprint_analytics.csv");

// Export burndown chart as a PNG image
await exportService.ExportBurndownChart(1, "png", "./burndown_chart.png");

// Export team metrics as JSON
await exportService.ExportTeamMetrics("PROJ", "json", "./team_metrics.json");

// Export team metrics as CSV
await exportService.ExportTeamMetrics("PROJ", "csv", "./team_metrics.csv");
```

## TeamDashboardService

The `TeamDashboardService` generates comprehensive team dashboards by combining analytics from multiple Jira projects. It integrates project analysis, team comparisons, and HTML dashboard generation to provide a holistic view of team performance across projects.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;

public class TeamDashboardService
{
    private readonly IAnalyticsService _analytics;
    private readonly ITeamComparisonService _comparison;
    private readonly IHtmlReportService _htmlReport;

    public TeamDashboardService(
        IAnalyticsService analytics,
        ITeamComparisonService comparison,
        IHtmlReportService htmlReport)
    {
        _analytics = analytics;
        _comparison = comparison;
        _htmlReport = htmlReport;
    }

    public async Task<TeamDashboard> GenerateTeamDashboard(string[] projectKeys, int sprintCount = 5)
    {
        // Get individual project analytics
        var projectAnalyses = new List<ProjectAnalysis>();
        foreach (var projectKey in projectKeys)
        {
            var analysis = await _analytics.AnalyzeSprints(projectKey, sprintCount);
            projectAnalyses.Add(analysis);
        }

        // Compare teams
        var comparison = await _comparison.CompareTeamsAsync(projectKeys, sprintCount);

        // Generate HTML dashboard
        var dashboardHtml = await _htmlReport.GenerateDashboardAsync(projectAnalyses, comparison);

        return new TeamDashboard
        {
            Projects = projectAnalyses,
            Comparison = comparison,
            DashboardHtml = dashboardHtml
        };
    }
}

// Register custom service
builder.Services.AddSingleton<TeamDashboardService>();
```

## JiraApiService

The `JiraApiService` class is the primary service for interacting with the Jira REST API. It provides methods to fetch projects, sprints, issues, team members, and various analytics data from Jira. This service handles all HTTP communication with the Jira API, including authentication, error handling, and data parsing.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

// Create the service (typically via dependency injection)
var httpClientFactory = new TestHttpClientFactory();
var config = new CliConfig();
var loggerFactory = LoggerFactory.Create(builder => {});
var logger = loggerFactory.CreateLogger<JiraApiService>();

var jiraService = new JiraApiService(httpClientFactory, config, logger);

// Verify Jira connection
bool isConnected = await jiraService.VerifyConnectionAsync();
Console.WriteLine($"Jira connection: {(isConnected ? "Connected" : "Failed")}");

// Get a project
var project = await jiraService.GetProjectAsync("PROJ");
if (project != null)
{
    Console.WriteLine($"Project: {project.Name} ({project.Key})");
    Console.WriteLine($"Created: {project.CreatedDate:yyyy-MM-dd}");
}

// Get project sprints
var sprints = await jiraService.GetProjectSprintsAsync("PROJ");
Console.WriteLine($"Sprints: {sprints.Count}");

// Get issues for a project
var issues = await jiraService.GetProjectIssuesAsync("PROJ");
Console.WriteLine($"Issues: {issues.Count}");

// Get team members
var team = await jiraService.GetProjectTeamAsync("PROJ");
Console.WriteLine($"Team members: {team.Count}");

// Search using JQL
var searchResult = await jiraService.SearchByJqlAsync("project = PROJ AND status = \"In Progress\"", maxResults: 20);
Console.WriteLine($"Found {searchResult.Issues.Count} issues matching criteria");

// Get burndown data for a sprint
var sprintId = 1;
var burndown = await jiraService.GetBurndownDataAsync(sprintId);
Console.WriteLine($"Burndown snapshots: {burndown.Count}");

// Get a specific issue
var issue = await jiraService.GetIssueAsync("PROJ-123");
if (issue != null)
{
    Console.WriteLine($"Issue: {issue.Key} - {issue.Summary}");
    Console.WriteLine($"Status: {issue.Status}, Points: {issue.StoryPoints}");
}
```

## IAnalyticsService

The `IAnalyticsService` interface provides comprehensive analytics capabilities for Jira projects, enabling teams to analyze sprint performance, team productivity, quality metrics, velocity trends, and identify overdue issues. It serves as the core analytics engine that powers various reporting and dashboard features in the jira-analytics-cli library.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

// Create service instance (typically injected via DI)
var analyticsService = new AnalyticsService(
    jiraService: new JiraApiService(),
    metricsRepository: new MetricsRepository(),
    logger: new Logger<AnalyticsService>(new LoggerFactory())
);

// Analyze sprint performance for a project
var sprintAnalysis = await analyticsService.AnalyzeSprints("PROJ", 5);
Console.WriteLine($"Sprints analyzed: {sprintAnalysis.Metrics.Count}");
Console.WriteLine($"Average velocity: {sprintAnalysis.AverageVelocity:F2} story points/day");
Console.WriteLine($"Overall health: {sprintAnalysis.OverallHealth}");

// Analyze team productivity
var teamAnalysis = await analyticsService.AnalyzeTeam("PROJ");
Console.WriteLine($"Top performers: {string.Join(", ", teamAnalysis.TopPerformers.Select(d => d.DisplayName))}");
Console.WriteLine($"Average productivity: {teamAnalysis.AverageProductivity:F2} story points/day");

// Analyze quality metrics
var qualityMetrics = await analyticsService.AnalyzeQuality("PROJ");
Console.WriteLine($"Total defects: {qualityMetrics.TotalDefects}");
Console.WriteLine($"Defect rate: {qualityMetrics.DefectRate:F2}%");
Console.WriteLine($"High risk areas: {string.Join(", ", qualityMetrics.HighRiskAreas)}");

// Analyze velocity trends
var velocityTrend = await analyticsService.AnalyzeVelocityTrend("PROJ", 10);
Console.WriteLine($"Trend: {velocityTrend.Trend}");
Console.WriteLine($"Trend slope: {velocityTrend.TrendSlope:F2}%");

// Analyze overdue issues
var overdueIssues = await analyticsService.AnalyzeOverdueIssues("PROJ");
Console.WriteLine($"Overdue issues: {overdueIssues.TotalOverdueCount}");
Console.WriteLine($"Critical overdue: {overdueIssues.CriticalCount}");
Console.WriteLine($"Average days overdue: {overdueIssues.AverageDaysOverdue:F1}");
```

## ITeamComparisonService

The `ITeamComparisonService` interface provides functionality to compare Jira analytics metrics across multiple projects (teams) side by side. It generates comprehensive reports that include velocity, completion rates, quality metrics, cycle times, and identifies the top-performing teams based on various KPIs.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

// Create service instance (typically injected via DI)
var comparisonService = new TeamComparisonService(
    analyticsService: new AnalyticsService(
        jiraService: new JiraApiService(),
        metricsRepository: new MetricsRepository(),
        logger: new Logger<AnalyticsService>(new LoggerFactory())
    ),
    logger: new Logger<TeamComparisonService>(new LoggerFactory())
);

// Compare multiple teams/projects
var projectKeys = new[] { "PROJ", "PLATFORM", "API" };
var comparisonReport = await comparisonService.CompareTeamsAsync(projectKeys, sprintCount: 5);

// Access comparison results
Console.WriteLine($"Report generated at: {comparisonReport.GeneratedAt}");
Console.WriteLine($"Teams compared: {comparisonReport.Teams.Count}");
Console.WriteLine($"Fastest team: {comparisonReport.FastestTeam}");
Console.WriteLine($"Highest quality team: {comparisonReport.HighestQualityTeam}");
Console.WriteLine($"Most consistent team: {comparisonReport.MostConsistentTeam}");

// Iterate through each team's metrics
foreach (var team in comparisonReport.Teams)
{
    Console.WriteLine($"\nTeam: {team.ProjectKey}");
    Console.WriteLine($"  Average velocity: {team.AverageVelocity:F2} story points/sprint");
    Console.WriteLine($"  Completion rate: {team.AvgCompletionRate:F1}%");
    Console.WriteLine($"  Total points delivered: {team.TotalPointsDelivered}");
    Console.WriteLine($"  Total issues completed: {team.TotalIssuesCompleted}");
    Console.WriteLine($"  Total defects: {team.TotalDefects}");
    Console.WriteLine($"  Defect rate: {team.DefectRate:F2}%");
    Console.WriteLine($"  Average cycle time: {team.AvgCycleTime:F1} days");
    Console.WriteLine($"  Overall health: {team.OverallHealth}");
    Console.WriteLine($"  Velocity trend: {team.VelocityTrend:+#;-#;0}%");
    Console.WriteLine($"  Sprint count: {team.SprintCount}");
}
```

## JiraHealthCheck

The `JiraHealthCheck` implements ASP.NET Core's health check interface to verify Jira API connectivity. It's useful for monitoring the availability of the Jira API service in production environments.

### Usage Example

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using JiraAnalyticsCli.Services;

public class JiraHealthCheck : IHealthCheck
{
    private readonly IJiraApiService _jiraService;

    public JiraHealthCheck(IJiraApiService jiraService)
    {
        _jiraService = jiraService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _jiraService.GetProjectsAsync();
            return HealthCheckResult.Healthy("Jira API is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Jira API is not accessible", ex);
        }
    }
}

// Register health check
builder.Services.AddHealthChecks()
    .AddCheck<JiraHealthCheck>("jira");
```

## AnalyticsService

The `AnalyticsService` provides comprehensive analytics capabilities for Jira projects, enabling teams to analyze sprint performance, team productivity, quality metrics, velocity trends, and identify overdue issues. It serves as the core analytics engine that powers various reporting and dashboard features in the jira-analytics-cli library.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

// Create service instance (typically injected via DI)
var analyticsService = new AnalyticsService(
    jiraService: new JiraApiService(),
    metricsRepository: new MetricsRepository(),
    logger: new Logger<AnalyticsService>(new LoggerFactory())
);

// Analyze sprint performance for a project
var sprintAnalysis = await analyticsService.AnalyzeSprints("PROJ", 5);
Console.WriteLine($"Sprints analyzed: {sprintAnalysis.Metrics.Count}");
Console.WriteLine($"Average velocity: {sprintAnalysis.AverageVelocity:F2} story points/day");
Console.WriteLine($"Overall health: {sprintAnalysis.OverallHealth}");

// Analyze team productivity
var teamAnalysis = await analyticsService.AnalyzeTeam("PROJ");
Console.WriteLine($"Top performers: {string.Join(", ", teamAnalysis.TopPerformers.Select(d => d.DisplayName))}");
Console.WriteLine($"Average productivity: {teamAnalysis.AverageProductivity:F2} story points/day");

// Analyze quality metrics
var qualityMetrics = await analyticsService.AnalyzeQuality("PROJ");
Console.WriteLine($"Total defects: {qualityMetrics.TotalDefects}");
Console.WriteLine($"Defect rate: {qualityMetrics.DefectRate:F2}%");
Console.WriteLine($"High risk areas: {string.Join(", ", qualityMetrics.HighRiskAreas)}");

// Analyze velocity trends
var velocityTrend = await analyticsService.AnalyzeVelocityTrend("PROJ", 10);
Console.WriteLine($"Trend: {velocityTrend.Trend}");
Console.WriteLine($"Trend slope: {velocityTrend.TrendSlope:F2}%");

// Analyze overdue issues
var overdueIssues = await analyticsService.AnalyzeOverdueIssues("PROJ");
Console.WriteLine($"Overdue issues: {overdueIssues.TotalOverdueCount}");
Console.WriteLine($"Critical overdue: {overdueIssues.CriticalCount}");
Console.WriteLine($"Average days overdue: {overdueIssues.AverageDaysOverdue:F1}");
```

## CachedAnalyticsService

The `CachedAnalyticsService` adds caching functionality to the analytics service to reduce API calls to Jira and improve performance. It caches analysis results for a configurable duration (typically 30 minutes) and returns cached results when available.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Caching.Memory;

public class CachedAnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsService _decorated;
    private readonly IMemoryCache _cache;

    public CachedAnalyticsService(IAnalyticsService decorated, IMemoryCache cache)
    {
        _decorated = decorated;
        _cache = cache;
    }

    public async Task<ProjectAnalysis> AnalyzeSprints(string projectKey, int sprintCount)
    {
        var cacheKey = $"Analytics_{projectKey}_{sprintCount}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _decorated.AnalyzeSprints(projectKey, sprintCount);
        });
    }
}

// Register cached version
builder.Services.AddSingleton<IAnalyticsService, CachedAnalyticsService>();
```

# ... rest of README content ...
