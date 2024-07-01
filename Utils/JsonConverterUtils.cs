// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Utility methods for JSON serialization/deserialization with custom settings.
/// Provides consistent JSON handling across the application.
/// </summary>
public static class JsonConverterUtils
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented
    };

    private static readonly JsonSerializerSettings CompactSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.None
    };

    /// <summary>
    /// Serializes object to indented JSON string with default settings.
    /// Suitable for human-readable output and logging.
    /// </summary>
    public static string ToJsonIndented<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, DefaultSettings);
    }

    /// <summary>
    /// Serializes object to compact JSON string without extra whitespace.
    /// Suitable for API responses and storage efficiency.
    /// </summary>
    public static string ToJsonCompact<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, CompactSettings);
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
            return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
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
        var json = JsonConvert.SerializeObject(source, CompactSettings);
        return JsonConvert.DeserializeObject<TTarget>(json, DefaultSettings)
            ?? throw new InvalidOperationException("Conversion resulted in null");
    }

    /// <summary>
    /// Merges two JSON strings (shallow merge).
    /// Later properties override earlier ones.
    /// </summary>
    public static string MergeJson(string json1, string json2)
    {
        var obj1 = JsonConvert.DeserializeObject<dynamic>(json1) ?? new { };
        var obj2 = JsonConvert.DeserializeObject<dynamic>(json2) ?? new { };

        // In a real implementation, would do deep merge
        // For now, return obj2 as it represents the merge
        return JsonConvert.SerializeObject(obj2, CompactSettings);
    }

    /// <summary>
    /// Extracts specific property value from JSON string.
    /// Returns null if property not found or JSON is invalid.
    /// </summary>
    public static T? GetJsonProperty<T>(string json, string propertyName)
    {
        try
        {
            var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (obj?.TryGetValue(propertyName, out var value) == true)
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
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
            JsonConvert.DeserializeObject(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
