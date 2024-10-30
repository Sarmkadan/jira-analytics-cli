// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SprintRepository"/>.
/// </summary>
public static class SprintRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the <see cref="SprintRepository"/> to a JSON string.
    /// </summary>
    /// <param name="value">The repository to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON representation of the repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SprintRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var sprints = value.GetAllSprints();
        return JsonSerializer.Serialize(sprints, indented ? GetIndentedOptions() : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SprintRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A new <see cref="SprintRepository"/> populated with the deserialized sprints.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static SprintRepository? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var sprints = JsonSerializer.Deserialize<List<Sprint>>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            }
        );

        if (sprints == null || sprints.Count == 0)
            return new SprintRepository(null!); // Logger will be null but methods handle it

        var repository = new SprintRepository(null!);
        repository.LoadSprints(sprints);
        return repository;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SprintRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized repository, or null on failure.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out SprintRepository? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

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

    private static JsonSerializerOptions GetIndentedOptions()
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = true
        };
        return options;
    }
}

/// <summary>
/// Extension methods for <see cref="SprintRepository"/> to support serialization operations.
/// </summary>
file static class SprintRepositoryExtensions
{
    /// <summary>
    /// Gets all sprints from the repository for serialization purposes.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <returns>A list of all sprints.</returns>
    public static List<Sprint> GetAllSprints(this SprintRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        // Since SprintRepository uses ConcurrentDictionary internally, we need to extract all values
        // This uses reflection to access the private _sprints field
        var field = typeof(SprintRepository).GetField(
            "_sprints",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field?.GetValue(repository) is System.Collections.Concurrent.ConcurrentDictionary<int, Sprint> dictionary)
        {
            return dictionary.Values.ToList();
        }

        return new List<Sprint>();
    }

    /// <summary>
    /// Loads sprints into the repository.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="sprints">The sprints to load.</param>
    public static void LoadSprints(this SprintRepository repository, List<Sprint> sprints)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(sprints);

        var field = typeof(SprintRepository).GetField(
            "_sprints",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field?.GetValue(repository) is System.Collections.Concurrent.ConcurrentDictionary<int, Sprint> dictionary)
        {
            dictionary.Clear();
            foreach (var sprint in sprints)
            {
                dictionary.AddOrUpdate(sprint.Id, sprint, (id, existing) => sprint);
            }
        }
    }
}