using FluentAssertions;
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the XmlFormatter argument validation.
/// </summary>
public class XmlFormatterTests
{
    private readonly XmlFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlFormatterTests"/> class.
    /// </summary>
    public XmlFormatterTests()
    {
        var loggerMock = new Mock<ILogger<XmlFormatter>>();
        this._formatter = new XmlFormatter(loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null logger.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new XmlFormatter(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    /// <summary>
    /// Verifies that Format rejects null data.
    /// </summary>
    [Fact]
    public void Format_ShouldThrow_WhenDataIsNull()
    {
        var act = () => this._formatter.Format(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("data");
    }

    /// <summary>
    /// Verifies that Format rejects a null root element name.
    /// </summary>
    [Fact]
    public void Format_ShouldThrow_WhenRootElementIsNull()
    {
        var act = () => this._formatter.Format(new { Name = "Test" }, null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("rootElement");
    }

    /// <summary>
    /// Verifies that Format rejects an empty root element name.
    /// </summary>
    [Fact]
    public void Format_ShouldThrow_WhenRootElementIsEmpty()
    {
        var act = () => this._formatter.Format(new { Name = "Test" }, string.Empty);

        act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("rootElement");
    }

    /// <summary>
    /// Verifies that Validate rejects a null XML string.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenXmlIsNull()
    {
        var act = () => this._formatter.Validate(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("xml");
    }

    /// <summary>
    /// Verifies that Prettify rejects a null XML string.
    /// </summary>
    [Fact]
    public void Prettify_ShouldThrow_WhenXmlIsNull()
    {
        var act = () => this._formatter.Prettify(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("xml");
    }

    /// <summary>
    /// Verifies that SelectValues rejects a null XML string.
    /// </summary>
    [Fact]
    public void SelectValues_ShouldThrow_WhenXmlIsNull()
    {
        var act = () => this._formatter.SelectValues(null!, "//Name");

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("xml");
    }

    /// <summary>
    /// Verifies that SelectValues rejects a null XPath expression.
    /// </summary>
    [Fact]
    public void SelectValues_ShouldThrow_WhenXpathIsNull()
    {
        var act = () => this._formatter.SelectValues("<root />", null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("xpathExpression");
    }
}
