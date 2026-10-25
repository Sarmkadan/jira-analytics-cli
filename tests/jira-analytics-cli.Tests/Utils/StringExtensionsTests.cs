// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Contains unit tests for the <see cref="StringExtensions"/> utility class.
/// </summary>
public class StringExtensionsTests
{
    // -------------------------------------------------------------------------
    // EnsurePrefix
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="StringExtensions.EnsurePrefix"/> adds the prefix only when it is missing.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="prefix">The prefix to ensure.</param>
    /// <param name="expected">The expected result.</param>
    [Theory]
    [InlineData("rest/api/2/issue", "/", "/rest/api/2/issue")]
    [InlineData("/rest/api/2/issue", "/", "/rest/api/2/issue")]
    [InlineData("123", "PROJ-", "PROJ-123")]
    [InlineData("PROJ-123", "PROJ-", "PROJ-123")]
    [InlineData("", "https://", "https://")]
    [InlineData("anything", "", "anything")]
    [InlineData("", "", "")]
    public void EnsurePrefix_AddsPrefixOnlyWhenMissing(string value, string prefix, string expected)
    {
        value.EnsurePrefix(prefix).Should().Be(expected);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.EnsurePrefix"/> compares case-sensitively (ordinal),
    /// so a differently-cased prefix is still treated as missing.
    /// </summary>
    [Fact]
    public void EnsurePrefix_IsCaseSensitive()
    {
        "proj-123".EnsurePrefix("PROJ-").Should().Be("PROJ-proj-123");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.EnsurePrefix"/> is idempotent: applying it twice
    /// yields the same result as applying it once.
    /// </summary>
    [Fact]
    public void EnsurePrefix_IsIdempotent()
    {
        var once = "api/issue".EnsurePrefix("/");
        var twice = once.EnsurePrefix("/");

        twice.Should().Be(once);
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.EnsurePrefix"/> throws when the value is null.
    /// </summary>
    [Fact]
    public void EnsurePrefix_ThrowsWhenValueIsNull()
    {
        string? value = null;

        var act = () => value!.EnsurePrefix("/");

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.EnsurePrefix"/> throws when the prefix is null.
    /// </summary>
    [Fact]
    public void EnsurePrefix_ThrowsWhenPrefixIsNull()
    {
        var act = () => "value".EnsurePrefix(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("prefix");
    }
}
