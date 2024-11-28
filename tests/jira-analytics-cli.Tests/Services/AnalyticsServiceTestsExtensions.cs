namespace JiraAnalyticsCli.Tests.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="AnalyticsServiceTests"/> to validate test results.
    /// These methods help verify expected counts and conditions in analytics service test assertions.
    /// </summary>
    public static class AnalyticsServiceTestsExtensions
    {
        /// <summary>
        /// Gets the expected count of overdue issues for an empty project.
        /// </summary>
        /// <param name="serviceTests">The test instance providing context for the validation.</param>
        /// <returns>Zero, as an empty project has no overdue issues.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static int GetExpectedOverdueCountForEmptyProject(this AnalyticsServiceTests serviceTests)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return 0;
        }

        /// <summary>
        /// Gets the expected count of critical overdue issues for an empty project.
        /// </summary>
        /// <param name="serviceTests">The test instance providing context for the validation.</param>
        /// <returns>Zero, as an empty project has no critical overdue issues.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static int GetExpectedCriticalCountForEmptyProject(this AnalyticsServiceTests serviceTests)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return 0;
        }

        /// <summary>
        /// Determines whether the expected overdue count is valid (non-negative).
        /// </summary>
        /// <param name="serviceTests">The test instance providing context for the validation.</param>
        /// <param name="expectedCount">The expected count of overdue issues to validate.</param>
        /// <returns>True if <paramref name="expectedCount"/> is non-negative; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static bool HasExpectedOverdueCount(this AnalyticsServiceTests serviceTests, int expectedCount)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return expectedCount >= 0;
        }

        /// <summary>
        /// Determines whether the expected critical overdue count is valid (non-negative).
        /// </summary>
        /// <param name="serviceTests">The test instance providing context for the validation.</param>
        /// <param name="expectedCount">The expected count of critical overdue issues to validate.</param>
        /// <returns>True if <paramref name="expectedCount"/> is non-negative; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceTests"/> is null.</exception>
        public static bool HasExpectedCriticalCount(this AnalyticsServiceTests serviceTests, int expectedCount)
        {
            ArgumentNullException.ThrowIfNull(serviceTests);
            return expectedCount >= 0;
        }
    }
}
