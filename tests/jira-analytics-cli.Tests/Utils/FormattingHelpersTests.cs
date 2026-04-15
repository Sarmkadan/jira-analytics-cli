// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

public class FormattingHelpersTests
{
    // -------------------------------------------------------------------------
    // FormatPercentage
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatPercentage_DefaultPrecision_ReturnsOneDecimalPlace()
    {
        FormattingHelpers.FormatPercentage(75.55).Should().Be("75.6%");
    }

    [Fact]
    public void FormatPercentage_ZeroPrecision_ReturnsWholeNumber()
    {
        FormattingHelpers.FormatPercentage(99.9, 0).Should().Be("100%");
    }

    [Fact]
    public void FormatPercentage_ZeroValue_ReturnsZeroPercent()
    {
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
        FormattingHelpers.FormatBytes(bytes).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // CreateTable
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateTable_NullHeaders_ReturnsEmpty()
    {
        FormattingHelpers.CreateTable(null!, new List<string[]>()).Should().BeEmpty();
    }

    [Fact]
    public void CreateTable_EmptyHeaders_ReturnsEmpty()
    {
        FormattingHelpers.CreateTable(Array.Empty<string>(), new List<string[]>()).Should().BeEmpty();
    }

    [Fact]
    public void CreateTable_EmptyRows_ReturnsEmpty()
    {
        FormattingHelpers.CreateTable(new[] { "Col1" }, new List<string[]>()).Should().BeEmpty();
    }

    [Fact]
    public void CreateTable_ValidData_ContainsHeadersAndRows()
    {
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
        FormattingHelpers.FormatStatus(status).Should().Contain(expectedContains);
    }

    // -------------------------------------------------------------------------
    // RepeatChar / Indent / CenterText
    // -------------------------------------------------------------------------

    [Fact]
    public void RepeatChar_ZeroCount_ReturnsEmpty()
    {
        FormattingHelpers.RepeatChar('-', 0).Should().BeEmpty();
    }

    [Fact]
    public void RepeatChar_PositiveCount_ReturnsRepeatedString()
    {
        FormattingHelpers.RepeatChar('=', 5).Should().Be("=====");
    }

    [Fact]
    public void Indent_DefaultSpaces_AddsTwoSpaces()
    {
        FormattingHelpers.Indent("text").Should().Be("  text");
    }

    [Fact]
    public void Indent_CustomSpaces_AddsCorrectIndentation()
    {
        FormattingHelpers.Indent("text", 4).Should().Be("    text");
    }

    [Fact]
    public void CenterText_ShorterThanWidth_CentersWithPadding()
    {
        var result = FormattingHelpers.CenterText("AB", 6);
        result.Should().HaveLength(6);
        result.Trim().Should().Be("AB");
    }

    [Fact]
    public void CenterText_EqualToWidth_ReturnsOriginal()
    {
        FormattingHelpers.CenterText("ABCDE", 5).Should().Be("ABCDE");
    }

    [Fact]
    public void CenterText_LongerThanWidth_ReturnsOriginal()
    {
        FormattingHelpers.CenterText("ABCDEFGH", 5).Should().Be("ABCDEFGH");
    }
}
