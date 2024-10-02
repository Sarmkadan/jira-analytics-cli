// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Tests for FormattingHelpers class.
/// </summary>
public class FormattingHelpersTests
{
    // -------------------------------------------------------------------------
    // FormatPercentage
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace()
    {
        /// <summary>
        /// Tests that FormatPercentage returns a string with one decimal place when precision is not specified.
        /// </summary>
        /// <param name="value">The value to format.</param>
        FormattingHelpers.FormatPercentage(75.55).Should().Be("75.5%");
    }

    [Fact]
    public void FormatPercentage_ZeroPrecision_ReturnsWholeNumber()
    {
        /// <summary>
        /// Tests that FormatPercentage returns a whole number when precision is 0.
        /// </summary>
        /// <param name="value">The value to format.</param>
        FormattingHelpers.FormatPercentage(99.9, 0).Should().Be("100%");
    }

    [Fact]
    public void FormatPercentage_ZeroValue_ReturnsZeroPercent()
    {
        /// <summary>
        /// Tests that FormatPercentage returns "0.0%" when the value is 0.
        /// </summary>
        FormattingHelpers.FormatPercentage(0).Should().Be("0.0%");
    }

    // -------------------------------------------------------------------------
    // FormatBytes
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(500, "500 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    public void FormatBytes_VariousSizes_ReturnsHumanReadable(long bytes, string expected)
    {
        /// <summary>
        /// Tests that FormatBytes returns a human-readable string for various sizes.
        /// </summary>
        /// <param name="bytes">The size to format.</param>
        /// <param name="expected">The expected result.</param>
        FormattingHelpers.FormatBytes(bytes).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // CreateTable
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateTable_NullHeaders_ReturnsEmpty()
    {
        /// <summary>
        /// Tests that CreateTable returns an empty string when headers are null.
        /// </summary>
        FormattingHelpers.CreateTable(null!, new List<string[]>()).Should().BeEmpty();
    }

    [Fact]
    public void CreateTable_EmptyHeaders_ReturnsEmpty()
    {
        /// <summary>
        /// Tests that CreateTable returns an empty string when headers are empty.
        /// </summary>
        FormattingHelpers.CreateTable(Array.Empty<string>(), new List<string[]>()).Should().BeEmpty();
    }

    [Fact]
    public void CreateTable_EmptyRows_ReturnsEmpty()
    {
        /// <summary>
        /// Tests that CreateTable returns an empty string when rows are empty.
        /// </summary>
        FormattingHelpers.CreateTable(new[] { "Col1" }, new List<string[]>()).Should().BeEmpty();
    }

    [Fact]
    public void CreateTable_ValidData_ContainsHeadersAndRows()
    {
        /// <summary>
        /// Tests that CreateTable returns a string containing headers and rows when data is valid.
        /// </summary>
        var headers = new[] { "Key", "Status" };
        var rows = new List<string[]>
        {
            new[] { "PROJ-1", "Done" },
            new[] { "PROJ-2", "Open" }
        };

        var result = FormattingHelpers.CreateTable(headers, rows);

        result.Should().Contain("Key");
        result.Should().Contain("Status");
        result.Should().Contain("PROJ-1");
        result.Should().Contain("PROJ-2");
        result.Should().Contain("|");
        result.Should().Contain("-");
    }

    [Fact]
    public void CreateTable_RowShorterThanHeaders_PadsWithEmpty()
    {
        /// <summary>
        /// Tests that CreateTable pads rows with empty strings when they are shorter than headers.
        /// </summary>
        var headers = new[] { "Col1", "Col2", "Col3" };
        var rows = new List<string[]> { new[] { "val1" } }; // only 1 column

        var result = FormattingHelpers.CreateTable(headers, rows);
        result.Should().Contain("val1");
    }

    // -------------------------------------------------------------------------
    // FormatStatus
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Done", "Done")]
    [InlineData("Closed", "Closed")]
    [InlineData("In Progress", "In Progress")]
    [InlineData("Open", "Open")]
    [InlineData("Blocked", "Blocked")]
    [InlineData("Unknown", "Unknown")]
    public void FormatStatus_VariousStatuses_ContainsOriginalStatus(string status, string expectedContains)
    {
        /// <summary>
        /// Tests that FormatStatus returns a string containing the original status.
        /// </summary>
        /// <param name="status">The status to format.</param>
        /// <param name="expectedContains">The expected result.</param>
        FormattingHelpers.FormatStatus(status).Should().Contain(expectedContains);
    }

    // -------------------------------------------------------------------------
    // RepeatChar / Indent / CenterText
    // -------------------------------------------------------------------------

    [Fact]
    public void RepeatChar_ZeroCount_ReturnsEmpty()
    {
        /// <summary>
        /// Tests that RepeatChar returns an empty string when count is 0.
        /// </summary>
        FormattingHelpers.RepeatChar('-', 0).Should().BeEmpty();
    }

    [Fact]
    public void RepeatChar_PositiveCount_ReturnsRepeatedString()
    {
        /// <summary>
        /// Tests that RepeatChar returns a string repeated the specified number of times.
        /// </summary>
        /// <param name="count">The number of times to repeat the character.</param>
        FormattingHelpers.RepeatChar('=', 5).Should().Be("=====");
    }

    [Fact]
    public void Indent_DefaultSpaces_AddsTwoSpaces()
    {
        /// <summary>
        /// Tests that Indent adds two spaces by default.
        /// </summary>
        /// <param name="text">The text to indent.</param>
        FormattingHelpers.Indent("text").Should().Be("  text");
    }

    [Fact]
    public void Indent_CustomSpaces_AddsCorrectIndentation()
    {
        /// <summary>
        /// Tests that Indent adds the specified number of spaces.
        /// </summary>
        /// <param name="text">The text to indent.</param>
        /// <param name="spaces">The number of spaces to add.</param>
        FormattingHelpers.Indent("text", 4).Should().Be("    text");
    }

    [Fact]
    public void CenterText_ShorterThanWidth_CentersWithPadding()
    {
        /// <summary>
        /// Tests that CenterText centers the text with padding when it is shorter than the width.
        /// </summary>
        /// <param name="text">The text to center.</param>
        /// <param name="width">The width to center the text in.</param>
        var result = FormattingHelpers.CenterText("AB", 6);
        result.Should().HaveLength(6);
        result.Trim().Should().Be("AB");
    }

    [Fact]
    public void CenterText_EqualToWidth_ReturnsOriginal()
    {
        /// <summary>
        /// Tests that CenterText returns the original text when it is equal to the width.
        /// </summary>
        /// <param name="text">The text to center.</param>
        /// <param name="width">The width to center the text in.</param>
        FormattingHelpers.CenterText("ABCDE", 5).Should().Be("ABCDE");
    }

    [Fact]
    public void CenterText_LongerThanWidth_ReturnsOriginal()
    {
        /// <summary>
        /// Tests that CenterText returns the original text when it is longer than the width.
        /// </summary>
        /// <param name="text">The text to center.</param>
        /// <param name="width">The width to center the text in.</param>
        FormattingHelpers.CenterText("ABCDEFGH", 5).Should().Be("ABCDEFGH");
    }
}
