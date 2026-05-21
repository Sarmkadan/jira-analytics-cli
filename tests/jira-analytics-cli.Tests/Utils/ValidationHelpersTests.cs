// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

public class ValidationHelpersTests
{
    // -------------------------------------------------------------------------
    // IsValidJiraIssueKey
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("PROJ-123", true)]
    [InlineData("AB-1", true)]
    [InlineData("A2B-99", true)]
    [InlineData("proj-123", false)]    // lowercase
    [InlineData("PROJ123", false)]     // no dash
    [InlineData("PROJ-", false)]       // no number
    [InlineData("-123", false)]        // no project
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    [InlineData("1PROJ-1", false)]     // starts with digit
    public void IsValidJiraIssueKey_VariousFormats_ReturnsExpected(string? key, bool expected)
    {
        ValidationHelpers.IsValidJiraIssueKey(key).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // IsValidProjectKey
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("PROJ", true)]
    [InlineData("A", true)]
    [InlineData("AB2C", true)]
    [InlineData("ABCDEFGHIJ", true)]   // exactly 10 chars
    [InlineData("ABCDEFGHIJK", false)] // 11 chars - too long
    [InlineData("proj", false)]        // lowercase
    [InlineData("1PROJ", false)]       // starts with digit
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidProjectKey_VariousFormats_ReturnsExpected(string? key, bool expected)
    {
        ValidationHelpers.IsValidProjectKey(key).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // IsValidUrl
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("https://jira.example.com", true)]
    [InlineData("http://localhost:8080", true)]
    [InlineData("ftp://files.example.com", false)]  // ftp not allowed
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

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValidSprintId_BoundaryValues_ReturnsExpected(int id, bool expected)
    {
        ValidationHelpers.IsValidSprintId(id).Should().Be(expected);
    }

    [Fact]
    public void IsValidStoryPoints_Null_ReturnsTrue()
    {
        ValidationHelpers.IsValidStoryPoints(null).Should().BeTrue();
    }

    [Fact]
    public void IsValidStoryPoints_Zero_ReturnsTrue()
    {
        ValidationHelpers.IsValidStoryPoints(0).Should().BeTrue();
    }

    [Fact]
    public void IsValidStoryPoints_Negative_ReturnsFalse()
    {
        ValidationHelpers.IsValidStoryPoints(-1).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // IsValidDateRange / IsValidPercentage
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValidDateRange_StartBeforeEnd_ReturnsTrue()
    {
        ValidationHelpers.IsValidDateRange(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)).Should().BeTrue();
    }

    [Fact]
    public void IsValidDateRange_StartEqualsEnd_ReturnsFalse()
    {
        var now = DateTime.UtcNow;
        ValidationHelpers.IsValidDateRange(now, now).Should().BeFalse();
    }

    [Fact]
    public void IsValidDateRange_StartAfterEnd_ReturnsFalse()
    {
        ValidationHelpers.IsValidDateRange(DateTime.UtcNow.AddDays(1), DateTime.UtcNow).Should().BeFalse();
    }

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

    [Fact]
    public void TruncateWithEllipsis_NullInput_ReturnsEmpty()
    {
        ValidationHelpers.TruncateWithEllipsis(null).Should().BeEmpty();
    }

    [Fact]
    public void TruncateWithEllipsis_EmptyInput_ReturnsEmpty()
    {
        ValidationHelpers.TruncateWithEllipsis("").Should().BeEmpty();
    }

    [Fact]
    public void TruncateWithEllipsis_ShortInput_ReturnsUnchanged()
    {
        ValidationHelpers.TruncateWithEllipsis("short", 50).Should().Be("short");
    }

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

    [Fact]
    public void SanitizeForCsv_NullInput_ReturnsEmpty()
    {
        ValidationHelpers.SanitizeForCsv(null).Should().BeEmpty();
    }

    [Fact]
    public void SanitizeForCsv_StringWithCommas_RemovesCommas()
    {
        ValidationHelpers.SanitizeForCsv("one,two,three").Should().Be("onetwothree");
    }

    [Fact]
    public void SanitizeForCsv_StringWithNewlines_RemovesNewlines()
    {
        ValidationHelpers.SanitizeForCsv("line1\nline2\rline3").Should().Be("line1line2line3");
    }

    [Fact]
    public void SanitizeForCsv_StringWithQuotes_RemovesQuotes()
    {
        ValidationHelpers.SanitizeForCsv("say \"hello\"").Should().Be("say hello");
    }

    // -------------------------------------------------------------------------
    // ToProgressBar
    // -------------------------------------------------------------------------

    [Fact]
    public void ToProgressBar_ZeroPercent_ReturnsAllEmpty()
    {
        var result = ValidationHelpers.ToProgressBar(0, 10);
        result.Should().StartWith("[").And.EndWith("]");
        result.Should().NotContain("█"); // no filled blocks
    }

    [Fact]
    public void ToProgressBar_HundredPercent_ReturnsAllFilled()
    {
        var result = ValidationHelpers.ToProgressBar(100, 10);
        result.Should().NotContain("░"); // no empty blocks (except brackets)
    }

    [Fact]
    public void ToProgressBar_NegativePercent_TreatsAsZero()
    {
        var result = ValidationHelpers.ToProgressBar(-10, 10);
        result.Should().StartWith("[");
    }

    [Fact]
    public void ToProgressBar_OverHundred_TreatsAsZero()
    {
        var result = ValidationHelpers.ToProgressBar(150, 10);
        result.Should().StartWith("[");
    }
}
