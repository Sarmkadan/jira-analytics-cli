// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace JiraAnalyticsCli.Filters;

/// <summary>
/// Filters and transforms command inputs and outputs.
/// Applies validation, sanitization, and formatting rules to commands.
/// </summary>
public class CommandFilter
{
    /// <summary>
    /// Validates project key format according to Jira standards.
    /// Jira keys must be 2-10 alphanumeric uppercase characters.
    /// </summary>
    public static bool ValidateProjectKey(string projectKey, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(projectKey))
        {
            errorMessage = "Project key cannot be empty";
            return false;
        }

        if (projectKey.Length < 2 || projectKey.Length > 10)
        {
            errorMessage = "Project key must be 2-10 characters";
            return false;
        }

        if (!Regex.IsMatch(projectKey, "^[A-Z][A-Z0-9]*$"))
        {
            errorMessage = "Project key must start with letter and contain only uppercase alphanumeric characters";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Normalizes project key to proper format (uppercase, trim).
    /// </summary>
    public static string NormalizeProjectKey(string projectKey)
    {
        return projectKey?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    /// <summary>
    /// Validates file path for security and accessibility.
    /// Prevents directory traversal and ensures path is writable.
    /// </summary>
    public static bool ValidateOutputPath(string filePath, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            errorMessage = "File path cannot be empty";
            return false;
        }

        // Prevent directory traversal
        if (filePath.Contains(".."))
        {
            errorMessage = "File path cannot contain parent directory references";
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);

            // Check if directory exists or can be created
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch
                {
                    errorMessage = $"Cannot create directory: {directory}";
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Invalid file path: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Validates export format is supported.
    /// </summary>
    public static bool ValidateExportFormat(string format, out string errorMessage)
    {
        errorMessage = string.Empty;
        var supportedFormats = new[] { "json", "csv", "xml", "markdown", "png", "pdf" };

        if (string.IsNullOrEmpty(format))
        {
            errorMessage = "Format cannot be empty";
            return false;
        }

        if (!supportedFormats.Contains(format.ToLower()))
        {
            errorMessage = $"Unsupported format. Supported formats: {string.Join(", ", supportedFormats)}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sanitizes output string for safe display and storage.
    /// Removes or escapes potentially harmful characters.
    /// </summary>
    public static string SanitizeOutput(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove control characters except newline and tab
        return Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
    }

    /// <summary>
    /// Truncates long strings for display while preserving readability.
    /// </summary>
    public static string TruncateForDisplay(string text, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 1) + "…";
    }

    /// <summary>
    /// Filters collection items based on predicate function.
    /// Useful for filtering results before display or export.
    /// </summary>
    public static IEnumerable<T> FilterItems<T>(IEnumerable<T> items, Func<T, bool> predicate)
    {
        return items?.Where(predicate) ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Applies multiple transformation filters to text in sequence.
    /// </summary>
    public static string ApplyFilters(string input, params Func<string, string>[] filters)
    {
        var result = input;

        foreach (var filter in filters)
        {
            result = filter(result);
        }

        return result;
    }
}
