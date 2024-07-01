// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Middleware;

/// <summary>
/// Context object passed through the middleware pipeline.
/// Carries request metadata, execution state, and response data through all middleware layers.
/// </summary>
public class PipelineContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Items { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();

    public string? CommandName { get; set; }
    public string? ProjectKey { get; set; }
    public string? UserId { get; set; }
    public int RetryCount { get; set; }

    public object? RequestData { get; set; }
    public object? ResponseData { get; set; }

    public Exception? Exception { get; set; }
    public bool HasError => Exception != null;

    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;

    /// <summary>
    /// Sets a key-value pair in the context items for passing data between middleware.
    /// Useful for sharing computed values or state across the pipeline.
    /// </summary>
    public void SetItem(string key, object value)
    {
        Items[key] = value;
    }

    /// <summary>
    /// Retrieves a previously set item from context, or returns default if not found.
    /// Used by middleware to access data set by earlier middleware in the chain.
    /// </summary>
    public T? GetItem<T>(string key)
    {
        if (Items.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    /// <summary>
    /// Adds an HTTP header to be included in requests.
    /// Middleware can set headers that should be propagated through the request pipeline.
    /// </summary>
    public void AddHeader(string name, string value)
    {
        Headers[name] = value;
    }
}
