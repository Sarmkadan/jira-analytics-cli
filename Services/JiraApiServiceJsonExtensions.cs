// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="JiraApiService"/>.
/// Enables serialization and deserialization of JiraApiService configuration for
/// testing, caching, and inter-process communication scenarios.
/// </summary>
public static class JiraApiServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the JiraApiService instance to a JSON string.
    /// </summary>
    /// <param name="value">The JiraApiService instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the JiraApiService.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this JiraApiService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        var serviceData = new
        {
            ServiceType = "JiraApiService"
        };

        return JsonSerializer.Serialize(serviceData, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a JiraApiService instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Always null, as JiraApiService with its dependencies cannot be deserialized.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or whitespace.</exception>
    public static JiraApiService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a JiraApiService instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains null as JiraApiService cannot be deserialized.</param>
    /// <returns>Always false, as JiraApiService with its dependencies cannot be deserialized.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or whitespace.</exception>
    public static bool TryFromJson(string json, out JiraApiService? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        value = null;
        return false;
    }
}