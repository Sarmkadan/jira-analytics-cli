namespace JiraAnalyticsCli.Tests.Services
{
    public static class AnalyticsServiceTestsExtensions
    {
        public static int GetExpectedOverdueCountForEmptyProject(this AnalyticsServiceTests serviceTests)
        {
            return 0;
        }

        public static int GetExpectedCriticalCountForEmptyProject(this AnalyticsServiceTests serviceTests)
        {
            return 0;
        }

        public static bool HasExpectedOverdueCount(this AnalyticsServiceTests serviceTests, int expectedCount)
        {
            return expectedCount == 0;
        }

        public static bool HasExpectedCriticalCount(this AnalyticsServiceTests serviceTests, int expectedCount)
        {
            return expectedCount == 0;
        }
    }
}