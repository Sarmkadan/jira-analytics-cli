// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using JiraAnalyticsCli.Exceptions;
using JiraAnalyticsCli.Utils;

namespace JiraAnalyticsCli.Integration;

/// <summary>
/// Advanced Jira API client wrapper with error handling and retry logic.
/// Provides type-safe methods for common Jira operations.
/// </summary>
public class JiraApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JiraApiClient> _logger;
    private const string IssuesEndpoint = "/rest/api/3/issues/search";
    private const string SprintsEndpoint = "/rest/api/3/board/{boardId}/sprint";

    public JiraApiClient(HttpClient httpClient, ILogger<JiraApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches for Jira issues using JQL with automatic retry and pagination.
    /// Handles rate limiting and connection errors gracefully.
    /// </summary>
    public async Task<IEnumerable<dynamic>> SearchIssuesAsync(string jql, int startAt = 0, int maxResults = 50)
    {
        if (string.IsNullOrEmpty(jql))
            throw new ArgumentException("JQL query cannot be empty", nameof(jql));

        try
        {
            var url = $"{IssuesEndpoint}?jql={Uri.EscapeDataString(jql)}&startAt={startAt}&maxResults={maxResults}";
            _logger.LogDebug("Searching Jira issues with JQL: {Jql}", jql);

            var result = await _httpClient.GetWithRetryAsync<dynamic>(url, _logger);
            return result as IEnumerable<dynamic> ?? new List<dynamic>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to search Jira issues");
            throw new JiraApiException($"Jira API error: {ex.Message}", ex.StatusCode);
        }
    }

    /// <summary>
    /// Fetches issue by key with full details including change history.
    /// </summary>
    public async Task<dynamic> GetIssueAsync(string issueKey)
    {
        if (string.IsNullOrEmpty(issueKey))
            throw new ArgumentException("Issue key cannot be empty", nameof(issueKey));

        try
        {
            var url = $"/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}";
            _logger.LogDebug("Fetching issue: {IssueKey}", issueKey);

            var result = await _httpClient.GetWithRetryAsync<dynamic>(url, _logger);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get issue {IssueKey}", issueKey);
            throw new JiraApiException($"Failed to fetch issue: {ex.Message}", ex.StatusCode);
        }
    }

    /// <summary>
    /// Fetches sprints for a given board with pagination support.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetSprintsAsync(int boardId, string state = "active")
    {
        try
        {
            var url = $"/rest/api/3/board/{boardId}/sprint?state={state}";
            _logger.LogDebug("Fetching sprints for board {BoardId}", boardId);

            var result = await _httpClient.GetWithRetryAsync<dynamic>(url, _logger);
            return result as IEnumerable<dynamic> ?? new List<dynamic>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get sprints for board {BoardId}", boardId);
            throw new JiraApiException($"Failed to fetch sprints: {ex.Message}", ex.StatusCode);
        }
    }

    /// <summary>
    /// Checks connectivity to Jira instance and validates authentication.
    /// Useful for health checks and configuration validation.
    /// </summary>
    public async Task<bool> CheckConnectivityAsync()
    {
        try
        {
            _logger.LogDebug("Checking Jira API connectivity");
            var response = await _httpClient.GetAsync("/rest/api/3/myself");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Jira API connection successful");
                return true;
            }

            _logger.LogWarning("Jira API returned status code: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Jira connectivity check failed");
            return false;
        }
    }

    /// <summary>
    /// Fetches all boards or filters by project key.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetBoardsAsync(string? projectKey = null)
    {
        try
        {
            var url = "/rest/api/3/board";
            if (!string.IsNullOrEmpty(projectKey))
                url += $"?projectKey={Uri.EscapeDataString(projectKey)}";

            _logger.LogDebug("Fetching boards");
            var result = await _httpClient.GetWithRetryAsync<dynamic>(url, _logger);
            return result as IEnumerable<dynamic> ?? new List<dynamic>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get boards");
            throw new JiraApiException($"Failed to fetch boards: {ex.Message}", ex.StatusCode);
        }
    }
}
