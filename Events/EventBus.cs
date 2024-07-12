// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Events;

/// <summary>
/// Central event bus for publishing and subscribing to domain events.
/// Uses pub-sub pattern for decoupled event handling across system.
/// </summary>
public class EventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly ILogger<EventBus> _logger;

    public EventBus(ILogger<EventBus> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribes to events of specific type.
    /// Handler will be called whenever event of that type is published.
    /// </summary>
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(
            eventType,
            new List<Delegate> { handler },
            (_, list) =>
            {
                list.Add(handler);
                return list;
            });

        _logger.LogDebug("Subscribed to event: {EventType}", eventType.Name);
    }

    /// <summary>
    /// Unsubscribes handler from specific event type.
    /// </summary>
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        var eventType = typeof(TEvent);

        if (_handlers.TryGetValue(eventType, out var list))
        {
            list.Remove(handler);
            _logger.LogDebug("Unsubscribed from event: {EventType}", eventType.Name);
        }
    }

    /// <summary>
    /// Publishes event to all subscribers asynchronously.
    /// Executes handlers in sequence; errors are logged but don't prevent other handlers.
    /// </summary>
    public async Task PublishAsync(DomainEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = @event.GetType();

        _logger.LogDebug(
            "Publishing event: {EventType} (EventId: {EventId})",
            @event.EventType,
            @event.EventId);

        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug("No handlers registered for event: {EventType}", eventType.Name);
            return;
        }

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Delegate handlerDelegate)
                {
                    var invokeTask = (Task?)handlerDelegate.DynamicInvoke(@event);
                    if (invokeTask != null)
                    {
                        tasks.Add(invokeTask);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing handler for event: {EventType}",
                    eventType.Name);
            }
        }

        if (tasks.Any())
        {
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
                _logger.LogDebug(
                    "Event published successfully: {EventType} (handled by {HandlerCount} handlers)",
                    @event.EventType,
                    handlers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awaiting event handlers for: {EventType}", eventType.Name);
            }
        }
    }

    /// <summary>
    /// Publishes event synchronously (blocks until all handlers complete).
    /// </summary>
    public void Publish(DomainEvent @event)
    {
        PublishAsync(@event).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets count of handlers registered for specific event type.
    /// </summary>
    public int GetHandlerCount<TEvent>() where TEvent : DomainEvent
    {
        return _handlers.TryGetValue(typeof(TEvent), out var handlers) ? handlers.Count : 0;
    }

    /// <summary>
    /// Clears all subscriptions.
    /// Useful for testing or complete cleanup.
    /// </summary>
    public void ClearAllSubscriptions()
    {
        _handlers.Clear();
        _logger.LogInformation("All event subscriptions cleared");
    }
}
