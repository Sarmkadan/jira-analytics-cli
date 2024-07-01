// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Extension methods for string manipulation and formatting.
/// Provides safe parsing, cleaning, and transformation utilities.
/// </summary>
public static class StringExtensions
{
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

        return value.Substring(0, Math.Max(0, maxLength - 1)) + "…";
    }

    /// <summary>
    /// Removes all whitespace characters from string.
    /// Useful for normalizing project keys or identifiers.
    /// </summary>
    public static string RemoveWhitespace(this string value)
    {
        return Regex.Replace(value, @"\s+", string.Empty);
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
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, "-+", "-");

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

        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\?", ".")
            .Replace("\\*", ".*") + "$";

        return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Gets common prefix shared between two strings.
    /// Useful for finding common parts in identifiers or file paths.
    /// </summary>
    public static string GetCommonPrefix(this string str1, string str2)
    {
        if (str1 == null || str2 == null)
            return string.Empty;

        var minLength = Math.Min(str1.Length, str2.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (str1[i] != str2[i])
                return str1.Substring(0, i);
        }

        return str1.Substring(0, minLength);
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
