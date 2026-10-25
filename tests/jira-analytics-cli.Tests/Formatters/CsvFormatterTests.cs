using FluentAssertions;
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the CsvFormatter argument validation.
/// </summary>
public class CsvFormatterTests
{
    private readonly CsvFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvFormatterTests"/> class.
    /// </summary>
    public CsvFormatterTests()
    {
        var loggerMock = new Mock<ILogger<CsvFormatter>>();
        this._formatter = new CsvFormatter(loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null logger.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new CsvFormatter(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    /// <summary>
    /// Verifies that Format rejects a null data collection.
    /// </summary>
    [Fact]
    public void Format_ShouldThrow_WhenDataIsNull()
    {
        var act = () => this._formatter.Format<string>(null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("data");
    }

    /// <summary>
    /// Verifies that FormatWithMapping rejects a null data collection.
    /// </summary>
    [Fact]
    public void FormatWithMapping_ShouldThrow_WhenDataIsNull()
    {
        var mapping = new Dictionary<string, Func<string, string>> { ["Value"] = s => s };

        var act = () => this._formatter.FormatWithMapping<string>(null!, mapping);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("data");
    }

    /// <summary>
    /// Verifies that FormatWithMapping rejects a null column mapping.
    /// </summary>
    [Fact]
    public void FormatWithMapping_ShouldThrow_WhenColumnMappingIsNull()
    {
        var act = () => this._formatter.FormatWithMapping<string>(new[] { "a" }, null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("columnMapping");
    }

    /// <summary>
    /// Verifies that FormatWithMapping reports data as the offending parameter when both arguments are null.
    /// </summary>
    [Fact]
    public void FormatWithMapping_ShouldReportData_WhenBothArgumentsAreNull()
    {
        var act = () => this._formatter.FormatWithMapping<string>(null!, null!);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("data");
    }
}
