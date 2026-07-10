// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using JiraAnalyticsCli.Utils;

namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Partial class that adds validation properties to <see cref="ValidationHelpersTests"/>.
/// This extends the existing test class to support validation of test data.
/// </summary>
public sealed partial class ValidationHelpersTests
{
    /// <summary>
    /// Gets or sets a Jira issue key for testing.
    /// </summary>
    public string? IssueKey { get; set; }

    /// <summary>
    /// Gets or sets a Jira project key for testing.
    /// </summary>
    public string? ProjectKey { get; set; }

    /// <summary>
    /// Gets or sets a URL for testing.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets an email address for testing.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets a sprint ID for testing.
    /// </summary>
    public int SprintId { get; set; }

    /// <summary>
    /// Gets or sets story points for testing.
    /// </summary>
    public int StoryPoints { get; set; }

    /// <summary>
    /// Gets or sets a start date for testing.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets an end date for testing.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets a percentage value for testing.
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// Validation helpers for the <see cref="ValidationHelpersTests"/> test data class.
/// Provides validation methods to ensure test data is valid before execution.
/// </summary>
public static class ValidationHelpersTestsValidation
{
    /// <summary>
    /// Validates that a <see cref="ValidationHelpersTests"/> instance contains valid data.
    /// </summary>
    /// <param name="value">The ValidationHelpersTests instance to validate.</param>
    /// <returns>A list of human-readable validation problems. Empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationHelpersTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Jira issue keys
        if (!string.IsNullOrEmpty(value.IssueKey) && !ValidationHelpers.IsValidJiraIssueKey(value.IssueKey))
        {
            problems.Add($"IssueKey '{value.IssueKey}' is not a valid Jira issue key format. Expected format: PROJ-123");
        }

        // Validate project keys
        if (!string.IsNullOrEmpty(value.ProjectKey) && !ValidationHelpers.IsValidProjectKey(value.ProjectKey))
        {
            problems.Add($"ProjectKey '{value.ProjectKey}' is not a valid Jira project key format. Expected uppercase alphanumeric, max 10 characters");
        }

        // Validate URLs
        if (!string.IsNullOrEmpty(value.Url) && !ValidationHelpers.IsValidUrl(value.Url))
        {
            problems.Add($"Url '{value.Url}' is not a valid URL format. Expected http:// or https:// protocol");
        }

        // Validate emails
        if (!string.IsNullOrEmpty(value.Email) && !ValidationHelpers.IsValidEmail(value.Email))
        {
            problems.Add($"Email '{value.Email}' is not a valid email format");
        }

        // Validate sprint IDs (must be positive)
        if (value.SprintId <= 0)
        {
            problems.Add($"SprintId '{value.SprintId}' must be a positive integer");
        }

        // Validate story points (must be non-negative)
        if (value.StoryPoints < 0)
        {
            problems.Add($"StoryPoints '{value.StoryPoints}' must be a non-negative integer");
        }

        // Validate date ranges (start must be before end)
        if (value.StartDate != default && value.EndDate != default && !ValidationHelpers.IsValidDateRange(value.StartDate, value.EndDate))
        {
            problems.Add($"Date range is invalid: StartDate ({value.StartDate}) must be before EndDate ({value.EndDate})");
        }

        // Validate percentage (0-100)
        if (value.Percentage < 0 || value.Percentage > 100)
        {
            problems.Add($"Percentage '{value.Percentage}' must be between 0 and 100 inclusive");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ValidationHelpersTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The ValidationHelpersTests instance to check.</param>
    /// <returns>true if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this ValidationHelpersTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ValidationHelpersTests"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems if invalid.
    /// </summary>
    /// <param name="value">The ValidationHelpersTests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation problems.</exception>
    public static void EnsureValid(this ValidationHelpersTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ValidationHelpersTests instance is invalid:{Environment.NewLine}" +
            string.Join(Environment.NewLine, problems)
        );
    }
}
