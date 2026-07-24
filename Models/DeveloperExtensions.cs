using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraAnalyticsCli.Models;

public static class DeveloperExtensions
{
    /// <summary>
    /// Gets a display label for a developer, falling back to email if name is not available.
    /// </summary>
    /// <param name="developer">The developer to get the label for.</param>
    /// <returns>A display label combining name and email.</returns>
    public static string DisplayLabel(this Developer developer)
    {
        if (developer == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(developer.Name))
        {
            return developer.Name;
        }

        return !string.IsNullOrWhiteSpace(developer.Email)
            ? developer.Email
            : string.Empty;
    }

    /// <summary>
    /// Extracts initials from a developer's name.
    /// </summary>
    /// <param name="developer">The developer to extract initials from.</param>
    /// <returns>Initials from the developer's name, or empty string if not available.</returns>
    public static string GetInitials(this Developer developer)
    {
        if (developer?.Name == null)
        {
            return string.Empty;
        }

        var name = developer.Name.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        var parts = name.Split(new[] { ' ', '-', '\'' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        // Take first letter of each part, up to 3 initials
        var initials = string.Concat(parts.Take(3).Select(p => p.Length > 0 ? p[0] : ' '));
        return initials.Trim();
    }

    /// <summary>
    /// Groups a collection of Jira issues by assignee.
    /// </summary>
    /// <param name="issues">The issues to group.</param>
    /// <param name="includeUnassigned">Whether to include unassigned issues in the result.</param>
    /// <returns>A dictionary mapping assignee names (or "Unassigned" string) to their assigned issues.</returns>
    public static Dictionary<string, List<JiraIssue>> GroupByAssignee(
        this IEnumerable<JiraIssue> issues,
        bool includeUnassigned = true)
    {
        if (issues == null)
        {
            return new Dictionary<string, List<JiraIssue>>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, List<JiraIssue>>(StringComparer.OrdinalIgnoreCase);

        foreach (var issue in issues)
        {
            var assigneeName = issue.Assignee ?? "Unassigned";

            if (!result.TryGetValue(assigneeName, out var issueList))
            {
                issueList = new List<JiraIssue>();
                result[assigneeName] = issueList;
            }

            issueList.Add(issue);
        }

        return result;
    }

    /// <summary>
    /// Groups a collection of Jira issues by assignee, returning a dictionary with Developer objects as keys.
    /// Note: This requires a collection of developers to match against issue assignee strings.
    /// </summary>
    /// <param name="issues">The issues to group.</param>
    /// <param name="developers">Collection of developers to match against issue assignees.</param>
    /// <param name="includeUnassigned">Whether to include unassigned issues in the result.</param>
    /// <returns>A dictionary mapping developers to their assigned issues.</returns>
    public static Dictionary<Developer, List<JiraIssue>> GroupByAssigneeWithDeveloper(
        this IEnumerable<JiraIssue> issues,
        IEnumerable<Developer> developers,
        bool includeUnassigned = true)
    {
        if (issues == null)
        {
            return new Dictionary<Developer, List<JiraIssue>>();
        }

        var developerLookup = developers?.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, Developer>(StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<Developer, List<JiraIssue>>();

        foreach (var issue in issues)
        {
            Developer assigneeDeveloper = null;

            if (!string.IsNullOrEmpty(issue.Assignee))
            {
                // Try to find matching developer by name
                if (developerLookup.TryGetValue(issue.Assignee, out var foundDeveloper))
                {
                    assigneeDeveloper = foundDeveloper;
                }
                else
                {
                    // Create a placeholder developer for the assignee string
                    assigneeDeveloper = new Developer { Name = issue.Assignee, Email = null };
                }
            }

            if (assigneeDeveloper != null)
            {
                if (!result.TryGetValue(assigneeDeveloper, out var issueList))
                {
                    issueList = new List<JiraIssue>();
                    result[assigneeDeveloper] = issueList;
                }

                issueList.Add(issue);
            }
            else if (includeUnassigned)
            {
                // Use a special key for unassigned issues
                var unassignedKey = new Developer { Name = "Unassigned", Email = null };
                if (!result.TryGetValue(unassignedKey, out var issueList))
                {
                    issueList = new List<JiraIssue>();
                    result[unassignedKey] = issueList;
                }

                issueList.Add(issue);
            }
        }

        return result;
    }
}
