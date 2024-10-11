# JiraApiException

Represents an exception thrown when interacting with the Jira REST API fails. This exception encapsulates the HTTP status code and response content from the failed API call, allowing consumers to handle API-specific errors programmatically.

## API

### `public int? StatusCode`

The HTTP status code returned by the Jira API, if available. This property will be `null` if the exception was raised before an HTTP response was received (e.g., during request preparation or network failure).

### `public string? ResponseContent`

The raw response content returned by the Jira API, if available. This property will be `null` if the exception was raised before an HTTP response was received or if the response had no content.

### `public JiraApiException()`

Initializes a new instance of the `JiraApiException` class with default values. Neither `StatusCode` nor `ResponseContent` will be populated.

### `public JiraApiException(string message)`

Initializes a new instance of the `JiraApiException` class with a specified error message.

**Parameters:**
- `message`: A string describing the error.

### `public JiraApiException(string message, Exception innerException)`

Initializes a new instance of the `JiraApiException` class with a specified error message and a reference to the inner exception that caused this exception.

**Parameters:**
- `message`: A string describing the error.
- `innerException`: The exception that caused the current exception.

### `public override string ToString()`

Returns a string representation of the `JiraApiException`, including the `StatusCode` and `ResponseContent` if available.

**Returns:**
A string that represents the current exception, formatted as:
