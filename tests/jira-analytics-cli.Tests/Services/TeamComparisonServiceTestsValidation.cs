// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;

namespace JiraAnalyticsCli.Tests.Services;

/// <summary>
/// Validation helpers for TeamComparisonService functionality.
/// Provides reusable validation logic for comparing team performance metrics.
/// </summary>
public static class TeamComparisonServiceTestsValidation
{
    /// <summary>
    /// Validates that a team comparison report contains the expected teams.
    /// </summary>
    /// <param name="report">The comparison report to validate.</param>
    /// <param name="expectedProjectKeys">The expected project keys that should be present in the report.</param>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when expectedProjectKeys is null.</exception>
    public static void ShouldContainTeams(this TeamComparisonReport report, IReadOnlyCollection<string> expectedProjectKeys)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(expectedProjectKeys);

        report.Teams.Should().NotBeNull($"because the report must contain team snapshots");
        report.Teams.Should().HaveCount(expectedProjectKeys.Count,
            $"because the report should contain exactly {expectedProjectKeys.Count} team(s)");

        foreach (var projectKey in expectedProjectKeys)
        {
            report.Teams.Should().Contain(t => string.Equals(t.ProjectKey, projectKey, StringComparison.OrdinalIgnoreCase),
                $"because the report should contain team with project key '{projectKey}'");
        }
    }

    /// <summary>
    /// Validates that a team comparison report identifies the fastest team correctly.
    /// </summary>
    /// <param name="report">The comparison report to validate.</param>
    /// <param name="expectedFastestTeam">The project key expected to be identified as the fastest team.</param>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    /// <exception cref="ArgumentException">Thrown when expectedFastestTeam is null or empty.</exception>
    public static void ShouldIdentifyFastestTeam(this TeamComparisonReport report, string expectedFastestTeam)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrEmpty(expectedFastestTeam);

        report.FastestTeam.Should().Be(expectedFastestTeam,
            $"because the fastest team should be '{expectedFastestTeam}'");
    }

    /// <summary>
    /// Validates that a team comparison report identifies the highest quality team correctly.
    /// </summary>
    /// <param name="report">The comparison report to validate.</param>
    /// <param name="expectedHighestQualityTeam">The project key expected to be identified as the highest quality team.</param>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    /// <exception cref="ArgumentException">Thrown when expectedHighestQualityTeam is null or empty.</exception>
    public static void ShouldIdentifyHighestQualityTeam(this TeamComparisonReport report, string expectedHighestQualityTeam)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrEmpty(expectedHighestQualityTeam);

        report.HighestQualityTeam.Should().Be(expectedHighestQualityTeam,
            $"because the highest quality team should be '{expectedHighestQualityTeam}'");
    }

    /// <summary>
    /// Validates that a team comparison report identifies the most consistent team correctly.
    /// </summary>
    /// <param name="report">The comparison report to validate.</param>
    /// <param name="expectedMostConsistentTeam">The project key expected to be identified as the most consistent team.</param>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    /// <exception cref="ArgumentException">Thrown when expectedMostConsistentTeam is null or empty.</exception>
    public static void ShouldIdentifyMostConsistentTeam(this TeamComparisonReport report, string expectedMostConsistentTeam)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrEmpty(expectedMostConsistentTeam);

        report.MostConsistentTeam.Should().Be(expectedMostConsistentTeam,
            $"because the most consistent team should be '{expectedMostConsistentTeam}'");
    }

    /// <summary>
    /// Validates that a team project snapshot has the expected velocity metrics.
    /// </summary>
    /// <param name="snapshot">The team snapshot to validate.</param>
    /// <param name="expectedVelocity">The expected average velocity value.</param>
    /// <param name="expectedDefectRate">The expected defect rate value.</param>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null.</exception>
    public static void ShouldHaveMetrics(this TeamProjectSnapshot snapshot, double expectedVelocity, double expectedDefectRate)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        snapshot.AverageVelocity.Should().BeApproximately(expectedVelocity, 0.1,
            $"because the average velocity should be approximately {expectedVelocity}");
        snapshot.DefectRate.Should().BeApproximately(expectedDefectRate, 0.1,
            $"because the defect rate should be approximately {expectedDefectRate}");
    }

    /// <summary>
    /// Validates that a team comparison report contains expected summary text.
    /// </summary>
    /// <param name="textReport">The text report to validate.</param>
    /// <param name="expectedProjectKeys">The project keys expected to be mentioned in the report.</param>
    /// <exception cref="ArgumentNullException">Thrown when textReport is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when expectedProjectKeys is null.</exception>
    public static void ShouldContainProjectKeys(this string textReport, IReadOnlyCollection<string> expectedProjectKeys)
    {
        ArgumentNullException.ThrowIfNull(textReport);
        ArgumentNullException.ThrowIfNull(expectedProjectKeys);

        foreach (var projectKey in expectedProjectKeys)
        {
            textReport.Should().Contain(projectKey,
                $"because the text report should mention project key '{projectKey}'");
        }
    }

    /// <summary>
    /// Validates that a team comparison report contains expected performance labels.
    /// </summary>
    /// <param name="textReport">The text report to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when textReport is null.</exception>
    public static void ShouldContainPerformanceLabels(this string textReport)
    {
        ArgumentNullException.ThrowIfNull(textReport);

        textReport.Should().Contain("Fastest team",
            "because the report should identify the fastest team");
        textReport.Should().Contain("Highest quality",
            "because the report should identify the highest quality team");
        textReport.Should().Contain("Most consistent",
            "because the report should identify the most consistent team");
    }
}