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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Jira project or null if not found.</returns>
    public Task<JiraProject?> GetProjectAsync(string projectKey, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all sprints for a given project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of sprints.</returns>
    public Task<List<Sprint>> GetProjectSprintsAsync(string projectKey, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a sprint by its ID.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sprint or null if not found.</returns>
    public Task<Sprint?> GetSprintAsync(int sprintId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all issues in a sprint.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of issues.</returns>
    public Task<List<JiraIssue>> GetSprintIssuesAsync(int sprintId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all issues in a project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of issues.</returns>
    public Task<List<JiraIssue>> GetProjectIssuesAsync(string projectKey, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the team assigned to a project.</summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of developers.</returns>
    public Task<List<Developer>> GetProjectTeamAsync(string projectKey, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a specific issue by its key.</summary>
    /// <param name="issueKey">The issue identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issue or null if not found.</returns>
    public Task<JiraIssue?> GetIssueAsync(string issueKey, CancellationToken cancellationToken = default);

    /// <summary>Retrieves burndown data for a sprint.</summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of burndown snapshots.</returns>
    public Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId, CancellationToken cancellationToken = default);

    /// <summary>Verifies the connection to the Jira instance.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the connection is successful, false otherwise.</returns>
    public Task<bool> VerifyConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an arbitrary JQL query and returns paginated results.
    /// </summary>
    /// <param name="jql">The JQL query string.</param>
    /// <param name="maxResults">Maximum number of issues to return.</param>
    /// <param name="startAt">Zero-based index of the first result for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A search result containing total count and issues.</returns>
    public Task<JiraSearchResult> SearchByJqlAsync(string jql, int maxResults = 50, int startAt = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an arbitrary JQL query and returns issues as a stream (order-independent).
    /// This method fetches issues in pages but yields them immediately without accumulating all in memory.
    /// </summary>
    /// <param name="jql">The JQL query string.</param>
    /// <param name="pageSize">Number of issues to fetch per page (recommended: 50-200).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of issues.</returns>
    public IAsyncEnumerable<JiraIssue> StreamIssuesByJqlAsync(string jql, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams all issues in a project without accumulating them all in memory.
    /// </summary>
    /// <param name="projectKey">The project identifier.</param>
    /// <param name="pageSize">Number of issues to fetch per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of issues.</returns>
    public IAsyncEnumerable<JiraIssue> StreamProjectIssuesAsync(string projectKey, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams all issues in a sprint without accumulating them all in memory.
    /// </summary>
    /// <param name="sprintId">The sprint identifier.</param>
    /// <param name="pageSize">Number of issues to fetch per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of issues.</returns>
    public IAsyncEnumerable<JiraIssue> StreamSprintIssuesAsync(int sprintId, int pageSize = 100, CancellationToken cancellationToken = default);
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
