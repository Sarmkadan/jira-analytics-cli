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
        var data = new { Name = "Test", Value = 123 };
        
        var result = _formatter.Format(data);
        
        result.Should().Contain("\"Name\":\"Test\"");
        result.Should().Contain("\"Value\":123");
    }

    /// <summary>
    /// Verifies that the Format method handles null properties by ignoring them.
    /// </summary>
    [Fact]
    public void Format_ShouldHandleNullPropertiesByIgnoringThem()
    {
        var data = new { Name = "Test", Description = (string?)null };
        
        var result = _formatter.Format(data);
        
        result.Should().NotContain("Description");
    }

    /// <summary>
    /// Verifies that the Validate method returns true for valid JSON.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnTrueForValidJson()
    {
        var json = "{\"name\":\"test\"}";
        
        var (isValid, errors) = _formatter.Validate(json);
        
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that the Validate method returns false for invalid JSON.
    /// </summary>
    [Fact]
    public void Validate_ShouldReturnFalseForInvalidJson()
    {
        var json = "{\"name\":\"test\""; // Missing closing brace
        
        var (isValid, errors) = _formatter.Validate(json);
        
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that the FormatWithMetadata method includes metadata.
    /// </summary>
    [Fact]
    public void FormatWithMetadata_ShouldIncludeMetadata()
    {
        var data = new { Id = 1 };
        
        var result = _formatter.FormatWithMetadata(data, "Report", "1.0");
        
        result.Should().Contain("\"title\":\"Report\"");
        result.Should().Contain("\"version\":\"1.0\"");
        result.Should().Contain("\"generatedBy\":\"jira-analytics-cli\"");
        result.Should().Contain("\"data\":{\"Id\":1}");
    }

    /// <summary>
    /// Verifies that the Prettify method formats minified JSON.
    /// </summary>
    [Fact]
    public void Prettify_ShouldFormatMinifiedJson()
    {
        var minified = "{\"a\":1}";
        
        var prettified = _formatter.Prettify(minified);
        
        prettified.Should().Contain(Environment.NewLine);
        prettified.Should().Contain("\"a\": 1");
    }
}
