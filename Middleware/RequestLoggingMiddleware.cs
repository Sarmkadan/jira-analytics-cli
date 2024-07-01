// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Middleware;

/// <summary>
/// Middleware for logging all commands and their execution metadata.
/// Provides audit trail and performance diagnostics for debugging and compliance.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly Func<PipelineContext, Task> _next;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger, Func<PipelineContext, Task> next)
    {
        _logger = logger;
        _next = next;
    }

    /// <summary>
    /// Logs request entry, delegates to next middleware, then logs completion.
    /// Captures timing and result status for performance analysis.
    /// </summary>
    public async Task InvokeAsync(PipelineContext context)
    {
        _logger.LogInformation(
            "Command started: {Command} | Project: {Project} | CorrelationId: {CorrelationId}",
            context.CommandName,
            context.ProjectKey ?? "N/A",
            context.CorrelationId);

        try
        {
            await _next(context);

            _logger.LogInformation(
                "Command completed successfully in {ElapsedMs}ms | CorrelationId: {CorrelationId}",
                context.ElapsedTime.TotalMilliseconds,
                context.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Command failed after {ElapsedMs}ms | CorrelationId: {CorrelationId}",
                context.ElapsedTime.TotalMilliseconds,
                context.CorrelationId);

            context.Exception = ex;
            throw;
        }
    }
}
