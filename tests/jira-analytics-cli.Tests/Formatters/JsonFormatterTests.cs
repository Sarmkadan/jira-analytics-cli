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
}
