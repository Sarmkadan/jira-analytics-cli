// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
}
