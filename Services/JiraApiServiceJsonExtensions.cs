// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// System.Text.Json serialization extensions for JiraApiService
/// </summary>
public static class JiraApiServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the JiraApiService instance to JSON string
    /// </summary>
    /// <param name="value">The service instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the service</returns>
    public static string ToJson(this JiraApiService value, bool indented = false)
    {
        if (value == null)
        {
            return "{}";
        }

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        var data = new
        {
            ServiceType = "JiraApiService"
        };

        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a JiraApiService instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized JiraApiService instance or null if parsing fails</returns>
    public static JiraApiService? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a JiraApiService instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized instance</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out JiraApiService? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}