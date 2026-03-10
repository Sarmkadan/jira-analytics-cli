// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Extension methods for string manipulation and formatting.
/// Provides safe parsing, cleaning, and transformation utilities.
/// </summary>
public static partial class StringExtensions
{
    // Source-generated regex patterns are emitted as DFA state machines at compile time —
    // no runtime Regex compilation, no heap allocation for the Regex object itself.
    [GeneratedRegex(@"\s+", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9\s\-]", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SlugInvalidCharsRegex();

    [GeneratedRegex(@"-+", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SlugMultipleHyphensRegex();

    // Dynamic wildcard patterns cannot be source-generated, so cache compiled instances
    // keyed on the raw pattern string to avoid re-compilation on repeated calls.
    private static readonly ConcurrentDictionary<string, Regex> _patternCache = new();

    /// <summary>
    /// Truncates string to maximum length and appends ellipsis if truncated.
    /// Useful for displaying long text in UI with fixed width constraints.
    /// </summary>
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= maxLength)
            return value;

        // string.Concat(ReadOnlySpan, string) avoids the intermediate Substring allocation.
        return string.Concat(value.AsSpan(0, Math.Max(0, maxLength - 1)), "…");
    }

    /// <summary>
    /// Removes all whitespace characters from string.
    /// Useful for normalizing project keys or identifiers.
    /// </summary>
    public static string RemoveWhitespace(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var span = value.AsSpan();

        // Fast path: return the original reference when there is nothing to remove.
        var hasWhitespace = false;
        foreach (var ch in span)
        {
            if (char.IsWhiteSpace(ch)) { hasWhitespace = true; break; }
        }
        if (!hasWhitespace) return value;

        // Rent a suitably-sized buffer from the shared pool to avoid a per-call heap allocation.
        var buffer = ArrayPool<char>.Shared.Rent(value.Length);
        try
        {
            var writeIdx = 0;
            foreach (var ch in span)
            {
                if (!char.IsWhiteSpace(ch))
                    buffer[writeIdx++] = ch;
            }
            return new string(buffer, 0, writeIdx);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Converts string to slug format (lowercase, hyphens instead of spaces).
    /// Useful for generating file names or URL-safe identifiers.
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var slug = value.ToLowerInvariant();
        slug = SlugInvalidCharsRegex().Replace(slug, string.Empty);
        slug = WhitespaceRegex().Replace(slug, "-");
        slug = SlugMultipleHyphensRegex().Replace(slug, "-");

        return slug.Trim('-');
    }

    /// <summary>
    /// Parses boolean from string with support for multiple formats.
    /// More lenient than bool.Parse for user input handling.
    /// </summary>
    public static bool TryParseBool(this string value, out bool result)
    {
        result = false;

        if (string.IsNullOrEmpty(value))
            return false;

        var lower = value.ToLowerInvariant().Trim();

        if (lower == "true" || lower == "yes" || lower == "1" || lower == "on" || lower == "y")
        {
            result = true;
            return true;
        }

        if (lower == "false" || lower == "no" || lower == "0" || lower == "off" || lower == "n")
        {
            result = false;
            return true;
        }

        return bool.TryParse(value, out result);
    }

    /// <summary>
    /// Repeats string specified number of times.
    /// More convenient than string.Concat or String.Format for repetition.
    /// </summary>
    public static string Repeat(this string value, int count)
    {
        if (count <= 0)
            return string.Empty;

        var builder = new StringBuilder(value.Length * count);

        for (int i = 0; i < count; i++)
        {
            builder.Append(value);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Checks if string matches a pattern using wildcard matching.
    /// Supports ? (single char) and * (multiple chars) wildcards.
    /// </summary>
    public static bool MatchesPattern(this string value, string pattern)
    {
        if (value == null || pattern == null)
            return value == pattern;

        // GetOrAdd compiles the Regex once per unique pattern, then reuses it on every
        // subsequent call — eliminates regex construction and JIT cost at runtime.
        var regex = _patternCache.GetOrAdd(pattern, static p =>
        {
            var regexPattern = "^" + Regex.Escape(p)
                .Replace("\\?", ".")
                .Replace("\\*", ".*") + "$";
            return new Regex(regexPattern,
                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                matchTimeout: TimeSpan.FromSeconds(1));
        });

        return regex.IsMatch(value);
    }

    /// <summary>
    /// Gets common prefix shared between two strings.
    /// Useful for finding common parts in identifiers or file paths.
    /// </summary>
    public static string GetCommonPrefix(this string str1, string str2)
    {
        if (str1 == null || str2 == null)
            return string.Empty;

        var span1 = str1.AsSpan();
        var span2 = str2.AsSpan();
        var minLength = Math.Min(span1.Length, span2.Length);

        int i;
        for (i = 0; i < minLength; i++)
        {
            if (span1[i] != span2[i])
                break;
        }

        // One Substring call at the end rather than one on every early-exit branch.
        return str1.Substring(0, i);
    }

    /// <summary>
    /// Escapes special characters for safe SQL/database use (basic escaping).
    /// Note: Use parameterized queries when possible instead of this method.
    /// </summary>
    public static string EscapeForSql(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Replace("'", "''");
    }
}
