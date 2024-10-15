# TeamComparisonServiceTests

`TeamComparisonServiceTests` is a test class within the `jira-analytics-cli` project that validates the functionality of a team comparison service. This suite ensures the service correctly aggregates, compares, and reports performance metrics across multiple Jira projects or teams, including sprint velocity, defect rates, and project key deduplication.

## API

### `public TeamComparisonServiceTests`
Constructor for the test class. Initializes any required test dependencies or mocks. No parameters or return value.

---

### `public async Task CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots`
**Purpose**: Verifies that the comparison service returns snapshots for both projects when provided with two distinct project keys.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `public async Task CompareTeamsAsync_IdentifiesFastestTeamCorrectly`
**Purpose**: Ensures the service accurately identifies the team with the highest sprint velocity (fastest team) based on the aggregated data.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `public async Task CompareTeamsAsync_IdentifiesHighestQualityTeamByLowestDefectRate`
**Purpose**: Validates that the service correctly determines the team with the highest quality (lowest defect rate) from the provided project data.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `public async Task CompareTeamsAsync_DeduplicatesProjectKeys`
**Purpose**: Confirms that the service deduplicates project keys when duplicate keys are provided, ensuring each project is only processed once.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `public async Task CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException`
**Purpose**: Tests that the service throws an `ArgumentException` when an empty collection of project keys is provided.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: `ArgumentException` if the project keys collection is empty.

---

### `public async Task CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException`
**Purpose**: Ensures the service throws an `ArgumentOutOfRangeException` when the sprint count parameter is zero or negative.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: `ArgumentOutOfRangeException` if the sprint count is zero or negative.

---

### `public void FormatAsText_WithTwoTeams_ContainsBothProjectKeys`
**Purpose**: Validates that the text-formatted output of the comparison service includes both project keys when comparing two teams.
**Parameters**: None.
**Return Value**: None.
**Throws**: None.

## Usage

### Example 1: Comparing Two Teams
