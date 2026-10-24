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
        this.StatusCode = statusCode;
        this.ResponseContent = responseContent;
    }

    public override string ToString()
    {
        var result = base.ToString();

        if (this.StatusCode.HasValue)
        {
            result += $"\nHTTP Status Code: {this.StatusCode}";
        }

        if (!string.IsNullOrEmpty(this.ResponseContent))
        {
            result += $"\nResponse: {this.ResponseContent}";
        }

        return result;
    }
}
