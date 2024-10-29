# StringExtensions
Provides a collection of pure‑function helpers for common string manipulation tasks in the **jira-analytics-cli** project. All methods are static, side‑effect free, and safe to call from multiple threads.

## API

### TruncateWithEllipsis
```csharp
public static string TruncateWithEllipsis(string input, int maxLength, string ellipsis = "...")
```
**Purpose** – Returns `input` shortened to `maxLength` characters, appending `ellipsis` when truncation occurs.  
**Parameters**  
- `input`: The string to truncate.  
- `maxLength`: Desired maximum length of the returned string (including the ellipsis). Must be non‑negative.  
- `ellipsis`: Optional string to append when truncation happens; defaults to `"..."`.  
**Return value** – The truncated string, or the original string if its length ≤ `maxLength`.  
**Exceptions** –  
- `ArgumentNullException` if `input` is `null`.  
- `ArgumentOutOfRangeException` if `maxLength` is negative.  

### RemoveWhitespace
```csharp
public static string RemoveWhitespace(string input)
```
**Purpose** – Strips all Unicode whitespace characters from `input`.  
**Parameters**  
- `input`: The string to process.  
**Return value** – A new string containing no whitespace characters.  
**Exceptions** – `ArgumentNullException` if `input` is `null`.  

### ToSlug
```csharp
public static string ToSlug(string input)
```
**Purpose** – Converts `input` to a URL‑friendly slug: lowercased, spaces replaced with hyphens, and all non‑alphanumeric characters removed.  
**Parameters**  
- `input`: The string to slugify.  
**Return value** – The slug representation of `input`.  
**Exceptions** – `ArgumentNullException` if `input` is `null`.  

### TryParseBool
```csharp
public static bool TryParseBool(string input, out bool result)
```
**Purpose** – Attempts to interpret `input` as a Boolean value. Accepts `"true"`, `"false"`, `"1"`, `"0"` (case‑insensitive).  
**Parameters**  
- `input`: The string to parse.  
- `result`: When the method returns `true`, contains the parsed Boolean value; otherwise undefined.  
**Return value** – `true` if `input` matches a recognized Boolean format; otherwise `false`.  
**Exceptions** – None.  

### Repeat
```csharp
public static string Repeat(string input, int count)
```
**Purpose** – Returns a new string consisting of `input` repeated `count` times.  
**Parameters**  
- `input`: The string to repeat.  
- `count`: Number of repetitions; must be non‑negative.  
**Return value** – The repeated string, or an empty string when `count` is zero.  
**Exceptions** –  
- `ArgumentNullException` if `input` is `null`.  
- `ArgumentOutOfRangeException` if `count` is negative.  

### MatchesPattern
```csharp
public static bool MatchesPattern(string input, string pattern)
```
**Purpose** – Determines whether `input` matches the regular expression `pattern`.  
**Parameters**  
- `input`: The string to test.  
- `pattern`: The regular expression pattern.  
**Return value** – `true` if `input` satisfies `pattern`; otherwise `false`.  
**Exceptions** –  
- `ArgumentNullException` if either `input` or `pattern` is `null`.  
- `ArgumentException` if `pattern` is not a valid regular expression.  

### GetCommonPrefix
```csharp
public static string GetCommonPrefix(IEnumerable<string> strings)
```
**Purpose** – Returns the longest common prefix shared by all strings in `strings`.  
**Parameters**  
- `strings`: A sequence of strings to evaluate.  
**Return value** – The common prefix; empty string if there is none or the sequence is empty.  
**Exceptions** –  
- `ArgumentNullException` if `strings` is `null`.  
- `ArgumentNullException` if any element in `strings` is `null`.  

### EscapeForSql
```csharp
public static string EscapeForSql(string input)
```
**Purpose** – Escapes single quotation marks in `input` for safe inclusion in an SQL literal by doubling them (`'` → `''`).  
**Parameters**  
- `input`: The string to escape.  
**Return value** – The escaped string suitable for use in an SQL query literal.  
**Exceptions** – `ArgumentNullException` if `input` is `null`.  

## Usage

### Example 1: Slug generation and length limiting
```csharp
string title = "  JIRA Analytics: Q4 Report  ";
string slug = StringExtensions.ToSlug(title);               // "jira-analytics-q4-report"
string limited = StringExtensions.TruncateWithEllipsis(slug, 20);
// limited => "jira-analytics-q4..."
```

### Example 2: Parsing a Boolean flag from configuration
```csharp
string rawFlag = config["EnableFeature"];
if (StringExtensions.TryParseBool(rawFlag, out bool enabled))
{
    featureToggle.IsEnabled = enabled;
}
else
{
    // fallback to default behavior
    featureToggle.IsEnabled = false;
}
```

## Notes
- All methods treat a `null` input as an error and throw `ArgumentNullException`; callers should validate or guard against null values where appropriate.  
- `TruncateWithEllipsis` considers the length of the supplied `ellipsis` when computing the result; if `maxLength` is smaller than the ellipsis length, the method returns only the ellipsis truncated to `maxLength`.  
- `RemoveWhitespace` removes every Unicode whitespace character (including spaces, tabs, newline, etc.), not just the ASCII space.  
- `ToSlug` does not preserve culture‑specific case mappings; it uses `ToLowerInvariant()`.  
- `TryParseBool` accepts the literals `"true"`, `"false"`, `"1"`, `"0"` in any combination of upper/lower case; any other value results in `false`.  
- `MatchesPattern` leverages `System.Text.RegularExpressions.Regex.IsMatch`; the pattern is compiled on each call, so for repeated use with the same pattern consider caching a `Regex` instance.  
- `GetCommonPrefix` operates in O(n × m) time where *n* is the number of strings and *m* is the length of the shortest string; it does not allocate intermediate copies of the input strings.  
- `EscapeForSql` is a minimal helper intended for simple literal escaping; it does not protect against SQL injection when used with identifiers or complex statements—use parameterized queries whenever possible.  
- Because all members are static and stateless, they are inherently thread‑safe and can be invoked concurrently without external synchronization.
