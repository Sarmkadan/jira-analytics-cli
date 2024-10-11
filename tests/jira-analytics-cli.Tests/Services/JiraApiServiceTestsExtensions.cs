using JiraAnalyticsCli.Tests.Services;
using System;

namespace JiraAnalyticsCli.Tests.Services
{
    public static class JiraApiServiceTestsExtensions
    {
        public static bool IsUnauthorized(this JiraApiServiceTests tests, Exception ex)
        {
            return ex.Message.Contains("401 Unauthorized");
        }

        public static bool IsServerError(this JiraApiServiceTests tests, Exception ex)
        {
            return ex.Message.Contains("500 Internal Server Error");
        }

        public static bool HasValidProjectResponse(this JiraApiServiceTests tests, object response)
        {
            return response != null;
        }

        public static bool HasEmptySprintIssuesResponse(this JiraApiServiceTests tests, object response)
        {
            return response == null || ((dynamic)response).Count == 0;
        }
    }
}
