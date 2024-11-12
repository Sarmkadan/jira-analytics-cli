// ... rest of README content ...
## HtmlReportServiceTests

The `HtmlReportServiceTests` class provides unit tests for the `HtmlReportService` class, verifying its behavior under various scenarios, including building HTML reports, escaping XSS characters, and handling edge cases. These tests ensure that the service correctly handles different error conditions and edge cases.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Services;
using JiraAnalyticsCli.Services;
using FluentAssertions;

// Create a test instance of HtmlReportServiceTests
var htmlReportServiceTests = new HtmlReportServiceTests();

// Test that the service correctly builds HTML with sprint data
htmlReportServiceTests.BuildHtml_WithSprintData_ContainsProjectKeyInTitle();

// Test that the service escapes XSS characters in project key
htmlReportServiceTests.BuildHtml_WithXssCharsInProjectKey_EscapesHtml();

// Test that the service produces a valid document with no sprints
htmlReportServiceTests.BuildHtml_WithNoSprints_StillProducesValidDocument();

// Test that the service includes a performer table in the report
htmlReportServiceTests.BuildHtml_WithTopPerformers_IncludesPerformerTable();

// Test that the service throws an exception with an invalid sprint count
await htmlReportServiceTests.GenerateReportAsync_WithInvalidSprintCount_ThrowsArgumentOutOfRangeException();

// Test that the service writes a file with HTML content
await htmlReportServiceTests.GenerateReportAsync_WritesFileWithHtmlContent();
```

// ... rest of README content ...
```