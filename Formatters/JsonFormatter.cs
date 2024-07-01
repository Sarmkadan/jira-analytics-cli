// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                Formatting = _prettyPrint ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(data, settings);
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
            var json = JsonConvert.SerializeObject(data);
            var jObject = JObject.Parse(json);

            var filtered = FilterJsonObject(jObject, includedFields);
            var filteredJson = JsonConvert.SerializeObject(filtered, _prettyPrint ? Formatting.Indented : Formatting.None);

            return filteredJson;
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
            var jObject = JObject.Parse(json);
            return jObject.ToString(Formatting.Indented);
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
            JObject.Parse(json);
            return (true, Array.Empty<string>());
        }
        catch (JsonReaderException ex)
        {
            errors.Add($"JSON parsing error at line {ex.LineNumber}: {ex.Message}");
        }
        catch (JsonSerializationException ex)
        {
            errors.Add($"JSON serialization error: {ex.Message}");
        }
        catch (Exception ex)
        {
            errors.Add($"Unexpected error: {ex.Message}");
        }

        return (false, errors.ToArray());
    }

    private JObject FilterJsonObject(JObject source, string[] fields)
    {
        var result = new JObject();

        foreach (var field in fields)
        {
            var token = source.SelectToken(field);
            if (token != null)
            {
                result.Add(field, token);
            }
        }

        return result;
    }
}
