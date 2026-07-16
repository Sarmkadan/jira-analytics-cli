// ... rest of README content ...

## StringExtensions

The `StringExtensions` class provides a set of extension methods for string manipulation and formatting. It offers utilities for truncating strings, removing whitespace, converting to slug format, parsing boolean values, repeating strings, matching patterns, finding common prefixes, and escaping special characters for safe SQL use.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Sample data
var longString = "This is a very long string that needs to be truncated.";
var whitespaceString = "   This string has leading and trailing whitespace.   ";
var slugString = "This string will be converted to slug format.";
var boolString = "true";

// Truncate string with ellipsis
var truncated = longString.TruncateWithEllipsis(20);
Console.WriteLine(truncated); // Output: "This is a very long..."

// Remove whitespace from string
var cleaned = whitespaceString.RemoveWhitespace();
Console.WriteLine(cleaned); // Output: "This string has leading and trailing whitespace."

// Convert string to slug format
var slug = slugString.ToSlug();
Console.WriteLine(slug); // Output: "this-string-will-be-converted-to-slug-format"

// Parse boolean value from string
if (boolString.TryParseBool(out bool result))
{
    Console.WriteLine(result); // Output: true
}

// Repeat string
var repeated = "Hello".Repeat(3);
Console.WriteLine(repeated); // Output: "HelloHelloHello"

// Match pattern
var pattern = "Hello*";
if ("HelloWorld".MatchesPattern(pattern))
{
    Console.WriteLine("Match found!"); // Output: Match found!
}

// Find common prefix
var prefix = "Hello".GetCommonPrefix("HelloWorld");
Console.WriteLine(prefix); // Output: "Hello"

// Escape special characters for SQL
var sqlString = "Hello'World";
var escaped = sqlString.EscapeForSql();
Console.WriteLine(escaped); // Output: "Hello''World"
```

