# DateTimeExtensionsTestsJsonExtensions

Provides JSON serialization and deserialization helpers for `DateTime` and `TimeSpan` values, used by the Jira Analytics CLI to normalize date/time representations in JSON payloads and configuration files.

## API

### `public static string ToJson(DateTime value)`

Serializes a `DateTime` to an ISO 8601 JSON string.

**Parameters**  
- `value` — The `DateTime` to serialize.

**Returns**  
A JSON string representation (e.g., `"2024-01-15T14:30:00Z"`).

**Throws**  
`ArgumentException` if `value.Kind` is `Unspecified`.

---

### `public static string ToJson(DateTime? value)`

Serializes a nullable `DateTime` to an ISO 8601 JSON string or `null`.

**Parameters**  
- `value` — The `DateTime?` to serialize.

**Returns**  
A JSON string representation, or `null` if `value` is `null`.

**Throws**  
`ArgumentException` if `value` has a value with `Kind == Unspecified`.

---

### `public static string ToJson(TimeSpan value)`

Serializes a `TimeSpan` to an ISO 8601 duration JSON string.

**Parameters**  
- `value` — The `TimeSpan` to serialize.

**Returns**  
A JSON string representation (e.g., `"P1DT2H30M"`).

**Throws**  
`ArgumentOutOfRangeException` if `value` is negative.

---

### `public static DateTime? FromJson(string json)`

Parses an ISO 8601 JSON string to a `DateTime`.

**Parameters**  
- `json` — The JSON string to parse (quotes included).

**Returns**  
The parsed `DateTime` in UTC, or `null` if `json` is `null` or empty.

**Throws**  
`FormatException` if `json` is not a valid ISO 8601 date/time string.  
`ArgumentNullException` if `json` is `null` (only when non-nullable overload is called).

---

### `public static bool TryFromJson(string json, out DateTime? result)`

Attempts to parse an ISO 8601 JSON string to a `DateTime` without throwing.

**Parameters**  
- `json` — The JSON string to parse (quotes included).  
- `result` — Receives the parsed `DateTime?` on success; `null` on failure.

**Returns**  
`true` if parsing succeeded; otherwise `false`.

**Throws**  
Does not throw.

---

### `public static TimeSpan? FromJsonToTimeSpan(string json)`

Parses an ISO 8601 duration JSON string to a `TimeSpan`.

**Parameters**  
- `json` — The JSON string to parse (quotes included).

**Returns**  
The parsed `TimeSpan`, or `null` if `json` is `null` or empty.

**Throws**  
`FormatException` if `json` is not a valid ISO 8601 duration string.  
`OverflowException` if the duration exceeds `TimeSpan` range.

---

### `public static bool TryFromJsonToTimeSpan(string json, out TimeSpan? result)`

Attempts to parse an ISO 8601 duration JSON string to a `TimeSpan` without throwing.

**Parameters**  
- `json` — The JSON string to parse (quotes included).  
- `result` — Receives the parsed `TimeSpan?` on success; `null` on failure.

**Returns**  
`true` if parsing succeeded; otherwise `false`.

**Throws**  
Does not throw.

## Usage

```csharp
// Serialize a DateTime and TimeSpan for JSON output
var issueCreated = new DateTime(2024, 3, 10, 12, 0, 0, DateTimeKind.Utc);
var workLogDuration = TimeSpan.FromHours(3.5);

string createdJson = DateTimeExtensionsTestsJsonExtensions.ToJson(issueCreated);
// createdJson == "\"2024-03-10T12:00:00Z\""

string durationJson = DateTimeExtensionsTestsJsonExtensions.ToJson(workLogDuration);
// durationJson == "\"PT3H30M\""
```

```csharp
// Deserialize from stored JSON configuration
string storedCreated = "\"2024-03-10T12:00:00Z\"";
string storedDuration = "\"PT3H30M\"";

if (DateTimeExtensionsTestsJsonExtensions.TryFromJson(storedCreated, out DateTime? created))
{
    Console.WriteLine($"Issue created: {created.Value:u}");
}

if (DateTimeExtensionsTestsJsonExtensions.TryFromJsonToTimeSpan(storedDuration, out TimeSpan? duration))
{
    Console.WriteLine($"Work logged: {duration.Value.TotalHours} hours");
}
```

## Notes

- All `DateTime` values must have `Kind == Utc` or `Kind == Local`; `Unspecified` throws on serialization to avoid ambiguity.
- `FromJson` and `FromJsonToTimeSpan` expect the input string to include surrounding JSON quotes (e.g., `"\"2024-01-01T00:00:00Z\""`). Callers reading raw JSON tokens should pass the token text directly.
- Negative `TimeSpan` values are rejected on serialization; durations are always positive.
- The `Try*` methods never throw and are preferred for parsing untrusted or user-supplied input.
- All methods are pure static functions with no shared state; they are fully thread-safe.
