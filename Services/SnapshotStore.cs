// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// File-backed <see cref="ISnapshotStore"/> that keeps one JSON-lines file per sprint under a
/// configurable data directory. Each line is an independently parseable JSON-serialized
/// <see cref="BurndownSnapshot"/>, keyed for lookup by the file's <c>SprintId</c> and ordered
/// within the file by <c>Timestamp</c>. Appending is append-only, so a run that is killed
/// mid-write can at most corrupt its own last line, which <see cref="LoadAsync"/> skips over.
/// </summary>
public class SnapshotStore : ISnapshotStore
{
    private const string SnapshotDirectoryEnvironmentVariable = "JIRA_SNAPSHOT_DIR";
    private const string DefaultDirectoryName = "snapshots";

    private static readonly SemaphoreSlim WriteLock = new(1, 1);

    private readonly string _dataDirectory;
    private readonly ILogger<SnapshotStore> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SnapshotStore"/>.
    /// </summary>
    /// <param name="logger">Logger used to report skipped/corrupt entries.</param>
    /// <param name="dataDirectory">
    /// Directory in which per-sprint snapshot files are stored. When null or whitespace, the
    /// <c>JIRA_SNAPSHOT_DIR</c> environment variable is used if set, otherwise a
    /// <c>snapshots</c> directory relative to the current working directory.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public SnapshotStore(ILogger<SnapshotStore> logger, string? dataDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _dataDirectory = ResolveDataDirectory(dataDirectory);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="snapshot"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="snapshot"/> fails validation.</exception>
    public async Task AppendAsync(BurndownSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        snapshot.Validate();

        Directory.CreateDirectory(_dataDirectory);
        var line = JsonSerializer.Serialize(snapshot) + Environment.NewLine;

        await WriteLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await File.AppendAllTextAsync(GetFilePath(snapshot.SprintId), line, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            WriteLock.Release();
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sprintId"/> is not positive.</exception>
    public async Task<IReadOnlyList<BurndownSnapshot>> LoadAsync(int sprintId, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sprintId, 0);

        var filePath = GetFilePath(sprintId);
        if (!File.Exists(filePath))
            return Array.Empty<BurndownSnapshot>();

        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken).ConfigureAwait(false);
        var snapshots = new List<BurndownSnapshot>(lines.Length);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            BurndownSnapshot? snapshot;
            try
            {
                snapshot = JsonSerializer.Deserialize<BurndownSnapshot>(line);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Skipping corrupt snapshot line {LineNumber} in {FilePath}", i + 1, filePath);
                continue;
            }

            if (snapshot is null)
            {
                _logger.LogWarning("Skipping empty snapshot line {LineNumber} in {FilePath}", i + 1, filePath);
                continue;
            }

            try
            {
                snapshot.Validate();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Skipping invalid snapshot line {LineNumber} in {FilePath}", i + 1, filePath);
                continue;
            }

            snapshots.Add(snapshot);
        }

        return snapshots.OrderBy(s => s.Timestamp).ToList();
    }

    private string GetFilePath(int sprintId) => Path.Combine(_dataDirectory, $"sprint-{sprintId}.jsonl");

    private static string ResolveDataDirectory(string? dataDirectory)
    {
        if (!string.IsNullOrWhiteSpace(dataDirectory))
            return dataDirectory;

        var fromEnvironment = Environment.GetEnvironmentVariable(SnapshotDirectoryEnvironmentVariable);
        return !string.IsNullOrWhiteSpace(fromEnvironment)
            ? fromEnvironment
            : Path.Combine(Directory.GetCurrentDirectory(), DefaultDirectoryName);
    }
}
