using System;
using System.Collections.Generic;

namespace JiraAnalyticsCli.Tests.Models
{
    /// <summary>
    /// Extension methods that make it easier to execute the individual test methods
    /// defined in <see cref="SprintMetricTests"/> and obtain simple pass/fail results.
    /// These helpers are intended for use in custom test runners or diagnostic scripts.
    /// </summary>
    public static class SprintMetricTestsExtensions
    {
        /// <summary>
        /// Executes the <c>GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage</c> test
        /// and returns <c>true</c> if it completes without throwing an exception.
        /// </summary>
        /// <param name="tests">The test instance to run against.</param>
        /// <returns><c>true</c> if the test passes without exception; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static bool RunGetCompletionRate_WithNonZeroPlannedPoints(this SprintMetricTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            try
            {
                tests.GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the <c>GetCompletionRate_WithZeroPlannedPoints_ReturnsZero</c> test
        /// and returns <c>true</c> if it completes without throwing an exception.
        /// </summary>
        /// <param name="tests">The test instance to run against.</param>
        /// <returns><c>true</c> if the test passes without exception; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static bool RunGetCompletionRate_WithZeroPlannedPoints(this SprintMetricTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            try
            {
                tests.GetCompletionRate_WithZeroPlannedPoints_ReturnsZero();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the <c>Validate_WithEndDateBeforeStartDate_ThrowsArgumentException</c> test
        /// and returns <c>true</c> only when an <see cref="ArgumentException"/> is thrown,
        /// indicating the test behaved as expected.
        /// </summary>
        /// <param name="tests">The test instance to run against.</param>
        /// <returns><c>true</c> if the expected exception is thrown; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static bool RunValidate_WithEndDateBeforeStartDate_ThrowsArgumentException(this SprintMetricTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            try
            {
                tests.Validate_WithEndDateBeforeStartDate_ThrowsArgumentException();
                // If no exception is thrown, the test failed.
                return false;
            }
            catch (ArgumentException)
            {
                // Expected outcome.
                return true;
            }
            catch (Exception)
            {
                // Unexpected exception type.
                return false;
            }
        }

        /// <summary>
        /// Executes all public test methods on <see cref="SprintMetricTests"/> and returns a
        /// dictionary mapping the test method name to a boolean indicating success.
        /// </summary>
        /// <param name="tests">The test instance to run against.</param>
        /// <returns>A dictionary mapping test names to their execution results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IDictionary<string, bool> RunAll(this SprintMetricTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var results = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(SprintMetricTests.GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage), tests.RunGetCompletionRate_WithNonZeroPlannedPoints() },
                { nameof(SprintMetricTests.GetCompletionRate_WithZeroPlannedPoints_ReturnsZero), tests.RunGetCompletionRate_WithZeroPlannedPoints() },
                { nameof(SprintMetricTests.GetHealthStatus_WithAllMetricsMeetingExcellentThresholds_ReturnsExcellent), ExecuteSafely(() => tests.GetHealthStatus_WithAllMetricsMeetingExcellentThresholds_ReturnsExcellent()) },
                { nameof(SprintMetricTests.GetHealthStatus_WithLowCompletionRateAndHighDefects_ReturnsCritical), ExecuteSafely(() => tests.GetHealthStatus_WithLowCompletionRateAndHighDefects_ReturnsCritical()) },
                { nameof(SprintMetricTests.GetRiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors), ExecuteSafely(() => tests.GetRiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors()) },
                { nameof(SprintMetricTests.Validate_WithEndDateBeforeStartDate_ThrowsArgumentException), tests.RunValidate_WithEndDateBeforeStartDate_ThrowsArgumentException() }
            };

            return results;
        }

        /// <summary>
        /// Helper that runs a void test method and returns true if it does not throw.
        /// </summary>
        /// <param name="testAction">The test action to execute.</param>
        /// <returns><c>true</c> if the action completes without exception; otherwise <c>false</c>.</returns>
        private static bool ExecuteSafely(Action testAction)
        {
            ArgumentNullException.ThrowIfNull(testAction);

            try
            {
                testAction();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}