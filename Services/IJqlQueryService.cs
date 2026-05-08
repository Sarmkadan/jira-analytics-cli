// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Executes custom JQL (Jira Query Language) queries and surfaces structured, paginated results.
/// </summary>
public interface IJqlQueryService
{
    /// <summary>
    /// Executes a raw JQL query string with optional result limit.
    /// </summary>
    /// <param name="jql">The JQL query string to execute.</param>
    /// <param name="maxResults">Maximum number of issues to return. Defaults to 50.</param>
    Task<JqlQueryResult> ExecuteQueryAsync(string jql, int maxResults = 50);

    /// <summary>
    /// Executes a structured JQL query defined by a <see cref="JqlQueryRequest"/>, supporting
    /// pagination and field selection.
    /// </summary>
    /// <param name="request">The fully configured query request.</param>
    Task<JqlQueryResult> ExecuteQueryAsync(JqlQueryRequest request);
}

/// <summary>
/// Encapsulates all parameters for a JQL query execution.
/// </summary>
public class JqlQueryRequest
{
    /// <summary>Gets or sets the JQL query string.</summary>
    [JsonPropertyName("jql")]
    public string Jql { get; set; } = string.Empty;

    /// <summary>Gets or sets the maximum number of results to return. Defaults to 50.</summary>
    [JsonPropertyName("maxResults")]
    public int MaxResults { get; set; } = 50;

    /// <summary>Gets or sets the zero-based index of the first result for pagination.</summary>
    [JsonPropertyName("startAt")]
    public int StartAt { get; set; } = 0;

    /// <summary>
    /// Gets or sets an optional human-readable label for this query, used in report headings.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

/// <summary>
/// Contains the paginated results of a JQL query execution.
/// </summary>
public class JqlQueryResult
{
    /// <summary>Gets or sets the JQL query string that was executed.</summary>
    [JsonPropertyName("jql")]
    public string Jql { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of issues matching the query in Jira.</summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>Gets or sets the zero-based index of the first issue returned.</summary>
    [JsonPropertyName("startAt")]
    public int StartAt { get; set; }

    /// <summary>Gets or sets the requested page size.</summary>
    [JsonPropertyName("maxResults")]
    public int MaxResults { get; set; }

    /// <summary>Gets or sets the issues returned by the query.</summary>
    [JsonPropertyName("issues")]
    public List<JiraIssue> Issues { get; set; } = new();

    /// <summary>Gets or sets the UTC timestamp when the query was executed.</summary>
    [JsonPropertyName("executedAt")]
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether the query completed successfully.</summary>
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; } = true;

    /// <summary>Gets or sets an error description when <see cref="IsSuccess"/> is <c>false</c>.</summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>Gets whether additional pages of results are available.</summary>
    [JsonIgnore]
    public bool HasMore => StartAt + Issues.Count < Total;
}
