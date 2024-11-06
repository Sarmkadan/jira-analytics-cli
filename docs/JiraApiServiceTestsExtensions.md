# JiraApiServiceTestsExtensions

Extension methods for testing Jira API service responses, providing utility functions to validate common error conditions and response structures.

## API

### `IsUnauthorized`
Determines if a response indicates an unauthorized access attempt.

- **Return value**: `true` if the response status code is 401 (Unauthorized); otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `IsServerError`
Determines if a response indicates a server error.

- **Return value**: `true` if the response status code is in the 5xx range; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `HasValidProjectResponse`
Validates whether a project-related API response is structurally valid.

- **Return value**: `true` if the response contains a non-null project key and a non-empty project name; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `HasEmptySprintIssuesResponse`
Checks if a sprint issues response contains no issues.

- **Return value**: `true` if the response contains an empty list of issues; otherwise, `false`.
- **Throws**: Does not throw exceptions.

## Usage
