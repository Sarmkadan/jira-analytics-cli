using System;

namespace JiraAnalyticsCli.Exceptions
{
    public static class JiraApiExceptionExtensions
    {
        public static bool IsServerError(this JiraApiException exception)
        {
            return exception.StatusCode.HasValue && exception.StatusCode.Value >= 500 && exception.StatusCode.Value < 600;
        }

        public static string GetErrorSummary(this JiraApiException exception)
        {
            return exception.ResponseContent != null ? 
                $"Jira API error {exception.StatusCode}: {exception.ResponseContent.Substring(0, Math.Min(100, exception.ResponseContent.Length))}..." : 
                $"Jira API error {exception.StatusCode}";
        }

        public static bool IsClientError(this JiraApiException exception)
        {
            return exception.StatusCode.HasValue && exception.StatusCode.Value >= 400 && exception.StatusCode.Value < 500;
        }
    }
}
