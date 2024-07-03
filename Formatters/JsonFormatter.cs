// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Formatters;

/// <summary>
/// Formats analytics data as JSON with support for prettification and compression.
/// Handles both object serialization and custom field transformations.
/// </summary>
public class JsonFormatter
{
    private readonly ILogger<JsonFormatter> _logger;
    private readonly bool _prettyPrint;

    public JsonFormatter(ILogger<JsonFormatter> logger, bool prettyPrint = true)
    {
        _logger = logger;
        _prettyPrint = prettyPrint;
    }

    /// <summary>
    /// Formats any object as JSON with indentation or compact format.
    /// Safely handles circular references and unsupported types.
    /// </summary>
    public string Format(object data)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = _prettyPrint,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            var json = JsonSerializer.Serialize(data, options);
            _logger.LogDebug("Formatted data to JSON: {JsonLength} bytes", json.Length);

            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting JSON");
            throw new InvalidOperationException("Failed to format JSON", ex);
        }
    }

    /// <summary>
    /// Adds custom metadata wrapper around core analytics data.
    /// Includes generation timestamp and version information.
    /// </summary>
    public string FormatWithMetadata(object data, string title, string version = "1.0.0")
    {
        var wrapper = new
        {
            metadata = new
            {
                title,
                version,
                generatedAt = DateTime.UtcNow,
                generatedBy = "jira-analytics-cli"
            },
            data
        };

        return Format(wrapper);
    }

    /// <summary>
    /// Extracts specific fields from JSON and returns simplified output.
    /// Useful for exporting only relevant columns/fields.
    /// </summary>
    public string FormatFiltered(object data, string[] includedFields)
    {
        try
        {
            var rawJson = JsonSerializer.Serialize(data);
            using var sourceDoc = JsonDocument.Parse(rawJson);
            var root = sourceDoc.RootElement;

            using var stream = new System.IO.MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = _prettyPrint });

            writer.WriteStartObject();
            foreach (var field in includedFields)
            {
                var token = SelectToken(root, field);
                if (token.HasValue)
                {
                    writer.WritePropertyName(field);
                    token.Value.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
            writer.Flush();

            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering JSON");
            throw new InvalidOperationException("Failed to filter JSON", ex);
        }
    }

    /// <summary>
    /// Converts JSON string to pretty-printed format.
    /// Useful for normalizing JSON that may be minified or poorly formatted.
    /// </summary>
    public string Prettify(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON provided for prettification");
            throw new InvalidOperationException("Invalid JSON format", ex);
        }
    }

    /// <summary>
    /// Validates JSON string for correctness.
    /// Returns list of validation errors if JSON is invalid.
    /// </summary>
    public (bool IsValid, string[] Errors) Validate(string json)
    {
        var errors = new List<string>();

        try
        {
            JsonDocument.Parse(json);
            return (true, Array.Empty<string>());
        }
        catch (JsonException ex)
        {
            errors.Add($"JSON parsing error at line {ex.LineNumber}: {ex.Message}");
        }
        catch (Exception ex)
        {
            errors.Add($"Unexpected error: {ex.Message}");
        }

        return (false, errors.ToArray());
    }

    private static JsonElement? SelectToken(JsonElement element, string path)
    {
        var parts = path.Split('.');
        var current = element;

        foreach (var part in parts)
        {
            if (!current.TryGetProperty(part, out current))
                return null;
        }

        return current;
    }
}
