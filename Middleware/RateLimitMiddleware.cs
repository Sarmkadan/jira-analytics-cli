// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Middleware;

/// <summary>
/// Rate limiting middleware to prevent API throttling and respect Jira limits.
/// Uses sliding window algorithm to track request counts within time windows.
/// </summary>
public class RateLimitMiddleware
{
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly Func<PipelineContext, Task> _next;
    private readonly Dictionary<string, RequestWindow> _windows = new();
    private readonly object _lockObj = new();

    private const int MaxRequestsPerMinute = 60;
    private const int MaxRequestsPerHour = 1000;

    public RateLimitMiddleware(ILogger<RateLimitMiddleware> logger, Func<PipelineContext, Task> next)
    {
        _logger = logger;
        _next = next;
    }

    /// <summary>
    /// Checks rate limits before allowing command execution.
    /// Applies exponential backoff on rate limit violations to recover gracefully.
    /// </summary>
    public async Task InvokeAsync(PipelineContext context)
    {
        var key = context.ProjectKey ?? "default";

        lock (_lockObj)
        {
            if (!_windows.ContainsKey(key))
            {
                _windows[key] = new RequestWindow();
            }

            var window = _windows[key];
            window.CleanupOldEntries();

            // Check minute limit
            var minuteCount = window.Requests.Count(r => r > DateTime.UtcNow.AddMinutes(-1));
            if (minuteCount >= MaxRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded (per minute) for project {Project}", key);
                context.SetItem("RateLimited", true);
                context.SetItem("RetryAfter", 60);
            }

            // Check hour limit
            var hourCount = window.Requests.Count(r => r > DateTime.UtcNow.AddHours(-1));
            if (hourCount >= MaxRequestsPerHour)
            {
                _logger.LogWarning("Rate limit exceeded (per hour) for project {Project}", key);
                context.SetItem("RateLimited", true);
                context.SetItem("RetryAfter", 3600);
            }

            window.Requests.Add(DateTime.UtcNow);
        }

        // Check if we're rate limited and implement backoff
        var rateLimited = context.GetItem<bool>("RateLimited");
        if (rateLimited)
        {
            var retryAfter = context.GetItem<int?>("RetryAfter") ?? 60;
            var backoffMs = retryAfter * 1000 + new Random().Next(0, 5000); // Add jitter

            _logger.LogInformation("Rate limited. Waiting {BackoffMs}ms before retry", backoffMs);
            await Task.Delay(backoffMs);
        }

        await _next(context);
    }

    private class RequestWindow
    {
        public List<DateTime> Requests { get; set; } = new();

        public void CleanupOldEntries()
        {
            var cutoff = DateTime.UtcNow.AddHours(-1);
            Requests.RemoveAll(r => r < cutoff);
        }
    }
}
