# JiraIssueTests

A test class containing unit tests for `JiraIssue` extension methods that evaluate issue state and metrics, such as overdue status, priority classification, and cycle time calculation.

## API

### `IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue`
Verifies that an issue is correctly identified as overdue when its due date is in the past and its status is still open.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions are thrown under valid test conditions.

### `IsOverdue_WhenStatusIsDone_ReturnsFalse`
Ensures that an issue marked as done is never considered overdue, regardless of due date.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions are thrown under valid test conditions.

### `IsOverdue_WhenResolutionDateIsSet_ReturnsFalse`
Checks that an issue with a resolution date set is not marked as overdue, even if the due date has passed.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions are thrown under valid test conditions.

### `IsHighPriority_WithCriticalPriority_ReturnsTrue`
Validates that an issue with "Critical" priority is correctly classified as high priority.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions are thrown under valid test conditions.

### `IsHighPriority_WithMediumPriority_ReturnsFalse`
Confirms that an issue with "Medium" priority is not classified as high priority.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions are thrown under valid test conditions.

### `GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution`
Tests that the cycle time (in days) between issue creation and resolution is accurately computed when a resolution date is available.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions are thrown under valid test conditions.

## Usage
