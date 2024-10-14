# CsvFormatterBenchmarksExtensions

Utility class providing CSV formatting and parsing operations for benchmarking Jira sprint metrics. Designed to convert collections of `SprintMetric` objects into structured CSV strings and parse grouped metric data back into typed dictionaries.

## API

### `FormatWithHeaders`

Formats a collection of `SprintMetric` objects into a CSV string with headers included.

- **Parameters**
  - `metrics` (`IEnumerable<SprintMetric>`): The collection of sprint metrics to format.
- **Return Value**
  - `string`: A CSV-formatted string with headers and data rows.
- **Exceptions**
  - Throws `ArgumentNullException` if `metrics` is `null`.

### `ParseGroupedBySprint`

Parses a CSV string into a dictionary grouped by sprint names, where each sprint maps to a list of its metrics.

- **Parameters**
  - `csv` (`string`): The CSV string to parse.
- **Return Value**
  - `Dictionary<string, List<SprintMetric>>`: A dictionary with sprint names as keys and lists of `SprintMetric` objects as values.
- **Exceptions**
  - Throws `ArgumentNullException` if `csv` is `null`.
  - Throws `FormatException` if the CSV is malformed or contains invalid data.

### `ValidateMetricsIntegrity`

Validates the integrity of a collection of `SprintMetric` objects by checking required fields and consistency.

- **Parameters**
  - `metrics` (`IEnumerable<SprintMetric>`): The collection of metrics to validate.
- **Return Value**
  - `bool`: `true` if all metrics are valid; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `metrics` is `null`.

### `FormatWithCalculations`

Formats a collection of `SprintMetric` objects into a CSV string with additional calculated fields appended.

- **Parameters**
  - `metrics` (`IEnumerable<SprintMetric>`): The collection of sprint metrics to format.
- **Return Value**
  - `string`: A CSV-formatted string with headers, data rows, and calculated fields.
- **Exceptions**
  - Throws `ArgumentNullException` if `metrics` is `null`.

## Usage
