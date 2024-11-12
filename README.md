// ... rest of README content ...
## JqlQueryServiceTests

The `JqlQueryServiceTests` class provides unit tests for the `JqlQueryService` class, verifying its behavior under various scenarios, including executing JQL queries, handling pagination, and formatting results. These tests ensure that the service correctly handles different error conditions and edge cases.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Services;
using JiraAnalyticsCli.Services;
using FluentAssertions;

// Create a test instance of JqlQueryService
var jqlQueryService = new JqlQueryService();

// Test that the service correctly maps Jira issues when executing a query
await jqlQueryService.ExecuteQueryAsync_WhenJiraReturnsIssues_MapsResultCorrectly();

// Test that the service returns success with no issues when Jira returns an empty result
await jqlQueryService.ExecuteQueryAsync_WhenJiraReturnsEmpty_ReturnsSuccessWithNoIssues();

// Test that the service returns a failure result when Jira throws an exception
await jqlQueryService.ExecuteQueryAsync_WhenJiraThrows_ReturnsFailureResult();

// Test that the service throws an ArgumentException when given an empty JQL query
await jqlQueryService.ExecuteQueryAsync_WithEmptyJql_ThrowsArgumentException();

// Test that the service correctly handles pagination requests
await jqlQueryService.ExecuteQueryAsync_WithPaginationRequest_PassesStartAtAndMaxResults();

// Verify that the service formats the result correctly when successful
jqlQueryService.FormatAsText_WithSuccessfulResult_ContainsIssueKeys().Should().Contain("Issue Key 1");

// Verify that the service returns an error message when the result is failed
jqlQueryService.FormatAsText_WithFailedResult_ReturnsErrorMessage().Should().Contain("Error message");
```

// ... rest of README content ...
