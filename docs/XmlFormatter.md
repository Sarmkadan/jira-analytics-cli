# XmlFormatter

Provides utilities for serializing, validating, and formatting XML content, primarily used to prepare Jira data for CLI output and further processing.

## API

### `public XmlFormatter()`

Initializes a new instance of the `XmlFormatter` class with default settings.

### `public string Format(string xml)`

Serializes the provided XML string into a standardized output format.

- **Parameters**
  - `xml`: The raw XML string to format.
- **Return value**
  - A formatted XML string.
- **Exceptions**
  - Throws `ArgumentNullException` if `xml` is `null`.
  - Throws `XmlException` if the input is not valid XML.

### `public (bool IsValid, string? Error) Validate(string xml)`

Validates the provided XML string and returns a tuple indicating validity and any error message.

- **Parameters**
  - `xml`: The XML string to validate.
- **Return value**
  - A tuple where `IsValid` is `true` if the XML is valid, otherwise `false`; `Error` contains a descriptive error message if validation fails, otherwise `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `xml` is `null`.

### `public string Prettify(string xml)`

Applies indentation and line breaks to the provided XML string for improved human readability.

- **Parameters**
  - `xml`: The XML string to prettify.
- **Return value**
  - A prettified XML string with consistent indentation.
- **Exceptions**
  - Throws `ArgumentNullException` if `xml` is `null`.
  - Throws `XmlException` if the input is not valid XML.

### `public List<string> SelectValues(string xml, string xpath)`

Extracts values from the provided XML using the specified XPath query.

- **Parameters**
  - `xml`: The XML string to query.
  - `xpath`: The XPath expression used to select nodes.
- **Return value**
  - A list of string values matching the XPath query.
- **Exceptions**
  - Throws `ArgumentNullException` if `xml` or `xpath` is `null`.
  - Throws `XmlException` if the input XML is invalid.
  - Throws `XPathException` if the XPath expression is invalid.

## Usage

```csharp
// Example 1: Format and validate XML
var formatter = new XmlFormatter();
string rawXml = "<issue><key>PROJ-123</key><summary>Fix the bug</summary></issue>";

// Validate XML
var (isValid, error) = formatter.Validate(rawXml);
if (!isValid)
{
    Console.WriteLine($"Validation error: {error}");
    return;
}

// Format and prettify
string formatted = formatter.Format(rawXml);
string pretty = formatter.Prettify(formatted);
Console.WriteLine(pretty);

// Example 2: Extract values using XPath
var keys = formatter.SelectValues(rawXml, "//key");
foreach (var key in keys)
{
    Console.WriteLine($"Found key: {key}");
}
```

## Notes

- **Thread safety**: Instances of `XmlFormatter` are not thread-safe; each thread should use its own instance.
- **Edge cases**: Methods assume well-formed XML; malformed input may result in exceptions rather than graceful handling. Empty strings are treated as invalid XML.
- **Performance**: XPath queries (`SelectValues`) may be expensive on large XML documents; consider caching or limiting query complexity in performance-sensitive scenarios.
