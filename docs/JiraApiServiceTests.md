# JiraApiServiceTests
The `JiraApiServiceTests` class is a collection of unit tests designed to validate the functionality of the `JiraApiService` class. These tests cover various scenarios, including successful and failed API requests, error handling, and edge cases. The purpose of this class is to ensure that the `JiraApiService` behaves as expected under different conditions, providing a reliable and robust integration with the Jira API.

## API
The following members are part of the `JiraApiServiceTests` class:
* `GetProjectAsync_Returns_Null_On_401_Unauthorized`: Tests that the `GetProjectAsync` method returns null when the API request returns a 401 Unauthorized response.
* `GetProjectAsync_Returns_Null_On_403_Forbidden`: Tests that the `GetProjectAsync` method returns null when the API request returns a 403 Forbidden response.
* `GetProjectAsync_Returns_Null_On_429_TooManyRequests`: Tests that the `GetProjectAsync` method returns null when the API request returns a 429 Too Many Requests response.
* `GetProjectAsync_Returns_Null_On_500_InternalServerError`: Tests that the `GetProjectAsync` method returns null when the API request returns a 500 Internal Server Error response.
* `GetSprintIssuesAsync_Returns_Empty_On_401_Unauthorized`: Tests that the `GetSprintIssuesAsync` method returns an empty result when the API request returns a 401 Unauthorized response.
* `GetSprintIssuesAsync_Returns_Empty_On_500_InternalServerError`: Tests that the `GetSprintIssuesAsync` method returns an empty result when the API request returns a 500 Internal Server Error response.
* `GetProjectSprintsAsync_Returns_Empty_On_403_Forbidden`: Tests that the `GetProjectSprintsAsync` method returns an empty result when the API request returns a 403 Forbidden response.
* `VerifyConnectionAsync_Returns_False_On_401_Unauthorized`: Tests that the `VerifyConnectionAsync` method returns false when the API request returns a 401 Unauthorized response.
* `VerifyConnectionAsync_Returns_True_On_200_OK`: Tests that the `VerifyConnectionAsync` method returns true when the API request returns a 200 OK response.
* `GetProjectAsync_Returns_Project_On_200_With_Valid_Json`: Tests that the `GetProjectAsync` method returns a project object when the API request returns a 200 OK response with valid JSON.
* `GetSprintIssuesAsync_Returns_Empty_When_Issues_Array_Missing`: Tests that the `GetSprintIssuesAsync` method returns an empty result when the issues array is missing from the response.
* `GetProjectSprintsAsync_Returns_Empty_When_Values_Array_Is_Empty`: Tests that the `GetProjectSprintsAsync` method returns an empty result when the values array is empty in the response.
* `GetSprintIssuesAsync_Parses_Multiple_Issues_Correctly`: Tests that the `GetSprintIssuesAsync` method correctly parses multiple issues from the response.
* `GetProjectSprintsAsync_Handles_Empty_Values_Array_As_Last_Page`: Tests that the `GetProjectSprintsAsync` method handles an empty values array as the last page of results.
* `GetSprintIssuesAsync_Returns_Empty_When_Issues_Array_Is_Empty`: Tests that the `GetSprintIssuesAsync` method returns an empty result when the issues array is empty in the response.
* `GetProjectAsync_Returns_Null_When_Request_Times_Out`: Tests that the `GetProjectAsync` method returns null when the API request times out.
* `GetSprintIssuesAsync_Returns_Empty_When_Request_Times_Out`: Tests that the `GetSprintIssuesAsync` method returns an empty result when the API request times out.
* `VerifyConnectionAsync_Returns_False_When_Request_Times_Out`: Tests that the `VerifyConnectionAsync` method returns false when the API request times out.
* `GetProjectAsync_Sends_Bearer_Authorization_Header`: Tests that the `GetProjectAsync` method sends a Bearer authorization header with the API request.
* `VerifyConnectionAsync_Sends_Accept_Json_Header`: Tests that the `VerifyConnectionAsync` method sends an Accept JSON header with the API request.

## Usage
Here are two examples of using the `JiraApiServiceTests` class:
```csharp
// Example 1: Testing the GetProjectAsync method
var jiraApiService = new JiraApiService();
var project = await jiraApiService.GetProjectAsync("projectKey");
Assert.IsNotNull(project);

// Example 2: Testing the VerifyConnectionAsync method
var isConnected = await jiraApiService.VerifyConnectionAsync();
Assert.IsTrue(isConnected);
```

## Notes
The `JiraApiServiceTests` class is designed to be thread-safe, as each test method is independent and does not share state with other tests. However, it is recommended to run these tests in isolation to avoid any potential conflicts. Additionally, the tests in this class assume that the `JiraApiService` class is properly configured and authenticated before running the tests. Edge cases, such as network errors or invalid API responses, are handled by the tests to ensure that the `JiraApiService` class behaves as expected under different conditions.
