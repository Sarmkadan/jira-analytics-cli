// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Events;

/// <summary>
/// Base class for event subscribers with built-in error handling and logging.
/// Derived classes implement HandleAsync to respond to specific events.
/// </summary>
public abstract class EventSubscriber<TEvent> where TEvent : DomainEvent
{
    protected readonly ILogger<EventSubscriber<TEvent>> Logger;

    protected EventSubscriber(ILogger<EventSubscriber<TEvent>> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the domain event. Override in derived class to implement logic.
    /// </summary>
    protected abstract Task HandleAsync(TEvent @event);

    /// <summary>
    /// Internal handler wrapper that adds error handling and logging.
    /// Derived classes should not override; use HandleAsync instead.
    /// </summary>
    public async Task OnEventAsync(TEvent @event)
    {
        if (@event == null)
        {
            Logger.LogWarning("Null event passed to handler");
            return;
        }

        try
        {
            Logger.LogDebug(
                "Event subscriber handling: {EventType} (EventId: {EventId})",
                @event.EventType,
                @event.EventId);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await HandleAsync(@event);
            stopwatch.Stop();

            Logger.LogInformation(
                "Event handler completed: {EventType} (duration: {DurationMs}ms)",
                @event.EventType,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling event: {EventType}", @event.EventType);
            await OnErrorAsync(@event, ex);
        }
    }

    /// <summary>
    /// Called when error occurs during event handling.
    /// Override to implement custom error handling logic.
    /// </summary>
    protected virtual Task OnErrorAsync(TEvent @event, Exception exception)
    {
        Logger.LogError(
            exception,
            "Event handler error for: {EventType} (EventId: {EventId})",
            @event.EventType,
            @event.EventId);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Sample subscriber for sprint analysis completed events.
/// Demonstrates proper event handling pattern.
/// </summary>
public class SprintAnalysisEventSubscriber : EventSubscriber<SprintAnalysisCompletedEvent>
{
    public SprintAnalysisEventSubscriber(ILogger<EventSubscriber<SprintAnalysisCompletedEvent>> logger)
        : base(logger)
    {
    }

    protected override async Task HandleAsync(SprintAnalysisCompletedEvent @event)
    {
        Logger.LogInformation(
            "Sprint analysis completed for project: {Project} | Sprint: {SprintId} | Velocity: {Velocity}",
            @event.ProjectKey,
            @event.SprintId,
            @event.Velocity);

        // Example: Could update database, send notifications, trigger other tasks, etc.
        await Task.CompletedTask;
    }
}

/// <summary>
/// Sample subscriber for processing errors.
/// Provides centralized error tracking and alerting.
/// </summary>
public class ErrorEventSubscriber : EventSubscriber<ProcessingErrorEvent>
{
    public ErrorEventSubscriber(ILogger<EventSubscriber<ProcessingErrorEvent>> logger)
        : base(logger)
    {
    }

    protected override async Task HandleAsync(ProcessingErrorEvent @event)
    {
        Logger.LogWarning(
            "Processing error occurred: {Message} | Operation: {Operation} | Severity: {Severity}",
            @event.ErrorMessage,
            @event.OperationName,
            @event.Severity);

        // Example: Could send alert, log to external service, etc.
        if (@event.Severity == "Critical")
        {
            Logger.LogError("CRITICAL ERROR: {Message}", @event.ErrorMessage);
            // Would send alert notifications here
        }

        await Task.CompletedTask;
    }
}
