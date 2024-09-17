// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Extension methods for HttpClient with retry logic and error handling.
/// Provides resilience patterns for API calls with configurable backoff.
/// </summary>
public static class HttpClientExtensions
{
    private const int DefaultMaxRetries = 3;
    private const int DefaultInitialDelayMs = 1000;

    /// <summary>
    /// Makes GET request with automatic retry on transient failures.
    /// Uses exponential backoff to avoid overwhelming servers.
    /// </summary>
    public static async Task<T?> GetWithRetryAsync<T>(
        this HttpClient client, string url, ILogger? logger = null)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>();
            },
            logger);
    }

    /// <summary>
    /// Makes POST request with JSON body and automatic retry.
    /// Serializes object to JSON and sends with proper content type.
    /// </summary>
    public static async Task<TResponse?> PostJsonWithRetryAsync<TRequest, TResponse>(
        this HttpClient client, string url, TRequest data, ILogger? logger = null)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                var response = await client.PostAsJsonAsync(url, data);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>();
            },
            logger);
    }

    /// <summary>
    /// Generic retry executor with exponential backoff and jitter.
    /// Handles transient failures (timeouts, 5xx status codes) automatically.
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        ILogger? logger = null,
        int maxRetries = DefaultMaxRetries,
        int initialDelayMs = DefaultInitialDelayMs)
    {
        var delay = initialDelayMs;
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex) when (ex.InnerException is TimeoutException or OperationCanceledException)
            {
                lastException = ex;
                logger?.LogWarning($"Timeout on attempt {attempt}/{maxRetries}");

                if (attempt < maxRetries)
                {
                    await Task.Delay(delay + new Random().Next(0, 500)); // Add jitter
                    delay = (int)(delay * 1.5); // Exponential backoff
                }
            }
            catch (HttpRequestException ex) when (IsTransient(ex))
            {
                lastException = ex;
                logger?.LogWarning($"Transient error on attempt {attempt}/{maxRetries}: {ex.Message}");

                if (attempt < maxRetries)
                {
                    await Task.Delay(delay + new Random().Next(0, 500));
                    delay = (int)(delay * 1.5);
                }
            }
            catch (Exception)
            {
                // Non-transient error, fail immediately
                throw;
            }
        }

        logger?.LogError($"Operation failed after {maxRetries} retries");
        throw lastException ?? new InvalidOperationException("Operation failed");
    }

    /// <summary>
    /// Adds standard headers for Jira API calls (auth, accept type, etc).
    /// Centralizes header management for consistent API communication.
    /// </summary>
    public static void AddJiraHeaders(this HttpClient client, string? apiToken = null, string? userAgent = null)
    {
        client.DefaultRequestHeaders.Clear();

        if (!string.IsNullOrEmpty(apiToken))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
        }

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", userAgent ?? "jira-analytics-cli/1.0");
        client.DefaultRequestHeaders.Add("X-Atlassian-Token", "no-check");
    }

    /// <summary>
    /// Checks if HTTP exception is transient (retryable).
    /// Transient errors include timeouts and specific HTTP status codes.
    /// </summary>
    private static bool IsTransient(HttpRequestException ex)
    {
        if (ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
            ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
            ex.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
        {
            return true;
        }

        return false;
    }
}
