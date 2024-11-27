// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Extension methods for Sprint to provide additional analytics capabilities
// =============================================================================

using System.Globalization;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Extension methods for the Sprint class providing additional analytics and utility functionality
/// </summary>
public static class SprintExtensions
{
    /// <summary>
    /// Gets the sprint completion percentage based on completed vs total issues
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Completion percentage (0-100) or 0 if no issues</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
    public static double GetCompletionPercentage(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var totalIssues = sprint.GetTotalIssueCount();
        if (totalIssues == 0)
            return 0;

        var completedIssues = sprint.GetCompletedIssueCount();
        return (completedIssues / (double)totalIssues) * 100;
    }

    /// <summary>
    /// Gets the sprint health score (0-100) based on multiple factors:
    /// - Completion percentage
    /// - Issue velocity
    /// - Overdue issues count
    /// - Blocked issues count
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Health score (0-100) where higher is better</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
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
        var overdueScore = Math.Max(0, 1.0 - (overdueIssues.Count / 10.0));

        var blockedIssues = sprint.GetBlockedIssues();
        var blockedScore = Math.Max(0, 1.0 - (blockedIssues.Count / 5.0));

        var healthScore = (completionScore * completionWeight) +
                         (velocityScore * velocityWeight) +
                         (overdueScore * overdueWeight) +
                         (blockedScore * blockedWeight);

        return healthScore * 100;
    }

    /// <summary>
    /// Gets the average cycle time for completed issues in the sprint (in days)
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Average cycle time in days, or 0 if no completed issues</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
    public static double GetAverageCycleTime(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var completedIssues = sprint.Issues
            .Where(i => i.Status is "Done" or "Closed")
            .ToList();

        if (completedIssues.Count == 0)
            return 0;

        var totalCycleTime = completedIssues.Sum(i => i.GetCycleTime());
        return totalCycleTime / completedIssues.Count;
    }

    /// <summary>
    /// Gets the sprint burn rate (issues completed per day)
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Burn rate (issues/day) or 0 if sprint has no duration</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
    public static double GetBurnRate(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var durationDays = sprint.GetDuration();
        if (durationDays <= 0)
            return 0;

        var completedIssues = sprint.GetCompletedIssueCount();
        return completedIssues / (double)durationDays;
    }

    /// <summary>
    /// Gets the sprint status summary as a formatted string
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Formatted status string</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
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
    /// Gets high-priority issues in the sprint
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Read-only list of high-priority issues</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
    public static IReadOnlyList<JiraIssue> GetHighPriorityIssues(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        return sprint.Issues
            .Where(i => i.IsHighPriority())
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets issues that are at risk of not being completed on time
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Read-only list of at-risk issues</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
    public static IReadOnlyList<JiraIssue> GetAtRiskIssues(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var today = DateTime.UtcNow;
        var endDate = sprint.EndDate;

        return sprint.Issues
            .Where(i => i.Status != "Done" && i.Status != "Closed" &&
                        i.DueDate.HasValue &&
                        i.DueDate.Value < endDate &&
                        i.DueDate.Value < today)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the sprint progress trend (positive, neutral, or negative)
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Trend indicator</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
    public static string GetProgressTrend(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var currentCompletion = sprint.GetCompletionPercentage();
        var velocity = sprint.GetVelocity();

        if (currentCompletion >= 90 && velocity >= 80)
            return "↗️ On Track";
        if (currentCompletion < 70 || velocity < 50)
            return "⚠️ At Risk";
        if (currentCompletion >= 70 && currentCompletion < 90 && velocity >= 50 && velocity < 80)
            return "→ Steady Progress";

        return "⏸️ Monitoring Required";
    }

    /// <summary>
    /// Gets the sprint's goal status
    /// </summary>
    /// <param name="sprint">The sprint instance</param>
    /// <returns>Goal status description</returns>
    /// <exception cref="ArgumentNullException">Thrown when sprint is null</exception>
    public static string GetGoalStatus(this Sprint sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        var completionPercent = sprint.GetCompletionPercentage();
        var storyPointsCompleted = sprint.GetCompletedStoryPoints();
        var storyPointsPlanned = sprint.GetPlannedStoryPoints();

        if (string.IsNullOrWhiteSpace(sprint.Goal))
            return "No goal defined";

        if (completionPercent >= 100)
            return $"✅ Goal achieved: {sprint.Goal}";

        if (completionPercent >= 80)
            return $"🎯 Goal likely achievable: {sprint.Goal} ({completionPercent:F1}% complete, {storyPointsCompleted}/{storyPointsPlanned} story points)";

        return $"❌ Goal at risk: {sprint.Goal} ({completionPercent:F1}% complete, {storyPointsCompleted}/{storyPointsPlanned} story points)";
    }
}