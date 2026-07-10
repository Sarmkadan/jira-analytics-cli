# XmlFormatterExtensions

A set of static helper methods for working with XML data in the **jira-analytics-cli** project. The members provide utilities for formatting collections, validating XML with detailed error information, selecting values by name, and producing a compact XML representation.

## API

### FormatCollection
- **Purpose**: Returns a formatted string representation of a collection (the collection to format is supplied implicitly by the calling context).
- **Parameters**: None.
- **Return Value**: A `string` containing the formatted collection.
- **Exceptions**: May throw `InvalidOperationException` if the underlying collection cannot be formatted.

### ValidateWithDetails
- **Purpose**: Validates XML content and reports whether it is well‑formed, along with any error message and the line number where the problem occurs.
- **Parameters**: None.
- **Return Value**: A value tuple `(bool IsValid, string? Error, int? LineNumber)` where `IsValid` indicates success, `Error` contains a descriptive message when validation fails, and `LineNumber` provides the line at which the error was detected (or `null` when valid).
- **Exceptions**: May throw `ArgumentException` if the XML source is not accessible, or `InvalidOperationException` for unexpected validation failures.

### SelectValuesByName
- **Purpose**: Extracts values from XML elements grouped by their element name, returning a dictionary where each key is an element name and the associated value is a list of the element’s inner text values.
- **Parameters**: None.
- **Return Value**: A `Dictionary<string, List<string>>` mapping element names to lists of their values.
- **Exceptions**: May throw `InvalidOperationException` if the XML cannot be parsed for selection.

### Compact
- **Purpose**: Produces a compact (whitespace‑minimized) string representation of XML.
- **Parameters**: None.
- **Return Value**: A `string` containing the compact XML.
- **Exceptions**: May throw `InvalidOperationException` if the XML cannot be compacted.

## Usage

```csharp
using JiraAnalyticsCli.Xml; // namespace containing XmlFormatterExtensions

// Example 1: Validate XML and handle the result
var validation = XmlFormatterExtensions.ValidateWithDetails();
if (!validation.IsValid)
{
    Console.Error.WriteLine(
        $"XML error on line {validation.LineNumber}: {validation.Error}");
}
else
{
    Console.WriteLine("XML is well‑formed.");
}

// Example 2: Obtain a compact XML string and a formatted collection
string compactXml = XmlFormatterExtensions.Compact();
string formatted = XmlFormatterExtensions.FormatCollection();

Console.WriteLine("Compact XML:");
Console.WriteLine(compactXml);
Console.WriteLine("Formatted collection:");
Console.WriteLine(formatted);
```

## Notes
- All members are **static** and do not rely on instance state; therefore they are thread‑safe as long as they do not mutate shared static data. The current implementation treats them as pure functions.
- If the methods are invoked before any XML source has been supplied (e.g., via preceding setup code), they may throw exceptions related to missing data—callers should ensure the required context is initialized.
- The `ValidateWithDetails` method returns a nullable `string` for `Error` and a nullable `int` for `LineNumber`; callers should check `IsValid` before relying on the other values.
- The `SelectValuesByName` method returns an empty dictionary when no matching elements are found rather than returning `null`.
