// ... rest of README content ...
## SprintMetricTests

The `SprintMetricTests` class provides unit tests for the `SprintMetric` model, verifying critical business logic methods that calculate sprint metrics. These tests validate the behavior of completion rate calculation, health status determination, risk score aggregation, and validation of input parameters.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using FluentAssertions;

// Create a test sprint with planned points and completion rate
var sprint = new SprintMetric
{
    PlannedPoints = 100,
    CompletedPoints = 80,
    EndDate = DateTime.UtcNow.AddDays(10),
    StartDate = DateTime.UtcNow.AddDays(-10)
};

// Test completion rate calculation
double completionRate = sprint.GetCompletionRate(); // Returns 80%

// Test health status determination
string healthStatus = sprint.GetHealthStatus(); // Returns "Excellent"

// Test risk score aggregation
double riskScore = sprint.GetRiskScore(); // Returns a calculated risk score

// Verify assertions using FluentAssertions
sprint.GetCompletionRate().Should().BeGreaterThan(0);
sprint.GetHealthStatus().Should().Be("Excellent");
```

// ... rest of README content ...
```