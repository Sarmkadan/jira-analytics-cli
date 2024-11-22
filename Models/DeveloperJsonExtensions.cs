// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="Developer"/> type
/// </summary>
public static class DeveloperJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes the <see cref="Developer"/> instance to JSON string using camelCase property naming
	/// </summary>
	/// <param name="value">The <see cref="Developer"/> instance to serialize</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability</param>
	/// <returns>A JSON string representation of the <see cref="Developer"/> instance</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
	public static string ToJson(this Developer value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes JSON string to <see cref="Developer"/> instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize, expected to use camelCase property naming</param>
	/// <returns>The deserialized <see cref="Developer"/> instance, or <see langword="null"/> if the JSON is invalid or empty</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
	public static Developer? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
			return null;

		try
		{
			return JsonSerializer.Deserialize<Developer>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize JSON string to <see cref="Developer"/> instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize, expected to use camelCase property naming</param>
	/// <param name="value">When this method returns, contains the deserialized <see cref="Developer"/> instance if successful; otherwise, <see langword="null"/></param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/></returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
	public static bool TryFromJson(string json, out Developer? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrWhiteSpace(json))
			return false;

		try
		{
			value = JsonSerializer.Deserialize<Developer>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}