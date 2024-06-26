// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Validation utilities for common scenarios and data validation
/// </summary>
public static class ValidationHelpers
{
    /// <summary>
    /// Validate Jira issue key format (e.g., PROJ-123)
    /// </summary>
    public static bool IsValidJiraIssueKey(string? issueKey)
    {
        if (string.IsNullOrWhiteSpace(issueKey))
            return false;

        var pattern = @"^[A-Z][A-Z0-9]+-\d+$";
        return Regex.IsMatch(issueKey, pattern);
    }

    /// <summary>
    /// Validate Jira project key format (uppercase alphanumeric)
    /// </summary>
    public static bool IsValidProjectKey(string? projectKey)
    {
        if (string.IsNullOrWhiteSpace(projectKey))
            return false;

        var pattern = @"^[A-Z][A-Z0-9]*$";
        return Regex.IsMatch(projectKey, pattern) && projectKey.Length <= 10;
    }

    /// <summary>
    /// Validate URL format
    /// </summary>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate sprint ID (positive integer)
    /// </summary>
    public static bool IsValidSprintId(int sprintId)
    {
        return sprintId > 0;
    }

    /// <summary>
    /// Validate story points (non-negative)
    /// </summary>
    public static bool IsValidStoryPoints(int? storyPoints)
    {
        return storyPoints == null || storyPoints.Value >= 0;
    }

    /// <summary>
    /// Validate date range (start before end)
    /// </summary>
    public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
    {
        return startDate < endDate;
    }

    /// <summary>
    /// Validate percentage value (0-100)
    /// </summary>
    public static bool IsValidPercentage(double value)
    {
        return value >= 0 && value <= 100;
    }

    /// <summary>
    /// Truncate string to maximum length with ellipsis
    /// </summary>
    public static string TruncateWithEllipsis(string? value, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Sanitize string for CSV/file output
    /// </summary>
    public static string SanitizeForCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Remove any characters that could break CSV format
        var sanitized = Regex.Replace(value, @"[,\r\n""]", string.Empty);
        return sanitized;
    }

    /// <summary>
    /// Convert percentage to visual progress bar
    /// </summary>
    public static string ToProgressBar(double percentage, int length = 20)
    {
        if (!IsValidPercentage(percentage))
            percentage = 0;

        var filled = (int)((percentage / 100) * length);
        var empty = length - filled;

        return "[" + new string('█', filled) + new string('░', empty) + "]";
    }
}
