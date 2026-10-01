// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Low-level HTTP client for Jira REST API communication with retry and authentication handling.
/// </summary>
public class JiraClient
{
    private readonly HttpClient _httpClient;
    private readonly JiraClientOptions _options;
    private readonly ILogger<JiraClient> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of <see cref="JiraClient"/>.
    /// </summary>
    /// <param name="httpClient">Pre-configured HTTP client (typically via IHttpClientFactory).</param>
    /// <param name="options">Jira connection options including base URL and credentials.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public JiraClient(HttpClient httpClient, IOptions<JiraClientOptions> options, ILogger<JiraClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConfigureAuthentication();
    }

    /// <summary>
    /// Sends a GET request to the specified Jira API endpoint and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="endpoint">Relative API path (e.g. "/rest/api/3/project").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized response or default if the request fails.</returns>
    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint);
        _logger.LogDebug("GET {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Sends a POST request with a JSON body to the specified Jira API endpoint.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type to deserialize the response body into.</typeparam>
    /// <param name="endpoint">Relative API path.</param>
    /// <param name="body">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized response.</returns>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint);
        _logger.LogDebug("POST {Url}", url);

        var response = await _httpClient.PostAsJsonAsync(url, body, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Fetches all pages of a paginated Jira API endpoint using startAt/maxResults.
    /// </summary>
    /// <typeparam name="T">The element type within each page.</typeparam>
    /// <param name="endpoint">Relative API path supporting pagination.</param>
    /// <param name="resultsProperty">JSON property name containing the results array.</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All results across all pages.</returns>
    public async Task<List<T>> GetAllPaginatedAsync<T>(string endpoint, string resultsProperty = "values", int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var allResults = new List<T>();
        int startAt = 0;
        bool hasMore = true;

        while (hasMore)
        {
            char separator = endpoint.Contains('?') ? '&' : '?';
            string pagedUrl = $"{endpoint}{separator}startAt={startAt}&maxResults={pageSize}";
            var url = BuildUrl(pagedUrl);

            _logger.LogDebug("GET paginated {Url} (startAt={StartAt})", url, startAt);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            var root = doc.RootElement;

            if (root.TryGetProperty(resultsProperty, out var values) && values.ValueKind == JsonValueKind.Array)
            {
                var items = values.Deserialize<List<T>>(_jsonOptions) ?? [];
                allResults.AddRange(items);

                hasMore = items.Count >= pageSize;
                startAt += items.Count;
            }
            else
            {
                hasMore = false;
            }
        }

        _logger.LogDebug("Fetched {Total} items from {Endpoint}", allResults.Count, endpoint);
        return allResults;
    }

    /// <summary>
    /// Tests connectivity to the Jira instance by requesting server info.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the server responded successfully.</returns>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                BuildUrl("/rest/api/3/serverInfo"), cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Jira connection test failed");
            return false;
        }
    }

    private string BuildUrl(string endpoint)
    {
        string baseUrl = _options.BaseUrl.TrimEnd('/');
        string path = endpoint.TrimStart('/');
        return $"{baseUrl}/{path}";
    }

    private void ConfigureAuthentication()
    {
        if (!string.IsNullOrEmpty(_options.ApiToken))
        {
            string credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_options.Username}:{_options.ApiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
}

/// <summary>
/// Configuration options for <see cref="JiraClient"/>.
/// </summary>
public class JiraClientOptions
{
    /// <summary>Gets or sets the Jira instance base URL.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the username for authentication.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the API token for authentication.</summary>
    public string ApiToken { get; set; } = string.Empty;
}
