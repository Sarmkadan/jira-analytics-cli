# StringBenchmarks

The `StringBenchmarks` class serves as a utility container for high-performance string manipulation operations commonly required within the `jira-analytics-cli` toolchain. It provides optimized implementations for frequent text processing tasks such as whitespace removal, truncation, slug generation, prefix comparison, and pattern matching, ensuring consistent behavior and reduced allocation overhead during analytics processing.

## API

### `RemoveWhitespace`
*   **Signature**: `public string RemoveWhitespace`
*   **Purpose**: Removes all whitespace characters from the target string.
*   **Parameters**: None (operates on the instance context or input defined by implementation).
*   **Return Value**: Returns a new `string` containing the original content with all whitespace characters removed.
*   **Exceptions**: Throws `ArgumentNullException` if the input string is null.

### `TruncateWithEllipsis`
*   **Signature**: `public string TruncateWithEllipsis`
*   **Purpose**: Shortens a string to a specified maximum length, appending an ellipsis if truncation occurs.
*   **Parameters**: None (operates on the instance context or input defined by implementation).
*   **Return Value**: Returns a `string` that is either the original input (if within limits) or a truncated version ending with an ellipsis.
*   **Exceptions**: Throws `ArgumentOutOfRangeException` if the specified maximum length is negative.

### `ToSlug`
*   **Signature**: `public string ToSlug`
*   **Purpose**: Converts a string into a URL-friendly slug format by lowercasing, removing special characters, and replacing spaces with hyphens.
*   **Parameters**: None (operates on the instance context or input defined by implementation).
*   **Return Value**: Returns a formatted `string` suitable for use in URLs or file identifiers.
*   **Exceptions**: Throws `ArgumentNullException` if the input string is null.

### `GetCommonPrefix`
*   **Signature**: `public string GetCommonPrefix`
*   **Purpose**: Identifies the longest common prefix shared between the target string and a comparison string.
*   **Parameters**: None (operates on the instance context or input defined by implementation).
*   **Return Value**: Returns a `string` representing the common starting sequence; returns an empty string if no common prefix exists.
*   **Exceptions**: Throws `ArgumentNullException` if either of the compared strings is null.

### `MatchesPattern`
*   **Signature**: `public bool MatchesPattern`
*   **Purpose**: Determines whether the target string matches a specific wildcard or regex pattern.
*   **Parameters**: None (operates on the instance context or input defined by implementation).
*   **Return Value**: Returns `true` if the string matches the pattern; otherwise, `false`.
*   **Exceptions**: Throws `ArgumentException` if the provided pattern is syntactically invalid.

## Usage

The following example demonstrates how to utilize the `ToSlug` and `RemoveWhitespace` members to normalize Jira issue summaries for file naming conventions:

```csharp
var benchmark = new StringBenchmarks();
var issueSummary = "BUG-101: Critical Failure in Auth Module ";

// Generate a safe filename slug
var fileName = benchmark.ToSlug(issueSummary); 
// Result: "bug-101-critical-failure-in-auth-module"

// Remove whitespace for compact logging keys
var logKey = benchmark.RemoveWhitespace(issueSummary); 
// Result: "BUG-101:CriticalFailureinAuthModule"
```

The next example illustrates using `TruncateWithEllipsis` and `GetCommonPrefix` to handle display limits and group similar issue keys:

```csharp
var benchmark = new StringBenchmarks();
var longDescription = "This error occurs when the user attempts to access the resource without valid credentials...";
var key1 = "PROJ-1234";
var key2 = "PROJ-1235";

// Truncate description for UI tooltip
var displayText = benchmark.TruncateWithEllipsis(longDescription, 50); 
// Result: "This error occurs when the user attempts to ac..."

// Identify shared project prefix
var prefix = benchmark.GetCommonPrefix(key1, key2); 
// Result: "PROJ-123"
```

## Notes

*   **Null Handling**: Most members returning strings (`RemoveWhitespace`, `ToSlug`, `GetCommonPrefix`) explicitly throw `ArgumentNullException` upon receiving null inputs. Callers must ensure inputs are validated or handle these exceptions appropriately.
*   **Thread Safety**: The `StringBenchmarks` class is stateless regarding the data it processes; however, the instance itself should not be considered thread-safe if internal caching mechanisms are introduced in future updates. Currently, multiple threads may safely invoke methods on the same instance provided the inputs are immutable strings.
*   **Edge Cases**:
    *   `TruncateWithEllipsis` returns the original string unchanged if the input length is less than or equal to the maximum length.
    *   `GetCommonPrefix` returns an empty string if the first characters of the compared strings do not match.
    *   `ToSlug` will reduce consecutive whitespace or special characters to a single hyphen to prevent malformed slugs.
