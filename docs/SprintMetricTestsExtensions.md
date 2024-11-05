# SprintMetricTestsExtensions

`SprintMetricTestsExtensions` is a static utility class that provides a set of executable test methods for validating sprint metric calculations within the `jira-analytics-cli` project. It exposes individual boolean-returning test runners for specific scenarios—such as completion rate computation with and without planned points, and input validation—as well as a dictionary-based aggregator that executes all tests and maps each test name to its pass/fail result.

## API

### `RunGetCompletionRate_WithNonZeroPlannedPoints`
```csharp
public static bool RunGetCompletionRate_WithNonZeroPlannedPoints
```
Executes a test that verifies the completion rate calculation when the sprint has a non-zero amount of planned story points. The method exercises the underlying `SprintMetric` logic with a known set of completed and planned points and returns `true` if the computed rate matches the expected value.

**Parameters:** None (static method).

**Return value:** `bool` — `true` if the completion rate matches the expected ratio for the given non-zero planned points; otherwise `false`.

**Throws:** No documented exceptions. Failures are reported via the return value.

### `RunGetCompletionRate_WithZeroPlannedPoints`
```csharp
public static bool RunGetCompletionRate_WithZeroPlannedPoints
```
Executes a test that verifies the completion rate calculation when the sprint has zero planned story points. This scenario typically guards against division-by-zero and ensures the implementation returns a sensible result (e.g., zero or a defined constant) rather than throwing or producing an invalid value.

**Parameters:** None (static method).

**Return value:** `bool` — `true` if the completion rate for zero planned points is handled correctly; otherwise `false`.

**Throws:** No documented exceptions. Failures are reported via the return value.

### `RunValidate_WithEndDateBeforeStartDate_ThrowsArgumentException`
```csharp
public static bool RunValidate_WithEndDateBeforeStartDate_ThrowsArgumentException
```
Executes a test that confirms the sprint validation logic correctly rejects a sprint whose end date precedes its start date. The method invokes the validation routine with such an invalid date range and expects an `ArgumentException` to be thrown.

**Parameters:** None (static method).

**Return value:** `bool` — `true` if an `ArgumentException` is thrown as expected when the end date is before the start date; `false` if no exception is thrown or a different exception type is raised.

**Throws:** No documented exceptions. The test itself does not throw; it catches the expected exception internally and returns a boolean result.

### `RunAll`
```csharp
public static IDictionary<string, bool> RunAll
```
A property (or parameterless static method) that executes all test methods defined in this class and returns a dictionary mapping each test’s descriptive name to its boolean outcome. This provides a single entry point for running the full suite and inspecting individual results.

**Parameters:** None (static member).

**Return value:** `IDictionary<string, bool>` — a dictionary where each key is a string identifying a specific test (e.g., `"GetCompletionRate_WithNonZeroPlannedPoints"`) and each value is `true` if that test passed, `false` otherwise.

**Throws:** No documented exceptions. Individual test failures are captured in the dictionary values rather than propagated.

## Usage

### Example 1: Running a single test and acting on the result
```csharp
bool passed = SprintMetricTestsExtensions.RunGetCompletionRate_WithNonZeroPlannedPoints;
if (passed)
{
    Console.WriteLine("Completion rate with non-zero points: PASSED");
}
else
{
    Console.Error.WriteLine("Completion rate with non-zero points: FAILED");
    Environment.ExitCode = 1;
}
```

### Example 2: Running all tests and summarizing outcomes
```csharp
IDictionary<string, bool> results = SprintMetricTestsExtensions.RunAll;
int failedCount = 0;

foreach (var kvp in results)
{
    string status = kvp.Value ? "PASS" : "FAIL";
    Console.WriteLine($"{kvp.Key}: {status}");
    if (!kvp.Value) failedCount++;
}

Console.WriteLine($"\nTotal: {results.Count} tests, {failedCount} failed.");
if (failedCount > 0)
{
    Environment.ExitCode = 1;
}
```

## Notes

- **Edge cases:** `RunGetCompletionRate_WithZeroPlannedPoints` specifically targets the division-by-zero boundary. The expected behavior (returning zero, a sentinel value, or a defined constant) is determined by the underlying `SprintMetric` implementation. `RunValidate_WithEndDateBeforeStartDate_ThrowsArgumentException` assumes that only `ArgumentException` (or a derived type) is thrown; any other exception type will cause the test to return `false`.
- **Thread safety:** All members are static and do not accept external state. The methods operate on internally constructed test data and do not mutate shared resources. Concurrent invocations of different test methods, or multiple calls to `RunAll`, are safe without external synchronization. However, the underlying sprint metric logic being tested may have its own thread-safety characteristics, which are outside the scope of this class.
- **Return value convention:** Each test method returns `bool` rather than throwing on failure, making them suitable for direct use in assertion-free scripting or CI pipelines where a simple pass/fail exit code is desired.
- **Dictionary keys in `RunAll`:** The exact string keys are derived from the test method names. Consumers relying on specific key names should avoid hard-coding assumptions and instead iterate the dictionary or check for expected keys defensively.
