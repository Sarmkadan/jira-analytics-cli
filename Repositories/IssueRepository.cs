// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// In-memory repository for issue data with caching and search capabilities
/// </summary>
public class IssueRepository : IIssueRepository
{
    private readonly ConcurrentDictionary<string, JiraIssue> _issues = new();
    private readonly ILogger<IssueRepository> _logger;

    public IssueRepository(ILogger<IssueRepository> logger)
    {
        _logger = logger;
    }

    public async Task<JiraIssue?> GetByKeyAsync(string issueKey)
    {
        try
        {
            _logger.LogDebug("Retrieving issue {IssueKey}", issueKey);
            return _issues.TryGetValue(issueKey, out var issue) ? issue : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issue {IssueKey}", issueKey);
            return null;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<JiraIssue>> GetByProjectAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Retrieving issues for project {ProjectKey}", projectKey);
            return _issues.Values
                .Where(i => i.ProjectKey == projectKey)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issues for project {ProjectKey}", projectKey);
            return new List<JiraIssue>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<JiraIssue>> GetBySprintAsync(int sprintId)
    {
        try
        {
            _logger.LogDebug("Retrieving issues for sprint {SprintId}", sprintId);
            return _issues.Values
                .Where(i => i.SprintId == sprintId)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issues for sprint {SprintId}", sprintId);
            return new List<JiraIssue>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<JiraIssue>> GetOverdueAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Retrieving overdue issues for project {ProjectKey}", projectKey);
            return _issues.Values
                .Where(i => i.ProjectKey == projectKey && i.IsOverdue())
                .OrderByDescending(i => i.DueDate)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue issues");
            return new List<JiraIssue>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<JiraIssue>> GetHighPriorityAsync(string projectKey)
    {
        try
        {
            _logger.LogDebug("Retrieving high priority issues for project {ProjectKey}", projectKey);
            return _issues.Values
                .Where(i => i.ProjectKey == projectKey && i.IsHighPriority())
                .OrderByDescending(i => i.Priority)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving high priority issues");
            return new List<JiraIssue>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task SaveAsync(JiraIssue issue)
    {
        try
        {
            if (issue == null)
                throw new ArgumentNullException(nameof(issue));

            issue.Validate();
            _issues.AddOrUpdate(issue.Key, issue, (key, existing) => issue);
            _logger.LogDebug("Saved issue {IssueKey}", issue.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving issue");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task SaveRangeAsync(List<JiraIssue> issues)
    {
        try
        {
            if (issues == null || !issues.Any())
                throw new ArgumentException("Issues collection cannot be null or empty");

            foreach (var issue in issues)
            {
                issue.Validate();
                _issues.AddOrUpdate(issue.Key, issue, (key, existing) => issue);
            }

            _logger.LogInformation("Saved {IssueCount} issues to repository", issues.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving issues");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public int GetCount()
    {
        return _issues.Count;
    }

    public void Clear()
    {
        _issues.Clear();
        _logger.LogInformation("Issue repository cleared");
    }
}
