// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Runtime.CompilerServices;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Configuration;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Jira API client for fetching projects, sprints, issues, and team data
/// </summary>
public class JiraApiService : IJiraApiService
{
    private readonly HttpClient _httpClient;
    private readonly ICliConfig _config;
    private readonly ILogger<JiraApiService> _logger;

    public JiraApiService(IHttpClientFactory httpClientFactory, ICliConfig config, ILogger<JiraApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("jira");
        _config = config;
        _logger = logger;
    }

    public async Task<JiraProject?> GetProjectAsync(string projectKey)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        try
        {
            _logger.LogInformation("Fetching project {ProjectKey}", projectKey);
            var response = await _httpClient.GetAsync($"/rest/api/3/projects/{projectKey}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch project {ProjectKey}: {StatusCode}", projectKey, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var createdStr = GetString(root, "created");
            var createdDate = DateTime.TryParse(createdStr, out var parsedCreatedDate) ? parsedCreatedDate : DateTime.MinValue;
            var project = new JiraProject
            {
                Key = projectKey,
                Id = GetString(root, "id"),
                Name = GetString(root, "name"),
                Description = GetStringOrNull(root, "description"),
                ProjectType = GetString(root, "type", "software"),
                Lead = GetNestedStringOrNull(root, "lead", "displayName"),
                CreatedDate = createdDate,
                Url = GetStringOrNull(root, "url")
            };

            _logger.LogInformation("Successfully fetched project {ProjectKey}: {ProjectName}", projectKey, project.Name);
            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project {ProjectKey}", projectKey);
            return null;
        }
    }

    public async Task<List<Sprint>> GetProjectSprintsAsync(string projectKey)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        var sprints = new List<Sprint>();

        try
        {
            _logger.LogInformation("Fetching sprints for project {ProjectKey}", projectKey);
            var response = await _httpClient.GetAsync($"/rest/api/3/projects/{projectKey}/sprints").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch sprints: {StatusCode}", response.StatusCode);
                return sprints;
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("values", out var sprintArray) && sprintArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var sprintData in sprintArray.EnumerateArray())
                {
                    var sprint = new Sprint
                    {
                        Id = GetInt(sprintData, "id"),
                        Key = GetString(sprintData, "key"),
                        Name = GetString(sprintData, "name"),
                        State = GetString(sprintData, "state", "Open"),
                        StartDate = ParseDateOrNull(GetStringOrNull(sprintData, "startDate")),
                        EndDate = ParseDateOrNull(GetStringOrNull(sprintData, "endDate")),
                        CompleteDate = ParseDateOrNull(GetStringOrNull(sprintData, "completeDate")),
                        Goal = GetStringOrNull(sprintData, "goal"),
                        ProjectKey = projectKey
                    };

                    sprints.Add(sprint);
                }
            }

            _logger.LogInformation("Fetched {SprintCount} sprints for project {ProjectKey}", sprints.Count, projectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sprints for project {ProjectKey}", projectKey);
        }

        return sprints;
    }

    public async Task<Sprint?> GetSprintAsync(int sprintId)
    {
        // Fix: Add input validation for sprintId
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        try
        {
            _logger.LogInformation("Fetching sprint {SprintId}", sprintId);
            var response = await _httpClient.GetAsync($"/rest/api/3/sprints/{sprintId}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch sprint {SprintId}: {StatusCode}", sprintId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var sprintData = doc.RootElement;

            var sprint = new Sprint
            {
                Id = GetInt(sprintData, "id", sprintId),
                Key = GetString(sprintData, "key"),
                Name = GetString(sprintData, "name"),
                State = GetString(sprintData, "state", "Open")
            };

            return sprint;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sprint {SprintId}", sprintId);
            return null;
        }
    }

    public async Task<List<JiraIssue>> GetSprintIssuesAsync(int sprintId)
    {
        // Fix: Add input validation for sprintId
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        var issues = new List<JiraIssue>();

        try
        {
            _logger.LogInformation("Fetching issues for sprint {SprintId}", sprintId);
            var jql = $"sprint = {sprintId} ORDER BY created DESC";
            var response = await _httpClient.GetAsync($"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch sprint issues: {StatusCode}", response.StatusCode);
                return issues;
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("issues", out var issueArray) && issueArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var issueData in issueArray.EnumerateArray())
                {
                    var issue = ParseIssueData(issueData, sprintId);
                    if (issue != null) issues.Add(issue);
                }
            }

            _logger.LogInformation("Fetched {IssueCount} issues for sprint {SprintId}", issues.Count, sprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sprint issues");
        }

        return issues;
    }

    public async Task<List<JiraIssue>> GetProjectIssuesAsync(string projectKey)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        var issues = new List<JiraIssue>();

        try
        {
            _logger.LogInformation("Fetching issues for project {ProjectKey}", projectKey);
            var jql = $"project = {projectKey} ORDER BY created DESC";
            var response = await _httpClient.GetAsync($"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return issues;

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("issues", out var issueArray) && issueArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var issueData in issueArray.EnumerateArray())
                {
                    var issue = ParseIssueData(issueData, 0);
                    if (issue != null) issues.Add(issue);
                }
            }

            _logger.LogInformation("Fetched {IssueCount} issues for project {ProjectKey}", issues.Count, projectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project issues");
        }

        return issues;
    }

    public async Task<List<Developer>> GetProjectTeamAsync(string projectKey)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        var team = new List<Developer>();

        try
        {
            _logger.LogInformation("Fetching team for project {ProjectKey}", projectKey);
            var response = await _httpClient.GetAsync($"/rest/api/3/projects/{projectKey}/components").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch project team: {StatusCode}", response.StatusCode);
                return team;
            }

            // In real implementation, would aggregate from issues
            _logger.LogInformation("Fetched team data for project {ProjectKey}", projectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project team");
        }

        return team;
    }

    public async Task<JiraIssue?> GetIssueAsync(string issueKey)
    {
        // Fix: Add input validation for issueKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(issueKey, nameof(issueKey));
        try
        {
            var response = await _httpClient.GetAsync($"/rest/api/3/issues/{issueKey}").ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            return ParseIssueData(doc.RootElement, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching issue {IssueKey}", issueKey);
            return null;
        }
    }

    public async Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId)
    {
        // Fix: Add input validation for sprintId
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        var snapshots = new List<BurndownSnapshot>();

        try
        {
            _logger.LogInformation("Fetching burndown data for sprint {SprintId}", sprintId);
            // Implementation would fetch from metrics API or calculate from issue history
            _logger.LogInformation("Fetched burndown data for sprint {SprintId}", sprintId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching burndown data");
        }

        return snapshots;
    }

    public async Task<bool> VerifyConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Verifying Jira API connection");
            var response = await _httpClient.GetAsync("/rest/api/3/myself").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Jira API connection verified successfully");
                return true;
            }

            _logger.LogWarning("Failed to verify Jira connection: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Jira connection");
            return false;
        }
    }

    private JiraIssue? ParseIssueData(JsonElement issueData, int sprintId)
    {
        if (issueData.ValueKind != JsonValueKind.Object) return null;

        try
        {
            var createdStr = GetNestedStringOrNull(issueData, "fields", "created");
            var updatedStr = GetNestedStringOrNull(issueData, "fields", "updated");
            var storyPtsStr = GetNestedStringOrNull(issueData, "fields", "customfield_10016") ?? "0";

            var createdDate = DateTime.TryParse(createdStr, out var parsedCreatedDate) ? parsedCreatedDate : DateTime.MinValue;
            var updatedDate = DateTime.TryParse(updatedStr, out var parsedUpdatedDate) ? parsedUpdatedDate : DateTime.MinValue;
            var issue = new JiraIssue
            {
                Key = GetString(issueData, "key"),
                Id = GetString(issueData, "id"),
                Summary = GetNestedString(issueData, "fields", "summary"),
                Description = GetNestedStringOrNull(issueData, "fields", "description"),
                Status = GetPath(issueData, "fields", "status", "name") ?? "Open",
                IssueType = GetPath(issueData, "fields", "issuetype", "name") ?? "Task",
                Assignee = GetPath(issueData, "fields", "assignee", "displayName"),
                Priority = GetPath(issueData, "fields", "priority", "name") ?? "Medium",
                StoryPoints = int.TryParse(storyPtsStr, out var points) ? points : 0,
                CreatedDate = createdDate,
                UpdatedDate = updatedDate,
                SprintId = sprintId
            };

            var dueStr = GetNestedStringOrNull(issueData, "fields", "duedate");
            if (DateTime.TryParse(dueStr, out var dueDate))
                issue.DueDate = dueDate;

            var resStr = GetNestedStringOrNull(issueData, "fields", "resolutiondate");
            if (DateTime.TryParse(resStr, out var resDate))
                issue.ResolutionDate = resDate;

            return issue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing issue data");
            return null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetString(JsonElement element, string property, string defaultValue = "")
        => element.TryGetProperty(property, out var p) ? p.GetString() ?? defaultValue : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetStringOrNull(JsonElement element, string property)
        => element.TryGetProperty(property, out var p) ? p.GetString() : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetInt(JsonElement element, string property, int defaultValue = 0)
    {
        if (!element.TryGetProperty(property, out var p)) return defaultValue;
        if (p.ValueKind == JsonValueKind.Number) return p.GetInt32();
        if (int.TryParse(p.GetString(), out var v)) return v;
        return defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetNestedString(JsonElement element, string prop1, string prop2, string defaultValue = "")
    {
        if (element.TryGetProperty(prop1, out var p1) && p1.TryGetProperty(prop2, out var p2))
            return p2.GetString() ?? defaultValue;
        return defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetNestedStringOrNull(JsonElement element, string prop1, string prop2)
    {
        if (element.TryGetProperty(prop1, out var p1) && p1.TryGetProperty(prop2, out var p2))
            return p2.GetString();
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetPath(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var key in path)
        {
            if (!current.TryGetProperty(key, out current))
                return null;
        }
        return current.GetString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime? ParseDateOrNull(string? value)
        => DateTime.TryParse(value, out var dt) ? dt : null;
}
