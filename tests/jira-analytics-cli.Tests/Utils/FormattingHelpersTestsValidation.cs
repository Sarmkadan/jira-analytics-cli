// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Reflection;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Validation helpers for FormattingHelpersTests to ensure test data is valid.
/// </summary>
public static class FormattingHelpersTestsValidation
{
    /// <summary>
    /// Validates a FormattingHelpersTests instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The FormattingHelpersTests instance to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this FormattingHelpersTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate FormatPercentage test cases
        problems.AddRange(ValidateFormatPercentageTests(value));

        // Validate FormatBytes test cases
        problems.AddRange(ValidateFormatBytesTests(value));

        // Validate CreateTable test cases
        problems.AddRange(ValidateCreateTableTests(value));

        // Validate FormatStatus test cases
        problems.AddRange(ValidateFormatStatusTests(value));

        // Validate RepeatChar test cases
        problems.AddRange(ValidateRepeatCharTests(value));

        // Validate Indent test cases
        problems.AddRange(ValidateIndentTests(value));

        // Validate CenterText test cases
        problems.AddRange(ValidateCenterTextTests(value));

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateFormatPercentageTests(FormattingHelpersTests tests)
    {
        var problems = new List<string>();

        // Test that FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace exists
        var method = typeof(FormattingHelpersTests).GetMethod("FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace");
        if (method == null)
        {
            problems.Add("Missing test method: FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace");
        }

        // Test that FormatPercentage_ZeroPrecision_ReturnsWholeNumber exists
        method = typeof(FormattingHelpersTests).GetMethod("FormatPercentage_ZeroPrecision_ReturnsWholeNumber");
        if (method == null)
        {
            problems.Add("Missing test method: FormatPercentage_ZeroPrecision_ReturnsWholeNumber");
        }

        // Test that FormatPercentage_ZeroValue_ReturnsZeroPercent exists
        method = typeof(FormattingHelpersTests).GetMethod("FormatPercentage_ZeroValue_ReturnsZeroPercent");
        if (method == null)
        {
            problems.Add("Missing test method: FormatPercentage_ZeroValue_ReturnsZeroPercent");
        }

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateFormatBytesTests(FormattingHelpersTests tests)
    {
        var problems = new List<string>();

        // Test that FormatBytes_VariousSizes_ReturnsHumanReadable exists
        var method = typeof(FormattingHelpersTests).GetMethod("FormatBytes_VariousSizes_ReturnsHumanReadable");
        if (method == null)
        {
            problems.Add("Missing test method: FormatBytes_VariousSizes_ReturnsHumanReadable");
        }
        else
        {
            // Verify it has the correct attributes
            var attributes = method.GetCustomAttributes(typeof(TheoryAttribute), false);
            if (attributes.Length == 0)
            {
                problems.Add("FormatBytes_VariousSizes_ReturnsHumanReadable should be marked with [Theory]");
            }
        }

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateCreateTableTests(FormattingHelpersTests tests)
    {
        var problems = new List<string>();

        // Test that CreateTable_NullHeaders_ReturnsEmpty exists
        var method = typeof(FormattingHelpersTests).GetMethod("CreateTable_NullHeaders_ReturnsEmpty");
        if (method == null)
        {
            problems.Add("Missing test method: CreateTable_NullHeaders_ReturnsEmpty");
        }

        // Test that CreateTable_EmptyHeaders_ReturnsEmpty exists
        method = typeof(FormattingHelpersTests).GetMethod("CreateTable_EmptyHeaders_ReturnsEmpty");
        if (method == null)
        {
            problems.Add("Missing test method: CreateTable_EmptyHeaders_ReturnsEmpty");
        }

        // Test that CreateTable_EmptyRows_ReturnsEmpty exists
        method = typeof(FormattingHelpersTests).GetMethod("CreateTable_EmptyRows_ReturnsEmpty");
        if (method == null)
        {
            problems.Add("Missing test method: CreateTable_EmptyRows_ReturnsEmpty");
        }

        // Test that CreateTable_ValidData_ContainsHeadersAndRows exists
        method = typeof(FormattingHelpersTests).GetMethod("CreateTable_ValidData_ContainsHeadersAndRows");
        if (method == null)
        {
            problems.Add("Missing test method: CreateTable_ValidData_ContainsHeadersAndRows");
        }

        // Test that CreateTable_RowShorterThanHeaders_PadsWithEmpty exists
        method = typeof(FormattingHelpersTests).GetMethod("CreateTable_RowShorterThanHeaders_PadsWithEmpty");
        if (method == null)
        {
            problems.Add("Missing test method: CreateTable_RowShorterThanHeaders_PadsWithEmpty");
        }

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateFormatStatusTests(FormattingHelpersTests tests)
    {
        var problems = new List<string>();

        // Test that FormatStatus_VariousStatuses_ContainsOriginalStatus exists
        var method = typeof(FormattingHelpersTests).GetMethod("FormatStatus_VariousStatuses_ContainsOriginalStatus");
        if (method == null)
        {
            problems.Add("Missing test method: FormatStatus_VariousStatuses_ContainsOriginalStatus");
        }
        else
        {
            // Verify it has the correct attributes
            var attributes = method.GetCustomAttributes(typeof(TheoryAttribute), false);
            if (attributes.Length == 0)
            {
                problems.Add("FormatStatus_VariousStatuses_ContainsOriginalStatus should be marked with [Theory]");
            }
        }

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateRepeatCharTests(FormattingHelpersTests tests)
    {
        var problems = new List<string>();

        // Test that RepeatChar_ZeroCount_ReturnsEmpty exists
        var method = typeof(FormattingHelpersTests).GetMethod("RepeatChar_ZeroCount_ReturnsEmpty");
        if (method == null)
        {
            problems.Add("Missing test method: RepeatChar_ZeroCount_ReturnsEmpty");
        }

        // Test that RepeatChar_PositiveCount_ReturnsRepeatedString exists
        method = typeof(FormattingHelpersTests).GetMethod("RepeatChar_PositiveCount_ReturnsRepeatedString");
        if (method == null)
        {
            problems.Add("Missing test method: RepeatChar_PositiveCount_ReturnsRepeatedString");
        }

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateIndentTests(FormattingHelpersTests tests)
    {
        var problems = new List<string>();

        // Test that Indent_DefaultSpaces_AddsTwoSpaces exists
        var method = typeof(FormattingHelpersTests).GetMethod("Indent_DefaultSpaces_AddsTwoSpaces");
        if (method == null)
        {
            problems.Add("Missing test method: Indent_DefaultSpaces_AddsTwoSpaces");
        }

        // Test that Indent_CustomSpaces_AddsCorrectIndentation exists
        method = typeof(FormattingHelpersTests).GetMethod("Indent_CustomSpaces_AddsCorrectIndentation");
        if (method == null)
        {
            problems.Add("Missing test method: Indent_CustomSpaces_AddsCorrectIndentation");
        }

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateCenterTextTests(FormattingHelpersTests tests)
    {
        var problems = new List<string>();

        // Test that CenterText_ShorterThanWidth_CentersWithPadding exists
        var method = typeof(FormattingHelpersTests).GetMethod("CenterText_ShorterThanWidth_CentersWithPadding");
        if (method == null)
        {
            problems.Add("Missing test method: CenterText_ShorterThanWidth_CentersWithPadding");
        }

        // Test that CenterText_EqualToWidth_ReturnsOriginal exists
        method = typeof(FormattingHelpersTests).GetMethod("CenterText_EqualToWidth_ReturnsOriginal");
        if (method == null)
        {
            problems.Add("Missing test method: CenterText_EqualToWidth_ReturnsOriginal");
        }

        // Test that CenterText_LongerThanWidth_ReturnsOriginal exists
        method = typeof(FormattingHelpersTests).GetMethod("CenterText_LongerThanWidth_ReturnsOriginal");
        if (method == null)
        {
            problems.Add("Missing test method: CenterText_LongerThanWidth_ReturnsOriginal");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a FormattingHelpersTests instance is valid.
    /// </summary>
    /// <param name="value">The FormattingHelpersTests instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this FormattingHelpersTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a FormattingHelpersTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The FormattingHelpersTests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if value contains validation problems.</exception>
    public static void EnsureValid(this FormattingHelpersTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FormattingHelpersTests instance is invalid:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems.Select(p => $"  - {p}")));
        }
    }
}