// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Interface for Jira API communication layer
/// </summary>
public interface IJiraApiService
{
    Task<JiraProject?> GetProjectAsync(string projectKey);
    Task<List<Sprint>> GetProjectSprintsAsync(string projectKey);
    Task<Sprint?> GetSprintAsync(int sprintId);
    Task<List<JiraIssue>> GetSprintIssuesAsync(int sprintId);
    Task<List<JiraIssue>> GetProjectIssuesAsync(string projectKey);
    Task<List<Developer>> GetProjectTeamAsync(string projectKey);
    Task<JiraIssue?> GetIssueAsync(string issueKey);
    Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId);
    Task<bool> VerifyConnectionAsync();

    /// <summary>
    /// Executes an arbitrary JQL query and returns paginated results.
    /// </summary>
    /// <param name="jql">The JQL query string.</param>
    /// <param name="maxResults">Maximum number of issues to return.</param>
    /// <param name="startAt">Zero-based index of the first result for pagination.</param>
    Task<JiraSearchResult> SearchByJqlAsync(string jql, int maxResults = 50, int startAt = 0);
}

/// <summary>
/// Paginated result set returned by a JQL search.
/// </summary>
public class JiraSearchResult
{
    /// <summary>Gets or sets the total number of issues matching the query.</summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>Gets or sets the zero-based index of the first returned issue.</summary>
    [JsonPropertyName("startAt")]
    public int StartAt { get; set; }

    /// <summary>Gets or sets the issues included in this page of results.</summary>
    [JsonPropertyName("issues")]
    public List<JiraIssue> Issues { get; set; } = new();
}
