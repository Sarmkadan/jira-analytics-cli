# Architecture Guide

This document describes the high-level architecture, design patterns, and key decisions in Jira Analytics CLI.

## System Architecture Overview

```
┌──────────────────────────────────────────────────────┐
│  User / External System                              │
│  (CLI, CI/CD, Scripts)                               │
└─────────────────┬──────────────────────────────────┘
                  │
┌─────────────────▼──────────────────────────────────┐
│  PRESENTATION LAYER                                  │
│  Program.cs (CLI Entry Point)                        │
│  System.CommandLine (Command Parsing)                │
│  ConsoleInterface.cs (Output Formatting)             │
└─────────────────┬──────────────────────────────────┘
                  │
┌─────────────────▼──────────────────────────────────┐
│  SERVICE LAYER (Business Logic)                      │
│  ┌────────────────────────────────────────────────┐│
│  │ JiraApiService    - Jira API v3 Communication  ││
│  │ AnalyticsService  - Metrics Calculation        ││
│  │ ReportService     - Report Generation          ││
│  │ ExportService     - Format Export              ││
│  └────────────────────────────────────────────────┘│
└─────────────────┬──────────────────────────────────┘
                  │
┌─────────────────▼──────────────────────────────────┐
│  REPOSITORY LAYER (Data Access)                      │
│  ┌────────────────────────────────────────────────┐│
│  │ IssueRepository   - Issue Caching & Queries    ││
│  │ SprintRepository  - Sprint Lifecycle           ││
│  │ MetricsRepository - Historical Metrics         ││
│  └────────────────────────────────────────────────┘│
│  In-Memory Cache with Configurable Expiration       │
└─────────────────┬──────────────────────────────────┘
                  │
┌─────────────────▼──────────────────────────────────┐
│  EXTERNAL INTEGRATION                                │
│  ┌────────────────────────────────────────────────┐│
│  │ Jira REST API v3                               ││
│  │ HTTPS / Bearer Token Authentication             ││
│  └────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────┘
```

## Design Patterns Used

### 1. Repository Pattern

**Purpose**: Abstract data access logic from business logic.

**Implementation**:
```
IIssueRepository (Interface)
├── IssueRepository (In-Memory Implementation)
│   ├── In-memory dictionary cache
│   ├── Thread-safe ConcurrentDictionary
│   └── Configurable expiration policy
```

**Benefits**:
- Easy to swap implementations (e.g., add database)
- Testable with mock repositories
- Centralized caching logic
- Query abstraction

### 2. Dependency Injection Pattern

**Purpose**: Loose coupling and testability.

**Container Setup** (Program.cs):
```csharp
services.AddSingleton<IJiraApiService, JiraApiService>();
services.AddSingleton<IAnalyticsService, AnalyticsService>();
services.AddHttpClient("jira")
    .ConfigureHttpClient(/* ... */);
```

**Benefits**:
- Easy to mock dependencies for testing
- Singleton lifetime for stateless services
- Automatic cleanup with IDisposable
- Configuration centralized in Program.cs

### 3. Strategy Pattern

**Purpose**: Support multiple export formats polymorphically.

**Usage**:
```csharp
// ExportService delegates to format-specific handlers
switch (format)
{
    case "json":
        await ExportAsJson(analysis, outputPath);
        break;
    case "csv":
        await ExportAsCsv(analysis, outputPath);
        break;
    case "png":
        await ExportAsPng(analysis, outputPath);
        break;
}
```

**Benefits**:
- Easy to add new formats without changing core logic
- Each format handles its own validation
- Clear separation of concerns

### 4. Async/Await Pattern

**Purpose**: Non-blocking I/O operations.

**Pattern**:
```csharp
public async Task<List<Sprint>> GetSprintsAsync(string projectKey)
{
    // Non-blocking HTTP call
    var response = await httpClient.GetAsync($"/projects/{projectKey}/sprints");
    // Non-blocking parsing
    var sprints = await ParseSprintsAsync(response);
    return sprints;
}
```

**Benefits**:
- Efficient resource utilization
- Better responsiveness
- No thread starvation

### 5. Observer/Event Bus Pattern

**Purpose**: Decouple components for real-time updates.

**Foundation**:
- `DomainEvent.cs` - Base event class
- `EventBus.cs` - Publish/subscribe implementation
- `EventSubscriber.cs` - Handler interface

**Use Cases**:
- Metric sync notifications
- Report generation completion
- Error notifications

## Layered Architecture Details

### Presentation Layer

**Files**:
- `Program.cs` - CLI entry point
- `Cli/CommandParser.cs` - Command parsing logic
- `Cli/ConsoleInterface.cs` - Console output formatting
- `Middleware/PipelineBuilder.cs` - Request pipeline

**Responsibilities**:
- Parse user input
- Build command tree
- Handle console output
- Orchestrate layers

### Service Layer

**Core Services**:

#### JiraApiService
```csharp
public interface IJiraApiService
{
    Task<JiraProject> GetProjectAsync(string projectKey);
    Task<List<Sprint>> GetSprintsAsync(string projectKey);
    Task<List<JiraIssue>> GetIssuesAsync(string jql);
    Task VerifyConnectionAsync();
}
```

**Features**:
- Jira REST API v3 integration
- Automatic retry with exponential backoff
- Rate limiting awareness
- Bearer token authentication

#### AnalyticsService
```csharp
public interface IAnalyticsService
{
    Task<ProjectAnalysis> AnalyzeSprints(string projectKey, int count);
    Task<SprintMetric> AnalyzeSprint(Sprint sprint);
    List<SprintMetric> CalculateTrends(List<SprintMetric> metrics);
    List<Developer> RankDevelopers(List<JiraIssue> issues);
}
```

**Metrics Calculated**:
- Velocity (completed story points)
- Completion rate (% of issues completed)
- Cycle time (creation to resolution)
- Quality score (based on defects)
- Risk score (overdue items, blockers)
- Developer productivity rankings

#### ReportService
```csharp
public interface IReportService
{
    string GenerateReport(ProjectAnalysis analysis);
    string GenerateHtmlReport(ProjectAnalysis analysis);
    Task GenerateBurndownChart(string projectKey, int sprintId, string path);
}
```

**Supported Formats**:
- Plain text with ASCII tables
- HTML with styling
- SkiaSharp-based charts

#### ExportService
```csharp
public interface IExportService
{
    Task ExportAnalytics(string projectKey, string format, string output);
}
```

**Formats**:
- JSON (full data structure)
- CSV (spreadsheet-friendly)
- PNG/JPEG (via SkiaSharp)
- PDF (via SkiaSharp)

### Repository Layer

**In-Memory Caching Strategy**:

```csharp
public class CachePolicy
{
    public TimeSpan ExpirationTime { get; set; }
    public int MaxItems { get; set; }
}

public class InMemoryCache<TKey, TValue>
{
    private readonly ConcurrentDictionary<TKey, (TValue data, DateTime expiry)> _cache;
    
    public bool TryGet(TKey key, out TValue value)
    {
        // Check expiration before returning
        if (_cache.TryGetValue(key, out var item))
        {
            if (DateTime.UtcNow < item.expiry)
            {
                value = item.data;
                return true;
            }
            _cache.TryRemove(key, out _);
        }
        value = default;
        return false;
    }
}
```

**Repository Implementations**:

1. **IssueRepository**
   - Caches issues by key
   - Query by project, sprint, status
   - Filter overdue, high-priority
   - Batch save operations

2. **SprintRepository**
   - Track sprint lifecycle
   - Recent sprint queries
   - Active sprint detection
   - Historical retrieval

3. **MetricsRepository**
   - Store historical metrics
   - Burndown snapshots
   - Trend analysis data
   - Time-series queries

## Configuration Management

**Configuration Sources** (in priority order):

1. Environment variables (`JIRA_BASE_URL`, etc.)
2. `appsettings.json` in application directory
3. `~/.jira-analytics/appsettings.json` (home directory)
4. Hardcoded defaults

**AppConfigurationProvider**:
```csharp
public class AppConfigurationProvider : IConfigurationProvider
{
    public CliConfig LoadConfiguration()
    {
        // 1. Load from environment
        // 2. Override with file config
        // 3. Apply defaults
        // 4. Validate configuration
    }
}
```

## Error Handling Strategy

### Exception Hierarchy

```
Exception
├── JiraApiException
│   ├── HTTP 401 (Authentication)
│   ├── HTTP 404 (Not Found)
│   ├── HTTP 429 (Rate Limited)
│   └── HTTP 500+ (Server Error)
├── ConfigurationException
│   ├── Missing required config
│   ├── Invalid token
│   └── Unreachable Jira instance
└── ValidationException
    ├── Invalid input
    ├── Missing data
    └── Constraint violations
```

### Error Handling Middleware

```csharp
public class ErrorHandlingMiddleware
{
    public async Task Execute(PipelineContext context)
    {
        try
        {
            await next();
        }
        catch (JiraApiException ex) when (ex.StatusCode == 401)
        {
            logger.LogError("Authentication failed - check API token");
            throw;
        }
        catch (JiraApiException ex) when (ex.StatusCode == 429)
        {
            logger.LogWarning("Rate limited - retry after {Seconds}s", ex.RetryAfter);
            throw;
        }
    }
}
```

## Performance Considerations

### Caching Strategy

**Default Settings**:
- Cache expiration: 15 minutes
- Max items in cache: 1000
- Cache keys: Issue key, Sprint ID, Project key

**Trade-offs**:
- Shorter expiration = more API calls
- Longer expiration = stale data risk

### Concurrency

**Thread Safety**:
- `ConcurrentDictionary` for repositories
- `HttpClient` reused (thread-safe)
- No shared mutable state in services

**Async Operations**:
- All I/O uses async/await
- No blocking calls (`.Result`, `.Wait()`)
- Task composition for parallel operations

### API Call Optimization

**Strategies**:
1. Batch issue queries with JQL
2. Cache sprint list per project
3. Lazy-load developer details
4. Pagination for large result sets

**Rate Limiting**:
- Jira Cloud: 30,000 requests per 10 minutes
- Exponential backoff on 429 (Too Many Requests)
- Request throttling middleware available

## Middleware Pipeline

**Pipeline Composition**:

```csharp
var pipeline = new PipelineBuilder()
    .Use<ErrorHandlingMiddleware>()
    .Use<RequestLoggingMiddleware>()
    .Use<RateLimitMiddleware>()
    .Use<CacheMiddleware>()
    .Build();
```

**Execution Flow**:
1. Error handling (catches all exceptions)
2. Request logging (logs all operations)
3. Rate limiting (throttles requests)
4. Cache checking (returns cached data if available)
5. Handler execution

## Extensibility Points

### Adding a New Export Format

1. Create formatter class:
```csharp
public class XmlFormatter : IFormatter
{
    public async Task ExportAsync(ProjectAnalysis analysis, string path)
    {
        // XML serialization logic
    }
}
```

2. Register in ExportService:
```csharp
case "xml":
    await new XmlFormatter().ExportAsync(analysis, path);
    break;
```

### Adding a New Service

1. Create interface:
```csharp
public interface INewService
{
    Task<T> GetDataAsync();
}
```

2. Create implementation with DI:
```csharp
public class NewService : INewService
{
    private readonly ILogger<NewService> _logger;
    public NewService(ILogger<NewService> logger) => _logger = logger;
}
```

3. Register in DI:
```csharp
services.AddSingleton<INewService, NewService>();
```

## Testing Strategy

**Unit Testing**:
- Mock repositories and services
- Test calculations in isolation
- Validate error handling

**Integration Testing**:
- Test with Jira sandbox instance
- Verify API integration
- Cache behavior under load

**Example Mock**:
```csharp
public class MockJiraApiService : IJiraApiService
{
    public Task<JiraProject> GetProjectAsync(string projectKey)
    {
        return Task.FromResult(new JiraProject { Key = projectKey });
    }
}
```

## Technology Stack Rationale

| Component | Choice | Why |
|-----------|--------|-----|
| Framework | .NET 10 | Latest LTS, excellent performance |
| CLI | System.CommandLine 2.0 | Modern, no external dependencies |
| HTTP | HttpClient | Built-in, pooled connections |
| Logging | Microsoft.Extensions.Logging | Standard .NET logging |
| DI | Microsoft.Extensions.DependencyInjection | Standard, lightweight |
| Graphics | SkiaSharp | Cross-platform, high-performance |
| JSON | System.Text.Json | Built-in, high-performance |

## Future Architecture Improvements

### Phase 2 (Planned)
- Database persistence (Entity Framework Core)
- Background task processing (Hangfire)
- Real-time webhooks
- Multi-team support

### Phase 3+ (Considered)
- Web API (ASP.NET Core)
- GraphQL support
- Custom metric definitions
- Machine learning predictions

---

For more details on specific components, see:
- [API Reference](./api-reference.md)
- [Getting Started](./getting-started.md)
- [Deployment Guide](./deployment.md)
