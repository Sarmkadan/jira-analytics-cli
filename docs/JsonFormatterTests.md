# JsonFormatterTests

Unit tests for the JSON formatting and validation utilities in the `jira-analytics-cli` project. This class verifies correct serialization, validation, and formatting behavior for JSON data structures, including edge cases such as null properties and minified input.

## API

### `public JsonFormatterTests`

The test class containing unit tests for JSON formatting and validation functionality. Serves as a container for test methods that validate the behavior of the `JsonFormatter` utility class.

### `public void Format_ShouldSerializeObjectToJson()`

Verifies that the `JsonFormatter.Format` method correctly serializes a C# object into a JSON string. The test ensures that all public properties of the input object are included in the output with proper JSON formatting.

**Parameters:** None

**Return value:** None

**Throws:** No exceptions are expected under normal test conditions.

### `public void Format_ShouldHandleNullPropertiesByIgnoringThem()`

Ensures that the `JsonFormatter.Format` method omits properties with null values from the serialized JSON output. This test validates that the formatter adheres to standard JSON serialization practices by excluding null fields.

**Parameters:** None

**Return value:** None

**Throws:** No exceptions are expected under normal test conditions.

### `public void Validate_ShouldReturnTrueForValidJson()`

Checks that the `JsonFormatter.Validate` method returns `true` when provided with a valid JSON string. The test confirms that the validation logic correctly identifies syntactically correct JSON input.

**Parameters:** None

**Return value:** None

**Throws:** No exceptions are expected under normal test conditions.

### `public void Validate_ShouldReturnFalseForInvalidJson()`

Confirms that the `JsonFormatter.Validate` method returns `false` when given an invalid JSON string. The test ensures that the validation logic rejects malformed or syntactically incorrect JSON input.

**Parameters:** None

**Return value:** None

**Throws:** No exceptions are expected under normal test conditions.

### `public void FormatWithMetadata_ShouldIncludeMetadata()`

Validates that the `JsonFormatter.FormatWithMetadata` method includes metadata in the serialized JSON output. The test ensures that additional contextual information, such as timestamps or identifiers, is appended to the JSON string as expected.

**Parameters:** None

**Return value:** None

**Throws:** No exceptions are expected under normal test conditions.

### `public void Prettify_ShouldFormatMinifiedJson()`

Tests that the `JsonFormatter.Prettify` method converts minified JSON strings into a human-readable, indented format. The test verifies that the output is properly formatted with consistent indentation and line breaks.

**Parameters:** None

**Return value:** None

**Throws:** No exceptions are expected under normal test conditions.

## Usage
