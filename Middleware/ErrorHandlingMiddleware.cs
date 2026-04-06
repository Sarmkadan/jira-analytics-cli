// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Exceptions;

namespace JiraAnalyticsCli.Middleware;

/// <summary>
/// Global error handling middleware that catches and normalizes exceptions.
/// Converts specific exceptions to appropriate error responses and status codes.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly Func<PipelineContext, Task> _next;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, Func<PipelineContext, Task> next)
    {
        _logger = logger;
        _next = next;
    }

    /// <summary>
    /// Wraps command execution with try-catch to handle errors gracefully.
    /// Logs errors at appropriate level and sets error context.
    /// </summary>
    public async Task InvokeAsync(PipelineContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ConfigurationException configEx)
        {
            _logger.LogError(configEx, "Configuration error in command {Command}", context.CommandName);
            context.Exception = configEx;
            context.SetItem("ErrorType", "configuration");
            context.SetItem("ErrorMessage", configEx.Message);
        }
        catch (JiraApiException jiraEx)
        {
            _logger.LogError(jiraEx, "Jira API error: {StatusCode}", jiraEx.StatusCode);
            context.Exception = jiraEx;
            context.SetItem("ErrorType", "api");
            context.SetItem("ErrorMessage", $"Jira API error: {jiraEx.Message}");
            context.SetItem("ErrorCode", jiraEx.StatusCode);
        }
        catch (OperationCanceledException cancelEx)
        {
            _logger.LogWarning(cancelEx, "Operation cancelled for {Command}", context.CommandName);
            context.Exception = cancelEx;
            context.SetItem("ErrorType", "cancelled");
            context.SetItem("ErrorMessage", "Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in {Command}", context.CommandName);
            context.Exception = ex;
            context.SetItem("ErrorType", "unhandled");
            context.SetItem("ErrorMessage", "An unexpected error occurred");
            context.SetItem("StackTrace", ex.StackTrace ?? string.Empty);
        }
    }
}
