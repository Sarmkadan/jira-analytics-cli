# AnalyticsServiceTests

Unit test suite for the `AnalyticsService` class, focusing on overdue issue analysis functionality. Contains test cases that verify correct counting and behavior when analyzing overdue issues in Jira projects, including edge cases like empty projects and mixed issue states.

## API

### `AnalyticsServiceTests`

Public class containing unit tests for `AnalyticsService` overdue issue analysis methods. Serves as the test fixture for verifying correct behavior when analyzing overdue issues in various project states.

### `AnalyzeOverdueIssues_WhenProjectHasNoIssues_ReturnsZeroCount`

Verifies that the `AnalyticsService.AnalyzeOverdueIssues` method returns a count of 0 when the project contains no issues. Ensures proper handling of empty project states without throwing exceptions.

**Parameters:**
None

**Return value:**
`Task` completing when the test assertion passes, indicating the method correctly returns 0 for empty projects.

**Exceptions:**
Does not throw under test conditions.

### `AnalyzeOverdueIssues_WithMixedIssues_CountsOnlyOverdueOnes`

Validates that `AnalyticsService.AnalyzeOverdueIssues` correctly identifies and counts only overdue issues when the project contains a mix of overdue, upcoming, and non-time-sensitive issues. Ensures filtering logic works as expected.

**Parameters:**
None

**Return value:**
`Task` completing when the test assertion passes, confirming only overdue issues are counted.

**Exceptions:**
Does not throw under test conditions.

### `AnalyzeOverdueIssues_WithHighPriorityOverdueIssues_CorrectlyCountsCritical`

Checks that `AnalyticsService.AnalyzeOverdueIssues` properly counts high-priority overdue issues as part of the total count. Ensures priority-based filtering does not interfere with overdue status detection.

**Parameters:**
None

**Return value:**
`Task` completing when the test assertion passes, confirming high-priority overdue issues are included in the count.

**Exceptions:**
Does not throw under test conditions.

## Usage
