// ... rest of README content ...
## JiraIssueExtensions

The `JiraIssueExtensions` class provides a set of extension methods for working with `JiraIssue` objects. These methods allow you to calculate various metrics and properties of Jira issues, such as age, blockage status, and priority level.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;

// Create a test instance of JiraIssue
var issue = new JiraIssue
{
    CreatedAt = DateTime.Now.AddDays(-10),
    DueDate = DateTime.Now.AddDays(5),
    Components = new[] { "Component1" },
    Priority = 2
};

// Calculate the age of the issue in days
int ageInDays = JiraIssueExtensions.GetAgeInDays(issue);

// Check if the issue is blocked
bool isBlocked = JiraIssueExtensions.IsBlocked(issue);

// Calculate the number of days until the issue is due
int daysUntilDue = JiraIssueExtensions.GetDaysUntilDue(issue);

// Check if the issue has a specific component
bool hasComponent = JiraIssueExtensions.HasComponent(issue, "Component1");

// Get the priority level of the issue
int priorityLevel = JiraIssueExtensions.GetPriorityLevel(issue);

// Get the estimated completion percentage of the issue
int estimatedCompletionPercentage = JiraIssueExtensions.GetEstimatedCompletionPercentage(issue);

Console.WriteLine($"Age in days: {ageInDays}");
Console.WriteLine($"Is blocked: {isBlocked}");
Console.WriteLine($"Days until due: {daysUntilDue}");
Console.WriteLine($"Has component: {hasComponent}");
Console.WriteLine($"Priority level: {priorityLevel}");
Console.WriteLine($"Estimated completion percentage: {estimatedCompletionPercentage}");
```

# ... rest of README content ...
