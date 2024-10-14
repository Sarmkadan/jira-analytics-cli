# SprintMetricTests

Provides a suite of unit tests that verify the behavior of the `SprintMetric` class in the Jira Analytics CLI. Each test method focuses on a specific scenario—such as calculating completion rates, determining health status, aggregating risk scores, or validating date ranges—to ensure the implementation conforms to the expected contract.

## API

### `public void GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage`
- **Purpose**: Confirms that `SprintMetric.GetCompletionRate` returns the correct percentage when the sprint has a non‑zero number of planned story points.
- **Parameters**: None.
- **Return Value**: None (the test passes if the assertion succeeds; otherwise the test framework throws an exception).
- **Throws**: May throw an assertion‑failed exception from the test framework if the calculated percentage does not match the expected value.

### `public void GetCompletionRate_WithZeroPlannedPoints_ReturnsZero`
- **Purpose**: Verifies that `SprintMetric.GetCompletionRate` returns zero when the sprint’s planned story points are zero, avoiding a division‑by‑zero error.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an assertion‑failed exception if the method does not return zero.

### `public void GetHealthStatus_WithAllMetricsMeetingExcellentThresholds_ReturnsExcellent`
- **Purpose**: Ensures that `SprintMetric.GetHealthStatus` yields the `Excellent` health level when all measured metrics (completion rate, defect count, etc.) satisfy the excellent thresholds.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an assertion‑failed exception if the returned health status is not `Excellent`.

### `public void GetHealthStatus_WithLowCompletionRateAndHighDefects_ReturnsCritical`
- **Purpose**: Checks that `SprintMetric.GetHealthStatus` returns the `Critical` health level when the completion rate is low and defect count is high,Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an assertion‑failed exception if the health status is not `Critical`.

### `public void RiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors`
- **Purpose: a low completion rate combined with a high number of defects.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an assertion‑failed exception if the health status differs from `Critical`.

### `public void GetRiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors`
- **Purpose**: Validates that `SprintMetric.GetRiskScore` correctly aggregates individual risk factors (overdue issues, scope changes, etc.) into a single risk score.
- **Parameters**: None.
- **Return Value**: None.
- **ThrowsAllRiskFactors`
- **Purpose**: Assertions`
- **Purpose**: Asserts that `SprintMetric.GetRiskScore` correctly sums contributions from overdue items and scope changes to produce the expected total risk score.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an assertion‑failed exception if the aggregated score does not match the expected value.

### `public void Validate_WithEndDateBeforeStartDate_ThrowsArgumentException`
- **Purpose**: Confirms that calling `SprintMetric.Validate` with an end date that precedes the start date results in an `ArgumentException`.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: The test expects an `ArgumentException`; if the method does not throw, the test framework will fail.

## Usage

The following examples illustrate how the behavior verified by `SprintMetricTests` can be exercised in production code or in additional test scenarios.

```csharp
// Example 1: Verifying completion rate calculation
var metric = new SprintMetric
{
    PlannedStoryPoints = 120,
    CompletedStoryPoints = 90
};

double completionRate = metric.GetCompletionRate();
// Expected: 90 / 120 * 100 = 75.0
Assert.Equal(75.0, completionRate);
```

```csharp
// Example 2: Ensuring validation throws for invalid date range
var metric = new SprintMetric
{
    StartDate = new DateTime(2024, 10, 10),
    EndDate   = new DateTime(2024, 10, 01) // End before start
};

Assert.Throws<ArgumentException>(() => metric.Validate());
```

## Notes

- The test methods are stateless; each test creates its own instance of `SprintMetric` (or uses fixtures) to avoid cross‑test contamination.
- Because the methods return `void` and rely on assertions, they are safe to invoke concurrently in a test runner; however, the production `SprintMetric` class should be examined for thread‑safety if its members access shared static state.
- Edge cases covered by these tests include zero planned points (to prevent division by zero), inverted date ranges, and combinations of metrics that map to extreme health or risk classifications.
- No external resources (files, network, databases) are accessed by these tests, making them suitable for execution in any standard unit‑test environment without additional setup.
