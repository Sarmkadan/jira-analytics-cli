// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Validation helpers for JiraIssueTests to ensure test data integrity.
/// </summary>
public static class JiraIssueTestsValidation
{
    /// <summary>
    /// Validates a JiraIssueTests instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The JiraIssueTests instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this JiraIssueTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue
        try
        {
            value.IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue();
        }
        catch (Exception ex)
        {
            problems.Add($"IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue failed: {ex.Message}");
        }

        // Validate IsOverdue_WhenStatusIsDone_ReturnsFalse
        try
        {
            value.IsOverdue_WhenStatusIsDone_ReturnsFalse();
        }
        catch (Exception ex)
        {
            problems.Add($"IsOverdue_WhenStatusIsDone_ReturnsFalse failed: {ex.Message}");
        }

        // Validate IsOverdue_WhenResolutionDateIsSet_ReturnsFalse
        try
        {
            value.IsOverdue_WhenResolutionDateIsSet_ReturnsFalse();
        }
        catch (Exception ex)
        {
            problems.Add($"IsOverdue_WhenResolutionDateIsSet_ReturnsFalse failed: {ex.Message}");
        }

        // Validate IsHighPriority_WithCriticalPriority_ReturnsTrue
        try
        {
            value.IsHighPriority_WithCriticalPriority_ReturnsTrue();
        }
        catch (Exception ex)
        {
            problems.Add($"IsHighPriority_WithCriticalPriority_ReturnsTrue failed: {ex.Message}");
        }

        // Validate IsHighPriority_WithMediumPriority_ReturnsFalse
        try
        {
            value.IsHighPriority_WithMediumPriority_ReturnsFalse();
        }
        catch (Exception ex)
        {
            problems.Add($"IsHighPriority_WithMediumPriority_ReturnsFalse failed: {ex.Message}");
        }

        // Validate GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution
        try
        {
            value.GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution();
        }
        catch (Exception ex)
        {
            problems.Add($"GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the JiraIssueTests instance is valid.
    /// </summary>
    /// <param name="value">The JiraIssueTests instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this JiraIssueTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the JiraIssueTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The JiraIssueTests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this JiraIssueTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JiraIssueTests instance is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}
