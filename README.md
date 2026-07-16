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

# ... rest of README content ...
