// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
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
            var response = await _httpClient.GetAsync($"/rest/api/3/projects/{projectKey}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch project {ProjectKey}: {StatusCode}", projectKey, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var createdStr = GetString(root, "created");
            var createdDate = ParseDateTimeInvariant(createdStr) ?? DateTime.MinValue;
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
            var response = await _httpClient.GetAsync($"/rest/api/3/projects/{projectKey}/sprints");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch sprints: {StatusCode}", response.StatusCode);
                return sprints;
            }

            var json = await response.Content.ReadAsStringAsync();
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
            var response = await _httpClient.GetAsync($"/rest/api/3/sprints/{sprintId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch sprint {SprintId}: {StatusCode}", sprintId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
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
            var response = await _httpClient.GetAsync($"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch sprint issues: {StatusCode}", response.StatusCode);
                return issues;
            }

            var json = await response.Content.ReadAsStringAsync();
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
            var response = await _httpClient.GetAsync($"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100");

            if (!response.IsSuccessStatusCode) return issues;

            var json = await response.Content.ReadAsStringAsync();
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

            // Jira has no dedicated "team roster" endpoint; the team is derived by
            // aggregating the distinct assignees across the project's issues.
            var issues = await GetProjectIssuesAsync(projectKey);

            var developersByAssignee = new Dictionary<string, Developer>(StringComparer.Ordinal);

            foreach (var issue in issues)
            {
                if (string.IsNullOrWhiteSpace(issue.Assignee))
                    continue;

                if (!developersByAssignee.TryGetValue(issue.Assignee, out var developer))
                {
                    developer = new Developer
                    {
                        Key = issue.Assignee,
                        Name = issue.Assignee,
                        DisplayName = issue.Assignee
                    };
                    developersByAssignee[issue.Assignee] = developer;
                }

                developer.AssignIssue(issue);
            }

            team.AddRange(developersByAssignee.Values.OrderBy(d => d.DisplayName, StringComparer.Ordinal));

            _logger.LogInformation("Fetched team data for project {ProjectKey}: {DeveloperCount} developers", projectKey, team.Count);
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
            var response = await _httpClient.GetAsync($"/rest/api/3/issues/{issueKey}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
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

            // Jira's REST API has no burndown endpoint; the standard approach is to
            // derive a snapshot from the sprint's current issue state.
            var issues = await GetSprintIssuesAsync(sprintId);

            var completedIssues = issues.Where(i => i.Status is "Done" or "Closed").ToList();
            var remainingIssues = issues.Except(completedIssues).ToList();

            var totalPoints = issues.Sum(i => i.StoryPoints ?? 0);
            var completedPoints = completedIssues.Sum(i => i.StoryPoints ?? 0);

            var snapshot = new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow,
                SprintId = sprintId,
                TotalStoryPoints = Math.Max(totalPoints, 1),
                CompletedStoryPoints = completedPoints,
                RemainingStoryPoints = Math.Max(totalPoints, 1) - completedPoints,
                TotalIssueCount = issues.Count,
                CompletedIssueCount = completedIssues.Count,
                RemainingIssueCount = remainingIssues.Count,
                ScopeChanges = 0
            };

            snapshots.Add(snapshot);

            _logger.LogInformation("Fetched burndown data for sprint {SprintId}: {Completed}/{Total} pts", sprintId, completedPoints, totalPoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching burndown data");
        }

        return snapshots;
    }

    public async Task<JiraSearchResult> SearchByJqlAsync(string jql, int maxResults = 50, int startAt = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jql, nameof(jql));

        var result = new JiraSearchResult { StartAt = startAt };

        try
        {
            _logger.LogInformation("Executing JQL search (startAt={Start}, maxResults={Max}): {Jql}", startAt, maxResults, jql);

            var url = $"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults={maxResults}&startAt={startAt}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("JQL search returned {StatusCode}", response.StatusCode);
                return result;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            result.Total = GetInt(root, "total");

            if (root.TryGetProperty("issues", out var issueArray) && issueArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var issueData in issueArray.EnumerateArray())
                {
                    var issue = ParseIssueData(issueData, 0);
                    if (issue != null) result.Issues.Add(issue);
                }
            }

            _logger.LogInformation("JQL search returned {Count} of {Total} issues", result.Issues.Count, result.Total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing JQL search");
        }

        return result;
    }

    public async Task<bool> VerifyConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Verifying Jira API connection");
            var response = await _httpClient.GetAsync("/rest/api/3/myself");

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

            var createdDate = ParseDateTimeInvariant(createdStr) ?? DateTime.MinValue;
            var updatedDate = ParseDateTimeInvariant(updatedStr) ?? DateTime.MinValue;
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
                StoryPoints = double.TryParse(storyPtsStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var points)
                    ? (int)Math.Round(points, MidpointRounding.AwayFromZero)
                    : 0,
                CreatedDate = createdDate,
                UpdatedDate = updatedDate,
                SprintId = sprintId
            };

            var dueStr = GetNestedStringOrNull(issueData, "fields", "duedate");
            var dueDate = ParseDateTimeInvariant(dueStr);
            if (dueDate.HasValue)
                issue.DueDate = dueDate;

            var resStr = GetNestedStringOrNull(issueData, "fields", "resolutiondate");
            var resDate = ParseDateTimeInvariant(resStr);
            if (resDate.HasValue)
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
        if (int.TryParse(p.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return v;
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
    private static DateTime? ParseDateOrNull(string? value) => ParseDateTimeInvariant(value);

    // Parses Jira's ISO-8601 timestamps using the invariant culture so parsing never depends
    // on the host machine's regional settings (a machine, not a human, produced this data).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime? ParseDateTimeInvariant(string? value)
        => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)
            ? dt
            : null;

    /// <summary>
    /// Streams issues from a JQL query in pages, yielding each issue as it's received.
    /// This avoids accumulating all issues in memory and allows processing large result sets efficiently.
    /// </summary>
    /// <param name="jql">The JQL query string.</param>
    /// <param name="pageSize">Number of issues to fetch per page.</param>
    /// <returns>Async stream of issues.</returns>
    public async IAsyncEnumerable<JiraIssue> StreamIssuesByJqlAsync(string jql, int pageSize = 100)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jql, nameof(jql));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0, nameof(pageSize));

        _logger.LogInformation("Streaming issues by JQL (pageSize={PageSize}): {Jql}", pageSize, jql);

        int startAt = 0;
        bool hasMoreResults = true;

        while (hasMoreResults)
        {
            List<JiraIssue>? pageIssues = null;
            
            try
            {
                var result = await SearchByJqlAsync(jql, pageSize, startAt);
                pageIssues = result.Issues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page of issues for JQL: {Jql}", jql);
                yield break;
            }

            if (pageIssues == null || pageIssues.Count == 0)
            {
                hasMoreResults = false;
                break;
            }

            foreach (var issue in pageIssues)
            {
                yield return issue;
            }

            startAt += pageSize;

            // Stop if we got fewer results than requested (last page)
            if (pageIssues.Count < pageSize)
            {
                hasMoreResults = false;
            }
        }

        _logger.LogInformation("Completed streaming {IssueCount} issues for JQL query", startAt);
    }

    public async IAsyncEnumerable<JiraIssue> StreamProjectIssuesAsync(string projectKey, int pageSize = 100)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0, nameof(pageSize));

        _logger.LogInformation("Streaming issues for project {ProjectKey} (pageSize={PageSize})", projectKey, pageSize);

        int startAt = 0;
        bool hasMoreResults = true;

        while (hasMoreResults)
        {
            List<JiraIssue>? pageIssues = null;

            try
            {
                var result = await SearchByJqlAsync($"project = {projectKey} ORDER BY created DESC", pageSize, startAt);
                pageIssues = result.Issues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page of issues for project {ProjectKey}", projectKey);
                yield break;
            }

            if (pageIssues == null || pageIssues.Count == 0)
            {
                hasMoreResults = false;
                break;
            }

            foreach (var issue in pageIssues)
            {
                yield return issue;
            }

            startAt += pageSize;

            // Stop if we got fewer results than requested (last page)
            if (pageIssues.Count < pageSize)
            {
                hasMoreResults = false;
            }
        }

        _logger.LogInformation("Completed streaming issues for project {ProjectKey}", projectKey);
    }

    public async IAsyncEnumerable<JiraIssue> StreamSprintIssuesAsync(int sprintId, int pageSize = 100)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0, nameof(pageSize));

        _logger.LogInformation("Streaming issues for sprint {SprintId} (pageSize={PageSize})", sprintId, pageSize);

        int startAt = 0;
        bool hasMoreResults = true;

        while (hasMoreResults)
        {
            List<JiraIssue>? pageIssues = null;

            try
            {
                var result = await SearchByJqlAsync($"sprint = {sprintId} ORDER BY created DESC", pageSize, startAt);
                pageIssues = result.Issues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page of issues for sprint {SprintId}", sprintId);
                yield break;
            }

            if (pageIssues == null || pageIssues.Count == 0)
            {
                hasMoreResults = false;
                break;
            }

            foreach (var issue in pageIssues)
            {
                yield return issue;
            }

            startAt += pageSize;

            // Stop if we got fewer results than requested (last page)
            if (pageIssues.Count < pageSize)
            {
                hasMoreResults = false;
            }
        }

        _logger.LogInformation("Completed streaming issues for sprint {SprintId}", sprintId);
    }
}
