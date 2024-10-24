# ReportService

Provides functionality to generate various types of reports from Jira data, including HTML, summary, and burndown charts. The service handles data retrieval, processing, and formatting into human-readable or chart-ready outputs.

## API

### `public ReportService`

Initializes a new instance of the `ReportService` class. The constructor prepares the service for report generation by setting up required dependencies such as data access and configuration.

### `public string GenerateReport`

Generates a default report based on the configured report type. This method selects the appropriate report generation logic internally and returns the report content as a string.

- **Returns**: A `string` containing the generated report content.
- **Throws**: `InvalidOperationException` if required dependencies are not initialized or if the report type is unsupported.

### `public async Task GenerateBurndownChart`

Asynchronously generates a burndown chart from sprint data and saves it to the specified file path.

- **Parameters**:
  - `sprintData`: The sprint data used to generate the burndown chart.
  - `outputPath`: The file path where the generated chart image will be saved.
- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**:
  - `ArgumentNullException` if `sprintData` or `outputPath` is `null`.
  - `IOException` if the file cannot be written to the specified path.
  - `InvalidOperationException` if the chart generation fails due to invalid data.

### `public string GenerateHtmlReport`

Generates an HTML-formatted report from the provided issue data.

- **Parameters**:
  - `issues`: A collection of Jira issues to include in the report.
- **Returns**: A `string` containing the HTML report content.
- **Throws**:
  - `ArgumentNullException` if `issues` is `null`.
  - `InvalidOperationException` if the HTML generation fails due to missing templates or configuration.

### `public string GenerateSummaryReport`

Generates a plain-text summary report from the provided issue data.

- **Parameters**:
  - `issues`: A collection of Jira issues to summarize.
- **Returns**: A `string` containing the summary report content.
- **Throws**:
  - `ArgumentNullException` if `issues` is `null`.
  - `InvalidOperationException` if the summary generation fails due to missing data or configuration.

## Usage

### Example 1: Generate a summary report from a list of issues
