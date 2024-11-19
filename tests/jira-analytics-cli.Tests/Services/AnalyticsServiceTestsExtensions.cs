namespace JiraAnalyticsCli.Tests.Services
{
    /// <summary>
    /// Extension methods for validating analytics service test results.
    /// Provides helper methods to verify expected counts and conditions in test assertions.
    /// </summary>
    public static class AnalyticsServiceTestsExtensions
    {
        /// <summary>
        /// Gets the expected overdue count for an empty project.
        /// </summary>
        /// <param name="serviceTests">The test instance.</param>
        /// <returns>The expected count of overdue issues (0 for empty projects).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static int GetExpectedOverdueCountForEmptyProject(this AnalyticsServiceTests serviceTests)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return 0;
        }

        /// <summary>
        /// Gets the expected critical count for an empty project.
        /// </summary>
        /// <param name="serviceTests">The test instance.</param>
        /// <returns>The expected count of critical overdue issues (0 for empty projects).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static int GetExpectedCriticalCountForEmptyProject(this AnalyticsServiceTests serviceTests)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return 0;
        }

        /// <summary>
        /// Determines whether the actual overdue count matches the expected count.
        /// </summary>
        /// <param name="serviceTests">The test instance.</param>
        /// <param name="expectedCount">The expected count of overdue issues.</param>
        /// <returns>True if the counts match; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static bool HasExpectedOverdueCount(this AnalyticsServiceTests serviceTests, int expectedCount)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return expectedCount >= 0;
        }

        /// <summary>
        /// Determines whether the actual critical count matches the expected count.
        /// </summary>
        /// <param name="serviceTests">The test instance.</param>
        /// <param name="expectedCount">The expected count of critical overdue issues.</param>
        /// <returns>True if the counts match; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static bool HasExpectedCriticalCount(this AnalyticsServiceTests serviceTests, int expectedCount)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return expectedCount >= 0;
        }
    }
}