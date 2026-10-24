// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// Repository interface for issue data access.
/// Methods return ValueTask because implementations operate on in-memory stores
/// and complete synchronously — ValueTask avoids the Task/state-machine allocation
/// that would be incurred by async Task on every call.
/// </summary>
public interface IIssueRepository
{
    public ValueTask<JiraIssue?> GetByKeyAsync(string issueKey);
    public ValueTask<List<JiraIssue>> GetByProjectAsync(string projectKey);
    public ValueTask<List<JiraIssue>> GetBySprintAsync(int sprintId);
    public ValueTask<List<JiraIssue>> GetOverdueAsync(string projectKey);
    public ValueTask<List<JiraIssue>> GetHighPriorityAsync(string projectKey);
    public ValueTask SaveAsync(JiraIssue issue);
    public ValueTask SaveRangeAsync(List<JiraIssue> issues);
}
