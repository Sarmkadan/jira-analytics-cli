# FormattingHelpers

Utility class providing consistent formatting helpers for CLI output, including numbers, dates, status indicators, tables, and text styling.

## API

### `public static string FormatPercentage(double value, int decimals = 2)`

Formats a double value as a percentage string with the specified number of decimal places.
- **Parameters**:
  - `value`: The percentage value (e.g., `0.75` for 75%).
  - `decimals`: Number of decimal places to display (default: `2`).
- **Returns**: A string representation of the percentage (e.g., `"75.00%"`).
- **Throws**: `ArgumentOutOfRangeException` if `decimals` is negative.

---

### `public static string FormatNumber(long value, string format = "N0")`

Formats a long integer with thousands separators and optional custom formatting.
- **Parameters**:
  - `value`: The number to format.
  - `format`: A numeric format string (default: `"N0"` for no decimals).
- **Returns**: A formatted number string (e.g., `"1,234"`).
- **Throws**: `FormatException` if `format` is invalid.

---

### `public static string FormatDecimal(decimal value, int decimals = 2)`

Formats a decimal value with the specified number of decimal places.
- **Parameters**:
  - `value`: The decimal value to format.
  - `decimals`: Number of decimal places to display (default: `2`).
- **Returns**: A formatted decimal string (e.g., `"123.45"`).
- **Throws**: `ArgumentOutOfRangeException` if `decimals` is negative.

---
### `public static string FormatDate(DateTime date, string format = "yyyy-MM-dd")`

Formats a `DateTime` as a date string using the specified format.
- **Parameters**:
  - `date`: The date to format.
  - `format`: A date format string (default: `"yyyy-MM-dd"`).
- **Returns**: A formatted date string (e.g., `"2023-12-31"`).
- **Throws**: `FormatException` if `format` is invalid.

---
### `public static string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")`

Formats a `DateTime` as a date and time string using the specified format.
- **Parameters**:
  - `dateTime`: The date and time to format.
  - `format`: A date/time format string (default: `"yyyy-MM-dd HH:mm:ss"`).
- **Returns**: A formatted date/time string (e.g., `"2023-12-31 23:59:59"`).
- **Throws**: `FormatException` if `format` is invalid.

---
### `public static string FormatBytes(long bytes)`

Formats a byte count into a human-readable string (e.g., KB, MB, GB).
- **Parameters**:
  - `bytes`: The byte count to format.
- **Returns**: A formatted string (e.g., `"1.23 MB"` for `1234567`).
- **Throws**: `ArgumentOutOfRangeException` if `bytes` is negative.

---
### `public static string CreateTable(IEnumerable<string[]> rows, string[] headers = null, int[] columnWidths = null)`

Constructs an ASCII table from a collection of string arrays.
- **Parameters**:
  - `rows`: The table rows (each row is a string array).
  - `headers`: Optional table headers (default: `null`).
  - `columnWidths`: Optional fixed column widths (default: `null` for auto-sizing).
- **Returns**: A formatted table string.
- **Throws**:
  - `ArgumentNullException` if `rows` is `null`.
  - `ArgumentException` if `rows` is empty or column counts are inconsistent.

---
### `public static string ColorText(string text, ConsoleColor color)`

Applies ANSI color codes to text for terminal output.
- **Parameters**:
  - `text`: The text to color.
  - `color`: The `ConsoleColor` to apply.
- **Returns**: A string with ANSI color codes (e.g., `"\x1b[31mError\x1b[0m"` for red).
- **Throws**: `ArgumentNullException` if `text` is `null`.

---
### `public static string FormatStatus(string status, bool isError = false)`

Formats a status string with optional error styling.
- **Parameters**:
  - `status`: The status text (e.g., `"Success"`).
  - `isError`: If `true`, applies error styling (default: `false`).
- **Returns**: A formatted status string (e.g., `"[ERROR] Failed"`).
- **Throws**: `ArgumentNullException` if `status` is `null`.

---
### `public static string RepeatChar(char c, int count)`

Repeats a character `count` times.
- **Parameters**:
  - `c`: The character to repeat.
  - `count`: The number of repetitions.
- **Returns**: A string of repeated characters (e.g., `"===="`).
- **Throws**: `ArgumentOutOfRangeException` if `count` is negative.

---
### `public static string Indent(string text, int spaces = 2)`

Indents each line of text by the specified number of spaces.
- **Parameters**:
  - `text`: The text to indent.
  - `spaces`: Number of spaces to indent (default: `2`).
- **Returns**: An indented string.
- **Throws**: `ArgumentOutOfRangeException` if `spaces` is negative.

---
### `public static string CenterText(string text, int width)`

Centers text within a fixed-width field by padding with spaces.
- **Parameters**:
  - `text`: The text to center.
  - `width`: The total field width.
- **Returns**: A centered string (e.g., `"  text  "` for `width = 7`).
- **Throws**:
  - `ArgumentNullException` if `text` is `null`.
  - `ArgumentOutOfRangeException` if `width` is less than text length.

## Usage

### Example 1: Formatting CLI Output
