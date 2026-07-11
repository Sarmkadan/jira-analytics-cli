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

// ... rest of README content ...
