// =============================================================================
// System.Text.Json serialization extensions for AnalyticsController
// Provides serialization/deserialization helpers for ASP.NET Core integration
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraAnalyticsIntegration
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="AnalyticsController"/>.
    /// Enables easy serialization and deserialization of AnalyticsController instances for
    /// API responses, caching, and inter-process communication.
    /// </summary>
    public static class AnalyticsControllerJsonExtensions
    {
        /// <summary>
        /// The JSON serialization options used by all extension methods.
        /// Uses camelCase property naming and includes property name case insensitivity.
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the AnalyticsController instance to a JSON string.
        /// </summary>
        /// <param name="value">The AnalyticsController instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the AnalyticsController.</returns>
        public static string ToJson(this AnalyticsController value, bool indented = false)
        {
            if (value == null)
            {
                return "{}";
            }

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = true
                }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an AnalyticsController instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized AnalyticsController instance, or null if JSON is invalid.</returns>
        public static AnalyticsController? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<AnalyticsController>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an AnalyticsController instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized AnalyticsController instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; false otherwise.</returns>
        public static bool TryFromJson(string json, out AnalyticsController? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<AnalyticsController>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}