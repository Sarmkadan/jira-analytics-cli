using System.Text.Json;

namespace JiraAnalyticsCli.Tests.Services
{
	/// <summary>
	/// Provides JSON serialization and deserialization extensions for <see cref="JiraApiServiceTests"/> test fixtures.
	/// </summary>
	public static class JiraApiServiceTestsJsonExtensions
	{
		// Cached options: camelCase property naming to match Jira API conventions.
		private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};

		/// <summary>
		/// Serializes the <paramref name="value"/> to JSON using camelCase property naming.
		/// </summary>
		/// <param name="value">The test instance to serialize. Cannot be <see langword="null"/>.</param>
		/// <param name="indented">If <see langword="true"/>, the output JSON will be indented for readability.</param>
		/// <returns>A JSON string representation of <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		public static string ToJson(this JiraApiServiceTests value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);

			var options = indented
				? new JsonSerializerOptions(_options) { WriteIndented = true }
				: _options;

			return JsonSerializer.Serialize(value, options);
		}

		/// <summary>
		/// Deserializes a JSON string into a <see cref="JiraApiServiceTests"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize. Can be <see langword="null"/> or empty.</param>
		/// <returns>
		/// The deserialized instance, or <see langword="null"/> if the JSON is <see langword="null"/>, empty, or whitespace.
		/// </returns>
		/// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
		public static JiraApiServiceTests? FromJson(string? json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return null;
			}

			return JsonSerializer.Deserialize<JiraApiServiceTests>(json, _options);
		}

		/// <summary>
		/// Attempts to deserialize a JSON string into a <see cref="JiraApiServiceTests"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize. Can be <see langword="null"/> or empty.</param>
		/// <param name="value">
		/// When this method returns, contains the deserialized value if the operation succeeded; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
		public static bool TryFromJson(string? json, out JiraApiServiceTests? value)
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