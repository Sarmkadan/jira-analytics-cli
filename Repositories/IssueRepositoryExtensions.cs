using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories
{
    /// <summary>
    /// Extension methods that provide additional query and filtering capabilities for <see cref="IssueRepository"/>.
    /// </summary>
    public static class IssueRepositoryExtensions
    {
        /// <summary>
        /// Gets all issues that are assigned to a specific user.
        /// </summary>
        /// <param name="repository">The issue repository instance.</param>
        /// <param name="projectKey">The project key to filter by.</param>
        /// <param name="assignee">The username to filter by.</param>
        /// <returns>A list of issues assigned to the specified user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="projectKey"/> or <paramref name="assignee"/> is null or whitespace.</exception>
        public static async ValueTask<List<JiraIssue>> GetByAssigneeAsync(this IssueRepository repository, string projectKey, string assignee)
        {
            ArgumentNullException.ThrowIfNull(repository);

            ArgumentException.ThrowIfNullOrEmpty(projectKey);
            ArgumentException.ThrowIfNullOrEmpty(assignee);

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="projectKey"/> is null or whitespace.</exception>
        public static async ValueTask<List<JiraIssue>> GetUnassignedAsync(this IssueRepository repository, string projectKey)
        {
            ArgumentNullException.ThrowIfNull(repository);

            ArgumentException.ThrowIfNullOrEmpty(projectKey);

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="projectKey"/> is null or whitespace, or when <paramref name="days"/> is negative.</exception>
        public static async ValueTask<List<JiraIssue>> GetDueWithinAsync(this IssueRepository repository, string projectKey, int days)
        {
            ArgumentNullException.ThrowIfNull(repository);

            ArgumentException.ThrowIfNullOrEmpty(projectKey);
            ArgumentOutOfRangeException.ThrowIfNegative(days);

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="projectKey"/> is null or whitespace, or when <paramref name="count"/> is not positive.</exception>
        public static async ValueTask<List<JiraIssue>> GetTopPriorityAsync(this IssueRepository repository, string projectKey, int count)
        {
            ArgumentNullException.ThrowIfNull(repository);

            ArgumentException.ThrowIfNullOrEmpty(projectKey);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);

            var allIssues = await repository.GetHighPriorityAsync(projectKey);
            return allIssues
                .OrderByDescending(issue => issue.Priority)
                .Take(count)
                .ToList();
        }
    }
}