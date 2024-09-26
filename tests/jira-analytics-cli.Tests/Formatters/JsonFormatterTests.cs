using FluentAssertions;
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JiraAnalyticsCli.Tests.Formatters;

public class JsonFormatterTests
{
    private readonly Mock<ILogger<JsonFormatter>> _loggerMock;
    private readonly JsonFormatter _formatter;

    public JsonFormatterTests()
    {
        _loggerMock = new Mock<ILogger<JsonFormatter>>();
        _formatter = new JsonFormatter(_loggerMock.Object, prettyPrint: false);
    }

    [Fact]
    public void Format_ShouldSerializeObjectToJson()
    {
        var data = new { Name = "Test", Value = 123 };
        
        var result = _formatter.Format(data);
        
        result.Should().Contain("\"Name\":\"Test\"");
        result.Should().Contain("\"Value\":123");
    }

    [Fact]
    public void Format_ShouldHandleNullPropertiesByIgnoringThem()
    {
        var data = new { Name = "Test", Description = (string?)null };
        
        var result = _formatter.Format(data);
        
        result.Should().NotContain("Description");
    }

    [Fact]
    public void Validate_ShouldReturnTrueForValidJson()
    {
        var json = "{\"name\":\"test\"}";
        
        var (isValid, errors) = _formatter.Validate(json);
        
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnFalseForInvalidJson()
    {
        var json = "{\"name\":\"test\""; // Missing closing brace
        
        var (isValid, errors) = _formatter.Validate(json);
        
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

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

    [Fact]
    public void Prettify_ShouldFormatMinifiedJson()
    {
        var minified = "{\"a\":1}";
        
        var prettified = _formatter.Prettify(minified);
        
        prettified.Should().Contain(Environment.NewLine);
        prettified.Should().Contain("\"a\": 1");
    }
}
