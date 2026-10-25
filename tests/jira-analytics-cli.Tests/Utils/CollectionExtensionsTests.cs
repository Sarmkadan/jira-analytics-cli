// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Contains unit tests for the <see cref="CollectionExtensions"/> utility class.
/// </summary>
public class CollectionExtensionsTests
{
    // -------------------------------------------------------------------------
    // WhereNotNull
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.WhereNotNull{T}"/> removes null elements
    /// and keeps the remaining elements in their original order.
    /// </summary>
    [Fact]
    public void WhereNotNull_RemovesNullElements_PreservingOrder()
    {
        string?[] source = ["first", null, "second", null, null, "third"];

        var result = source.WhereNotNull().ToList();

        result.Should().Equal("first", "second", "third");
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.WhereNotNull{T}"/> returns an empty sequence
    /// when every element is null.
    /// </summary>
    [Fact]
    public void WhereNotNull_AllNulls_ReturnsEmpty()
    {
        string?[] source = [null, null];

        source.WhereNotNull().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.WhereNotNull{T}"/> returns an empty sequence
    /// for an empty input.
    /// </summary>
    [Fact]
    public void WhereNotNull_EmptySource_ReturnsEmpty()
    {
        IEnumerable<string?> source = [];

        source.WhereNotNull().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.WhereNotNull{T}"/> returns the same instances
    /// that were present in the source (no copying or substitution).
    /// </summary>
    [Fact]
    public void WhereNotNull_ReturnsSameInstances()
    {
        var a = new object();
        var b = new object();
        object?[] source = [a, null, b];

        var result = source.WhereNotNull().ToList();

        result.Should().HaveCount(2);
        result[0].Should().BeSameAs(a);
        result[1].Should().BeSameAs(b);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.WhereNotNull{T}"/> throws
    /// <see cref="ArgumentNullException"/> when the source is null.
    /// </summary>
    [Fact]
    public void WhereNotNull_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<string?>? source = null;

        var act = () => source!.WhereNotNull();

        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }
}
