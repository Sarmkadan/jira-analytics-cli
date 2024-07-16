// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Executes custom JQL queries via the Jira API and surfaces structured, paginated results.
/// Includes a convenience static formatter for console output.
/// </summary>
public class JqlQueryService : IJqlQueryService
{
    private readonly IJiraApiService _jiraApiService;
    private readonly ILogger<JqlQueryService> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="JqlQueryService"/>.
    /// </summary>
    public JqlQueryService(IJiraApiService jiraApiService, ILogger<JqlQueryService> logger)
    {
        _jiraApiService = jiraApiService ?? throw new ArgumentNullException(nameof(jiraApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<JqlQueryResult> ExecuteQueryAsync(string jql, int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(jql))
            throw new ArgumentException("JQL query cannot be empty.", nameof(jql));

        return ExecuteQueryAsync(new JqlQueryRequest { Jql = jql, MaxResults = maxResults });
    }

    /// <inheritdoc/>
    public async Task<JqlQueryResult> ExecuteQueryAsync(JqlQueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Jql))
            throw new ArgumentException("JQL query cannot be empty.", nameof(request));

        _logger.LogInformation(
            "Executing JQL query (maxResults={Max}, startAt={Start}): {Jql}",
            request.MaxResults, request.StartAt, request.Jql);

        try
        {
            var searchResult = await _jiraApiService.SearchByJqlAsync(
                request.Jql,
                request.MaxResults,
                request.StartAt);

            var result = new JqlQueryResult
            {
                Jql        = request.Jql,
                Total      = searchResult.Total,
                StartAt    = searchResult.StartAt,
                MaxResults = request.MaxResults,
                Issues     = searchResult.Issues,
                IsSuccess  = true
            };

            _logger.LogInformation(
                "JQL query returned {Count} of {Total} matching issues",
                result.Issues.Count, result.Total);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JQL query execution failed: {Jql}", request.Jql);
            return new JqlQueryResult
            {
                Jql          = request.Jql,
                IsSuccess    = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Formats a <see cref="JqlQueryResult"/> as a human-readable text table for console output.
    /// </summary>
    /// <param name="result">The query result to format.</param>
    /// <returns>A multi-line string representation suitable for console or file output.</returns>
    public static string FormatAsText(JqlQueryResult result)
    {
        if (!result.IsSuccess)
            return $"Query failed: {result.ErrorMessage}";

        var sb = new StringBuilder();
        sb.AppendLine($"JQL  : {result.Jql}");
        sb.AppendLine($"Total: {result.Total} matching issues — showing {result.Issues.Count} (page start: {result.StartAt})");
        sb.AppendLine($"Run  : {result.ExecutedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        if (!result.Issues.Any())
        {
            sb.AppendLine("No issues matched the query.");
            return sb.ToString();
        }

        const int keyW   = 15;
        const int typeW  = 12;
        const int statusW = 16;
        const int assignW = 20;
        const int prioW  = 10;
        const int ptsW   = 6;

        sb.AppendLine(
            $"{"Key".PadRight(keyW)} {"Type".PadRight(typeW)} {"Status".PadRight(statusW)} " +
            $"{"Assignee".PadRight(assignW)} {"Priority".PadRight(prioW)} {"Pts".PadRight(ptsW)} Summary");
        sb.AppendLine(new string('─', 105));

        foreach (var issue in result.Issues)
        {
            var key      = issue.Key.PadRight(keyW)[..keyW];
            var type     = issue.IssueType.PadRight(typeW)[..typeW];
            var status   = issue.Status.PadRight(statusW)[..statusW];
            var assignee = (issue.Assignee ?? "Unassigned").PadRight(assignW)[..assignW];
            var priority = issue.Priority.PadRight(prioW)[..prioW];
            var pts      = (issue.StoryPoints?.ToString() ?? "-").PadRight(ptsW)[..ptsW];
            var summary  = issue.Summary.Length > 60 ? issue.Summary[..57] + "..." : issue.Summary;

            sb.AppendLine($"{key} {type} {status} {assignee} {priority} {pts} {summary}");
        }

        if (result.HasMore)
        {
            var remaining = result.Total - result.StartAt - result.Issues.Count;
            sb.AppendLine();
            sb.AppendLine($"  ...{remaining} more result(s). Use --start-at {result.StartAt + result.Issues.Count} to see the next page.");
        }

        return sb.ToString();
    }
}
