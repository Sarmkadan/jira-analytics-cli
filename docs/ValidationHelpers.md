# ValidationHelpers

A collection of pure static helper methods used throughout the Jira Analytics CLI to validate common input formats and to produce formatted strings for console output. The methods contain no internal state and rely only on their arguments, making them safe to call from any thread.

## API

### IsValidJiraIssueKey
- **Purpose:** Checks whether a string matches the typical Jira issue key pattern (uppercase project key, hyphen, numeric issue ID).  
- **Parameters:** `string issueKey` – the value to validate.  
- **Returns:** `true` if `issueKey` conforms to the pattern; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `issueKey` is `null`.

### IsValidProjectKey
- **Purpose:** Verifies that a string is a valid Jira project key (uppercase letters, optionally digits, length 1‑10).  
- **Parameters:** `string projectKey` – the value to validate.  
- **Returns:** `true` if `projectKey` is valid; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `projectKey` is `null`.

### IsValidUrl
- **Purpose:** Determines if a string is a well‑formed absolute URL (http/https scheme).  
- **Parameters:** `string url` – the value to validate.  
- **Returns:** `true` if `url` is a valid URL; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `url` is `null`.

### IsValidEmail
- **Purpose:** Checks whether a string looks like a valid e‑mail address (simple RFC‑5322‑like check).  
- **Parameters:** `string email` – the value to validate.  
- **Returns:** `true` if `email` appears valid; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `email` is `null`.

### IsValidSprintId
- **Purpose:** Validates that a string represents a numeric sprint identifier used by Jira.  
- **Parameters:** `string sprintId` – the value to validate.  
- **Returns:** `true` if `sprintId` consists only of digits and is not empty; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `sprintId` is `null`.

### IsValidStoryPoints
- **Purpose:** Confirms that a string is an acceptable story‑points value (numeric, “?”, or a range like “3‑5”).  
- **Parameters:** `string storyPoints` – the value to validate.  
- **Returns:** `true` if `storyPoints` matches an accepted format; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `storyPoints` is `null`.

### IsValidDateRange
- **Purpose:** Checks whether a string denotes a date range in the form `YYYY-MM-DD..YYYY-MM-DD` (single date also accepted).  
- **Parameters:** `string dateRange` – the value to validate.  
- **Returns:** `true` if `dateRange` matches the expected pattern; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `dateRange` is `null`.

### IsValidPercentage
- **Purpose:** Determines if a string represents a percentage (e.g., “45”, “45%”, “0.5%”).  
- **Parameters:** `string percentage` – the value to validate.  
- **Returns:** `true` if `percentage` is a valid percentage representation; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if `percentage` is `null`.

### TruncateWithEllipsis
- **Purpose:** Shortens a string to a specified length, appending an ellipsis when truncation occurs.  
- **Parameters:**  
  - `string input` – the text to truncate.  
  - `int maxLength` – the maximum length of the returned string (including the ellipsis). Must be non‑negative.  
- **Returns:** The original string if its length ≤ `maxLength`; otherwise the first `maxLength‑3` characters followed by `"..."`. If `maxLength` is less than 3, returns a string of `maxLength` periods.  
- **Exceptions:**  
  - `ArgumentNullException` if `input` is `null`.  
  - `ArgumentOutOfRangeException` if `maxLength` is negative.

### SanitizeForCsv
- **Purpose:** Escapes a string so it can be safely placed inside a CSV field (handles commas, quotes, and newlines).  
- **Parameters:** `string input` – the value to sanitize.  
- **Returns:** A CSV‑safe string; if the input contains a comma, double‑quote, or newline, it is wrapped in double quotes and any internal double‑quotes are doubled.  
- **Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ToProgressBar
- **Purpose:** Produces a textual progress bar suitable for console output.  
- **Parameters:**  
  - `int current` – the current progress value (must be ≥ 0).  
  - `int total` – the total value representing 100 % progress (must be > 0).  
- **Returns:** A string of the form `"[====>....] 42%"` where the filled portion reflects `current/total`.  
- **Exceptions:**  
  - `ArgumentOutOfRangeException` if `current` < 0, `total` ≤ 0, or `current` > total.

## Usage

```csharp
using JiraAnalyticsCli.Helpers;

// Validate a Jira issue key before processing
string issueKey = "PROJ-123";
if (ValidationHelpers.IsValidJiraIssueKey(issueKey))
{
    Console.WriteLine($"Issue key {issueKey} is valid.");
}
else
{
    Console.WriteLine($"Issue key {issueKey} is invalid.");
}

// Truncate a long description for display
string description = "This is an extremely long issue description that exceeds the console width.";
string shortDesc = ValidationHelpers.TruncateWithEllipsis(description, 40);
Console.WriteLine(shortDesc);
// Output: This is an extremely long issue descript...
```

```csharp
using JiraAnalyticsCli.Helpers;

// Validate an e‑mail address supplied by the user
string userEmail = args.Length > 0 ? args[0] : string.Empty;
if (ValidationHelpers.IsValidEmail(userEmail))
{
    Console.WriteLine("E‑mail address is valid.");
}
else
{
    Console.WriteLine("E‑mail address is invalid.");
}

// Show a progress bar while iterating over a collection
int processed = 0;
int totalItems = items.Count;
foreach (var item in items)
{
    // …process item…
    processed++;
    string bar = ValidationHelpers.ToProgressBar(processed, totalItems);
    Console.Write($"\r{bar}");
}
Console.WriteLine();
```

## Notes

- All validation methods treat a `null` argument as invalid and throw `ArgumentNullException`; empty strings are evaluated according to the specific format rules (e.g., an empty string is never a valid issue key or URL).  
- Whitespace is **not** trimmed automatically; callers should normalize input (e.g., `string.Trim()`) before validation if whitespace‑insensitivity is desired.  
- The validation logic is culture‑invariant; it relies only on ASCII characters and does not depend on the current thread’s culture.  
- Because the type contains no static fields and all methods are pure functions of their inputs, the helpers are thread‑safe and can be invoked concurrently from any number of threads without external synchronization.  
- `TruncateWithEllipsis` reserves three characters for the ellipsis; if `maxLength` is less than three, the method returns a string consisting solely of periods to avoid producing a negative‑length substring.  
- `SanitizeForCsv` follows RFC 4180 conventions: fields containing a comma, double‑quote, or newline are wrapped in quotes, and internal double‑quotes are escaped by doubling them.  
- `ToProgressBar` uses a fixed‑width bar of ten characters; the filled proportion is calculated as `(int)Math.Round(10.0 * current / total)`. The returned string always includes a space before the percentage value for consistent alignment.
