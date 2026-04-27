# Custom Metrics Plugin Guide

This guide shows how to extend Jira Analytics CLI with your own metrics by implementing the built-in extension points.

## Overview

The CLI exposes three interfaces you can implement to add custom logic without modifying core code:

| Interface | Purpose |
|-----------|---------|
| `IAnalyticsService` | Add custom sprint / project analysis methods |
| `IMetricsRepository` | Persist and query your own metric data |
| `IJiraApiService` | Wrap or replace the default Jira REST client |

---

## Quick Start: Custom Sprint Metric

### 1. Create the custom service

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;

public class CycleTimeMetricService
{
    private readonly IJiraApiService _jira;
    private readonly ILogger<CycleTimeMetricService> _logger;

    public CycleTimeMetricService(IJiraApiService jira,
        ILogger<CycleTimeMetricService> logger)
    {
        _jira = jira;
        _logger = logger;
    }

    /// <summary>
    /// Returns average cycle time (days from creation to resolution)
    /// for all issues in the given sprint.
    /// </summary>
    public async Task<double> GetAverageCycleTimeDaysAsync(int sprintId)
    {
        var issues = await _jira.GetSprintIssuesAsync(sprintId);

        var resolved = issues
            .Where(i => i.ResolutionDate.HasValue)
            .Select(i => (i.ResolutionDate!.Value - i.CreatedDate).TotalDays)
            .ToList();

        if (resolved.Count == 0)
        {
            _logger.LogWarning("Sprint {SprintId} has no resolved issues; cycle time is 0", sprintId);
            return 0;
        }

        var avg = resolved.Average();
        _logger.LogInformation("Sprint {SprintId} average cycle time: {Days:F1} days", sprintId, avg);
        return avg;
    }
}
```

### 2. Register it with the DI container

In `Program.cs`, inside `ConfigureServices`:

```csharp
services.AddSingleton<CycleTimeMetricService>();
```

### 3. Use it in a command handler

```csharp
var cycleTimeCmd = new Command("cycle-time", "Show average cycle time for a sprint");
cycleTimeCmd.AddOption(new Option<int>("--sprint-id", "Sprint ID") { IsRequired = true });

cycleTimeCmd.SetHandler(async (sprintId) =>
{
    var svc = serviceProvider.GetRequiredService<CycleTimeMetricService>();
    var days = await svc.GetAverageCycleTimeDaysAsync(sprintId);
    Console.WriteLine($"Average cycle time: {days:F1} days");
}, new Option<int>("--sprint-id"));

rootCommand.AddCommand(cycleTimeCmd);
```

---

## Configuration: Enabling / Disabling Custom Metrics

Add a section to `appsettings.json`:

```json
{
  "CustomMetrics": {
    "CycleTime": {
      "Enabled": true,
      "ExcludeSubTasks": true,
      "StatusStart": "In Progress",
      "StatusEnd": "Done"
    },
    "ThroughputWindow": {
      "Enabled": false,
      "RollingDays": 14
    }
  }
}
```

Bind the section in `ConfigureServices`:

```csharp
services.Configure<CustomMetricsOptions>(
    configuration.GetSection("CustomMetrics"));
```

Where `CustomMetricsOptions` is:

```csharp
public class CustomMetricsOptions
{
    public CycleTimeOptions CycleTime { get; set; } = new();
    public ThroughputWindowOptions ThroughputWindow { get; set; } = new();
}

public class CycleTimeOptions
{
    public bool Enabled { get; set; } = true;
    public bool ExcludeSubTasks { get; set; } = false;
    public string StatusStart { get; set; } = "In Progress";
    public string StatusEnd { get; set; } = "Done";
}

public class ThroughputWindowOptions
{
    public bool Enabled { get; set; } = true;
    public int RollingDays { get; set; } = 14;
}
```

---

## Implementing a Custom `IMetricsRepository`

If you want to store computed metrics in an external database instead of memory:

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;

public class SqliteMetricsRepository : IMetricsRepository
{
    private readonly string _connectionString;

    public SqliteMetricsRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task SaveMetricAsync(SprintMetric metric)
    {
        // INSERT INTO sprint_metrics (sprint_id, sprint_name, ...) VALUES (@id, @name, ...)
        await Task.CompletedTask; // replace with actual DB call
    }

    public async Task<List<SprintMetric>> GetMetricsAsync(string projectKey)
    {
        // SELECT * FROM sprint_metrics WHERE project_key = @projectKey
        return await Task.FromResult(new List<SprintMetric>());
    }

    public async Task<SprintMetric?> GetLatestMetricAsync(string projectKey)
    {
        var all = await GetMetricsAsync(projectKey);
        return all.OrderByDescending(m => m.EndDate).FirstOrDefault();
    }
}
```

Register it instead of the default:

```csharp
// Remove the default registration and add your own
services.AddSingleton<IMetricsRepository>(_ =>
    new SqliteMetricsRepository("Data Source=metrics.db"));
```

---

## Implementing a Custom `IJiraApiService`

To add caching, audit logging, or support an alternative issue tracker:

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;

public class CachedJiraApiService : IJiraApiService
{
    private readonly IJiraApiService _inner;
    private readonly Dictionary<string, object> _cache = new();

    public CachedJiraApiService(IJiraApiService inner)
        => _inner = inner;

    public async Task<JiraProject?> GetProjectAsync(string projectKey)
    {
        if (_cache.TryGetValue($"project:{projectKey}", out var cached))
            return (JiraProject?)cached;

        var project = await _inner.GetProjectAsync(projectKey);
        if (project != null)
            _cache[$"project:{projectKey}"] = project;

        return project;
    }

    // Delegate remaining methods to the inner service
    public Task<List<Sprint>> GetProjectSprintsAsync(string projectKey) =>
        _inner.GetProjectSprintsAsync(projectKey);

    public Task<Sprint?> GetSprintAsync(int sprintId) =>
        _inner.GetSprintAsync(sprintId);

    public Task<List<JiraIssue>> GetSprintIssuesAsync(int sprintId) =>
        _inner.GetSprintIssuesAsync(sprintId);

    public Task<List<JiraIssue>> GetProjectIssuesAsync(string projectKey) =>
        _inner.GetProjectIssuesAsync(projectKey);

    public Task<List<Developer>> GetProjectTeamAsync(string projectKey) =>
        _inner.GetProjectTeamAsync(projectKey);

    public Task<JiraIssue?> GetIssueAsync(string issueKey) =>
        _inner.GetIssueAsync(issueKey);

    public Task<List<BurndownSnapshot>> GetBurndownDataAsync(int sprintId) =>
        _inner.GetBurndownDataAsync(sprintId);

    public Task<bool> VerifyConnectionAsync() =>
        _inner.VerifyConnectionAsync();
}
```

Register with the decorator pattern:

```csharp
services.AddSingleton<JiraApiService>(); // concrete type
services.AddSingleton<IJiraApiService>(sp =>
    new CachedJiraApiService(sp.GetRequiredService<JiraApiService>()));
```

---

## Best Practices

- **Keep metrics stateless** — store computed results in `IMetricsRepository`, not in memory inside the service.
- **Validate inputs early** — use `ArgumentNullException.ThrowIfNullOrWhiteSpace` for project keys and `ArgumentOutOfRangeException.ThrowIfNegativeOrZero` for sprint IDs.
- **Log with structured data** — use `_logger.LogInformation("Sprint {SprintId} metric: {Value}", sprintId, value)` so logs are queryable.
- **Use async consistently** — all custom methods should be `async Task<T>` to avoid blocking the CLI thread.
- **Write tests** — mock `IJiraApiService` with Moq and test edge cases (empty results, API errors).

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Custom service never called | DI registration order | Register before `BuildServiceProvider()` |
| `NullReferenceException` in metric | `GetSprintIssuesAsync` returned empty | Add a null/empty guard before computing |
| Metric always returns 0 | `ResolutionDate` not set on issues | Confirm Jira field `resolutiondate` is mapped |
| Config section not binding | Section name mismatch | Ensure `appsettings.json` key matches `GetSection("CustomMetrics")` |
