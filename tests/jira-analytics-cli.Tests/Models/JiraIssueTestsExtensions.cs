namespace JiraAnalyticsCli.Tests.Models;

public static class JiraIssueTestsExtensions
{
    public static void ExecuteAllOverdueTests(this JiraIssueTests tests)
    {
        tests.IsOverdue_WhenDueDateIsInPastAndStatusIsOpen_ReturnsTrue();
        tests.IsOverdue_WhenStatusIsDone_ReturnsFalse();
        tests.IsOverdue_WhenResolutionDateIsSet_ReturnsFalse();
    }

    public static void ExecuteAllPriorityTests(this JiraIssueTests tests)
    {
        tests.IsHighPriority_WithCriticalPriority_ReturnsTrue();
        tests.IsHighPriority_WithMediumPriority_ReturnsFalse();
    }

    public static void ExecuteCycleTimeTest(this JiraIssueTests tests)
    {
        tests.GetCycleTime_WhenResolutionDateIsSet_ReturnsDaysBetweenCreationAndResolution();
    }
}
