# ValidationHelpersTests
The `ValidationHelpersTests` class is a test suite designed to validate the functionality of various helper methods used for input validation in the `jira-analytics-cli` project. These tests cover a range of scenarios, including validation of Jira issue keys, project keys, URLs, email addresses, sprint IDs, story points, date ranges, and percentages, as well as string manipulation methods for truncation and sanitization.

## API
* `public void IsValidJiraIssueKey_VariousFormats_ReturnsExpected`: Tests the validation of Jira issue keys in various formats.
* `public void IsValidProjectKey_VariousFormats_ReturnsExpected`: Tests the validation of project keys in various formats.
* `public void IsValidUrl_VariousFormats_ReturnsExpected`: Tests the validation of URLs in various formats.
* `public void IsValidEmail_VariousFormats_ReturnsExpected`: Tests the validation of email addresses in various formats.
* `public void IsValidSprintId_BoundaryValues_ReturnsExpected`: Tests the validation of sprint IDs with boundary values.
* `public void IsValidStoryPoints_Null_ReturnsTrue`: Tests that null story points are considered valid.
* `public void IsValidStoryPoints_Zero_ReturnsTrue`: Tests that zero story points are considered valid.
* `public void IsValidStoryPoints_Negative_ReturnsFalse`: Tests that negative story points are not considered valid.
* `public void IsValidDateRange_StartBeforeEnd_ReturnsTrue`: Tests that date ranges with a start date before the end date are considered valid.
* `public void IsValidDateRange_StartEqualsEnd_ReturnsFalse`: Tests that date ranges with a start date equal to the end date are not considered valid.
* `public void IsValidDateRange_StartAfterEnd_ReturnsFalse`: Tests that date ranges with a start date after the end date are not considered valid.
* `public void IsValidPercentage_BoundaryValues_ReturnsExpected`: Tests the validation of percentages with boundary values.
* `public void TruncateWithEllipsis_NullInput_ReturnsEmpty`: Tests that truncating a null string with an ellipsis returns an empty string.
* `public void TruncateWithEllipsis_EmptyInput_ReturnsEmpty`: Tests that truncating an empty string with an ellipsis returns an empty string.
* `public void TruncateWithEllipsis_ShortInput_ReturnsUnchanged`: Tests that truncating a short string with an ellipsis returns the original string.
* `public void TruncateWithEllipsis_LongInput_TruncatesWithDots`: Tests that truncating a long string with an ellipsis truncates the string and appends an ellipsis.
* `public void SanitizeForCsv_NullInput_ReturnsEmpty`: Tests that sanitizing a null string for CSV output returns an empty string.
* `public void SanitizeForCsv_StringWithCommas_RemovesCommas`: Tests that sanitizing a string with commas for CSV output removes the commas.
* `public void SanitizeForCsv_StringWithNewlines_RemovesNewlines`: Tests that sanitizing a string with newlines for CSV output removes the newlines.
* `public void SanitizeForCsv_StringWithQuotes_RemovesQuotes`: Tests that sanitizing a string with quotes for CSV output removes the quotes.

## Usage
The following examples demonstrate how to use the validation methods in a C# application:
```csharp
// Example 1: Validating a Jira issue key
string issueKey = "PROJ-123";
if (ValidationHelpers.IsValidJiraIssueKey(issueKey))
{
    Console.WriteLine("Issue key is valid");
}
else
{
    Console.WriteLine("Issue key is not valid");
}

// Example 2: Sanitizing a string for CSV output
string input = "Hello, World!\nThis is a test.";
string sanitized = ValidationHelpers.SanitizeForCsv(input);
Console.WriteLine(sanitized);
```

## Notes
The `ValidationHelpersTests` class is designed to be thread-safe, as it does not rely on any shared state or external resources. However, the methods being tested may have their own thread-safety considerations. Additionally, some of the validation methods may have edge cases that are not explicitly tested, such as extremely large or small input values. It is recommended to review the implementation of each method to understand its specific behavior and limitations.
