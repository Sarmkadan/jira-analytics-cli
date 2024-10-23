# JqlQueryService

Provides functionality to execute JQL (Jira Query Language) queries against a Jira instance and retrieve structured results. The service handles asynchronous execution, query formatting, and result parsing for integration with analytics and reporting workflows.

## API

### `JqlQueryService`

The primary service class responsible for executing JQL queries. This class is designed for dependency injection and supports asynchronous operations.

### `public Task<JqlQueryResult> ExecuteQueryAsync(string jql)`

Executes a JQL query against the configured Jira instance and returns the results asynchronously.

- **Parameters**:
  - `jql` (string): The JQL query string to execute. Must not be null or empty.
- **Return value**: A `Task<JqlQueryResult>` that resolves to the query results upon completion.
- **Exceptions**:
  - Throws `ArgumentException` if the provided `jql` is null or whitespace.
  - Throws `InvalidOperationException` if the service is not properly configured or the Jira connection is unavailable.
  - Throws `JiraApiException` if the Jira API returns an error response.

### `public async Task<JqlQueryResult> ExecuteQueryAsync(string jql, CancellationToken cancellationToken)`

Executes a JQL query with support for cancellation and asynchronous streaming of results.

- **Parameters**:
  - `jql` (string): The JQL query string to execute. Must not be null or empty.
  - `cancellationToken` (CancellationToken): A token to monitor for cancellation requests.
- **Return value**: A `Task<JqlQueryResult>` that resolves to the query results upon completion or cancellation.
- **Exceptions**:
  - Throws `ArgumentException` if the provided `jql` is null or whitespace.
  - Throws `OperationCanceledException` if the operation is canceled via the `cancellationToken`.
  - Throws `InvalidOperationException` if the service is not properly configured or the Jira connection is unavailable.
  - Throws `JiraApiException` if the Jira API returns an error response.

### `public static string FormatAsText(string jql)`

Formats a JQL query string into a human-readable text representation. Useful for logging or user-facing output.

- **Parameters**:
  - `jql` (string): The JQL query string to format. Must not be null or empty.
- **Return value**: A formatted string representation of the JQL query.
- **Exceptions**:
  - Throws `ArgumentException` if the provided `jql` is null or whitespace.

## Usage
