// ... rest of README content ...
## TeamComparisonServiceTests

The `TeamComparisonServiceTests` class provides unit tests for the `TeamComparisonService` class, verifying its behavior under various scenarios, including team comparison, snapshot creation, and data deduplication. These tests ensure that the service correctly handles different error conditions and edge cases.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Services;
using JiraAnalyticsCli.Services;
using FluentAssertions;

// Create a test instance of TeamComparisonService
var teamComparisonService = new TeamComparisonService();

// Test that the service correctly identifies the fastest team
await teamComparisonService.CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots();

// Test that the service correctly identifies the highest quality team by lowest defect rate
await teamComparisonService.CompareTeamsAsync_IdentifiesHighestQualityTeamByLowestDefectRate();

// Test that the service correctly deduplicates project keys
await teamComparisonService.CompareTeamsAsync_DeduplicatesProjectKeys();

// Test that the service throws an ArgumentException when given empty project keys
await teamComparisonService.CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException();

// Test that the service throws an ArgumentOutOfRangeException when given zero sprint count
await teamComparisonService.CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException();

// Verify assertions using FluentAssertions
teamComparisonService.FormatAsText_WithTwoTeams_ContainsBothProjectKeys().Should().Contain("Project Key 1");
```

// ... rest of README content ...
```