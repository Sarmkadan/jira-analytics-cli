
## SprintExtensions

The `SprintExtensions` class provides extension methods for the `Sprint` model that enhance sprint analytics capabilities. It includes methods for calculating completion percentages, health scores, velocity metrics, cycle times, burn rates, and risk assessments. The extensions also provide status summaries, trend analysis, and goal tracking to help teams monitor sprint progress and identify potential issues.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;

// Create a sample sprint
var sprint = new Sprint
{
    Key = "PROJ-2026-Q3-SPR1",
    Name = "Q3 2026 Sprint 1",
    State = "Active",
    Goal = "Implement authentication and authorization features",
    StartDate = new DateTime(2026, 7, 1),
    EndDate = new DateTime(2026, 7, 15),
    Issues = new List<JiraIssue>
    {
        new JiraIssue
        {
            Key = "PROJ-101",
            Summary = "Implement login page",
            Status = "In Progress",
            Priority = "High",
            CreatedDate = new DateTime(2026, 7, 1),
            DueDate = new DateTime(2026, 7, 10),
            StoryPoints = 5,
            ResolutionDate = null
        },
        new JiraIssue
        {
            Key = "PROJ-102",
            Summary = "Fix authentication bug",
            Status = "Done",
            Priority = "Critical",
            CreatedDate = new DateTime(2026, 6, 28),
            DueDate = new DateTime(2026, 7, 5),
            StoryPoints = 3,
            ResolutionDate = new DateTime(2026, 7, 3)
        },
        new JiraIssue
        {
            Key = "PROJ-103",
            Summary = "Implement role-based authorization",
            Status = "To Do",
            Priority = "High",
            CreatedDate = new DateTime(2026, 7, 2),
            DueDate = new DateTime(2026, 7, 12),
            StoryPoints = 8,
            ResolutionDate = null
        },
        new JiraIssue
        {
            Key = "PROJ-104",
            Summary = "Update documentation",
            Status = "Open",
            Priority = "Medium",
            CreatedDate = new DateTime(2026, 7, 1),
            DueDate = new DateTime(2026, 7, 14),
            StoryPoints = 2,
            ResolutionDate = null
        }
    }
};

// Calculate key metrics
var completionPercent = sprint.GetCompletionPercentage();
Console.WriteLine($"Completion: {completionPercent:F1}%"); // Output: Completion: 25.0%

var healthScore = sprint.GetHealthScore();
Console.WriteLine($"Health Score: {healthScore:F1}%"); // Output: Health Score: 45.0%

var averageCycleTime = sprint.GetAverageCycleTime();
Console.WriteLine($"Average Cycle Time: {averageCycleTime:F1} days"); // Output: Average Cycle Time: 5.0 days

var burnRate = sprint.GetBurnRate();
Console.WriteLine($"Burn Rate: {burnRate:F2} issues/day"); // Output: Burn Rate: 0.20 issues/day

// Get status summary
var statusSummary = sprint.GetStatusSummary();
Console.WriteLine(statusSummary);
// Output: Sprint PROJ-2026-Q3-SPR1: Q3 2026 Sprint 1 | State: Active | 
// Completion: 25.0% (1/4) | Health: 45.0% | Velocity: 0.0% | Burn Rate: 0.20 issues/day

// Get high-priority issues
var highPriorityIssues = sprint.GetHighPriorityIssues();
Console.WriteLine($"High Priority Issues: {highPriorityIssues.Count}"); // Output: High Priority Issues: 2

// Get at-risk issues
var atRiskIssues = sprint.GetAtRiskIssues();
Console.WriteLine($"At Risk Issues: {atRiskIssues.Count}"); // Output: At Risk Issues: 0

// Get progress trend
var trend = sprint.GetProgressTrend();
Console.WriteLine($"Progress Trend: {trend}"); // Output: Progress Trend: ⏸️ Monitoring Required

// Get goal status
var goalStatus = sprint.GetGoalStatus();
Console.WriteLine($"Goal Status: {goalStatus}");
// Output: Goal Status: 🎯 Goal likely achievable: Implement authentication and authorization features 
// (25.0% complete, 3/18 story points)
```
