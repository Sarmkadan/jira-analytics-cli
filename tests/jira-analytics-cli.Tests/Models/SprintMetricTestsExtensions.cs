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
        public static bool RunGetCompletionRate_WithNonZeroPlannedPoints(this SprintMetricTests tests)
        {
            if (tests == null) throw new ArgumentNullException(nameof(tests));

            try
            {
                tests.GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the <c>GetCompletionRate_WithZeroPlannedPoints_ReturnsZero</c> test
        /// and returns <c>true</c> if it completes without throwing an exception.
        /// </summary>
        public static bool RunGetCompletionRate_WithZeroPlannedPoints(this SprintMetricTests tests)
        {
            if (tests == null) throw new ArgumentNullException(nameof(tests));

            try
            {
                tests.GetCompletionRate_WithZeroPlannedPoints_ReturnsZero();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the <c>Validate_WithEndDateBeforeStartDate_ThrowsArgumentException</c> test
        /// and returns <c>true</c> only when an <see cref="ArgumentException"/> is thrown,
        /// indicating the test behaved as expected.
        /// </summary>
        public static bool RunValidate_WithEndDateBeforeStartDate_ThrowsArgumentException(this SprintMetricTests tests)
        {
            if (tests == null) throw new ArgumentNullException(nameof(tests));

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
            catch
            {
                // Unexpected exception type.
                return false;
            }
        }

        /// <summary>
        /// Executes all public test methods on <see cref="SprintMetricTests"/> and returns a
        /// dictionary mapping the test method name to a boolean indicating success.
        /// </summary>
        public static IDictionary<string, bool> RunAll(this SprintMetricTests tests)
        {
            if (tests == null) throw new ArgumentNullException(nameof(tests));

            var results = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(SprintMetricTests.GetCompletionRate_WithNonZeroPlannedPoints_ReturnsCorrectPercentage), tests.RunGetCompletionRate_WithNonZeroPlannedPoints() },
                { nameof(SprintMetricTests.GetCompletionRate_WithZeroPlannedPoints_ReturnsZero), tests.RunGetCompletionRate_WithZeroPlannedPoints() },
                { nameof(SprintMetricTests.GetHealthStatus_WithAllMetricsMeetingExcellentThresholds_ReturnsExcellent), 
                    ExecuteSafely(() => tests.GetHealthStatus_WithAllMetricsMeetingExcellentThresholds_ReturnsExcellent()) },
                { nameof(SprintMetricTests.GetHealthStatus_WithLowCompletionRateAndHighDefects_ReturnsCritical), 
                    ExecuteSafely(() => tests.GetHealthStatus_WithLowCompletionRateAndHighDefects_ReturnsCritical()) },
                { nameof(SprintMetricTests.GetRiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors), 
                    ExecuteSafely(() => tests.GetRiskScore_WithOverdueAndScopeChanges_AggregatesAllRiskFactors()) },
                { nameof(SprintMetricTests.Validate_WithEndDateBeforeStartDate_ThrowsArgumentException), 
                    tests.RunValidate_WithEndDateBeforeStartDate_ThrowsArgumentException() }
            };

            return results;
        }

        // Helper that runs a void test method and returns true if it does not throw.
        private static bool ExecuteSafely(Action testAction)
        {
            try
            {
                testAction();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
