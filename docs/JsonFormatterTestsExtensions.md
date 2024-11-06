# JsonFormatterTestsExtensions

The `JsonFormatterTestsExtensions` class provides a set of extension methods for the `JsonFormatterTests` type, enabling concise and readable assertions on JSON strings within unit tests. These methods are designed to validate the presence or absence of keys, the existence of key-value pairs, and the overall validity or invalidity of JSON output produced by the formatter under test. They simplify common verification patterns and integrate seamlessly with the test framework used in the `jira-analytics-cli` project.

## API

### `ShouldContainKeyValue`

Asserts that a given JSON string contains a specific key with a matching value.

```csharp
public static void ShouldContainKeyValue(
    this JsonFormatterTests test,
    string json,
    string key,
    object value
)
```

- **Parameters**  
  - `test` – The `JsonFormatterTests` instance on which the extension is invoked.  
  - `json` – The JSON string to inspect.  
  - `key` – The expected key (case-sensitive).  
  - `value` – The expected value associated with the key. Comparison uses the default equality comparer.  
- **Return value** – `void`.  
- **Throws** – `AssertionException` (or equivalent) if the key is not found or the value does not match.

### `ShouldContainKey`

Asserts that a given JSON string contains a specific key, regardless of its value.

```csharp
public static void ShouldContainKey(
    this JsonFormatterTests test,
    string json,
    string key
)
```

- **Parameters**  
  - `test` – The `JsonFormatterTests` instance.  
  - `json` – The JSON string to inspect.  
  - `key` – The expected key (case-sensitive).  
- **Return value** – `void`.  
- **Throws** – `AssertionException` if the key is absent.

### `ShouldNotContainKey`

Asserts that a given JSON string does **not** contain a specified key.

```csharp
public static void ShouldNotContainKey(
    this JsonFormatterTests test,
    string json,
    string key
)
```

- **Parameters**  
  - `test` – The `JsonFormatterTests` instance.  
  - `json` – The JSON string to inspect.  
  - `key` – The key that must be absent (case-sensitive).  
- **Return value** – `void`.  
- **Throws** – `AssertionException` if the key is present.

### `ShouldBeValidWithNoErrors`

Asserts that a JSON string is syntactically valid and contains no error indicators (e.g., no `"error"` or `"errors"` keys, or no malformed structure as defined by the test context).

```csharp
public static void ShouldBeValidWithNoErrors(
    this JsonFormatterTests test,
    string json
)
```

- **Parameters**  
  - `test` – The `JsonFormatterTests` instance.  
  - `json` – The JSON string to validate.  
- **Return value** – `void`.  
- **Throws** – `AssertionException` if the JSON is invalid or contains error markers.

### `ShouldBeInvalidWithErrors`

Asserts that a JSON string is either syntactically invalid or contains expected error indicators (e.g., an `"error"` key with a non-null value).

```csharp
public static void ShouldBeInvalidWithErrors(
    this JsonFormatterTests test,
    string json
)
```

- **Parameters**  
  - `test` – The `JsonFormatterTests` instance.  
  - `json` – The JSON string to inspect.  
- **Return value** – `void`.  
- **Throws** – `AssertionException` if the JSON is valid and error-free.

## Usage

The following examples demonstrate typical usage within an xUnit or NUnit test class that inherits from or uses `JsonFormatterTests`.

```csharp
using YourProject.Tests;

public class JsonFormatterTests : TestBase
{
    [Fact]
    public void Format_WithValidData_ContainsExpectedKeyValue()
    {
        var json = "{\"name\": \"Alice\", \"age\": 30}";
        this.ShouldContainKeyValue(json, "name", "Alice");
        this.ShouldContainKey(json, "age");
        this.ShouldNotContainKey(json, "address");
        this.ShouldBeValidWithNoErrors(json);
    }

    [Fact]
    public void Format_WithMissingField_ProducesError()
    {
        var json = "{\"error\": \"Missing required field 'email'\"}";
        this.ShouldContainKey(json, "error");
        this.ShouldBeInvalidWithErrors(json);
    }
}
```

## Notes

- **Null and empty arguments** – Passing `null` for `json`, `key`, or `value` may result in a `NullReferenceException` or an assertion failure depending on the internal implementation. Always ensure that `json` is a non-null, well-formed JSON string.  
- **Case sensitivity** – Key comparisons are case-sensitive. Use exact casing as expected in the JSON output.  
- **Thread safety** – All methods are static and do not modify any shared state. They are safe to call concurrently from multiple threads, provided the `json` argument is not mutated during the assertion.  
- **Test framework dependency** – These extensions rely on the assertion mechanism of the underlying test framework (e.g., xUnit, NUnit). The exact exception type thrown on failure is framework-specific.  
- **JSON parsing** – The methods internally parse the `json` string. Malformed JSON may cause a `JsonException` before any assertion logic runs.
