# TeamComparisonServiceTestsValidation

Validation suite for `TeamComparisonService` that verifies correct behavior of team comparison metrics and validations. This class contains static test methods that assert expected outcomes for team performance analysis, including team presence, metric calculations, and label validations.

## API

### `ShouldContainTeams`

Validates that the comparison result contains all expected teams. The method checks that the provided collection of team identifiers matches the teams present in the comparison result.

- **Parameters**
  - `comparisonResult`: The team comparison result to validate.
  - `expectedTeams`: Collection of team identifiers expected to be present.
- **Return value**: `void`
- **Throws**: `ArgumentNullException` if `comparisonResult` or `expectedTeams` is `null`.
- **Throws**: `InvalidOperationException` if any expected team is missing from the result.

### `ShouldIdentifyFastestTeam`

Ensures that the fastest team is correctly identified based on performance metrics such as cycle time or throughput. The validation compares the identified fastest team against the expected value.

- **Parameters**
  - `comparisonResult`: The team comparison result containing performance metrics.
  - `expectedFastestTeam`: The team identifier expected to be identified as the fastest.
- **Return value**: `void`
- **Throws**: `ArgumentNullException` if `comparisonResult` is `null`.
- **Throws**: `InvalidOperationException` if the fastest team cannot be determined or does not match `expectedFastestTeam`.

### `ShouldIdentifyHighestQualityTeam`

Verifies that the team with the highest quality metrics (e.g., lowest defect rate or highest code review approval rate) is correctly identified in the comparison result.

- **Parameters**
  - `comparisonResult`: The team comparison result containing quality metrics.
  - `expectedHighestQualityTeam`: The team identifier expected to be identified as having the highest quality.
- **Return value**: `void`
- **Throws**: `ArgumentNullException` if `comparisonResult` is `null`.
- **Throws**: `InvalidOperationException` if the highest quality team cannot be determined or does not match `expectedHighestQualityTeam`.

### `ShouldIdentifyMostConsistentTeam`

Checks that the team with the most consistent performance (e.g., lowest variance in cycle time or delivery rate) is correctly identified.

- **Parameters**
  - `comparisonResult`: The team comparison result containing consistency metrics.
  - `expectedMostConsistentTeam`: The team identifier expected to be identified as the most consistent.
- **Return value**: `void`
- **Throws**: `ArgumentNullException` if `comparisonResult` is `null`.
- **Throws**: `InvalidOperationException` if the most consistent team cannot be determined or does not match `expectedMostConsistentTeam`.

### `ShouldHaveMetrics`

Validates that the comparison result contains all expected performance metrics for each team. The method ensures that required metrics such as cycle time, throughput, and defect rate are present.

- **Parameters**
  - `comparisonResult`: The team comparison result to validate.
  - `expectedMetrics`: Collection of metric names expected to be present.
- **Return value**: `void`
- **Throws**: `ArgumentNullException` if `comparisonResult` or `expectedMetrics` is `null`.
- **Throws**: `InvalidOperationException` if any expected metric is missing for any team.

### `ShouldContainProjectKeys`

Ensures that the comparison result includes the correct project keys associated with each team. This validation checks that team-to-project mappings are accurate and complete.

- **Parameters**
  - `comparisonResult`: The team comparison result to validate.
  - `expectedProjectKeys`: Dictionary mapping team identifiers to expected project keys.
- **Return value**: `void`
- **Throws**: `ArgumentNullException` if `comparisonResult` or `expectedProjectKeys` is `null`.
- **Throws**: `InvalidOperationException` if any team's project key does not match the expected value.

### `ShouldContainPerformanceLabels`

Validates that each team in the comparison result is tagged with the correct performance labels (e.g., "Fast", "High Quality", "Consistent"). The method checks that labels are applied consistently and accurately.

- **Parameters**
  - `comparisonResult`: The team comparison result to validate.
  - `expectedLabels`: Dictionary mapping team identifiers to expected sets of performance labels.
- **Return value**: `void`
- **Throws**: `ArgumentNullException` if `comparisonResult` or `expectedLabels` is `null`.
- **Throws**: `InvalidOperationException` if any team's labels do not match the expected set.

## Usage
