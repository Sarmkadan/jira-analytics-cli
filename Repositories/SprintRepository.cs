// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Repositories;

/// <summary>
/// In-memory repository for sprint data with lifecycle management
/// </summary>
public class SprintRepository : ISprintRepository
{
    private readonly ConcurrentDictionary<int, Sprint> _sprints = new();
    private readonly ILogger<SprintRepository> _logger;

    public SprintRepository(ILogger<SprintRepository> logger)
    {
        this._logger = logger;
    }

    public async Task<Sprint?> GetByIdAsync(int sprintId)
    {
        try
        {
            this._logger.LogDebug("Retrieving sprint {SprintId}", sprintId);
            return this._sprints.TryGetValue(sprintId, out var sprint) ? sprint : null;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving sprint {SprintId}", sprintId);
            return null;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<Sprint>> GetByProjectAsync(string projectKey)
    {
        try
        {
            this._logger.LogDebug("Retrieving sprints for project {ProjectKey}", projectKey);
            return this._sprints.Values
                .Where(s => s.ProjectKey == projectKey)
                .OrderByDescending(s => s.EndDate)
                .ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving sprints for project {ProjectKey}", projectKey);
            return new List<Sprint>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<Sprint>> GetActiveSprints()
    {
        try
        {
            this._logger.LogDebug("Retrieving active sprints");
            return this._sprints.Values
                .Where(s => s.IsActive())
                .ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving active sprints");
            return new List<Sprint>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task<List<Sprint>> GetRecentClosedSprints(int count)
    {
        try
        {
            this._logger.LogDebug("Retrieving {Count} recent closed sprints", count);
            return this._sprints.Values
                .Where(s => s.IsClosed())
                .OrderByDescending(s => s.CompleteDate)
                .Take(count)
                .ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving closed sprints");
            return new List<Sprint>();
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task SaveAsync(Sprint sprint)
    {
        try
        {
            if (sprint == null)
            {
                throw new ArgumentNullException(nameof(sprint));
            }

            sprint.Validate();
            this._sprints.AddOrUpdate(sprint.Id, sprint, (id, existing) => sprint);
            this._logger.LogDebug("Saved sprint {SprintId}: {SprintName}", sprint.Id, sprint.Name);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error saving sprint");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public async Task SaveRangeAsync(List<Sprint> sprints)
    {
        try
        {
            if (sprints == null || !sprints.Any())
            {
                throw new ArgumentException("Sprints collection cannot be null or empty");
            }

            foreach (var sprint in sprints)
            {
                sprint.Validate();
                this._sprints.AddOrUpdate(sprint.Id, sprint, (id, existing) => sprint);
            }

            this._logger.LogInformation("Saved {SprintCount} sprints to repository", sprints.Count);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error saving sprints");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    public int GetCount()
    {
        return this._sprints.Count;
    }

    public void Clear()
    {
        this._sprints.Clear();
        this._logger.LogInformation("Sprint repository cleared");
    }
}
