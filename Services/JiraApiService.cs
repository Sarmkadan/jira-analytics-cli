// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Jira API client for fetching projects, sprints, issues, and team data
/// </summary>
public class JiraApiService : IJiraApiService
{
    private const string ApiV3Base = "/rest/api/3";
    private const string ProjectsRoute = "/projects";
    private const string SprintsRoute = "/sprints";
    private const string SearchRoute = "/search";
    private const string IssuesRoute = "/issues";
    private const string MyselfRoute = "/myself";
    private const string JqlParam = "jql";
    private const string MaxResultsParam = "maxResults";
    private const string StartAtParam = "startAt";
    private const string CreatedField = "created";
    private const string DescriptionField = "description";
    private const string DisplayNameField = "displayName";
    private const string FieldsField = "fields";
    private const string IdField = "id";
    private const string IssuesField = "issues";
    private const string KeyField = "key";
    private const string NameField = "name";
    private const string StateField = "state";
    private const string EmptyDefault = "";
    private const string MediumDefault = "Medium";
    private const string NumericZeroDefault = "0";
    private const string OpenDefault = "Open";
    private const string SoftwareDefault = "software";
    private const string TaskDefault = "Task";
    private const string DefaultMaxResults = "100";
    private const string OrderByCreatedDescending = $"ORDER BY {CreatedField} DESC";

    private readonly HttpClient _httpClient;
    private readonly ICliConfig _config;
    private readonly ILogger<JiraApiService> _logger;

    public JiraApiService(IHttpClientFactory httpClientFactory, ICliConfig config, ILogger<JiraApiService> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this._httpClient = httpClientFactory.CreateClient("jira");
        this._config = config;
        this._logger = logger;
    }

    public async Task<JiraProject?> GetProjectAsync(string projectKey, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        return await this.GetAndParseAsync(
            $"{ApiV3Base}{ProjectsRoute}/{projectKey}",
            root =>
            {
                var createdStr = GetString(root, CreatedField);
                var createdDate = ParseDateTimeInvariant(createdStr) ?? DateTime.MinValue;
                var project = new JiraProject
                {
                    Key = projectKey,
                    Id = GetString(root, IdField),
                    Name = GetString(root, NameField),
                    Description = GetStringOrNull(root, DescriptionField),
                    ProjectType = GetString(root, "type", SoftwareDefault),
                    Lead = GetNestedStringOrNull(root, "lead", DisplayNameField),
                    CreatedDate = createdDate,
                    Url = GetStringOrNull(root, "url")
                };

                this._logger.LogInformation("Successfully fetched project {ProjectKey}: {ProjectName}", projectKey, project.Name);
                return project;
            },
            $"project {projectKey}",
            () => this._logger.LogInformation("Fetching project {ProjectKey}", projectKey),
            statusCode => this._logger.LogWarning("Failed to fetch project {ProjectKey}: {StatusCode}", projectKey, statusCode),
            ex => this._logger.LogError(ex, "Error fetching project {ProjectKey}", projectKey),
            cancellationToken);
    }

    public async Task<List<Sprint>> GetProjectSprintsAsync(string projectKey, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        var sprints = new List<Sprint>();
        return await this.GetAndParseAsync(
            $"{ApiV3Base}{ProjectsRoute}/{projectKey}{SprintsRoute}",
            root =>
            {
                if (root.TryGetProperty("values", out var sprintArray) && sprintArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var sprintData in sprintArray.EnumerateArray())
                    {
                        var sprint = new Sprint
                        {
                            Id = GetInt(sprintData, IdField),
                            Key = GetString(sprintData, KeyField),
                            Name = GetString(sprintData, NameField),
                            State = GetString(sprintData, StateField, OpenDefault),
                            StartDate = ParseDateOrNull(GetStringOrNull(sprintData, "startDate")),
                            EndDate = ParseDateOrNull(GetStringOrNull(sprintData, "endDate")),
                            CompleteDate = ParseDateOrNull(GetStringOrNull(sprintData, "completeDate")),
                            Goal = GetStringOrNull(sprintData, "goal"),
                            ProjectKey = projectKey
                        };

                        sprints.Add(sprint);
                    }
                }

                this._logger.LogInformation("Fetched {SprintCount} sprints for project {ProjectKey}", sprints.Count, projectKey);
                return sprints;
            },
            $"sprints for project {projectKey}",
            () => this._logger.LogInformation("Fetching sprints for project {ProjectKey}", projectKey),
            statusCode => this._logger.LogWarning("Failed to fetch sprints: {StatusCode}", statusCode),
            ex => this._logger.LogError(ex, "Error fetching sprints for project {ProjectKey}", projectKey),
            cancellationToken) ?? sprints;
    }

    public async Task<Sprint?> GetSprintAsync(int sprintId, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for sprintId
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        return await this.GetAndParseAsync(
            $"{ApiV3Base}{SprintsRoute}/{sprintId}",
            sprintData => new Sprint
            {
                Id = GetInt(sprintData, IdField, sprintId),
                Key = GetString(sprintData, KeyField),
                Name = GetString(sprintData, NameField),
                State = GetString(sprintData, StateField, OpenDefault)
            },
            $"sprint {sprintId}",
            () => this._logger.LogInformation("Fetching sprint {SprintId}", sprintId),
            statusCode => this._logger.LogWarning("Failed to fetch sprint {SprintId}: {StatusCode}", sprintId, statusCode),
            ex => this._logger.LogError(ex, "Error fetching sprint {SprintId}", sprintId),
            cancellationToken);
    }

    public async Task<List<JiraIssue>> GetSprintIssuesAsync(int sprintId, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for sprintId
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        var jql = $"sprint = {sprintId} {OrderByCreatedDescending}";
        var issues = new List<JiraIssue>();
        return await this.GetAndParseAsync(
            $"{ApiV3Base}{SearchRoute}?{JqlParam}={Uri.EscapeDataString(jql)}&{MaxResultsParam}={DefaultMaxResults}",
            root =>
            {
                if (root.TryGetProperty(IssuesField, out var issueArray) && issueArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var issueData in issueArray.EnumerateArray())
                    {
                        var issue = this.ParseIssueData(issueData, sprintId);
                        if (issue != null)
                        {
                            issues.Add(issue);
                        }
                    }
                }

                this._logger.LogInformation("Fetched {IssueCount} issues for sprint {SprintId}", issues.Count, sprintId);
                return issues;
            },
            $"issues for sprint {sprintId}",
            () => this._logger.LogInformation("Fetching issues for sprint {SprintId}", sprintId),
            statusCode => this._logger.LogWarning("Failed to fetch sprint issues: {StatusCode}", statusCode),
            ex => this._logger.LogError(ex, "Error fetching sprint issues"),
            cancellationToken) ?? issues;
    }

    public async Task<List<JiraIssue>> GetProjectIssuesAsync(string projectKey, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        var jql = $"project = {projectKey} {OrderByCreatedDescending}";
        var issues = new List<JiraIssue>();
        return await this.GetAndParseAsync(
            $"{ApiV3Base}{SearchRoute}?{JqlParam}={Uri.EscapeDataString(jql)}&{MaxResultsParam}={DefaultMaxResults}",
            root =>
            {
                if (root.TryGetProperty(IssuesField, out var issueArray) && issueArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var issueData in issueArray.EnumerateArray())
                    {
                        var issue = this.ParseIssueData(issueData, 0);
                        if (issue != null)
                        {
                            issues.Add(issue);
                        }
                    }
                }

                this._logger.LogInformation("Fetched {IssueCount} issues for project {ProjectKey}", issues.Count, projectKey);
                return issues;
            },
            $"issues for project {projectKey}",
            () => this._logger.LogInformation("Fetching issues for project {ProjectKey}", projectKey),
            logFailure: null,
            ex => this._logger.LogError(ex, "Error fetching project issues"),
            cancellationToken) ?? issues;
    }

    public async Task<List<Developer>> GetProjectTeamAsync(string projectKey, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for projectKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        var team = new List<Developer>();

        try
        {
            this._logger.LogInformation("Fetching team for project {ProjectKey}", projectKey);

            // Jira has no dedicated "team roster" endpoint; the team is derived by
            // aggregating the distinct assignees across the project's issues.
            var issues = await this.GetProjectIssuesAsync(projectKey, cancellationToken);

            var developersByAssignee = new Dictionary<string, Developer>(StringComparer.Ordinal);

            foreach (var issue in issues)
            {
                if (string.IsNullOrWhiteSpace(issue.Assignee))
                {
                    continue;
                }

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

            this._logger.LogInformation("Fetched team data for project {ProjectKey}: {DeveloperCount} developers", projectKey, team.Count);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            this._logger.LogError(ex, "Error fetching project team");
        }

        return team;
    }

    public async Task<JiraIssue?> GetIssueAsync(string issueKey, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for issueKey
        ArgumentNullException.ThrowIfNullOrWhiteSpace(issueKey, nameof(issueKey));
        return await this.GetAndParseAsync(
            $"{ApiV3Base}{IssuesRoute}/{issueKey}",
            root => this.ParseIssueData(root, 0),
            $"issue {issueKey}",
            logStart: null,
            logFailure: null,
            ex => this._logger.LogError(ex, "Error fetching issue {IssueKey}", issueKey),
            cancellationToken);
    }

    public async Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId, CancellationToken cancellationToken = default)
    {
        // Fix: Add input validation for sprintId
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        var snapshots = new List<BurndownSnapshot>();

        try
        {
            this._logger.LogInformation("Fetching burndown data for sprint {SprintId}", sprintId);

            // Jira's REST API has no burndown endpoint; the standard approach is to
            // derive a snapshot from the sprint's current issue state.
            var issues = await this.GetSprintIssuesAsync(sprintId, cancellationToken);

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

            this._logger.LogInformation("Fetched burndown data for sprint {SprintId}: {Completed}/{Total} pts", sprintId, completedPoints, totalPoints);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            this._logger.LogError(ex, "Error fetching burndown data");
        }

        return snapshots;
    }

    public async Task<JiraSearchResult> SearchByJqlAsync(string jql, int maxResults = 50, int startAt = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(jql, nameof(jql));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxResults, nameof(maxResults));
        ArgumentOutOfRangeException.ThrowIfNegative(startAt, nameof(startAt));

        var fallback = new JiraSearchResult { StartAt = startAt };
        var url = $"{ApiV3Base}{SearchRoute}?{JqlParam}={Uri.EscapeDataString(jql)}&{MaxResultsParam}={maxResults}&{StartAtParam}={startAt}";
        return await this.GetAndParseAsync(
            url,
            root =>
            {
                fallback.Total = GetInt(root, "total");

                if (root.TryGetProperty(IssuesField, out var issueArray) && issueArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var issueData in issueArray.EnumerateArray())
                    {
                        var issue = this.ParseIssueData(issueData, 0);
                        if (issue != null)
                        {
                            fallback.Issues.Add(issue);
                        }
                    }
                }

                this._logger.LogInformation("JQL search returned {Count} of {Total} issues", fallback.Issues.Count, fallback.Total);
                return fallback;
            },
            "JQL search",
            () => this._logger.LogInformation("Executing JQL search (startAt={Start}, maxResults={Max}): {Jql}", startAt, maxResults, jql),
            statusCode => this._logger.LogWarning("JQL search returned {StatusCode}", statusCode),
            ex => this._logger.LogError(ex, "Error executing JQL search"),
            cancellationToken) ?? fallback;
    }

    public async Task<bool> VerifyConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this._logger.LogInformation("Verifying Jira API connection");
            var response = await this._httpClient.GetAsync($"{ApiV3Base}{MyselfRoute}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                this._logger.LogInformation("Jira API connection verified successfully");
                return true;
            }

            this._logger.LogWarning("Failed to verify Jira connection: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            this._logger.LogError(ex, "Error verifying Jira connection");
            return false;
        }
    }

    private async Task<T?> GetAndParseAsync<T>(
        string requestUri,
        Func<JsonElement, T> map,
        string operationName,
        Action? logStart,
        Action<HttpStatusCode>? logFailure,
        Action<Exception> logError,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName, nameof(operationName));

        try
        {
            logStart?.Invoke();
            var response = await this._httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logFailure?.Invoke(response.StatusCode);
                return default;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            return map(doc.RootElement);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logError(ex);
            return default;
        }
    }

    private JiraIssue? ParseIssueData(JsonElement issueData, int sprintId)
    {
        if (issueData.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        try
        {
            var createdStr = GetNestedStringOrNull(issueData, FieldsField, CreatedField);
            var updatedStr = GetNestedStringOrNull(issueData, FieldsField, "updated");
            var storyPtsStr = GetNestedStringOrNull(issueData, FieldsField, "customfield_10016") ?? NumericZeroDefault;

            var createdDate = ParseDateTimeInvariant(createdStr) ?? DateTime.MinValue;
            var updatedDate = ParseDateTimeInvariant(updatedStr) ?? DateTime.MinValue;
            var issue = new JiraIssue
            {
                Key = GetString(issueData, KeyField),
                Id = GetString(issueData, IdField),
                Summary = GetNestedString(issueData, FieldsField, "summary"),
                Description = GetNestedStringOrNull(issueData, FieldsField, DescriptionField),
                Status = GetPath(issueData, FieldsField, "status", NameField) ?? OpenDefault,
                IssueType = GetPath(issueData, FieldsField, "issuetype", NameField) ?? TaskDefault,
                Assignee = GetPath(issueData, FieldsField, "assignee", DisplayNameField),
                Priority = GetPath(issueData, FieldsField, "priority", NameField) ?? MediumDefault,
                StoryPoints = double.TryParse(storyPtsStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var points)
                    ? (int)Math.Round(points, MidpointRounding.AwayFromZero)
                    : 0,
                CreatedDate = createdDate,
                UpdatedDate = updatedDate,
                SprintId = sprintId
            };

            var dueStr = GetNestedStringOrNull(issueData, FieldsField, "duedate");
            var dueDate = ParseDateTimeInvariant(dueStr);
            if (dueDate.HasValue)
            {
                issue.DueDate = dueDate;
            }

            var resStr = GetNestedStringOrNull(issueData, FieldsField, "resolutiondate");
            var resDate = ParseDateTimeInvariant(resStr);
            if (resDate.HasValue)
            {
                issue.ResolutionDate = resDate;
            }

            return issue;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error parsing issue data");
            return null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetString(JsonElement element, string property, string defaultValue = EmptyDefault)
        => element.TryGetProperty(property, out var p) ? p.GetString() ?? defaultValue : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetStringOrNull(JsonElement element, string property)
        => element.TryGetProperty(property, out var p) ? p.GetString() : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetInt(JsonElement element, string property, int defaultValue = 0)
    {
        if (!element.TryGetProperty(property, out var p))
        {
            return defaultValue;
        }

        if (p.ValueKind == JsonValueKind.Number)
        {
            return p.GetInt32();
        }

        if (int.TryParse(p.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            return v;
        }

        return defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetNestedString(JsonElement element, string prop1, string prop2, string defaultValue = EmptyDefault)
    {
        if (element.TryGetProperty(prop1, out var p1) && p1.TryGetProperty(prop2, out var p2))
        {
            return p2.GetString() ?? defaultValue;
        }

        return defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetNestedStringOrNull(JsonElement element, string prop1, string prop2)
    {
        if (element.TryGetProperty(prop1, out var p1) && p1.TryGetProperty(prop2, out var p2))
        {
            return p2.GetString();
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? GetPath(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var key in path)
        {
            if (!current.TryGetProperty(key, out current))
            {
                return null;
            }
        }
        return current.GetString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime? ParseDateOrNull(string? value) => ParseDateTimeInvariant(value);

    // Parses Jira's ISO-8601 timestamps using the invariant culture so parsing never depends
    // on the host machine's regional settings (a machine, not a human, produced this data).
    // Jira emits timestamps with a numeric UTC offset (e.g. "+0200"), not "Z". Parsing those
    // with DateTimeStyles.RoundtripKind yields DateTimeKind.Local, converted using whatever
    // time zone the process happens to run in. Mixing that with DateTimeKind.Utc values
    // (e.g. DateTime.UtcNow used elsewhere) silently corrupts duration math across a DST
    // change, because plain DateTime subtraction ignores Kind entirely. Normalizing every
    // parsed timestamp to UTC here makes all downstream DateTime arithmetic Kind-consistent.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTime? ParseDateTimeInvariant(string? value)
        => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto)
            ? dto.UtcDateTime
            : null;

    /// <summary>
    /// Streams issues from a JQL query in pages, yielding each issue as it's received.
    /// This avoids accumulating all issues in memory and allows processing large result sets efficiently.
    /// </summary>
    /// <param name="jql">The JQL query string.</param>
    /// <param name="pageSize">Number of issues to fetch per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of issues.</returns>
    public async IAsyncEnumerable<JiraIssue> StreamIssuesByJqlAsync(string jql, int pageSize = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(jql, nameof(jql));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        this._logger.LogInformation("Streaming issues by JQL (pageSize={PageSize}): {Jql}", pageSize, jql);

        int startAt = 0;
        bool hasMoreResults = true;

        while (hasMoreResults)
        {
            List<JiraIssue>? pageIssues = null;
            
            try
            {
                var result = await this.SearchByJqlAsync(jql, pageSize, startAt, cancellationToken);
                pageIssues = result.Issues;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                this._logger.LogError(ex, "Error fetching page of issues for JQL: {Jql}", jql);
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

        this._logger.LogInformation("Completed streaming {IssueCount} issues for JQL query", startAt);
    }

    public async IAsyncEnumerable<JiraIssue> StreamProjectIssuesAsync(string projectKey, int pageSize = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        this._logger.LogInformation("Streaming issues for project {ProjectKey} (pageSize={PageSize})", projectKey, pageSize);

        int startAt = 0;
        bool hasMoreResults = true;

        while (hasMoreResults)
        {
            List<JiraIssue>? pageIssues = null;

            try
            {
                var result = await this.SearchByJqlAsync($"project = {projectKey} {OrderByCreatedDescending}", pageSize, startAt, cancellationToken);
                pageIssues = result.Issues;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                this._logger.LogError(ex, "Error fetching page of issues for project {ProjectKey}", projectKey);
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

        this._logger.LogInformation("Completed streaming issues for project {ProjectKey}", projectKey);
    }

    public async IAsyncEnumerable<JiraIssue> StreamSprintIssuesAsync(int sprintId, int pageSize = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sprintId, nameof(sprintId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        this._logger.LogInformation("Streaming issues for sprint {SprintId} (pageSize={PageSize})", sprintId, pageSize);

        int startAt = 0;
        bool hasMoreResults = true;

        while (hasMoreResults)
        {
            List<JiraIssue>? pageIssues = null;

            try
            {
                var result = await this.SearchByJqlAsync($"sprint = {sprintId} {OrderByCreatedDescending}", pageSize, startAt, cancellationToken);
                pageIssues = result.Issues;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                this._logger.LogError(ex, "Error fetching page of issues for sprint {SprintId}", sprintId);
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

        this._logger.LogInformation("Completed streaming issues for sprint {SprintId}", sprintId);
    }
}
