using JiraAnalyticsCli.Tests.Services;
using System;

namespace JiraAnalyticsCli.Tests.Services
{
    /// <summary>
    /// Extension methods for testing assertions on <see cref="JiraApiServiceTests"/> scenarios.
    /// </summary>
    public static class JiraApiServiceTestsExtensions
    {
        /// <summary>
        /// Determines whether the specified exception represents an unauthorized access error (HTTP 401).
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="ex">The exception to check.</param>
        /// <returns><see langword="true"/> if the exception message contains "401 Unauthorized"; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        public static bool IsUnauthorized(this JiraApiServiceTests tests, Exception ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentNullException.ThrowIfNull(ex.Message);

            return ex.Message.Contains("401 Unauthorized", StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specified exception represents an internal server error (HTTP 500).
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="ex">The exception to check.</param>
        /// <returns><see langword="true"/> if the exception message contains "500 Internal Server Error"; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="ex.Message"/> is <see langword="null"/>.</exception>
        public static bool IsServerError(this JiraApiServiceTests tests, Exception ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentNullException.ThrowIfNull(ex.Message);

            return ex.Message.Contains("500 Internal Server Error", StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the response indicates a valid project response (non-null).
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="response">The API response to validate.</param>
        /// <returns><see langword="true"/> if the response is non-null; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static bool HasValidProjectResponse(this JiraApiServiceTests tests, object response)
        {
            ArgumentNullException.ThrowIfNull(response);
            return response is not null;
        }

        /// <summary>
        /// Determines whether the sprint issues response is null.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="response">The API response to check.</param>
        /// <returns><see langword="true"/> if the response is null; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static bool HasEmptySprintIssuesResponse(this JiraApiServiceTests tests, object response)
        {
            ArgumentNullException.ThrowIfNull(response);

            return response switch
            {
                null => true,
                _ => false
            };
        }
    }
}
