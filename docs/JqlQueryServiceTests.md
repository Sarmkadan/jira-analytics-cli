# JqlQueryServiceTests

Test suite for the `JqlQueryService` component, verifying correct mapping of Jira query results, error handling, and formatting behavior.

## API

### `JqlQueryServiceTests()`
Default constructor. Creates an instance of the test class. No parameters. Returns a new `JqlQueryServiceTests` object. Does not throw under normal circumstances.

### `ExecuteQueryAsync_WhenJiraReturnsIssues_MapsResultCorrectly()`
Asynchronous test method. Validates that when the mocked Jira client returns a collection of issues, the service maps the response to a successful result containing the expected issue data. Takes no parameters. Returns a `Task` representing the asynchronous operation. Throws an exception only if the test assertion fails (e.g., `AssertFailedException` or similar) indicating a mismatch between expected and actual mapping.

### `ExecuteQueryAsync_WhenJiraReturnsEmpty_ReturnsSuccessWithNoIssues()`
Asynchronous test method. Ensures that an empty issue collection from Jira yields a successful result with zero issues. No parameters. Returns a `Task`. Throws only on test failure, indicating that the service did not treat an empty response as a successful empty result.

### `ExecuteQueryAsync_WhenJiraThrows_ReturnsFailureResult()`
Asynchronous test method. Confirms that when the Jira client throws an exception, the service returns a failure result encapsulating the error. No parameters. Returns a `Task`. Throws only if the test's assertions do not hold (e.g., the service did not produce a failure result).

### `ExecuteQueryAsync_WithEmptyJql_ThrowsArgumentException()`
Asynchronous test method. Checks that supplying an empty or whitespace‑only JQL string causes the service to throw an `ArgumentException`. No parameters. Returns a `Task`. Throws only when the test fails to observe the expected exception.

### `ExecuteQueryAsync_WithPaginationRequest_PassesStartAtAndMaxResults()`
Asynchronous test method. Verifies that when a pagination request is supplied, the service forwards the correct `startAt` and `maxResults` values to the underlying Jira client. No parameters. Returns a `Task`. Throws only if the test asserts that the parameters were not passed correctly.

### `FormatAsText_WithSuccessfulResult_ContainsIssueKeys()`
Synchronous test method. Asserts that formatting a successful query result produces a plain‑text representation that includes the keys of the returned issues. No parameters. Returns `void`. Throws only on assertion failure, indicating missing or incorrect issue keys in the formatted output.

### `FormatAsText_WithFailedResult_ReturnsErrorMessage()`
Synchronous test method. Confirms that formatting a failed query result yields a string containing the error message from the failure. No parameters. Returns `void`. Throws only if the test's expectations are not met (e.g., error message absent or malformed).

## Usage

```csharp
using System.Threading.Tasks;
using Xunit;

public class ExampleTests
{
    [Fact]
    public async Task RunsSuccessfulQueryTest()
    {
        var testSuite = new JqlQueryServiceTests();
        await testSuite.ExecuteQueryAsync_WhenJiraReturnsIssues_MapsResultCorrectly();
        // Test passes if no exception is thrown.
    }

    [Fact]
    public void ChecksFormattingOfFailedResult()
    {
        var testSuite = new JqlQueryServiceTests();
        testSuite.FormatAsText_WithFailedResult_ReturnsErrorMessage();
        // Test passes if the formatted output contains the expected error message.
    }
}
```

```csharp
using System.Threading.Tasks;

public class ManualInvocationExample
{
    public async Task ExecuteAllTests()
    {
        var suite = new JqlQueryServiceTests();

        await suite.ExecuteQueryAsync_WhenJiraReturnsEmpty_ReturnsSuccessWithNoIssues();
        await suite.ExecuteQueryAsync_WithPaginationRequest_PassesStartAtAndMaxResults();
        await suite.ExecuteQueryAsync_WithEmptyJql_ThrowsArgumentException();

        // Synchronous tests can be called directly.
        suite.FormatAsText_WithSuccessfulResult_ContainsIssueKeys();
        suite.FormatAsText_WithFailedResult_ReturnsErrorMessage();
    }
}
```

## Notes

- The test class contains no mutable state; each method relies solely on its own mocks and assertions. Consequently, instances are thread‑safe for concurrent invocation, though the typical usage pattern is sequential execution via a test runner.
- Methods that validate exception behavior (`WithEmptyJql_ThrowsArgumentException` and `WhenJiraThrows_ReturnsFailureResult`) assume the service under test throws the specified exception type; any deviation will cause the test to fail.
- Empty JQL strings are defined as `null`, `""`, or strings consisting only of whitespace; the service is expected to treat all as invalid input.
- When Jira returns a non‑empty collection, the test verifies that each issue’s key, summary, and other relevant fields are correctly present in the mapped result.
- The formatting tests (`FormatAsText_*`) examine only the textual output; they do not assert any particular layout beyond the presence of expected substrings.
- No asynchronous deadlocks or race conditions are introduced by the test methods themselves; any concurrency concerns would reside in the production `JqlQueryService` implementation, not in this test suite.
