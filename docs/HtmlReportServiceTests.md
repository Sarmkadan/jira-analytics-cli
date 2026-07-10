# HtmlReportServiceTests

The `HtmlReportServiceTests` class serves as the dedicated test suite for validating the behavior of the HTML report generation logic within the `jira-analytics-cli` application. It verifies that the service correctly constructs HTML documents using sprint data, handles edge cases such as empty datasets or missing sprints, ensures security by escaping special characters to prevent Cross-Site Scripting (XSS) vulnerabilities, and manages file I/O operations asynchronously. Additionally, it confirms that invalid input parameters result in the appropriate exceptions being thrown.

## API

### `public HtmlReportServiceTests()`
Initializes a new instance of the `HtmlReportServiceTests` class. This constructor prepares the test context required to execute the validation methods defined in this class. It accepts no parameters and does not return a value.

### `public void BuildHtml_WithSprintData_ContainsProjectKeyInTitle()`
Validates that when sprint data is provided, the generated HTML document includes the specific project key within the document title element.
*   **Parameters**: None (relies on test context setup).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion failure if the project key is missing from the title.

### `public void BuildHtml_WithXssCharsInProjectKey_EscapesHtml()`
Ensures that special characters (such as `<`, `>`, `&`, `"`, `'`) present in the project key are properly HTML-encoded in the output to prevent XSS attacks.
*   **Parameters**: None (relies on test context setup with malicious input strings).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion failure if the output contains unescaped special characters.

### `public void BuildHtml_WithNoSprints_StillProducesValidDocument()`
Verifies that the service generates a well-formed HTML document even when the input collection contains no sprint data.
*   **Parameters**: None (relies on test context setup with an empty sprint list).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion failure if the resulting document is malformed or null.

### `public void BuildHtml_WithTopPerformers_IncludesPerformerTable()`
Confirms that when top performer data is available, the generated HTML includes a specific table section detailing these performers.
*   **Parameters**: None (relies on test context setup with performer data).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion failure if the performer table is absent from the output.

### `public async Task GenerateReportAsync_WithInvalidSprintCount_ThrowsArgumentOutOfRangeException()`
Asynchronously verifies that the report generation method throws an `ArgumentOutOfRangeException` when provided with an invalid number of sprints (e.g., a negative count).
*   **Parameters**: None (relies on test context setup with invalid integer values).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion failure if the expected `ArgumentOutOfRangeException` is not raised.

### `public async Task GenerateReportAsync_WritesFileWithHtmlContent()`
Asynchronously validates that the report generation method successfully writes the constructed HTML content to the specified file path.
*   **Parameters**: None (relies on test context setup with a temporary file path).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion failure if the file is not created or does not contain the expected HTML content.

## Usage

The following examples demonstrate how the `HtmlReportServiceTests` class might be utilized within a test runner framework like xUnit or NUnit to verify specific behaviors.

**Example 1: Verifying HTML Escaping**
This test ensures that if a project key contains script tags, they are rendered as text rather than executable code.

```csharp
[TestFixture]
public class SecurityValidation
{
    [Test]
    public void ValidateXssProtection()
    {
        var tests = new HtmlReportServiceTests();
        // The test method internally sets up a project key like "<script>alert('x')</script>"
        // and asserts that the output contains "&lt;script&gt;" instead.
        tests.BuildHtml_WithXssCharsInProjectKey_EscapesHtml();
    }
}
```

**Example 2: Verifying Asynchronous File Generation**
This test confirms that the service correctly handles file I/O and awaits the completion of the write operation.

```csharp
[TestFixture]
public class IoValidation
{
    [Test]
    public async Task ValidateFileWriteOperation()
    {
        var tests = new HtmlReportServiceTests();
        // The test method internally configures a valid sprint count and a temp path,
        // then awaits the file write completion.
        await tests.GenerateReportAsync_WritesFileWithHtmlContent();
        
        // Additional assertions on the file system could follow here
    }
}
```

## Notes

*   **Thread Safety**: As this class represents a test suite, its methods are designed to be executed in isolation by a test runner. While the individual test methods do not share mutable static state, the underlying `HtmlReportService` being tested should be instantiated fresh for each test case to ensure thread safety during parallel test execution.
*   **Edge Cases**: The suite explicitly covers the edge case of empty data sets (`BuildHtml_WithNoSprints_StillProducesValidDocument`), ensuring the system does not crash or produce null references when no sprint data is available. It also covers security edge cases regarding input sanitization.
*   **Asynchronous Behavior**: Two members (`GenerateReportAsync_...`) return `Task` objects, indicating that the underlying operations involve I/O or network latency. Test runners consuming this class must await these tasks to avoid premature test completion.
*   **Exception Handling**: The `GenerateReportAsync_WithInvalidSprintCount_ThrowsArgumentOutOfRangeException` method specifically validates boundary conditions for integer inputs, ensuring that negative or zero values where positive integers are expected are rejected immediately.
