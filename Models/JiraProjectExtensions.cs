using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JiraAnalyticsCli.Models
{
    /// <summary>
    /// Extension methods that provide additional insight and convenience operations for <see cref="JiraProject"/>.
    /// </summary>
    public static class JiraProjectExtensions
    {
        /// <summary>
        /// Gets the total number of overdue issues for the project.
        /// </summary>
        /// <param name="project">The project to evaluate.</param>
        /// <returns>The count of overdue issues; 0 if the project has none.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is <c>null</c>.</exception>
        public static int GetOverdueIssueCount(this JiraProject project)
        {
            ArgumentNullException.ThrowIfNull(project);
            return project.GetAllOverdueIssues()?.Count ?? 0;
        }

        /// <summary>
        /// Gets the total number of blocked issues for the project.
        /// </summary>
        /// <param name="project">The project to evaluate.</param>
        /// <returns>The count of blocked issues; 0 if the project has none.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is <c>null</c>.</exception>
        public static int GetBlockedIssueCount(this JiraProject project)
        {
            ArgumentNullException.ThrowIfNull(project);
            return project.GetAllBlockedIssues()?.Count ?? 0;
        }

        /// <summary>
        /// Calculates the percentage of sprints that have been completed.
        /// </summary>
        /// <param name="project">The project whose sprint completion rate is required.</param>
        /// <returns>
        /// A value between 0 and 100 representing the proportion of completed sprints.
        /// Returns 0 when the project reports no total sprints.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is <c>null</c>.</exception>
        public static double GetSprintCompletionPercentage(this JiraProject project)
        {
            ArgumentNullException.ThrowIfNull(project);
            int total = project.GetTotalSprintCount();
            if (total == 0)
                return 0;

            return (double)project.GetCompletedSprintCount() / total * 100;
        }

        /// <summary>
        /// Retrieves the most recent sprint metrics for the project.
        /// </summary>
        /// <param name="project">The project to query.</param>
        /// <param name="count">
        /// The maximum number of recent metrics to return. Must be greater than zero.
        /// If the project contains fewer metrics, all available metrics are returned.
        /// </param>
        /// <returns>
        /// An <see cref="IReadOnlyList{T}"/> containing up to <paramref name="count"/> of the latest
        /// <see cref="SprintMetric"/> entries, ordered from oldest to newest.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than or equal to zero.</exception>
        public static IReadOnlyList<SprintMetric> GetRecentSprintMetrics(this JiraProject project, int count = 5)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

            // TakeLast is available in .NET 6+. It returns the last 'count' elements preserving order.
            var recent = project.MetricsHistory
                .TakeLast(count)
                .ToList();

            return recent.AsReadOnly();
        }
    }
}
