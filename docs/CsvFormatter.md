# CsvFormatter

The `CsvFormatter` class provides utilities for converting objects to and from CSV format. It supports generic formatting with optional column mapping and parsing of CSV strings into strongly‑typed lists.

## API

### CsvFormatter()
Creates a new instance of the `CsvFormatter` class.  
**Parameters:** None.  
**Return value:** A ready‑to‑use formatter.  
**Exceptions:** None under normal conditions.

### string Format<T>()
Produces a CSV string representation of a default instance of type `T`. The method uses reflection to enumerate public properties of `T` and writes their values in property‑declaration order.  
**Parameters:** None (only the generic type argument `T`).  
**Return value:** A single‑line CSV string containing the property values of a new `T()` instance.  
**Exceptions:**  
- `InvalidOperationException` if `T` does not have a parameterless constructor.  
- `TargetInvocationException` if a property getter throws during reflection.

### string FormatWithMapping<T>()
Produces a CSV string using an explicit property‑to‑column mapping defined on the formatter instance. The mapping must be configured prior to calling this method (configuration members are not part of the public surface documented here).  
**Parameters:** None (only the generic type argument `T`).  
**Return value:** A CSV string where columns appear in the order specified by the mapping.  
**Exceptions:**  
- `InvalidOperationException` if no mapping has been supplied for type `T`.  
- `InvalidOperationException` if `T` lacks a parameterless constructor.  
- `TargetInvocationException` if accessing a mapped property throws.

### List<T> Parse<T>(string csv) where T : class, new()
Parses a CSV string into a list of objects of type `T`. Each line after an optional header is mapped to a new instance; property values are assigned by matching column names to property names (case‑insensitive).  
**Parameters:**  
- `csv`: The CSV content to parse. Must not be `null`.  
**Return value:** A `List<T>` containing one object per non‑empty line in the input.  
**Exceptions:**  
- `ArgumentNullException` if `csv` is `null`.  
- `FormatException` if a line contains a different number of fields than expected or if a value cannot be converted to the target property type.  
- `InvalidOperationException` if `T` does not have a parameterless constructor.  

## Usage

```csharp
var formatter = new CsvFormatter();

// Simple formatting of a POCO
public class Issue
{
    public string Key { get; set; }
    public string Summary { get; set; }
}
var issue = new Issue { Key = "PROJ-1", Summary = "Fix bug" };
string csv = formatter.Format<Issue>(); // yields "PROJ-1,Fix bug"
Console.WriteLine(csv);
```

```csharp
// Parsing CSV with a header line
string csvData = @"Key,Summary
PROJ-1,Fix bug
PROJ-2,Add feature";

var formatter = new CsvFormatter();
List<Issue> issues = formatter.Parse<Issue>(csvData);
// issues contains two Issue objects with properties set accordingly
```

## Notes

- The formatter assumes that all properties of `T` are readable and writable; read‑only or write‑only properties are ignored during parsing and may cause missing values during formatting.  
- CSV parsing does not support quoted fields containing embedded commas or newlines; such data will be treated as separate columns and likely cause a `FormatException`.  
- The class holds no mutable state aside from any mapping configuration (not shown in the public API). Consequently, instances are thread‑safe for concurrent calls to `Format<T>`, `FormatWithMapping<T>`, and `Parse<T>` provided that any mapping configuration is completed before the first use and not altered thereafter.  
- If a custom mapping is required, it must be established via the formatter’s non‑public configuration members before invoking `FormatWithMapping<T>`; otherwise the method will throw an `InvalidOperationException`.  
- Performance scales linearly with the number of properties and lines; for large CSV streams consider processing in chunks rather than loading the entire string into memory.
