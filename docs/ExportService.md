# ExportService

The `ExportService` provides asynchronous methods to export Jira analytics data in various formats and chart types. It is designed to generate reports for analytics, burndown charts, and team metrics, with support for JSON and CSV output formats.

## API

### `ExportAnalytics`

Exports general Jira analytics data. The exported data includes high-level metrics such as issue counts, resolution times, and sprint progress.

- **Parameters**:
  - `projectKey` (string): The Jira project key to filter analytics data.
  - `startDate` (DateTime): The start date for the analytics period.
  - `endDate` (DateTime): The end date for the analytics period.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `projectKey` is null.
  - Throws `ArgumentException` if `startDate` is after `endDate`.

### `ExportBurndownChart`

Generates a burndown chart for a specified sprint. The chart visualizes remaining work over time, typically used to track sprint progress.

- **Parameters**:
  - `sprintId` (string): The ID of the sprint to generate the burndown chart for.
  - `outputPath` (string): The file path where the chart image will be saved.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `sprintId` or `outputPath` is null.
  - Throws `FileNotFoundException` if the sprint with the given `sprintId` does not exist.

### `ExportTeamMetrics`

Exports team-specific metrics, such as velocity, cycle time, and throughput, for a given project and time period.

- **Parameters**:
  - `projectKey` (string): The Jira project key to filter team metrics.
  - `startDate` (DateTime): The start date for the metrics period.
  - `endDate` (DateTime): The end date for the metrics period.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `projectKey` is null.
  - Throws `ArgumentException` if `startDate` is after `endDate`.

### `ExportAsJson`

Exports the provided data model as a JSON file to the specified output path.

- **Parameters**:
  - `data` (object): The data model to serialize and export.
  - `outputPath` (string): The file path where the JSON file will be saved.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `data` or `outputPath` is null.
  - Throws `JsonException` if serialization of the data model fails.

### `ExportAsCsv`

Exports the provided data model as a CSV file to the specified output path.

- **Parameters**:
  - `data` (IEnumerable): The collection of data to serialize and export.
  - `outputPath` (string): The file path where the CSV file will be saved.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `data` or `outputPath` is null.
  - Throws `CsvHelperException` if serialization of the data model fails.

## Usage

### Example 1: Exporting analytics for a project
