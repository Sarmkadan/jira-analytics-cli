// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for JiraIssue to provide additional analytics and filtering capabilities
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Provides extension methods for JiraIssue to enhance analytics capabilities
/// </summary>
public static class JiraIssueExtensions
{
    /// <summary>
    /// Calculates the age of the issue in days (time since creation)
    /// </summary>
    /// <param name="issue">The Jira issue</param>
    /// <returns>Number of days since issue was created</returns>
    public static int GetAgeInDays(this JiraIssue issue)
    {
        if (issue.CreatedDate == default)
            return 0;

        var days = (DateTime.UtcNow - issue.CreatedDate).TotalDays;
        return Math.Max(0, (int)days);
    }

    /// <summary>
    /// Determines if an issue is blocked based on its status and priority
    /// </summary>
    /// <param name="issue">The Jira issue</param>
    /// <returns>True if issue is blocked, false otherwise</returns>
    public static bool IsBlocked(this JiraIssue issue)
    {
        // Issues are considered blocked if they're high priority and not in progress
        return issue.IsHighPriority() &&
               !issue.IsInProgress() &&
               !issue.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) &&
               !issue.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the days remaining until the due date (negative if overdue)
    /// </summary>
    /// <param name="issue">The Jira issue</param>
    /// <returns>Days remaining until due date (negative if overdue)</returns>
    public static int GetDaysUntilDue(this JiraIssue issue)
    {
        if (!issue.DueDate.HasValue)
            return int.MaxValue; // No due date = effectively infinite time

        var daysRemaining = (issue.DueDate.Value - DateTime.UtcNow).TotalDays;
        return (int)daysRemaining;
    }

    /// <summary>
    /// Calculates the progress stagnation days - how long the issue has been in the same status
    /// </summary>
    /// <param name="issue">The Jira issue</param>
    /// <returns>Number of days the issue has been in its current status</returns>
    public static int GetStagnationDays(this JiraIssue issue)
    {
        // If issue is resolved, stagnation is from last status change to resolution
        if (issue.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) ||
            issue.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
        {
            if (issue.ResolutionDate.HasValue)
            {
                return (int)(issue.ResolutionDate.Value - issue.UpdatedDate).TotalDays;
            }

            return (int)(DateTime.UtcNow - issue.UpdatedDate).TotalDays;
        }

        // For active issues, stagnation is from last update to now
        return (int)(DateTime.UtcNow - issue.UpdatedDate).TotalDays;
    }

    /// <summary>
    /// Checks if an issue belongs to a specific component
    /// </summary>
    /// <param name="issue">The Jira issue</param>
    /// <param name="componentName">Component name to check for</param>
    /// <returns>True if issue contains the component, false otherwise</returns>
    public static bool HasComponent(this JiraIssue issue, string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentName) || issue.Components == null || issue.Components.Count == 0)
            return false;

        return issue.Components.Any(c => c.Equals(componentName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the issue's priority level as an integer for sorting/comparison
    /// Higher numbers indicate higher priority
    /// </summary>
    /// <param name="issue">The Jira issue</param>
    /// <returns>Priority level (1=Critical, 2=Blocker, 3=High, 4=Medium, 5=Low)</returns>
    public static int GetPriorityLevel(this JiraIssue issue)
    {
        return issue.Priority switch
        {
            "Critical" => 1,
            "Blocker" => 2,
            "High" => 3,
            "Medium" => 4,
            "Low" => 5,
            _ => 4 // Default to Medium
        };
    }

    /// <summary>
    /// Calculates the estimated completion percentage based on story points and time
    /// </summary>
    /// <param name="issue">The Jira issue</param>
    /// <returns>Estimated completion percentage (0-100)</returns>
    public static int GetEstimatedCompletionPercentage(this JiraIssue issue)
    {
        if (!issue.StoryPoints.HasValue || issue.StoryPoints.Value <= 0)
            return 0;

        var ageInDays = issue.GetAgeInDays();

        // Simple estimation: larger stories should progress faster initially
        // This is a heuristic - adjust based on your team's velocity
        var expectedDaysPerPoint = Math.Max(1, 10 - issue.StoryPoints.Value);
        var expectedCompletionDays = issue.StoryPoints.Value * expectedDaysPerPoint;

        if (ageInDays >= expectedCompletionDays && issue.IsInProgress())
            return 90; // Close to done

        if (ageInDays >= expectedCompletionDays * 0.7 && issue.IsInProgress())
            return 70; // Making good progress

        if (ageInDays >= expectedCompletionDays * 0.4 && issue.IsInProgress())
            return 40; // Started making progress

        return 5; // Just started
    }
}