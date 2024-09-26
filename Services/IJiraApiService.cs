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
    /// <summary>Retrieves a Jira project by its key.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>The Jira project or null if not found.</returns>
    Task<JiraProject?> GetProjectAsync(string projectKey);
    
    /// <summary>Retrieves all sprints for a given project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>A list of sprints.</returns>
    Task<List<Sprint>> GetProjectSprintsAsync(string projectKey);
    
    /// <summary>Retrieves a sprint by its ID.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <returns>The sprint or null if not found.</returns>
    Task<Sprint?> GetSprintAsync(int sprintId);
    
    /// <summary>Retrieves all issues in a sprint.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <returns>A list of issues.</returns>
    Task<List<JiraIssue>> GetSprintIssuesAsync(int sprintId);
    
    /// <summary>Retrieves all issues in a project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>A list of issues.</returns>
    Task<List<JiraIssue>> GetProjectIssuesAsync(string projectKey);
    
    /// <summary>Retrieves the team assigned to a project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <returns>A list of developers.</returns>
    Task<List<Developer>> GetProjectTeamAsync(string projectKey);
    
    /// <summary>Retrieves a specific issue by its key.</summary>
    /// <param name="issueKey">The issue identifier.</param>
    /// <returns>The issue or null if not found.</returns>
    Task<JiraIssue?> GetIssueAsync(string issueKey);
    
    /// <summary>Retrieves burndown data for a sprint.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <returns>A list of burndown snapshots.</returns>
    Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId);
    
    /// <summary>Verifies the connection to the Jira instance.</summary>
    /// <returns>True if the connection is successful, false otherwise.</returns>
    Task<bool> VerifyConnectionAsync();

    /// <summary>
    /// Executes an arbitrary JQL query and returns paginated results.
    /// </summary>
    /// <param name="jql">The JQL query string.</param>
    /// <param name="maxResults">Maximum number of issues to return.</param>
    /// <param name="startAt">Zero-based index of the first result for pagination.</param>
    /// <returns>A search result containing total count and issues.</returns>
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
