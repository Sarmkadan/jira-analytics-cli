using System.Text.Json;

namespace JiraAnalyticsCli.Tests.Services
{
    /// <summary>
    /// JSON serialization helpers for <see cref="JiraApiServiceTests"/>.
    /// </summary>
    public static class JiraApiServiceTestsJsonExtensions
    {
        // Cached options: camelCase property naming.
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the <paramref name="value"/> to JSON.
        /// </summary>
        /// <param name="value">The test instance to serialize.</param>
        /// <param name="indented">If true, the output JSON will be indented.</param>
        /// <returns>A JSON string representation of <paramref name="value"/>.</returns>
        public static string ToJson(this JiraApiServiceTests value, bool indented = false)
        {
            if (value is null)
                return string.Empty;

            var options = indented
                ? new JsonSerializerOptions(_options) { WriteIndented = true }
                : _options;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="JiraApiServiceTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized instance, or <c>null</c> if the JSON is <c>null</c> or empty.</returns>
        public static JiraApiServiceTests? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<JiraApiServiceTests>(json, _options);
        }

        /// <summary>
        /// Tries to deserialize a JSON string into a <see cref="JiraApiServiceTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="value">When this method returns, contains the deserialized value if the operation succeeded; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
        public static bool TryFromJson(string json, out JiraApiServiceTests? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
