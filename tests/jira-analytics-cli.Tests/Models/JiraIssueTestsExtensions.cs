namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Provides extension methods to execute groups of related test cases for <see cref="JiraIssueTests"/>.
/// </summary>
public static class JiraIssueTestsExtensions
{
    /// <summary>
    /// Executes all overdue-related test cases.
    /// </summary>
    /// <param name="tests">The test instance to execute tests against.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static void ExecuteAllOverdueTests(this JiraIssueTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue();
        tests.IsOverdue_WhenStatusIsDone_ReturnsFalse();
        tests.IsOverdue_WhenResolutionDateIsSet_ReturnsFalse();
    }

    /// <summary>
    /// Executes all priority-related test cases.
    /// </summary>
    /// <param name="tests">The test instance to execute tests against.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static void ExecuteAllPriorityTests(this JiraIssueTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.IsHighPriority_WithCriticalPriority_ReturnsTrue();
        tests.IsHighPriority_WithMediumPriority_ReturnsFalse();
    }

    /// <summary>
    /// Executes the cycle time test case.
    /// </summary>
    /// <param name="tests">The test instance to execute tests against.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static void ExecuteCycleTimeTest(this JiraIssueTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution();
    }
}