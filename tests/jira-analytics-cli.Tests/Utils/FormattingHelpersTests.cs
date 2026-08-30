// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

public class FormattingHelpersTests
{
    [Theory]
    [InlineData(12.34, 1, "12.3%")]
    [InlineData(12.345, 2, "12.35%")]
    [InlineData(-12.34, 1, "-12.3%")]
    public void FormatPercentage_ReturnsExpectedValue(
        double value,
        int decimalPlaces,
        string expected)
    {
        Assert.Equal(expected, FormattingHelpers.FormatPercentage(value, decimalPlaces));
    }

    [Fact]
    public void FormatPercentage_UsesOneDecimalPlaceByDefault()
    {
        Assert.Equal("42.5%", FormattingHelpers.FormatPercentage(42.5));
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    public void FormatBytes_FormatsUnitBoundaries(long bytes, string expected)
    {
        Assert.Equal(expected, FormattingHelpers.FormatBytes(bytes));
    }

    [Fact]
    public void CreateTable_WhenHeadersAreWiderThanRows_PadsValuesToHeaderWidth()
    {
        var headers = new[] { "Identifier", "Current Status" };
        var rows = new List<string[]> { new[] { "A", "Done" } };

        var result = FormattingHelpers.CreateTable(headers, rows);

        var expected =
            "| Identifier | Current Status | " + Environment.NewLine +
            "|------------|----------------|" + Environment.NewLine +
            "| A          | Done           | " + Environment.NewLine;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CreateTable_WithNoRows_ReturnsEmptyString()
    {
        Assert.Equal(
            string.Empty,
            FormattingHelpers.CreateTable(new[] { "Header" }, new List<string[]>()));
    }

    [Theory]
    [InlineData("Done", "✅ Done")]
    [InlineData("Closed", "✅ Closed")]
    [InlineData("In Progress", "🔄 In Progress")]
    [InlineData("In Review", "🔄 In Review")]
    [InlineData("Open", "📋 Open")]
    [InlineData("Blocked", "🚫 Blocked")]
    [InlineData("On Hold", "⏸️  On Hold")]
    [InlineData("Unknown", "❓ Unknown")]
    public void FormatStatus_ReturnsExpectedIndicator(string status, string expected)
    {
        Assert.Equal(expected, FormattingHelpers.FormatStatus(status));
    }

    [Fact]
    public void RepeatChar_WithZeroCount_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, FormattingHelpers.RepeatChar('-', 0));
    }

    [Fact]
    public void RepeatChar_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FormattingHelpers.RepeatChar('-', -1));
    }

    [Fact]
    public void Indent_WithMultiLineText_PrefixesTheTextOnce()
    {
        var text = "first line" + Environment.NewLine + "second line";

        var result = FormattingHelpers.Indent(text, 4);

        Assert.Equal("    " + text, result);
    }

    [Fact]
    public void CenterText_WhenTextIsLongerThanWidth_ReturnsOriginalText()
    {
        const string text = "longer than width";

        Assert.Equal(text, FormattingHelpers.CenterText(text, 5));
    }
}
