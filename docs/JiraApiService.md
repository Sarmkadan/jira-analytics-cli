# JiraApiService

The `JiraApiService` class serves as the primary interface for interacting with the Jira REST API within the `jira-analytics-cli` application. It encapsulates all asynchronous network operations required to retrieve project metadata, sprint details, issue lists, team information, and burndown data, providing a unified abstraction layer that handles serialization and HTTP communication for analytics workflows.

## API

### `JiraApiService`
Initializes a new instance of the `JiraApiService` class. This constructor typically configures the underlying HTTP client and authentication mechanisms required to communicate with the target Jira instance.

### `GetProjectAsync`
Retrieves metadata for a specific Jira project.
*   **Parameters**: Accepts a project key or identifier (specific parameter names depend on implementation context, typically a string key).
*   **Returns**: `Task<JiraProject?>`. Returns a `JiraProject` object if the project exists; otherwise, returns `null`.
*   **Throws**: Throws network-related exceptions if the connection fails or the API returns an unexpected error status.

### `GetProjectSprintsAsync`
Fetches the list of sprints associated with a specific project.
*   **Parameters**: Accepts a project identifier.
*   **Returns**: `Task<List<Sprint>>`. Returns a list of `Sprint` objects. The list may be empty if no sprints are found, but the task itself does not return `null`.
*   **Throws**: Throws if the project ID is invalid or if the API request fails.

### `GetSprintAsync`
Retrieves detailed information for a specific sprint.
*   **Parameters**: Accepts a sprint ID.
*   **Returns**: `Task<Sprint?>`. Returns a `Sprint` object if found; otherwise, returns `null`.
*   **Throws**: Throws on communication errors or if the sprint ID format is invalid.

### `GetSprintIssuesAsync`
Retrieves all issues contained within a specific sprint.
*   **Parameters**: Accepts a sprint ID.
*   **Returns**: `Task<List<JiraIssue>>`. Returns a list of `JiraIssue` objects. Returns an empty list if the sprint contains no issues.
*   **Throws**: Throws if the sprint does not exist or if the API query fails.

### `GetProjectIssuesAsync`
Fetches all issues associated with a specific project.
*   **Parameters**: Accepts a project identifier.
*   **Returns**: `Task<List<JiraIssue>>`. Returns a comprehensive list of `JiraIssue` objects for the project.
*   **Throws**: Throws if the project is not found or if the volume of data causes a timeout or API error.

### `GetProjectTeamAsync`
Retrieves the list of developers or users associated with a specific project.
*   **Parameters**: Accepts a project identifier.
*   **Returns**: `Task<List<Developer>>`. Returns a list of `Developer` objects representing the project team.
*   **Throws**: Throws if the project permissions prevent accessing user data or if the request fails.

### `GetIssueAsync`
Retrieves detailed data for a single issue by its key.
*   **Parameters**: Accepts an issue key (e.g., "PROJ-123").
*   **Returns**: `Task<JiraIssue?>`. Returns a `JiraIssue` object if the key exists; otherwise, returns `null`.
*   **Throws**: Throws on network failures or if the issue key format is malformed.

### `GetBurndownDataAsync`
Calculates or retrieves historical burndown snapshots for a specific sprint or project context.
*   **Parameters**: Accepts context identifiers (typically sprint or project IDs).
*   **Returns**: `Task<List<BurndownSnapshot>>`. Returns a chronological list of `BurndownSnapshot` objects.
*   **Throws**: Throws if insufficient data exists to generate the report or if the API calculation endpoint fails.

### `SearchByJqlAsync`
Executes a custom Jira Query Language (JQL) search.
*   **Parameters**: Accepts a JQL query string.
*   **Returns**: `Task<JiraSearchResult>`. Returns a `JiraSearchResult` object containing matched issues and pagination metadata.
*   **Throws**: Throws if the JQL syntax is invalid or if the query exceeds API limits.

### `VerifyConnectionAsync`
Validates the connectivity and authentication credentials against the Jira instance.
*   **Parameters**: None.
*   **Returns**: `Task<bool>`. Returns `true` if the connection is successful and credentials are valid; otherwise, returns `false`.
*   **Throws**: Generally does not throw for authentication failures (returns `false`), but may throw on critical network configuration errors.

## Usage

### Example 1: Retrieving Project Metrics
The following example demonstrates how to initialize the service, verify connectivity, and aggregate sprint issues for a specific project.

```csharp
var jiraService = new JiraApiService();
var isConnected = await jiraService.VerifyConnectionAsync();

if (isConnected)
{
    var projectKey = "ANALYTICS";
    var sprints = await jiraService.GetProjectSprintsAsync(projectKey);
    
    foreach (var sprint in sprints)
    {
        var issues = await jiraService.GetSprintIssuesAsync(sprint.Id);
        Console.WriteLine($"Sprint {sprint.Name}: {issues.Count} issues found.");
    }
}
else
{
    Console.Error.WriteLine("Failed to connect to Jira instance.");
}
```

### Example 2: Custom JQL Search and Issue Detail Fetching
This example performs a custom search for high-priority bugs and fetches full details for the first result.

```csharp
var jiraService = new JiraApiService();
var jqlQuery = "project = 'CORE' AND type = Bug AND priority = Highest";

var searchResult = await jiraService.SearchByJqlAsync(jqlQuery);

if (searchResult.Issues.Any())
{
    var firstIssueKey = searchResult.Issues.First().Key;
    var detailedIssue = await jiraService.GetIssueAsync(firstIssueKey);

    if (detailedIssue != null)
    {
        Console.WriteLine($"Detailed status for {firstIssueKey}: {detailedIssue.Status}");
    }
}
```

## Notes

*   **Null Handling**: Several methods (`GetProjectAsync`, `GetSprintAsync`, `GetIssueAsync`) return `null` to indicate that a specific resource was not found rather than throwing an exception. Callers must explicitly handle these null returns to avoid `NullReferenceException`.
*   **Empty Collections**: Methods returning lists (`GetProjectSprintsAsync`, `GetSprintIssuesAsync`, etc.) will return an empty `List<T>` rather than `null` when no data is available.
*   **Thread Safety**: As an `async`-based service relying on underlying HTTP clients, `JiraApiService` instances should generally be treated as stateless regarding request execution. However, if the internal `HttpClient` is not statically managed or injected, care should be taken not to dispose of the service while pending tasks are executing. Multiple concurrent calls on the same instance are supported provided the underlying network configuration allows it.
*   **Rate Limiting**: The methods do not internally implement retry logic for HTTP 429 (Too Many Requests) responses. Consumers implementing bulk data retrieval (e.g., iterating through many sprints) should implement external throttling or retry policies.
*   **JQL Validation**: `SearchByJqlAsync` relies on the server-side validation of the JQL string. Invalid syntax will result in an exception rather than an empty result set.
