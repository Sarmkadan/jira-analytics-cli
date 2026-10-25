using FluentAssertions;
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the JsonFormatter class.
/// </summary>
public class JsonFormatterTests
{
    private readonly Mock<ILogger<JsonFormatter>> _loggerMock;
    private readonly JsonFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFormatterTests"/> class.
    /// </summary>
    public JsonFormatterTests()
    {
        _loggerMock = new Mock<ILogger<JsonFormatter>>();
        _formatter = new JsonFormatter(_loggerMock.Object, prettyPrint: false);
    }

    /// <summary>
    /// Verifies that the Format method serializes an object to JSON.
    /// </summary>
    [Fact]
    public void Format_ShouldSerializeObjectToJson()
    {
        const string testName = nameof(Format_ShouldSerializeObjectToJson);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        var data = new { Name = "Test", Value = 123 };
        const string itemId = "test-item";

        _loggerMock.Object.LogInformation("Format called with {ItemId}", itemId);
        string result;
        try
        {
            result = _formatter.Format(data);
            _loggerMock.Object.LogInformation("Format completed with {ItemId}", itemId);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Failed to format {ItemId}", itemId);
            throw;
        }

        result.Should().Contain("\"Value\":123");

        _loggerMock.Object.LogInformation("Finished {TestName}", testName);
    }

    /// <summary>
    /// Verifies that the Format method handles null properties by ignoring them.
    /// </summary>
    [Fact]
    public void Format_ShouldHandleNullPropertiesByIgnoringThem()
    {
        const string testName = nameof(Format_ShouldHandleNullPropertiesByIgnoringThem);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        var data = new { Name = "Test", Description = (string?)null };

        var result = _formatter.Format(data);

        result.Should().NotContain("Description");

        _loggerMock.Object.LogInformation("Finished {TestName}", testName);
    }

    /// <summary>
    /// Verifies that the Validate method returns true for valid JSON.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnTrueForValidJson()
    {
        const string testName = nameof(Validate_ShouldReturnTrueForValidJson);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        var json = "{\"name\":\"test\"}";

        var (isValid, errors) = _formatter.Validate(json);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();

        _loggerMock.Object.LogInformation("Finished {TestName}", testName);
    }

    /// <summary>
    /// Verifies that the Validate method returns false for invalid JSON.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnFalseForInvalidJson()
    {
        const string testName = nameof(Validate_ShouldReturnFalseForInvalidJson);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        var json = "{\"name\":\"test\""; // Missing closing brace

        var (isValid, errors) = _formatter.Validate(json);

        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();

        _loggerMock.Object.LogInformation("Finished {TestName}", testName);
    }

    /// <summary>
    /// Verifies that the FormatWithMetadata method includes metadata.
    /// </summary>
    [Fact]
    public void FormatWithMetadata_ShouldIncludeMetadata()
    {
        const string testName = nameof(FormatWithMetadata_ShouldIncludeMetadata);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        var data = new { Id = 1 };

        var result = _formatter.FormatWithMetadata(data, "Report", "1.0");

        result.Should().Contain("\"title\":\"Report\"");
        result.Should().Contain("\"version\":\"1.0\"");
        result.Should().Contain("\"generatedBy\":\"jira-analytics-cli\"");
        result.Should().Contain("\"data\":{\"Id\":1}");

        _loggerMock.Object.LogInformation("Finished {TestName}", testName);
    }

    /// <summary>
    /// Verifies that the Prettify method formats minified JSON.
    /// </summary>
    [Fact]
    public void Prettify_ShouldFormatMinifiedJson()
    {
        const string testName = nameof(Prettify_ShouldFormatMinifiedJson);
        _loggerMock.Object.LogInformation("Starting {TestName}", testName);

        var minified = "{\"a\":1}";

        var prettified = _formatter.Prettify(minified);

        prettified.Should().Contain(Environment.NewLine);
        prettified.Should().Contain("\"a\": 1");

        _loggerMock.Object.LogInformation("Finished {TestName}", testName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null logger.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrowWhenLoggerIsNull()
    {
        Action act = () => _ = new JsonFormatter(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Verifies that the Format method rejects null data.
    /// </summary>
    [Fact]
    public void Format_ShouldThrowWhenDataIsNull()
    {
        Action act = () => _formatter.Format(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("data");
    }

    /// <summary>
    /// Verifies that the FormatWithMetadata method rejects null data.
    /// </summary>
    [Fact]
    public void FormatWithMetadata_ShouldThrowWhenDataIsNull()
    {
        Action act = () => _formatter.FormatWithMetadata(null!, "Report");

        act.Should().Throw<ArgumentNullException>().WithParameterName("data");
    }

    /// <summary>
    /// Verifies that the FormatWithMetadata method rejects a null title.
    /// </summary>
    [Fact]
    public void FormatWithMetadata_ShouldThrowWhenTitleIsNull()
    {
        Action act = () => _formatter.FormatWithMetadata(new { Id = 1 }, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("title");
    }

    /// <summary>
    /// Verifies that the FormatWithMetadata method rejects a null version.
    /// </summary>
    [Fact]
    public void FormatWithMetadata_ShouldThrowWhenVersionIsNull()
    {
        Action act = () => _formatter.FormatWithMetadata(new { Id = 1 }, "Report", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("version");
    }

    /// <summary>
    /// Verifies that the FormatFiltered method rejects null data.
    /// </summary>
    [Fact]
    public void FormatFiltered_ShouldThrowWhenDataIsNull()
    {
        Action act = () => _formatter.FormatFiltered(null!, ["Id"]);

        act.Should().Throw<ArgumentNullException>().WithParameterName("data");
    }

    /// <summary>
    /// Verifies that the FormatFiltered method rejects null included fields.
    /// </summary>
    [Fact]
    public void FormatFiltered_ShouldThrowWhenIncludedFieldsIsNull()
    {
        Action act = () => _formatter.FormatFiltered(new { Id = 1 }, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("includedFields");
    }

    /// <summary>
    /// Verifies that the Prettify method rejects null JSON.
    /// </summary>
    [Fact]
    public void Prettify_ShouldThrowWhenJsonIsNull()
    {
        Action act = () => _formatter.Prettify(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("json");
    }

    /// <summary>
    /// Verifies that the Validate method rejects null JSON.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrowWhenJsonIsNull()
    {
        Action act = () => _formatter.Validate(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("json");
    }
}
