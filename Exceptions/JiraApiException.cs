// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Exceptions;

/// <summary>
/// Exception thrown when Jira API communication fails
/// </summary>
public class JiraApiException : Exception
{
    public int? StatusCode { get; }
    public string? ResponseContent { get; }

    public JiraApiException(string message)
        : base(message) { }

    public JiraApiException(string message, Exception innerException)
        : base(message, innerException) { }

    public JiraApiException(string message, int statusCode, string? responseContent = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public override string ToString()
    {
        var result = base.ToString();

        if (StatusCode.HasValue)
            result += $"\nHTTP Status Code: {StatusCode}";

        if (!string.IsNullOrEmpty(ResponseContent))
            result += $"\nResponse: {ResponseContent}";

        return result;
    }
}
