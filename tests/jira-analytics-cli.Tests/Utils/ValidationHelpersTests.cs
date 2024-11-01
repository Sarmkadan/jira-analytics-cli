// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Contains unit tests for the <see cref="ValidationHelpers"/> utility class.
/// Tests various validation methods for Jira-related data formats including issue keys,
/// project keys, URLs, emails, sprint IDs, story points, date ranges, percentages,
/// string truncation, CSV sanitization, and progress bar generation.
/// </summary>
public partial class ValidationHelpersTests
{
    // -------------------------------------------------------------------------
    // IsValidJiraIssueKey
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidJiraIssueKey"/> correctly validates various Jira issue key formats.
    /// Validates that issue keys must be uppercase, contain a dash, have a project prefix,
    /// and end with a numeric identifier.
    /// </summary>
    /// <param name="key">The issue key to validate.</param>
    /// <param name="expected">The expected validation result (true for valid, false for invalid).</param>
    [Theory]
    [InlineData("PROJ-123", true)]
    [InlineData("AB-1", true)]
    [InlineData("A2B-99", true)]
    [InlineData("proj-123", false)] // lowercase
    [InlineData("PROJ123", false)] // no dash
    [InlineData("PROJ-", false)] // no number
    [InlineData("-123", false)] // no project
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData(" ", false)]
    [InlineData("1PROJ-1", false)] // starts with digit
    public void IsValidJiraIssueKey_VariousFormats_ReturnsExpected(string? key, bool expected)
    {
        ValidationHelpers.IsValidJiraIssueKey(key).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // IsValidProjectKey
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidProjectKey"/> correctly validates various Jira project key formats.
    /// Validates that project keys must be uppercase, start with a letter, and be between 1-10 characters long.
    /// </summary>
    /// <param name="key">The project key to validate.</param>
    /// <param name="expected">The expected validation result (true for valid, false for invalid).</param>
    [Theory]
    [InlineData("PROJ", true)]
    [InlineData("A", true)]
    [InlineData("AB2C", true)]
    [InlineData("ABCDEFGHIJ", true)] // exactly 10 chars
    [InlineData("ABCDEFGHIJK", false)] // 11 chars - too long
    [InlineData("proj", false)] // lowercase
    [InlineData("1PROJ", false)] // starts with digit
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidProjectKey_VariousFormats_ReturnsExpected(string? key, bool expected)
    {
        ValidationHelpers.IsValidProjectKey(key).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // IsValidUrl
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidUrl"/> correctly validates various URL formats.
    /// Validates that URLs must use http:// or https:// protocols and be properly formatted.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="expected">The expected validation result (true for valid, false for invalid).</param>
    [Theory]
    [InlineData("https://jira.example.com", true)]
    [InlineData("http://localhost:8080", true)]
    [InlineData("ftp://files.example.com", false)] // ftp not allowed
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidUrl_VariousFormats_ReturnsExpected(string? url, bool expected)
    {
        ValidationHelpers.IsValidUrl(url).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // IsValidEmail
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidEmail"/> correctly validates various email formats.
    /// Validates that emails must contain an @ symbol and have a valid domain structure.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <param name="expected">The expected validation result (true for valid, false for invalid).</param>
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("name.surname@domain.org", true)]
    [InlineData("notanemail", false)]
    [InlineData("@domain.com", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_VariousFormats_ReturnsExpected(string? email, bool expected)
    {
        ValidationHelpers.IsValidEmail(email).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // IsValidSprintId / IsValidStoryPoints
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidSprintId"/> correctly validates sprint ID values.
    /// Validates that sprint IDs must be positive integers.
    /// </summary>
    /// <param name="id">The sprint ID to validate.</param>
    /// <param name="expected">The expected validation result (true for valid, false for invalid).</param>
    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValidSprintId_BoundaryValues_ReturnsExpected(int id, bool expected)
    {
        ValidationHelpers.IsValidSprintId(id).Should().Be(expected);
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidStoryPoints"/> returns true when passed null.
    /// Null story points are considered valid (no points assigned).
    /// </summary>
    [Fact]
    public void IsValidStoryPoints_Null_ReturnsTrue()
    {
        ValidationHelpers.IsValidStoryPoints(null).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidStoryPoints"/> returns true when passed zero.
    /// Zero story points are considered valid.
    /// </summary>
    [Fact]
    public void IsValidStoryPoints_Zero_ReturnsTrue()
    {
        ValidationHelpers.IsValidStoryPoints(0).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidStoryPoints"/> returns false when passed a negative value.
    /// Negative story points are considered invalid.
    /// </summary>
    [Fact]
    public void IsValidStoryPoints_Negative_ReturnsFalse()
    {
        ValidationHelpers.IsValidStoryPoints(-1).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsValidDateRange / IsValidPercentage
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidDateRange"/> returns true when the start date is before the end date.
    /// Validates that date ranges must have a start date that precedes the end date.
    /// </summary>
    [Fact]
    public void IsValidDateRange_StartBeforeEnd_ReturnsTrue()
    {
        ValidationHelpers.IsValidDateRange(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidDateRange"/> returns false when the start date equals the end date.
    /// Equal dates are not considered valid date ranges.
    /// </summary>
    [Fact]
    public void IsValidDateRange_StartEqualsEnd_ReturnsFalse()
    {
        var now = DateTime.UtcNow;
        ValidationHelpers.IsValidDateRange(now, now).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidDateRange"/> returns false when the start date is after the end date.
    /// Reverse date ranges are not considered valid.
    /// </summary>
    [Fact]
    public void IsValidDateRange_StartAfterEnd_ReturnsFalse()
    {
        ValidationHelpers.IsValidDateRange(DateTime.UtcNow.AddDays(1), DateTime.UtcNow).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.IsValidPercentage"/> correctly validates percentage values.
    /// Validates that percentages must be between 0 and 100 inclusive.
    /// </summary>
    /// <param name="value">The percentage value to validate.</param>
    /// <param name="expected">The expected validation result (true for valid, false for invalid).</param>
    [Theory]
    [InlineData(0, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(-0.1, false)]
    [InlineData(100.1, false)]
    public void IsValidPercentage_BoundaryValues_ReturnsExpected(double value, bool expected)
    {
        ValidationHelpers.IsValidPercentage(value).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // TruncateWithEllipsis
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.TruncateWithEllipsis"/> returns an empty string when passed null.
    /// Null inputs should result in empty string output.
    /// </summary>
    [Fact]
    public void TruncateWithEllipsis_NullInput_ReturnsEmpty()
    {
        ValidationHelpers.TruncateWithEllipsis(null).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.TruncateWithEllipsis"/> returns an empty string when passed an empty string.
    /// Empty inputs should result in empty string output.
    /// </summary>
    [Fact]
    public void TruncateWithEllipsis_EmptyInput_ReturnsEmpty()
    {
        ValidationHelpers.TruncateWithEllipsis("").Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.TruncateWithEllipsis"/> returns the input unchanged when it's shorter than the specified limit.
    /// Short strings should not be truncated.
    /// </summary>
    [Fact]
    public void TruncateWithEllipsis_ShortInput_ReturnsUnchanged()
    {
        ValidationHelpers.TruncateWithEllipsis("short", 50).Should().Be("short");
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.TruncateWithEllipsis"/> truncates long strings and adds ellipsis.
    /// Long strings should be truncated to the specified length with "..." appended.
    /// </summary>
    [Fact]
    public void TruncateWithEllipsis_LongInput_TruncatesWithDots()
    {
        var result = ValidationHelpers.TruncateWithEllipsis("A very long string that exceeds the limit", 20);
        result.Should().HaveLength(20);
        result.Should().EndWith("...");
    }

    // -------------------------------------------------------------------------
    // SanitizeForCsv
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.SanitizeForCsv"/> returns an empty string when passed null.
    /// Null inputs should result in empty string output for CSV compatibility.
    /// </summary>
    [Fact]
    public void SanitizeForCsv_NullInput_ReturnsEmpty()
    {
        ValidationHelpers.SanitizeForCsv(null).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.SanitizeForCsv"/> removes commas from strings.
    /// Commas are removed to prevent CSV formatting issues.
    /// </summary>
    [Fact]
    public void SanitizeForCsv_StringWithCommas_RemovesCommas()
    {
        ValidationHelpers.SanitizeForCsv("one,two,three").Should().Be("onetwothree");
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.SanitizeForCsv"/> removes newlines from strings.
    /// Newlines are removed to prevent CSV formatting issues.
    /// </summary>
    [Fact]
    public void SanitizeForCsv_StringWithNewlines_RemovesNewlines()
    {
        ValidationHelpers.SanitizeForCsv("line1\nline2\rline3").Should().Be("line1line2line3");
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.SanitizeForCsv"/> removes quotes from strings.
    /// Quotes are removed to prevent CSV formatting issues.
    /// </summary>
    [Fact]
    public void SanitizeForCsv_StringWithQuotes_RemovesQuotes()
    {
        ValidationHelpers.SanitizeForCsv("say \"hello\"").Should().Be("say hello");
    }

    // -------------------------------------------------------------------------
    // ToProgressBar
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.ToProgressBar"/> returns a progress bar with all empty blocks for 0%.
    /// Zero percent should result in a progress bar with no filled blocks.
    /// </summary>
    [Fact]
    public void ToProgressBar_ZeroPercent_ReturnsAllEmpty()
    {
        var result = ValidationHelpers.ToProgressBar(0, 10);
        result.Should().StartWith("[").And.EndWith("]");
        result.Should().NotContain("█"); // no filled blocks
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.ToProgressBar"/> returns a progress bar with all filled blocks for 100%.
    /// 100% should result in a progress bar with no empty blocks.
    /// </summary>
    [Fact]
    public void ToProgressBar_HundredPercent_ReturnsAllFilled()
    {
        var result = ValidationHelpers.ToProgressBar(100, 10);
        result.Should().NotContain("░"); // no empty blocks (except brackets)
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.ToProgressBar"/> treats negative percentages as 0.
    /// Negative values should be clamped to 0.
    /// </summary>
    [Fact]
    public void ToProgressBar_NegativePercent_TreatsAsZero()
    {
        var result = ValidationHelpers.ToProgressBar(-10, 10);
        result.Should().StartWith("[");
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelpers.ToProgressBar"/> treats values over 100% as 100%.
    /// Values over 100 should be clamped to 100.
    /// </summary>
    [Fact]
    public void ToProgressBar_OverHundred_TreatsAsZero()
    {
        var result = ValidationHelpers.ToProgressBar(150, 10);
        result.Should().StartWith("[");
    }
}