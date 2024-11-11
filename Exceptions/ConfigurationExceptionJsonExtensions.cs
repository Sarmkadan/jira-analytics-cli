// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace JiraAnalyticsCli.Exceptions;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ConfigurationException"/>
/// </summary>
public static class ConfigurationExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a <see cref="ConfigurationException"/> to a JSON string
    /// </summary>
    /// <param name="value">The exception to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the exception</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this ConfigurationException value, bool indented = false)
        => value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions);

    /// <summary>
    /// Deserializes a <see cref="ConfigurationException"/> from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized <see cref="ConfigurationException"/>, or <see langword="null"/> if JSON is invalid or whitespace</returns>
    /// <remarks>
    /// Returns null for invalid JSON, empty strings, or whitespace-only strings.
    /// For more control over error handling, use <see cref="TryFromJson"/> instead.
    /// </remarks>
    public static ConfigurationException? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ConfigurationException>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ConfigurationException"/> from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="ConfigurationException"/>
    /// if successful, or <see langword="null"/> if deserialization failed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the JSON was successfully deserialized; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryFromJson(string json, out ConfigurationException? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ConfigurationException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}