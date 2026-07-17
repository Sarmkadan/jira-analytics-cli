// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Provides validation for <see cref="JiraIssueTests"/> instances to ensure test methods execute correctly.
/// </summary>
public static class JiraIssueTestsValidation
{
    /// <summary>
    /// Validates that all test methods in a <see cref="JiraIssueTests"/> instance execute successfully.
    /// </summary>
    /// <param name="tests">The JiraIssueTests instance containing test methods to validate.</param>
    /// <returns>A read-only list of validation problems; empty if all tests pass.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this JiraIssueTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var problems = new List<string>();

        // Test overdue logic
        ValidateOverdueTests(tests, problems);

        // Test priority logic
        ValidatePriorityTests(tests, problems);

        // Test cycle time logic
        ValidateCycleTimeTests(tests, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="JiraIssueTests"/> instance contains valid test methods.
    /// </summary>
    /// <param name="tests">The JiraIssueTests instance to check.</param>
    /// <returns>True if all test methods execute successfully; otherwise, false.</returns>
    public static bool IsValid(this JiraIssueTests tests)
    {
        return tests.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that all test methods in the <see cref="JiraIssueTests"/> instance execute successfully.
    /// </summary>
    /// <param name="tests">The JiraIssueTests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if any test method fails, containing a list of problems.</exception>
    public static void EnsureValid(this JiraIssueTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var problems = tests.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JiraIssueTests test methods failed validation:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    private static void ValidateOverdueTests(JiraIssueTests tests, List<string> problems)
    {
        try
        {
            tests.IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue();
        }
        catch (Exception ex)
        {
            problems.Add($"IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue failed: {ex.Message}");
        }

        try
        {
            tests.IsOverdue_WhenStatusIsDone_ReturnsFalse();
        }
        catch (Exception ex)
        {
            problems.Add($"IsOverdue_WhenStatusIsDone_ReturnsFalse failed: {ex.Message}");
        }

        try
        {
            tests.IsOverdue_WhenResolutionDateIsSet_ReturnsFalse();
        }
        catch (Exception ex)
        {
            problems.Add($"IsOverdue_WhenResolutionDateIsSet_ReturnsFalse failed: {ex.Message}");
        }
    }

    private static void ValidatePriorityTests(JiraIssueTests tests, List<string> problems)
    {
        try
        {
            tests.IsHighPriority_WithCriticalPriority_ReturnsTrue();
        }
        catch (Exception ex)
        {
            problems.Add($"IsHighPriority_WithCriticalPriority_ReturnsTrue failed: {ex.Message}");
        }

        try
        {
            tests.IsHighPriority_WithMediumPriority_ReturnsFalse();
        }
        catch (Exception ex)
        {
            problems.Add($"IsHighPriority_WithMediumPriority_ReturnsFalse failed: {ex.Message}");
        }
    }

    private static void ValidateCycleTimeTests(JiraIssueTests tests, List<string> problems)
    {
        try
        {
            tests.GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution();
        }
        catch (Exception ex)
        {
            problems.Add($"GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution failed: {ex.Message}");
        }
    }
}
