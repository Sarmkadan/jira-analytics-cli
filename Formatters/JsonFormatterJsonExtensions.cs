// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace JiraAnalyticsCli.Formatters;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="JsonFormatter"/>.
/// </summary>
public static class JsonFormatterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="JsonFormatter"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The formatter instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the formatter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this JsonFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="JsonFormatter"/> instance.
    /// </summary>
    /// <remarks>
    /// Note: <see cref="JsonFormatter"/> is a service class and is not designed to be deserialized from JSON.
    /// This method exists for API consistency and always returns a new instance.
    /// </remarks>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="JsonFormatter"/> instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static JsonFormatter? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            _ = doc.RootElement; // Validate JSON structure
        }
        catch (JsonException)
        {
            throw;
        }

        return new JsonFormatter(null!, false);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="JsonFormatter"/> instance.
    /// </summary>
    /// <remarks>
    /// Note: <see cref="JsonFormatter"/> is a service class and is not designed to be deserialized from JSON.
    /// This method always returns true if the JSON is valid, and returns a new instance.
    /// </remarks>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized formatter, or null on failure.</param>
    /// <returns>True if the JSON is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out JsonFormatter? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        value = null;

        try
        {
            value = FromJson(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}