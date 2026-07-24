// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Security utility for JQL query construction and validation
// =============================================================================

using System.Text;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Provides utilities for safely constructing and validating JQL queries.
/// </summary>
public static class JqlHelper
{
    /// <summary>
    /// Maximum allowed sprint ID value to prevent integer overflow and ensure valid sprint IDs.
    /// </summary>
    public const int MaxSprintId = int.MaxValue / 2; // Conservative limit

    /// <summary>
    /// Maximum allowed project key length to prevent excessively long keys.
    /// </summary>
    public const int MaxProjectKeyLength = 100;

    /// <summary>
    /// Maximum allowed issue key length to prevent excessively long keys.
    /// </summary>
    public const int MaxIssueKeyLength = 100;

    /// <summary>
    /// Escapes a sprint ID for use in JQL queries.
    /// Validates that the sprint ID is within acceptable bounds before returning.
    /// </summary>
    /// <param name="sprintId">The sprint ID to escape.</param>
    /// <returns>The escaped sprint ID as a string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sprintId is invalid.</exception>
    public static string EscapeSprintId(int sprintId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sprintId, 0, nameof(sprintId));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sprintId, MaxSprintId, nameof(sprintId));

        return sprintId.ToString();
    }

    /// <summary>
    /// Escapes a project key for use in JQL queries.
    /// Validates the project key format and length before returning.
    /// </summary>
    /// <param name="projectKey">The project key to escape.</param>
    /// <returns>The escaped project key as a string.</returns>
    /// <exception cref="ArgumentException">Thrown if projectKey is invalid.</exception>
    public static string EscapeProjectKey(string projectKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));

        if (projectKey.Length > MaxProjectKeyLength)
        {
            throw new ArgumentException(
                $"Project key exceeds maximum length of {MaxProjectKeyLength} characters",
                nameof(projectKey));
        }

        // Validate project key format: uppercase letters and numbers only
        // Jira project keys are typically 2-10 uppercase alphanumeric characters
        foreach (var c in projectKey)
        {
            if (!char.IsAsciiLetterUpper(c) && !char.IsAsciiDigit(c))
            {
                throw new ArgumentException(
                    "Project key must contain only uppercase letters and digits",
                    nameof(projectKey));
            }
        }

        return projectKey;
    }

    /// <summary>
    /// Escapes an issue key for use in JQL queries.
    /// Validates the issue key format and length before returning.
    /// </summary>
    /// <param name="issueKey">The issue key to escape.</param>
    /// <returns>The escaped issue key as a string.</returns>
    /// <exception cref="ArgumentException">Thrown if issueKey is invalid.</exception>
    public static string EscapeIssueKey(string issueKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issueKey, nameof(issueKey));

        if (issueKey.Length > MaxIssueKeyLength)
        {
            throw new ArgumentException(
                $"Issue key exceeds maximum length of {MaxIssueKeyLength} characters",
                nameof(issueKey));
        }

        // Issue keys are typically in format PROJECT-123
        // Validate format: uppercase letters, dash, then digits
        bool hasDash = false;
        bool hasDigits = false;

        for (int i = 0; i < issueKey.Length; i++)
        {
            var c = issueKey[i];

            if (c == '-')
            {
                if (hasDash || i == 0 || i == issueKey.Length - 1)
                {
                    throw new ArgumentException(
                        "Issue key must be in format PROJECT-123",
                        nameof(issueKey));
                }
                hasDash = true;
            }
            else if (char.IsAsciiLetterUpper(c))
            {
                if (hasDash)
                {
                    throw new ArgumentException(
                        "Digits must follow the dash in issue key",
                        nameof(issueKey));
                }
            }
            else if (char.IsAsciiDigit(c))
            {
                hasDigits = true;
            }
            else
            {
                throw new ArgumentException(
                    "Issue key must contain only uppercase letters, dash, and digits",
                    nameof(issueKey));
            }
        }

        if (!hasDash || !hasDigits)
        {
            throw new ArgumentException(
                "Issue key must be in format PROJECT-123",
                nameof(issueKey));
        }

        return issueKey;
    }

    /// <summary>
    /// Validates and clamps a maxResults parameter to prevent excessive data retrieval.
    /// </summary>
    /// <param name="maxResults">The requested max results.</param>
    /// <param name="config">Configuration containing maximum allowed values.</param>
    /// <returns>The clamped max results value.</returns>
    public static int ClampMaxResults(int maxResults, IJiraApiServiceConfig config)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxResults, 0, nameof(maxResults));

        // Clamp to the configured maximum
        var clamped = Math.Min(maxResults, config.JiraApiMaxResultsPerRequest);

        if (maxResults > config.JiraApiMaxResultsPerRequest)
        {
            // Log warning about clamping (would be logged by caller)
        }

        return clamped;
    }

    /// <summary>
    /// Validates and clamps a pageSize parameter to prevent excessive page sizes.
    /// </summary>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="config">Configuration containing maximum allowed values.</param>
    /// <returns>The clamped page size value.</returns>
    public static int ClampPageSize(int pageSize, IJiraApiServiceConfig config)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0, nameof(pageSize));

        // Clamp to the configured maximum
        var clamped = Math.Min(pageSize, config.JiraApiMaxPageSize);

        if (pageSize > config.JiraApiMaxPageSize)
        {
            // Log warning about clamping (would be logged by caller)
        }

        return clamped;
    }

    /// <summary>
    /// Validates and clamps a startAt parameter to prevent integer overflow in pagination.
    /// </summary>
    /// <param name="startAt">The requested start position.</param>
    /// <param name="config">Configuration containing maximum allowed values.</param>
    /// <returns>The clamped startAt value.</returns>
    public static int ClampStartAt(int startAt, IJiraApiServiceConfig config)
    {
        // Clamp to prevent integer overflow in subsequent calculations
        // Use a conservative limit based on max results and max pages
        var maxSafeStartAt = config.JiraApiMaxResultsPerRequest * config.JiraApiMaxPages;
        var clamped = Math.Min(startAt, maxSafeStartAt);

        if (startAt > maxSafeStartAt)
        {
            // Log warning about clamping (would be logged by caller)
        }

        return Math.Max(0, clamped); // Ensure non-negative
    }
}

/// <summary>
/// Configuration interface for Jira API service limits.
/// </summary>
public interface IJiraApiServiceConfig
{
    /// <summary>Maximum number of issues to fetch per API request.</summary>
    int JiraApiMaxResultsPerRequest { get; }

    /// <summary>Maximum number of pages to fetch when streaming results.</summary>
    int JiraApiMaxPages { get; }

    /// <summary>Maximum allowed page size for pagination operations.</summary>
    int JiraApiMaxPageSize { get; }
}