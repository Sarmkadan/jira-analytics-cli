// ... rest of README content ...
## FormattingHelpersTests

The `FormattingHelpersTests` class provides a set of test methods for verifying the behavior of various formatting helper methods. These methods are designed to format data in a human-readable way, making it easier to understand and work with.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Utils;

// Create a test instance of FormattingHelpersTests
var formattingHelpersTests = new FormattingHelpersTests();

// Test that the `FormatPercentage` method returns a percentage with one decimal place by default
formattingHelpersTests.FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace();

// Test that the `FormatBytes` method returns a human-readable representation of a byte size
formattingHelpersTests.FormatBytes_VariousSizes_ReturnsHumanReadable();

// Test that the `CreateTable` method returns an empty table when given null headers
formattingHelpersTests.CreateTable_NullHeaders_ReturnsEmpty();

// Test that the `FormatStatus` method returns the original status when given a valid status
formattingHelpersTests.FormatStatus_VariousStatuses_ContainsOriginalStatus();

// Test that the `RepeatChar` method returns an empty string when given a count of zero
formattingHelpersTests.RepeatChar_ZeroCount_ReturnsEmpty();

// Test that the `Indent` method adds two spaces by default
formattingHelpersTests.Indent_DefaultSpaces_AddsTwoSpaces();

// Test that the `CenterText` method centers text within a given width
formattingHelpersTests.CenterText_ShorterThanWidth_CentersWithPadding();
```

## ValidationHelpersTests

The `ValidationHelpersTests` class provides a comprehensive suite of unit tests that validate the correctness of various input validation helpers used throughout the application. It covers scenarios for Jira issue keys, project keys, URLs, email addresses, sprint IDs, story points, date ranges, percentages, string truncation, and CSV sanitization, ensuring that each helper behaves as expected across a range of edge cases.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Utils;

// Create an instance of the test class
var validationTests = new ValidationHelpersTests();

// Run a few representative validation tests
validationTests.IsValidJiraIssueKey_VariousFormats_ReturnsExpected();
validationTests.IsValidProjectKey_VariousFormats_ReturnsExpected();
validationTests.IsValidUrl_VariousFormats_ReturnsExpected();
validationTests.IsValidEmail_VariousFormats_ReturnsExpected();
validationTests.IsValidSprintId_BoundaryValues_ReturnsExpected();
validationTests.IsValidStoryPoints_Null_ReturnsTrue();
validationTests.IsValidStoryPoints_Zero_ReturnsTrue();
validationTests.IsValidStoryPoints_Negative_ReturnsFalse();
validationTests.IsValidDateRange_StartBeforeEnd_ReturnsTrue();
validationTests.IsValidDateRange_StartEqualsEnd_ReturnsFalse();
validationTests.IsValidDateRange_StartAfterEnd_ReturnsFalse();
validationTests.IsValidPercentage_BoundaryValues_ReturnsExpected();
validationTests.TruncateWithEllipsis_NullInput_ReturnsEmpty();
validationTests.TruncateWithEllipsis_EmptyInput_ReturnsEmpty();
validationTests.TruncateWithEllipsis_ShortInput_ReturnsUnchanged();
validationTests.TruncateWithEllipsis_LongInput_TruncatesWithDots();
validationTests.SanitizeForCsv_NullInput_ReturnsEmpty();
validationTests.SanitizeForCsv_StringWithCommas_RemovesCommas();
validationTests.SanitizeForCsv_StringWithNewlines_RemovesNewlines();
validationTests.SanitizeForCsv_StringWithQuotes_RemovesQuotes();
```

# ... rest of README content ...
