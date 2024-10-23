# IJqlQueryService

The `IJqlQueryService` interface defines the contract for executing JQL (Jira Query Language) queries against a Jira instance, returning paginated results and metadata about the query execution.

## API

### `Jql`
- **Purpose**: Gets or sets the JQL query string to be executed.
- **Type**: `string`
- **Remarks**: Must be a valid JQL query. No validation is performed by this interface; invalid queries may result in runtime errors when executed.

### `MaxResults`
- **Purpose**: Gets or sets the maximum number of issues to return per page.
- **Type**: `int`
- **Remarks**: Must be a non-negative integer. Values greater than the Jira server's configured limit may be truncated.

### `StartAt`
- **Purpose**: Gets or sets the zero-based index of the first issue to return.
- **Type**: `int`
- **Remarks**: Used for pagination. Must be a non-negative integer. Values beyond the total available issues may return an empty list.

### `Label`
- **Purpose**: Gets or sets an optional label associated with the query for tracking or categorization.
- **Type**: `string?`
- **Remarks**: May be `null` or an empty string. Intended for client-side use; not used by the Jira server.

### `Total`
- **Purpose**: Gets the total number of issues matching the JQL query across all pages.
- **Type**: `int`
- **Remarks**: Read-only. Populated after query execution. May be zero if no issues match.

### `Issues`
- **Purpose**: Gets the list of issues returned by the current page of the query.
- **Type**: `List<JiraIssue>`
- **Remarks**: Read-only. Empty if no issues match the current page or if the query failed.

### `ExecutedAt`
- **Purpose**: Gets the timestamp when the query was last executed.
- **Type**: `DateTime`
- **Remarks**: Read-only. Populated after query execution. Represents server or client time depending on implementation.

### `IsSuccess`
- **Purpose**: Gets a value indicating whether the query executed successfully.
- **Type**: `bool`
- **Remarks**: Read-only. `true` if the query completed without error; `false` otherwise.

### `ErrorMessage`
- **Purpose**: Gets the error message if the query failed.
- **Type**: `string?`
- **Remarks**: Read-only. `null` if `IsSuccess` is `true`. Contains server or client-side error details if applicable.

## Usage

### Example 1: Basic Query Execution
