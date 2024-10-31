// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace JiraAnalyticsCli.Tests.Services;

/// <summary>
/// Validation helpers for the TeamComparisonServiceTests class.
/// </summary>
public static class TeamComparisonServiceTestsValidation
{
    /// <summary>
    /// Validates that the TeamComparisonServiceTests instance meets all requirements.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this TeamComparisonServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that all test methods are present and can be invoked
        problems.AddRange(ValidateTestMethodInvocations(value));

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the TeamComparisonServiceTests instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(this TeamComparisonServiceTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the TeamComparisonServiceTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValid(this TeamComparisonServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TeamComparisonServiceTests validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    private static IReadOnlyList<string> ValidateTestMethodInvocations(TeamComparisonServiceTests value)
    {
        var problems = new List<string>();

        try
        {
            // CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots
            var task1 = value.CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots();
            if (task1 == null)
            {
                problems.Add("CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots returned null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots threw: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            // CompareTeamsAsync_IdentifiesFastestTeamCorrectly
            var task2 = value.CompareTeamsAsync_IdentifiesFastestTeamCorrectly();
            if (task2 == null)
            {
                problems.Add("CompareTeamsAsync_IdentifiesFastestTeamCorrectly returned null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CompareTeamsAsync_IdentifiesFastestTeamCorrectly threw: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            // CompareTeamsAsync_IdentifiesHighestQualityTeamByLowestDefectRate
            var task3 = value.CompareTeamsAsync_IdentifiesHighestQualityTeamByLowestDefectRate();
            if (task3 == null)
            {
                problems.Add("CompareTeamsAsync_IdentifiesHighestQualityTeamByLowestDefectRate returned null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CompareTeamsAsync_IdentifiesHighestQualityTeamByLowestDefectRate threw: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            // CompareTeamsAsync_DeduplicatesProjectKeys
            var task4 = value.CompareTeamsAsync_DeduplicatesProjectKeys();
            if (task4 == null)
            {
                problems.Add("CompareTeamsAsync_DeduplicatesProjectKeys returned null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CompareTeamsAsync_DeduplicatesProjectKeys threw: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            // CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException
            var task5 = value.CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException();
            if (task5 == null)
            {
                problems.Add("CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException returned null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException threw: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            // CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException
            var task6 = value.CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException();
            if (task6 == null)
            {
                problems.Add("CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException returned null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException threw: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            // FormatAsText_WithTwoTeams_ContainsBothProjectKeys
            value.FormatAsText_WithTwoTeams_ContainsBothProjectKeys();
        }
        catch (Exception ex)
        {
            problems.Add($"FormatAsText_WithTwoTeams_ContainsBothProjectKeys threw: {ex.GetType().Name}: {ex.Message}");
        }

        return problems;
    }
}
