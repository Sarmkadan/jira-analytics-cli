// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for SprintMetric type
/// </summary>
public static class SprintMetricJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the SprintMetric instance to JSON string
    /// </summary>
    /// <param name="value">SprintMetric instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    public static string ToJson(this SprintMetric value, bool indented = false)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes JSON string to SprintMetric instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized SprintMetric instance or null if JSON is invalid</returns>
    public static SprintMetric? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<SprintMetric>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to SprintMetric instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized SprintMetric instance</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out SprintMetric? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            value = JsonSerializer.Deserialize<SprintMetric>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}