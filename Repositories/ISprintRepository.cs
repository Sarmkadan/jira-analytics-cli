// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// Repository interface for sprint data access
/// </summary>
public interface ISprintRepository
{
    Task<Sprint?> GetByIdAsync(int sprintId);
    Task<List<Sprint>> GetByProjectAsync(string projectKey);
    Task<List<Sprint>> GetActiveSprints();
    Task<List<Sprint>> GetRecentClosedSprints(int count);
    Task SaveAsync(Sprint sprint);
    Task SaveRangeAsync(List<Sprint> sprints);
}
