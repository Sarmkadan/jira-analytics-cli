# ReportServiceValidation

Utility class providing validation methods for report-related inputs in the Jira Analytics CLI. These methods check the structural validity of parameters used when generating reports, such as project keys, sprint identifiers, output paths, and report formats. Each validation method returns a list of error messages; if the list is empty, the input is considered valid.

## API

### `public static IReadOnlyList<string> Validate(string input)`

Validates a generic report input string. Returns a list of validation error messages. An empty list indicates the input is valid. This method is intended for use when the specific type of input is not known or when general-purpose validation is sufficient.

**Returns**
`IReadOnlyList<string>` – A list of error messages; empty if valid.

---

### `public static bool IsValid(string input)`

Determines whether a generic report input string is valid. Returns `true` if the input passes validation; otherwise, `false`.

**Returns**
`bool` – `true` if the input is valid; otherwise, `false`.

---

### `public static void EnsureValid(string input)`

Validates a generic report input string and throws an exception if the input is invalid. If validation fails, throws an `ArgumentException` with a message describing the validation errors.

**Parameters**
- `input` (`string`) – The input string to validate.

**Throws**
- `ArgumentException` – If the input is invalid.

---

### `public static IReadOnlyList<string> ValidateProjectKey(string projectKey)`

Validates a Jira project key. Returns a list of validation error messages. An empty list indicates the project key is valid. Project keys must adhere to Jira conventions (typically 2–10 uppercase letters, optionally followed by digits).

**Parameters**
- `projectKey` (`string`) – The project key to validate.

**Returns**
`IReadOnlyList<string>` – A list of error messages; empty if valid.

---

### `public static IReadOnlyList<string> ValidateSprintId(string sprintId)`

Validates a sprint identifier. Returns a list of validation error messages. An empty list indicates the sprint ID is valid. Sprint IDs are expected to be numeric strings representing valid identifiers in Jira.

**Parameters**
- `sprintId` (`string`) – The sprint ID to validate.

**Returns**
`IReadOnlyList<string>` – A list of error messages; empty if valid.

---
### `public static IReadOnlyList<string> ValidateOutputPath(string outputPath)`

Validates a file system output path. Returns a list of validation error messages. An empty list indicates the path is valid. The path must be non-empty, not contain invalid characters, and be writable according to the application’s context.

**Parameters**
- `outputPath` (`string`) – The output path to validate.

**Returns**
`IReadOnlyList<string>` – A list of error messages; empty if valid.

---
### `public static IReadOnlyList<string> ValidateFormat(string format)`

Validates a report format specifier. Returns a list of validation error messages. An empty list indicates the format is valid. Supported formats are typically constrained to a predefined set (e.g., "csv", "json", "html").

**Parameters**
- `format` (`string`) – The format specifier to validate.

**Returns**
`IReadOnlyList<string>` – A list of error messages; empty if valid.

## Usage
