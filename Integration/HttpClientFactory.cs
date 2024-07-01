// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Utils;

namespace JiraAnalyticsCli.Integration;

/// <summary>
/// Factory for creating and configuring HttpClient instances.
/// Centralizes HTTP client configuration with retry policies and headers.
/// </summary>
public class HttpClientFactory
{
    private readonly ILogger<HttpClientFactory> _logger;
    private readonly string? _apiToken;
    private readonly string? _baseUrl;

    public HttpClientFactory(ILogger<HttpClientFactory> logger, string? apiToken = null, string? baseUrl = null)
    {
        _logger = logger;
        _apiToken = apiToken;
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Creates and configures HttpClient for Jira API calls.
    /// Sets up authentication, headers, and timeout handling.
    /// </summary>
    public HttpClient CreateJiraClient()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            UseCookies = true
        });

        if (!string.IsNullOrEmpty(_baseUrl))
        {
            client.BaseAddress = new Uri(_baseUrl);
        }

        client.Timeout = TimeSpan.FromSeconds(30);
        client.AddJiraHeaders(_apiToken, "jira-analytics-cli/1.0");

        _logger.LogInformation("Created Jira HTTP client with timeout {TimeoutSeconds}s", client.Timeout.TotalSeconds);

        return client;
    }

    /// <summary>
    /// Creates HttpClient for generic API calls with custom configuration.
    /// Useful for webhook handlers or external service integration.
    /// </summary>
    public HttpClient CreateGenericClient(TimeSpan? timeout = null, Dictionary<string, string>? headers = null)
    {
        var client = new HttpClient();

        client.Timeout = timeout ?? TimeSpan.FromSeconds(30);

        client.DefaultRequestHeaders.Add("User-Agent", "jira-analytics-cli/1.0");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                client.DefaultRequestHeaders.Add(key, value);
            }
        }

        _logger.LogDebug("Created generic HTTP client with {HeaderCount} custom headers", headers?.Count ?? 0);

        return client;
    }

    /// <summary>
    /// Creates HttpClient with custom message handler for testing or specialized logging.
    /// </summary>
    public HttpClient CreateClientWithHandler(HttpMessageHandler handler, TimeSpan? timeout = null)
    {
        var client = new HttpClient(handler);
        client.Timeout = timeout ?? TimeSpan.FromSeconds(30);

        return client;
    }
}
