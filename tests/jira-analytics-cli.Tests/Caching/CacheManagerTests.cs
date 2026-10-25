// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Caching;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JiraAnalyticsCli.Tests.Caching;

/// <summary>
/// Contains unit tests for the <see cref="CacheManager"/> class.
/// Tests argument guards on public members and basic store round-trips.
/// </summary>
public class CacheManagerTests
{
    private static CacheManager CreateManager() =>
        new(NullLogger<CacheManager>.Instance);

    private static CachePolicy CreatePolicy(string key) =>
        CachePolicy.WithAbsoluteExpiration(key, TimeSpan.FromMinutes(5));

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that the constructor rejects a null logger.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new CacheManager(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -------------------------------------------------------------------------
    // GetStore
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CacheManager.GetStore"/> rejects a null store name.
    /// </summary>
    [Fact]
    public void GetStore_NullStoreName_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.GetStore(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("storeName");
    }

    // -------------------------------------------------------------------------
    // SetDefault / GetDefault
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CacheManager.SetDefault{T}"/> rejects a null key.
    /// </summary>
    [Fact]
    public void SetDefault_NullKey_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.SetDefault<string?>(null!, "value");

        act.Should().Throw<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.SetDefault{T}"/> rejects a null value.
    /// </summary>
    [Fact]
    public void SetDefault_NullValue_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.SetDefault<string?>("key", null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.GetDefault{T}"/> rejects a null key.
    /// </summary>
    [Fact]
    public void GetDefault_NullKey_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.GetDefault<string>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that a value stored with <see cref="CacheManager.SetDefault{T}"/> is returned by <see cref="CacheManager.GetDefault{T}"/>.
    /// </summary>
    [Fact]
    public void SetDefault_ThenGetDefault_ReturnsStoredValue()
    {
        var manager = CreateManager();

        manager.SetDefault("key", "value");

        manager.GetDefault<string>("key").Should().Be("value");
    }

    // -------------------------------------------------------------------------
    // Set / Get
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CacheManager.Set{T}"/> rejects a null store name.
    /// </summary>
    [Fact]
    public void Set_NullStoreName_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Set<string?>(null!, "key", "value", CreatePolicy("key"));

        act.Should().Throw<ArgumentNullException>().WithParameterName("storeName");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Set{T}"/> rejects a null key.
    /// </summary>
    [Fact]
    public void Set_NullKey_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Set<string?>("store", null!, "value", CreatePolicy("key"));

        act.Should().Throw<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Set{T}"/> rejects a null value.
    /// </summary>
    [Fact]
    public void Set_NullValue_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Set<string?>("store", "key", null, CreatePolicy("key"));

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Set{T}"/> rejects a null policy.
    /// </summary>
    [Fact]
    public void Set_NullPolicy_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Set<string?>("store", "key", "value", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("policy");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Get{T}"/> rejects a null store name.
    /// </summary>
    [Fact]
    public void Get_NullStoreName_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Get<string>(null!, "key");

        act.Should().Throw<ArgumentNullException>().WithParameterName("storeName");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Get{T}"/> rejects a null key.
    /// </summary>
    [Fact]
    public void Get_NullKey_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Get<string>("store", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that a value stored with <see cref="CacheManager.Set{T}"/> is returned by <see cref="CacheManager.Get{T}"/> from the same named store.
    /// </summary>
    [Fact]
    public void Set_ThenGet_ReturnsStoredValueFromNamedStore()
    {
        var manager = CreateManager();

        manager.Set("store", "key", "value", CreatePolicy("key"));

        manager.Get<string>("store", "key").Should().Be("value");
    }

    // -------------------------------------------------------------------------
    // Contains / Remove / ClearStore
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that <see cref="CacheManager.Contains"/> rejects a null key.
    /// </summary>
    [Fact]
    public void Contains_NullKey_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Contains(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Contains"/> rejects a null store name.
    /// </summary>
    [Fact]
    public void Contains_NullStoreName_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Contains("key", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("storeName");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Remove"/> rejects a null key.
    /// </summary>
    [Fact]
    public void Remove_NullKey_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Remove(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("key");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.Remove"/> rejects a null store name.
    /// </summary>
    [Fact]
    public void Remove_NullStoreName_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.Remove("key", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("storeName");
    }

    /// <summary>
    /// Tests that <see cref="CacheManager.ClearStore"/> rejects a null store name.
    /// </summary>
    [Fact]
    public void ClearStore_NullStoreName_ThrowsArgumentNullException()
    {
        var manager = CreateManager();

        var act = () => manager.ClearStore(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("storeName");
    }
}
