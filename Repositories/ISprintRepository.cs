// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// Repository interface for Jira sprint data access. Provides CRUD operations
/// for sprint entities with filtering by project, status, and recency.
/// </summary>
public interface ISprintRepository
{
    /// <summary>
    /// Retrieves a sprint by its Jira sprint ID.
    /// </summary>
    /// <param name="sprintId">The Jira sprint identifier.</param>
    /// <returns>The matching <see cref="Sprint"/>, or <c>null</c> if not found.</returns>
    Task<Sprint?> GetByIdAsync(int sprintId);

    /// <summary>
    /// Returns all sprints belonging to the specified Jira project, ordered by start date.
    /// </summary>
    /// <param name="projectKey">The Jira project key (e.g., "PROJ").</param>
    /// <returns>A list of sprints for the project.</returns>
    Task<List<Sprint>> GetByProjectAsync(string projectKey);

    /// <summary>
    /// Returns all sprints currently in the "active" state across all projects.
    /// </summary>
    /// <returns>A list of active sprints.</returns>
    Task<List<Sprint>> GetActiveSprints();

    /// <summary>
    /// Returns the most recently closed sprints across all projects.
    /// </summary>
    /// <param name="count">Maximum number of closed sprints to return.</param>
    /// <returns>A list of closed sprints ordered by completion date descending.</returns>
    Task<List<Sprint>> GetRecentClosedSprints(int count);

    /// <summary>
    /// Persists a single sprint entity (insert or update).
    /// </summary>
    /// <param name="sprint">The sprint to save.</param>
    Task SaveAsync(Sprint sprint);

    /// <summary>
    /// Persists multiple sprint entities in a single batch operation.
    /// </summary>
    /// <param name="sprints">The list of sprints to save.</param>
    Task SaveRangeAsync(List<Sprint> sprints);
}
