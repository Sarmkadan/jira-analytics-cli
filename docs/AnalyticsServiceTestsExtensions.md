# AnalyticsServiceTestsExtensions

Static helper class providing predefined expected values used in unit tests for the analytics service when dealing with an empty project.

## API

### GetExpectedOverdueCountForEmptyProject
- **Purpose**: Returns the integer value that represents the expected number of overdue items for a project that contains no data.
- **Parameters**: None.
- **Return Value**: An `int` indicating the expected overdue count (typically `0`).
- **Exceptions**: None.

### GetExpectedCriticalCountForEmptyProject
- **Purpose**: Returns the integer value that represents the expected number of critical items for a project that contains no data.
- **Parameters**: None.
- **Return Value**: An `int` indicating the expected critical count (typically `0`).
- **Exceptions**: None.

### HasExpectedOverdueCount
- **Purpose**: Indicates whether the test scenario expects an overdue count to be present.
- **Parameters**: None.
- **Return Value**: `true` if an overdue count is expected; otherwise `false`.
- **Exceptions**: None.

### HasExpectedCriticalCount
- **Purpose**: Indicates whether the test scenario expects a critical count to be present.
- **Parameters**: None.
- **Return Value**: `true` if a critical count is expected; otherwise `false`.
- **Exceptions**: None.

## Usage

```csharp
// Arrange
var emptyProject = new Project { Id = Guid.Empty, Name = "Empty" };

// Act
var analytics = AnalyticsService.GetProjectAnalytics(emptyProject);

// Assert
Assert.AreEqual(AnalyticsServiceTestsExtensions.GetExpectedOverdueCountForEmptyProject,
                analytics.OverdueCount);
Assert.AreEqual(AnalyticsServiceTestsExtensions.GetExpectedCriticalCountForEmptyProject,
                analytics.CriticalCount);
```

```csharp
// Arrange
var projectWithData = TestDataGenerator.CreateProjectWithIssues();

// Act
var result = AnalyticsService.Evaluate(projectWithData);

// Assert
if (AnalyticsServiceTestsExtensions.HasExpectedOverdueCount)
{
    Assert.IsTrue(result.OverdueCount > 0);
}
if (AnalyticsServiceTestsExtensions.HasExpectedCriticalCount)
{
    Assert.IsTrue(result.CriticalCount > 0);
}
```

## Notes

- The returned values are constants intended for the *empty project* test case; using them with non‑empty projects will likely cause assertions to fail.
- Because the members are static and contain no mutable state, they are inherently thread‑safe and can be invoked concurrently from any number of test threads.
- The boolean helpers (`HasExpectedOverdueCount`, `HasExpectedCriticalCount`) should be set according to the specific test scenario; if they return `false`, the corresponding count assertions should be omitted.
