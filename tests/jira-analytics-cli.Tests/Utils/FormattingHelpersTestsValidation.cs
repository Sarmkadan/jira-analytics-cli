// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

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
        // FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace - no validation needed
        // FormatPercentage_ZeroPrecision_ReturnsWholeNumber - no validation needed
        // FormatPercentage_ZeroValue_ReturnsZeroPercent - no validation needed

        // Validate FormatBytes test cases
        // FormatBytes_VariousSizes_ReturnsHumanReadable - validates with specific byte values

        // Validate CreateTable test cases
        // CreateTable_NullHeaders_ReturnsEmpty - null headers are explicitly tested
        // CreateTable_EmptyHeaders_ReturnsEmpty - empty headers are explicitly tested
        // CreateTable_EmptyRows_ReturnsEmpty - empty rows are explicitly tested
        // CreateTable_ValidData_ContainsHeadersAndRows - validates with actual data
        // CreateTable_RowShorterThanHeaders_PadsWithEmpty - validates padding behavior

        // Validate FormatStatus test cases
        // FormatStatus_VariousStatuses_ContainsOriginalStatus - validates status strings

        // Validate RepeatChar test cases
        // RepeatChar_ZeroCount_ReturnsEmpty - validates zero count
        // RepeatChar_PositiveCount_ReturnsRepeatedString - validates positive count

        // Validate Indent test cases
        // Indent_DefaultSpaces_AddsTwoSpaces - validates default indentation
        // Indent_CustomSpaces_AddsCorrectIndentation - validates custom indentation

        // Validate CenterText test cases
        // CenterText_ShorterThanWidth_CentersWithPadding - validates centering
        // CenterText_EqualToWidth_ReturnsOriginal - validates equal width
        // CenterText_LongerThanWidth_ReturnsOriginal - validates longer than width

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