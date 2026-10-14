// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Checks for report and output options. Each check returns a message naming the option and the
/// problem, or <c>null</c> when the value is acceptable. Checks never create directories or files.
/// </summary>
public static class OutputOptionsValidator
{
    /// <summary>Smallest allowed number of sprints for <c>--sprints</c>.</summary>
    public const int MinSprints = 1;

    /// <summary>Largest allowed number of sprints for <c>--sprints</c>.</summary>
    public const int MaxSprints = 50;

    /// <summary>Formats the <c>export</c> command writes, in the order they are listed in help.</summary>
    public static readonly IReadOnlyList<string> ExportFormats = ["png", "jpg", "svg", "json", "csv", "md"];

    /// <summary>Formats the <c>jql</c> and <c>team-compare</c> commands print.</summary>
    public static readonly IReadOnlyList<string> TextOrJsonFormats = ["text", "json"];

    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Checks that a format is one of the supported values. Matching ignores case.
    /// </summary>
    /// <param name="format">Format given on the command line.</param>
    /// <param name="supported">Supported format names.</param>
    /// <param name="optionName">Option name used in the message.</param>
    /// <returns>A message listing the supported formats, or <c>null</c> when the format is supported.</returns>
    public static string? CheckFormat(string? format, IReadOnlyCollection<string> supported, string optionName = "--format")
    {
        ArgumentNullException.ThrowIfNull(supported);

        if (supported.Any(name => string.Equals(name, format, StringComparison.OrdinalIgnoreCase)))
            return null;

        var list = string.Join(", ", supported);
        return string.IsNullOrWhiteSpace(format)
            ? $"{optionName} requires a value. Supported formats: {list}."
            : $"{optionName} '{format}' is not supported. Supported formats: {list}.";
    }

    /// <summary>
    /// Checks the number of sprints to analyze. It must be between <see cref="MinSprints"/> and <see cref="MaxSprints"/>.
    /// </summary>
    /// <param name="count">Value given on the command line.</param>
    /// <param name="optionName">Option name used in the message.</param>
    /// <returns>A message when the count is out of range, otherwise <c>null</c>.</returns>
    public static string? CheckSprintCount(int count, string optionName = "--sprints")
    {
        if (count >= MinSprints && count <= MaxSprints)
            return null;

        return $"{optionName} must be between {MinSprints} and {MaxSprints} (got {count.ToString(CultureInfo.InvariantCulture)}).";
    }

    /// <summary>
    /// Checks an optional date range. Each bound is <c>yyyy-MM-dd</c> and may be omitted. When both are given, start must not be after end.
    /// </summary>
    /// <param name="from">Start of the range, or null when open.</param>
    /// <param name="to">End of the range, or null when open.</param>
    /// <returns>The first problem found, or <c>null</c> when the range is valid.</returns>
    public static string? CheckDateRange(string? from, string? to)
    {
        DateOnly? start = null;
        if (!string.IsNullOrEmpty(from))
        {
            if (!TryParseDate(from, out var parsed))
                return $"--from must be a date in {DateFormat} format (got '{from}').";
            start = parsed;
        }

        DateOnly? end = null;
        if (!string.IsNullOrEmpty(to))
        {
            if (!TryParseDate(to, out var parsed))
                return $"--to must be a date in {DateFormat} format (got '{to}').";
            end = parsed;
        }

        if (start is { } startDate && end is { } endDate && startDate > endDate)
            return $"--from ({from}) must not be after --to ({to}).";

        return null;
    }

    /// <summary>
    /// Checks an output file path. The path is optional. <c>-</c> is accepted as given.
    /// The file's directory must already exist or be creatable.
    /// </summary>
    /// <param name="path">Path given on the command line.</param>
    /// <param name="optionName">Option name used in the message.</param>
    /// <returns>A message when the path cannot be used, otherwise <c>null</c>.</returns>
    public static string? CheckOutputFile(string? path, string optionName = "--output")
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (!TryGetFullPath(path, optionName, out var fullPath, out var problem))
            return problem;

        if (Directory.Exists(fullPath))
            return $"{optionName} '{path}' is a directory. Give a file path.";

        if (string.IsNullOrEmpty(Path.GetFileName(fullPath)))
            return $"{optionName} '{path}' does not name a file.";

        var directory = Path.GetDirectoryName(fullPath);
        return directory is null ? null : CheckDirectoryExistsOrCreatable(directory, path, optionName);
    }

    /// <summary>
    /// Checks an output directory. The directory is optional. It must already exist or be creatable.
    /// </summary>
    /// <param name="path">Directory given on the command line.</param>
    /// <param name="optionName">Option name used in the message.</param>
    /// <returns>A message when the directory cannot be used, otherwise <c>null</c>.</returns>
    public static string? CheckOutputDirectory(string? path, string optionName = "--output-dir")
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (!TryGetFullPath(path, optionName, out var fullPath, out var problem))
            return problem;

        return CheckDirectoryExistsOrCreatable(fullPath, path, optionName);
    }

    /// <summary>
    /// Walks up from <paramref name="directory"/> to the nearest existing entry. A missing directory can be
    /// created only if that entry is a directory. This does not test write permission.
    /// </summary>
    private static string? CheckDirectoryExistsOrCreatable(string directory, string given, string optionName)
    {
        var current = directory;
        while (true)
        {
            if (Directory.Exists(current))
                return null;

            if (File.Exists(current))
                return $"{optionName} '{given}' cannot be used because '{current}' is a file, not a directory.";

            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrEmpty(parent) || parent == current)
                return $"{optionName} '{given}' does not exist and cannot be created.";

            current = parent;
        }
    }

    private static bool TryGetFullPath(string path, string optionName, out string fullPath, out string? problem)
    {
        try
        {
            fullPath = Path.GetFullPath(path);
            problem = null;
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            fullPath = string.Empty;
            problem = $"{optionName} '{path}' is not a valid path ({ex.Message}).";
            return false;
        }
    }

    private static bool TryParseDate(string value, out DateOnly date) =>
        DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
