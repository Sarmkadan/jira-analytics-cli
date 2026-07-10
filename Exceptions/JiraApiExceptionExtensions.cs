using System;

namespace JiraAnalyticsCli.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="JiraApiException"/> to simplify error handling and classification.
    /// </summary>
    public static class JiraApiExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception represents a server error (HTTP 5xx status code).
        /// </summary>
        /// <param name="exception">The exception to check. Must not be null.</param>
        /// <returns>True if the exception has a server error status code; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static bool IsServerError(this JiraApiException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.StatusCode.HasValue && exception.StatusCode.Value >= 500 && exception.StatusCode.Value < 600;
        }

        /// <summary>
        /// Gets a human-readable summary of the Jira API error.
        /// </summary>
        /// <param name="exception">The exception containing error details. Must not be null.</param>
        /// <returns>A formatted error summary string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static string GetErrorSummary(this JiraApiException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception.StatusCode switch
            {
                null => "Jira API error with no status code",
                var statusCode when exception.ResponseContent is { Length: > 0 } =>
                    $"Jira API error {statusCode}: {exception.ResponseContent[..Math.Min(100, exception.ResponseContent.Length)]}...",
                var statusCode => $"Jira API error {statusCode}"
            };
        }

        /// <summary>
        /// Determines whether the exception represents a client error (HTTP 4xx status code).
        /// </summary>
        /// <param name="exception">The exception to check. Must not be null.</param>
        /// <returns>True if the exception has a client error status code; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static bool IsClientError(this JiraApiException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.StatusCode.HasValue && exception.StatusCode.Value >= 400 && exception.StatusCode.Value < 500;
        }
    }
}