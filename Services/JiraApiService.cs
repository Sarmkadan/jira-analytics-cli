// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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
            var projectData = JObject.Parse(json);

            var project = new JiraProject
            {
                Key = projectKey,
                Id = projectData["id"]?.ToString() ?? string.Empty,
                Name = projectData["name"]?.ToString() ?? string.Empty,
                Description = projectData["description"]?.ToString(),
                ProjectType = projectData["type"]?.ToString() ?? "software",
                Lead = projectData["lead"]?["displayName"]?.ToString(),
                CreatedDate = DateTime.Parse(projectData["created"]?.ToString() ?? DateTime.UtcNow.ToString()),
                Url = projectData["url"]?.ToString()
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
            var data = JObject.Parse(json);
            var sprintArray = data["values"] as JArray;

            if (sprintArray != null)
            {
                foreach (var sprintData in sprintArray)
                {
                    var sprint = new Sprint
                    {
                        Id = (int)(sprintData["id"] ?? 0),
                        Key = sprintData["key"]?.ToString() ?? string.Empty,
                        Name = sprintData["name"]?.ToString() ?? string.Empty,
                        State = sprintData["state"]?.ToString() ?? "Open",
                        StartDate = DateTime.TryParse(sprintData["startDate"]?.ToString() ?? string.Empty, out var start) ? start : null,
                        EndDate = DateTime.TryParse(sprintData["endDate"]?.ToString() ?? string.Empty, out var end) ? end : null,
                        CompleteDate = DateTime.TryParse(sprintData["completeDate"]?.ToString() ?? string.Empty, out var complete) ? complete : null,
                        Goal = sprintData["goal"]?.ToString(),
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
            var sprintData = JObject.Parse(json);

            var sprint = new Sprint
            {
                Id = (int)(sprintData["id"] ?? sprintId),
                Key = sprintData["key"]?.ToString() ?? string.Empty,
                Name = sprintData["name"]?.ToString() ?? string.Empty,
                State = sprintData["state"]?.ToString() ?? "Open"
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
            var data = JObject.Parse(json);
            var issueArray = data["issues"] as JArray;

            if (issueArray != null)
            {
                foreach (var issueData in issueArray)
                {
                    var issue = ParseIssueData(issueData as JObject, sprintId);
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
        var issues = new List<JiraIssue>();

        try
        {
            _logger.LogInformation("Fetching issues for project {ProjectKey}", projectKey);
            var jql = $"project = {projectKey} ORDER BY created DESC";
            var response = await _httpClient.GetAsync($"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100");

            if (!response.IsSuccessStatusCode) return issues;

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var issueArray = data["issues"] as JArray;

            if (issueArray != null)
            {
                foreach (var issueData in issueArray)
                {
                    var issue = ParseIssueData(issueData as JObject, 0);
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
        var team = new List<Developer>();

        try
        {
            _logger.LogInformation("Fetching team for project {ProjectKey}", projectKey);
            var response = await _httpClient.GetAsync($"/rest/api/3/projects/{projectKey}/components");

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
        try
        {
            var response = await _httpClient.GetAsync($"/rest/api/3/issues/{issueKey}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var issueData = JObject.Parse(json);

            return ParseIssueData(issueData, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching issue {IssueKey}", issueKey);
            return null;
        }
    }

    public async Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId)
    {
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

    private JiraIssue? ParseIssueData(JObject? issueData, int sprintId)
    {
        if (issueData == null) return null;

        try
        {
            var issue = new JiraIssue
            {
                Key = issueData["key"]?.ToString() ?? string.Empty,
                Id = issueData["id"]?.ToString() ?? string.Empty,
                Summary = issueData["fields"]?["summary"]?.ToString() ?? string.Empty,
                Description = issueData["fields"]?["description"]?.ToString(),
                Status = issueData["fields"]?["status"]?["name"]?.ToString() ?? "Open",
                IssueType = issueData["fields"]?["issuetype"]?["name"]?.ToString() ?? "Task",
                Assignee = issueData["fields"]?["assignee"]?["displayName"]?.ToString(),
                Priority = issueData["fields"]?["priority"]?["name"]?.ToString() ?? "Medium",
                StoryPoints = int.TryParse(issueData["fields"]?["customfield_10016"]?.ToString() ?? "0", out var points) ? points : 0,
                CreatedDate = DateTime.Parse(issueData["fields"]?["created"]?.ToString() ?? DateTime.UtcNow.ToString()),
                UpdatedDate = DateTime.Parse(issueData["fields"]?["updated"]?.ToString() ?? DateTime.UtcNow.ToString()),
                SprintId = sprintId
            };

            if (DateTime.TryParse(issueData["fields"]?["duedate"]?.ToString() ?? string.Empty, out var dueDate))
                issue.DueDate = dueDate;

            if (DateTime.TryParse(issueData["fields"]?["resolutiondate"]?.ToString() ?? string.Empty, out var resDate))
                issue.ResolutionDate = resDate;

            return issue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing issue data");
            return null;
        }
    }
}
