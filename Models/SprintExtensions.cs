// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Extension methods for Sprint to provide additional analytics capabilities
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Provides extension methods for <see cref="Sprint"/> to enhance sprint analytics capabilities with
/// comprehensive tracking of completion, health, velocity, and risk assessment.
/// </summary>
public static class SprintExtensions
{
    private const double MaxOverdueIssuesForFullScore = 10.0;
    private const double MaxBlockedIssuesForFullScore = 5.0;
    private const double CompletionThresholdForOnTrack = 90.0;
    private const double VelocityThresholdForOnTrack = 80.0;
    private const double CompletionThresholdForAtRisk = 70.0;
    private const double VelocityThresholdForAtRisk = 50.0;

    /// <summary>
    /// Calculates the sprint completion percentage based on completed versus total issues.
    /// </summary>
    /// <param name="sprint">The sprint instance to calculate completion for.</param>
    /// <returns>Completion percentage as a value between 0 and 100, or 0 if no issues exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static double GetCompletionPercentage(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var totalIssues = sprint.GetTotalIssueCount();
        return totalIssues == 0 ? 0 : (sprint.GetCompletedIssueCount() / (double)totalIssues) * 100;
    }

    /// <summary>
    /// Calculates the sprint health score (0-100) based on multiple factors:
    /// - Completion percentage (40% weight)
    /// - Issue velocity (30% weight)
    /// - Overdue issues count (20% weight)
    /// - Blocked issues count (10% weight)
    /// </summary>
    /// <param name="sprint">The sprint instance to evaluate.</param>
    /// <returns>Health score as a value between 0 and 100, where higher is better.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static double GetHealthScore(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        const double completionWeight = 0.4;
        const double velocityWeight = 0.3;
        const double overdueWeight = 0.2;
        const double blockedWeight = 0.1;

        var completionScore = Math.Min(sprint.GetCompletionPercentage() / 100.0, 1.0);
        var velocityScore = Math.Min(sprint.GetVelocity() / 100.0, 1.0);

        var overdueIssues = sprint.GetOverdueIssues();
        var overdueScore = Math.Max(0, 1.0 - (overdueIssues.Count / MaxOverdueIssuesForFullScore));

        var blockedIssues = sprint.GetBlockedIssues();
        var blockedScore = Math.Max(0, 1.0 - (blockedIssues.Count / MaxBlockedIssuesForFullScore));

        var healthScore = (completionScore * completionWeight) +
                         (velocityScore * velocityWeight) +
                         (overdueScore * overdueWeight) +
                         (blockedScore * blockedWeight);

        return healthScore * 100;
    }

    /// <summary>
    /// Calculates the average cycle time for completed issues in the sprint (in days).
    /// </summary>
    /// <param name="sprint">The sprint instance to analyze.</param>
    /// <returns>Average cycle time in days, or 0 if no completed issues exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static double GetAverageCycleTime(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var completedIssues = sprint.Issues
            .Where(i => i.Status is "Done" or "Closed")
            .ToList();

        return completedIssues.Count == 0
            ? 0
            : completedIssues.Sum(i => i.GetCycleTime()) / completedIssues.Count;
    }

    /// <summary>
    /// Calculates the sprint burn rate (issues completed per day).
    /// </summary>
    /// <param name="sprint">The sprint instance to evaluate.</param>
    /// <returns>Burn rate as issues per day, or 0 if sprint has no duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static double GetBurnRate(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var durationDays = sprint.GetDuration();
        return durationDays <= 0
            ? 0
            : sprint.GetCompletedIssueCount() / durationDays;
    }

    /// <summary>
    /// Generates a formatted sprint status summary string.
    /// </summary>
    /// <param name="sprint">The sprint instance to summarize.</param>
    /// <returns>A formatted status string with key metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static string GetStatusSummary(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var completionPercent = sprint.GetCompletionPercentage();
        var healthScore = sprint.GetHealthScore();
        var burnRate = sprint.GetBurnRate();
        var velocity = sprint.GetVelocity();

        return $"Sprint {sprint.Key}: {sprint.Name} | " +
               $"State: {sprint.State} | " +
               $"Completion: {completionPercent:F1}% ({sprint.GetCompletedIssueCount()}/{sprint.GetTotalIssueCount()}) | " +
               $"Health: {healthScore:F1}% | " +
               $"Velocity: {velocity:F1}% | " +
               $"Burn Rate: {burnRate:F2} issues/day";
    }

    /// <summary>
    /// Gets high-priority issues in the sprint as a read-only list.
    /// </summary>
    /// <param name="sprint">The sprint instance to query.</param>
    /// <returns>Read-only list of high-priority issues.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<JiraIssue> GetHighPriorityIssues(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        return sprint.Issues
            .Where(i => i.IsHighPriority())
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Identifies issues that are at risk of not being completed on time.
    /// </summary>
    /// <param name="sprint">The sprint instance to analyze.</param>
    /// <returns>Read-only list of at-risk issues.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<JiraIssue> GetAtRiskIssues(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var today = DateTime.UtcNow;
        var endDate = sprint.EndDate;

        return sprint.Issues
            .Where(i => i.Status != "Done" &&
                        i.Status != "Closed" &&
                        i.DueDate.HasValue &&
                        i.DueDate.Value < endDate &&
                        i.DueDate.Value < today)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Determines the sprint progress trend based on completion and velocity metrics.
    /// </summary>
    /// <param name="sprint">The sprint instance to evaluate.</param>
    /// <returns>A trend indicator string ("On Track", "At Risk", "Steady Progress", or "Monitoring Required").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static string GetProgressTrend(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var currentCompletion = sprint.GetCompletionPercentage();
        var velocity = sprint.GetVelocity();

        return currentCompletion >= CompletionThresholdForOnTrack && velocity >= VelocityThresholdForOnTrack
            ? "↗️ On Track"
            : currentCompletion < CompletionThresholdForAtRisk || velocity < VelocityThresholdForAtRisk
                ? "⚠️ At Risk"
                : currentCompletion >= CompletionThresholdForAtRisk && currentCompletion < CompletionThresholdForOnTrack &&
                  velocity >= VelocityThresholdForAtRisk && velocity < VelocityThresholdForOnTrack
                    ? "→ Steady Progress"
                    : "⏸️ Monitoring Required";
    }

    /// <summary>
    /// Evaluates the sprint's goal status based on completion metrics.
    /// </summary>
    /// <param name="sprint">The sprint instance to evaluate.</param>
    /// <returns>A goal status description with emoji indicators.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sprint"/> is <see langword="null"/>.</exception>
    public static string GetGoalStatus(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var completionPercent = sprint.GetCompletionPercentage();
        var storyPointsCompleted = sprint.GetCompletedStoryPoints();
        var storyPointsPlanned = sprint.GetPlannedStoryPoints();

        return string.IsNullOrWhiteSpace(sprint.Goal)
            ? "No goal defined"
            : completionPercent >= 100
                ? $"✅ Goal achieved: {sprint.Goal}"
                : completionPercent >= 80
                    ? $"🎯 Goal likely achievable: {sprint.Goal} ({completionPercent:F1}% complete, {storyPointsCompleted}/{storyPointsPlanned} story points)"
                    : $"❌ Goal at risk: {sprint.Goal} ({completionPercent:F1}% complete, {storyPointsCompleted}/{storyPointsPlanned} story points)";
    }
}
