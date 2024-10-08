using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories
{
    public static class IssueRepositoryExtensions
    {
        /// <summary>
        /// Gets all issues that are assigned to a specific user.
        /// </summary>
        /// <param name="repository">The issue repository instance.</param>
        /// <param name="projectKey">The project key to filter by.</param>
        /// <param name="assignee">The username to filter by.</param>
        /// <returns>A list of issues assigned to the specified user.</returns>
        public static async ValueTask<List<JiraIssue>> GetByAssigneeAsync(this IssueRepository repository, string projectKey, string assignee)
        {
            if (string.IsNullOrWhiteSpace(assignee))
            {
                throw new ArgumentException("Assignee cannot be null or whitespace.", nameof(assignee));
            }

            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be null or whitespace.", nameof(projectKey));
            }

            var allIssues = await repository.GetByProjectAsync(projectKey);
            return allIssues
                .Where(issue => issue.Assignee?.Equals(assignee, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        /// <summary>
        /// Gets all issues that are unassigned.
        /// </summary>
        /// <param name="repository">The issue repository instance.</param>
        /// <param name="projectKey">The project key to filter by.</param>
        /// <returns>A list of unassigned issues.</returns>
        public static async ValueTask<List<JiraIssue>> GetUnassignedAsync(this IssueRepository repository, string projectKey)
        {
            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be null or whitespace.", nameof(projectKey));
            }

            var allIssues = await repository.GetByProjectAsync(projectKey);
            return allIssues
                .Where(issue => string.IsNullOrWhiteSpace(issue.Assignee))
                .ToList();
        }

        /// <summary>
        /// Gets all issues that are due within a specified number of days.
        /// </summary>
        /// <param name="repository">The issue repository instance.</param>
        /// <param name="projectKey">The project key to filter by.</param>
        /// <param name="days">Number of days to look ahead.</param>
        /// <returns>A list of issues due within the specified timeframe.</returns>
        public static async ValueTask<List<JiraIssue>> GetDueWithinAsync(this IssueRepository repository, string projectKey, int days)
        {
            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be null or whitespace.", nameof(projectKey));
            }

            if (days < 0)
            {
                throw new ArgumentException("Days must be a non-negative value.", nameof(days));
            }

            var cutoffDate = DateTime.Today.AddDays(days);
            var allIssues = await repository.GetByProjectAsync(projectKey);

            return allIssues
                .Where(issue => issue.DueDate.HasValue && issue.DueDate.Value.Date <= cutoffDate)
                .OrderBy(issue => issue.DueDate)
                .ToList();
        }

        /// <summary>
        /// Gets the top N highest priority issues.
        /// </summary>
        /// <param name="repository">The issue repository instance.</param>
        /// <param name="projectKey">The project key to filter by.</param>
        /// <param name="count">Number of issues to return.</param>
        /// <returns>A list of the highest priority issues.</returns>
        public static async ValueTask<List<JiraIssue>> GetTopPriorityAsync(this IssueRepository repository, string projectKey, int count)
        {
            if (string.IsNullOrWhiteSpace(projectKey))
            {
                throw new ArgumentException("Project key cannot be null or whitespace.", nameof(projectKey));
            }

            if (count <= 0)
            {
                throw new ArgumentException("Count must be a positive value.", nameof(count));
            }

            var allIssues = await repository.GetHighPriorityAsync(projectKey);
            return allIssues
                .OrderByDescending(issue => issue.Priority)
                .Take(count)
                .ToList();
        }
    }
}