// ... rest of README content ...
## JiraApiServiceTests

The `JiraApiServiceTests` class provides unit tests for the `JiraApiService` class, verifying its behavior under various scenarios, including HTTP status codes, JSON parsing, and request timeouts. These tests ensure that the service correctly handles different error conditions and edge cases.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Services;
using JiraAnalyticsCli.Services;
using FluentAssertions;

// Create a test instance of JiraApiService
var jiraApiService = new JiraApiService();

// Test that the service returns null on 401 Unauthorized
await jiraApiService.GetProjectAsync_Returns_Null_On_401_Unauthorized();

// Test that the service returns null on 403 Forbidden
await jiraApiService.GetProjectAsync_Returns_Null_On_403_Forbidden();

// Test that the service returns null on 429 TooManyRequests
await jiraApiService.GetProjectAsync_Returns_Null_On_429_TooManyRequests();

// Test that the service returns null on 500 InternalServerError
await jiraApiService.GetProjectAsync_Returns_Null_On_500_InternalServerError();

// Verify assertions using FluentAssertions
jiraApiService.GetProjectAsync_Returns_Null_On_401_Unauthorized().Should().BeNull();
```

// ... rest of README content ...
