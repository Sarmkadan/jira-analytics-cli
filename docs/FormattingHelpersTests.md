# FormattingHelpersTests

`FormattingHelpersTests` is the unit test suite for the `FormattingHelpers` utility class in the `jira-analytics-cli` project. It validates the correctness of formatting, table generation, string manipulation, and status display helper methods. Each test method targets a specific behavior, including default parameters, edge cases such as null or empty inputs, and boundary conditions like zero-length strings or oversized content.

## API

### FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace
Verifies that `FormatPercentage` with its default precision argument produces a string with exactly one decimal place.  
**Parameters:** None (test method).  
**Returns:** `void`.  
**Throws:** Assertion failures if the formatted output does not match the expected pattern.

### FormatPercentage_ZeroPrecision_ReturnsWholeNumber
Confirms that `FormatPercentage` invoked with a precision of zero yields a whole-number percentage string without a decimal point.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures on format mismatch.

### FormatPercentage_ZeroValue_ReturnsZeroPercent
Ensures that formatting a zero value results in the string `"0%"` (or its equivalent with the configured precision).  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the output is not a zero-percent representation.

### FormatBytes_VariousSizes_ReturnsHumanReadable
Tests that `FormatBytes` correctly converts byte counts of different magnitudes into human-readable strings (e.g., `"1.5 KB"`, `"3 MB"`).  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if any size produces an incorrect unit or value.

### CreateTable_NullHeaders_ReturnsEmpty
Validates that passing a `null` headers collection to `CreateTable` returns an empty result (likely an empty string or empty table object).  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the result is not empty.

### CreateTable_EmptyHeaders_ReturnsEmpty
Validates that an empty headers collection causes `CreateTable` to return an empty result.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the result is not empty.

### CreateTable_EmptyRows_ReturnsEmpty
Confirms that when headers are provided but the rows collection is empty, `CreateTable` returns an empty result.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the result is not empty.

### CreateTable_ValidData_ContainsHeadersAndRows
Tests that with valid headers and rows, the output contains the header text and row data in the expected table structure.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if headers or row content is missing.

### CreateTable_RowShorterThanHeaders_PadsWithEmpty
Ensures that when a row has fewer columns than the header row, the missing cells are padded with empty strings so the table remains rectangular.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if padding is absent or incorrect.

### FormatStatus_VariousStatuses_ContainsOriginalStatus
Verifies that `FormatStatus` embeds the original status string within its output for multiple status values (e.g., `"Open"`, `"In Progress"`, `"Done"`).  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the original status text is not present in the formatted result.

### RepeatChar_ZeroCount_ReturnsEmpty
Tests that repeating a character zero times returns an empty string.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the result is not empty.

### RepeatChar_PositiveCount_ReturnsRepeatedString
Confirms that a positive count produces a string consisting of the character repeated exactly that many times.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the length or content is wrong.

### Indent_DefaultSpaces_AddsTwoSpaces
Verifies that the `Indent` method with no explicit space count prepends exactly two spaces to the input.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the indentation is not two spaces.

### Indent_CustomSpaces_AddsCorrectIndentation
Tests that providing a custom number of spaces results in that exact amount of leading whitespace.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the indentation width does not match.

### CenterText_ShorterThanWidth_CentersWithPadding
Ensures that text shorter than the target width is centered by adding appropriate padding on both sides.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the text is not centered or padding is asymmetric.

### CenterText_EqualToWidth_ReturnsOriginal
Confirms that text whose length equals the target width is returned unchanged, with no additional padding.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the string is altered.

### CenterText_LongerThanWidth_ReturnsOriginal
Verifies that text exceeding the target width is returned as-is, without truncation or padding.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** Assertion failures if the string is modified.

## Usage

```csharp
// Example 1: Validating percentage formatting and table creation together
[TestMethod]
public void ReportGeneration_FormattingComponents_BehaveCorrectly()
{
    var tests = new FormattingHelpersTests();

    // Percentages
    tests.FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace();
    tests.FormatPercentage_ZeroValue_ReturnsZeroPercent();

    // Table with mixed row lengths
    tests.CreateTable_ValidData_ContainsHeadersAndRows();
    tests.CreateTable_RowShorterThanHeaders_PadsWithEmpty();
}
```

```csharp
// Example 2: String utility validation for CLI output alignment
[TestMethod]
public void ConsoleOutput_AlignmentHelpers_WorkAsExpected()
{
    var tests = new FormattingHelpersTests();

    tests.CenterText_ShorterThanWidth_CentersWithPadding();
    tests.CenterText_LongerThanWidth_ReturnsOriginal();
    tests.Indent_CustomSpaces_AddsCorrectIndentation();
    tests.RepeatChar_PositiveCount_ReturnsRepeatedString();
}
```

## Notes

- **Edge cases:** The suite explicitly covers null headers, empty collections, zero counts, zero values, and text that is shorter, equal to, or longer than a target width. Row-column mismatches are handled by padding with empty strings rather than throwing.
- **Thread safety:** These are unit test methods with no shared mutable state. They are safe to run in parallel test runners, but they do not themselves test thread safety of the underlying `FormattingHelpers` methods.
- **Immutability:** All methods return `void` and rely on assertions. They do not modify shared state, making them idempotent and safe for repeated execution.
- **Assumptions:** The tests assume the underlying formatting methods are deterministic and culture-invariant where appropriate (e.g., percentage decimal separators). Any culture-sensitive formatting should be verified separately if relevant.
