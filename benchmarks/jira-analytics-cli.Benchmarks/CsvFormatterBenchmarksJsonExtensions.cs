// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace JiraAnalyticsCli.Benchmarks;

/// <summary>
/// Provides JSON serialization/deserialization extensions for <see cref="CsvFormatterBenchmarks"/>.
/// </summary>
public static class CsvFormatterBenchmarksJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="CsvFormatterBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this CsvFormatterBenchmarks value, bool indented = false)
        => value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CsvFormatterBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <see langword="null"/> if the JSON is <see langword="null"/> or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
    public static CsvFormatterBenchmarks? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<CsvFormatterBenchmarks>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="CsvFormatterBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized instance, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    public static bool TryFromJson(string json, out CsvFormatterBenchmarks? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<CsvFormatterBenchmarks>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}