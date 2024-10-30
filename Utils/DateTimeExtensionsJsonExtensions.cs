// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// System.Text.Json serialization extensions for DateTime
/// </summary>
public static class DateTimeExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a DateTime value to a JSON string
    /// </summary>
    /// <param name="value">The DateTime value to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    public static string ToJson(this DateTime value, bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a DateTime value from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized DateTime value or null if JSON is null/empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static DateTime? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DateTime>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a DateTime value from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized value</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out DateTime? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<DateTime>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}