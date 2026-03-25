// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// Repository interface for issue data access
/// </summary>
public interface IIssueRepository
{
    Task<JiraIssue?> GetByKeyAsync(string issueKey);
    Task<List<JiraIssue>> GetByProjectAsync(string projectKey);
    Task<List<JiraIssue>> GetBySprintAsync(int sprintId);
    Task<List<JiraIssue>> GetOverdueAsync(string projectKey);
    Task<List<JiraIssue>> GetHighPriorityAsync(string projectKey);
    Task SaveAsync(JiraIssue issue);
    Task SaveRangeAsync(List<JiraIssue> issues);
}
