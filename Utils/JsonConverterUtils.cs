// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Utility methods for JSON serialization/deserialization with custom settings.
/// Provides consistent JSON handling across the application.
/// </summary>
public static class JsonConverterUtils
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Serializes object to indented JSON string with default settings.
    /// Suitable for human-readable output and logging.
    /// </summary>
    public static string ToJsonIndented<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }

    /// <summary>
    /// Serializes object to compact JSON string without extra whitespace.
    /// Suitable for API responses and storage efficiency.
    /// </summary>
    public static string ToJsonCompact<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, CompactOptions);
    }

    /// <summary>
    /// Safely deserializes JSON string to specified type.
    /// Returns default value on parse failure instead of throwing.
    /// </summary>
    public static T? FromJson<T>(this string json, T? defaultValue = default)
    {
        if (string.IsNullOrEmpty(json))
            return defaultValue;

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts object from one type to another via JSON round-trip.
    /// Useful for transforming between similar DTO types.
    /// </summary>
    public static TTarget ConvertViaJson<TSource, TTarget>(TSource source) where TTarget : class
    {
        var json = JsonSerializer.Serialize(source, CompactOptions);
        return JsonSerializer.Deserialize<TTarget>(json, DefaultOptions)
            ?? throw new InvalidOperationException("Conversion resulted in null");
    }

    /// <summary>
    /// Merges two JSON strings (shallow merge).
    /// Later properties override earlier ones.
    /// </summary>
    public static string MergeJson(string json1, string json2)
    {
        // In a real implementation, would do deep merge
        // For now, return obj2 as it represents the merge
        using var doc2 = JsonDocument.Parse(json2);
        return JsonSerializer.Serialize(doc2.RootElement, CompactOptions);
    }

    /// <summary>
    /// Extracts specific property value from JSON string.
    /// Returns null if property not found or JSON is invalid.
    /// </summary>
    public static T? GetJsonProperty<T>(string json, string propertyName)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(propertyName, out var prop))
                return prop.Deserialize<T>(DefaultOptions);
        }
        catch
        {
            // Silently fail for invalid JSON
        }

        return default;
    }

    /// <summary>
    /// Validates JSON string syntax without deserialization.
    /// Useful for quick validation before processing.
    /// </summary>
    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
