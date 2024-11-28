// ... rest of README content ...
## JsonFormatterTestsExtensions

The `JsonFormatterTestsExtensions` class provides a set of extension methods for testing the behavior of the `JsonFormatter` class. These methods allow you to verify that the formatter produces the expected output for various input scenarios.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Formatters;

// Create a test instance of JsonFormatterTests
var jsonFormatterTests = new JsonFormatterTests();

// Test that the formatter contains a specific key-value pair
jsonFormatterTests.ShouldContainKeyValue("key", "value");

// Test that the formatter contains a specific key
jsonFormatterTests.ShouldContainKey("key");

// Test that the formatter does not contain a specific key
jsonFormatterTests.ShouldNotContainKey("key");

// Test that the formatter is valid with no errors
jsonFormatterTests.ShouldBeValidWithNoErrors();

// Test that the formatter is invalid with errors
jsonFormatterTests.ShouldBeInvalidWithErrors();
```

# ... rest of README content ...
