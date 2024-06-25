// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// In-memory repository for issue data with caching and search capabilities.
/// All operations complete synchronously (ConcurrentDictionary), so ValueTask
/// is used throughout to eliminate the Task object and async state-machine
/// allocations that would otherwise occur on every call.
/// </summary>
public class IssueRepository : IIssueRepository
{
    private readonly ConcurrentDictionary<string, JiraIssue> _issues = new();
    private readonly ILogger<IssueRepository> _logger;

    public IssueRepository(ILogger<IssueRepository> logger)
    {
        _logger = logger;
    }

    public ValueTask<JiraIssue?> GetByKeyAsync(string issueKey)
    {
        try
        {
            _logger.LogDebug("Retrieving issue {IssueKey}", issueKey);
            _issues.TryGetValue(issueKey, out var issue);
            return ValueTask.FromResult<JiraIssue?>(issue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issue {IssueKey}", issueKey);
            return ValueTask.FromResult<JiraIssue?>(null);
        }
    }

    public ValueTask<List<JiraIssue>> GetByProjectAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Retrieving issues for project {ProjectKey}", projectKey);
            var result = _issues.Values
                .Where(i => i.ProjectKey == projectKey)
                .ToList();
            return ValueTask.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issues for project {ProjectKey}", projectKey);
            return ValueTask.FromResult(new List<JiraIssue>());
        }
    }

    public ValueTask<List<JiraIssue>> GetBySprintAsync(int sprintId)
    {
        try
        {
            _logger.LogDebug("Retrieving issues for sprint {SprintId}", sprintId);
            var result = _issues.Values
                .Where(i => i.SprintId == sprintId)
                .ToList();
            return ValueTask.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issues for sprint {SprintId}", sprintId);
            return ValueTask.FromResult(new List<JiraIssue>());
        }
    }

    public ValueTask<List<JiraIssue>> GetOverdueAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Retrieving overdue issues for project {ProjectKey}", projectKey);
            var result = _issues.Values
                .Where(i => i.ProjectKey == projectKey && i.IsOverdue())
                .OrderByDescending(i => i.DueDate)
                .ToList();
            return ValueTask.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue issues");
            return ValueTask.FromResult(new List<JiraIssue>());
        }
    }

    public ValueTask<List<JiraIssue>> GetHighPriorityAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Retrieving high priority issues for project {ProjectKey}", projectKey);
            var result = _issues.Values
                .Where(i => i.ProjectKey == projectKey && i.IsHighPriority())
                .OrderByDescending(i => i.Priority)
                .ToList();
            return ValueTask.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving high priority issues");
            return ValueTask.FromResult(new List<JiraIssue>());
        }
    }

    public ValueTask SaveAsync(JiraIssue issue)
    {
        try
        {
            if (issue == null)
                throw new ArgumentNullException(nameof(issue));

            issue.Validate();
            _issues.AddOrUpdate(issue.Key, issue, (_, _) => issue);
            _logger.LogDebug("Saved issue {IssueKey}", issue.Key);
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving issue");
            throw;
        }
    }

    public ValueTask SaveRangeAsync(List<JiraIssue> issues)
    {
        try
        {
            if (issues == null || !issues.Any())
                throw new ArgumentException("Issues collection cannot be null or empty");

            foreach (var issue in issues)
            {
                issue.Validate();
                _issues.AddOrUpdate(issue.Key, issue, (_, _) => issue);
            }

            _logger.LogInformation("Saved {IssueCount} issues to repository", issues.Count);
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving issues");
            throw;
        }
    }

    public int GetCount() => _issues.Count;

    public void Clear()
    {
        _issues.Clear();
        _logger.LogInformation("Issue repository cleared");
    }
}
