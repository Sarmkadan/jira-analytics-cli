# TeamComparisonServiceTestsExtensions

Extension methods for testing `TeamComparisonService` behavior. These helpers streamline snapshot verification and result assertions in unit tests by encapsulating common comparison result checks and formatting logic.

## API

### `GetComparisonResultTextAsync`

Generates a formatted text representation of a team comparison result for snapshot or assertion purposes.

- **Parameters**
  - `comparisonResult` (`TeamComparisonResult`): The comparison result to format.
- **Return Value**
  - `Task<string>`: A task resolving to the formatted text representation of the comparison result.
- **Exceptions**
  - Throws `ArgumentNullException` if `comparisonResult` is `null`.

### `VerifyTeamComparisonSnapshot`

Verifies that the provided team comparison result matches an expected snapshot, using the test framework's snapshot assertion mechanism.

- **Parameters**
  - `comparisonResult` (`TeamCompletionResult`): The comparison result to verify.
  - `snapshotName` (`string`): The name of the snapshot to compare against.
- **Return Value**
  - `Task`: A task that completes when the verification is done.
- **Exceptions**
  - Throws `ArgumentNullException` if `comparisonResult` or `snapshotName` is `null`.
  - Throws `SnapshotVerificationException` if the comparison result does not match the snapshot.

### `VerifyFastestTeamIdentified`

Verifies that the comparison result identifies the expected fastest team and includes it in the output.

- **Parameters**
  - `comparisonResult` (`TeamComparisonResult`): The comparison result to verify.
  - `expectedTeamName` (`string`): The name of the team expected to be identified as fastest.
- **Return Value**
  - `Task`: A task that completes when the verification is done.
- **Exceptions**
  - Throws `ArgumentNullException` if `comparisonResult` or `expectedTeamName` is `null`.
  - Throws `VerificationException` if the fastest team in the result does not match `expectedTeamName`.

### `VerifyHighestQualityTeamIdentified`

Verifies that the comparison result identifies the expected highest-quality team and includes it in the output.

- **Parameters**
  - `comparisonResult` (`TeamComparisonResult`): The comparison result to verify.
  - `expectedTeamName` (`string`): The name of the team expected to be identified as highest quality.
- **Return Value**
  - `Task`: A task that completes when the verification is done.
- **Exceptions**
  - Throws `ArgumentNullException` if `comparisonResult` or `expectedTeamName` is `null`.
  - Throws `VerificationException` if the highest-quality team in the result does not match `expectedTeamName`.

## Usage
