# Jira Analytics CLI

A powerful command-line tool for advanced Jira analytics, sprint metrics, team performance analysis, and professional burndown chart generation.

## Features

- **Sprint Analytics**: Analyze sprint velocity, completion rates, and historical trends
- **Team Metrics**: Track developer productivity, workload distribution, and performance rankings
- **Burndown Charts**: Generate detailed burndown visualizations using SkiaSharp
- **Quality Metrics**: Monitor defect rates, quality scores, and high-risk areas
- **Overdue Tracking**: Identify and report on overdue issues and blockers
- **Multi-Format Export**: Export analytics as PNG, PDF, JSON, CSV, or formatted text
- **Real-time Integration**: Direct Jira API integration with Atlassian Cloud and Server

## Project Structure

```
jira-analytics-cli/
├── Program.cs                      # CLI entry point and command definitions
├── Models/                         # Domain models
│   ├── JiraIssue.cs              # Issue representation with metrics
│   ├── Sprint.cs                 # Sprint with analytics capabilities
│   ├── Developer.cs              # Team member and productivity tracking
│   ├── SprintMetric.cs           # Aggregated sprint metrics
│   ├── BurndownSnapshot.cs       # Point-in-time burndown data
│   └── JiraProject.cs            # Project with team and sprint data
├── Services/                      # Business logic layer
│   ├── IJiraApiService.cs        # Jira API interface
│   ├── JiraApiService.cs         # HTTP client for Jira REST API
│   ├── IAnalyticsService.cs      # Analytics calculation interface
│   ├── AnalyticsService.cs       # Metrics and trend calculations
│   ├── IReportService.cs         # Report generation interface
│   ├── ReportService.cs          # Text and HTML report generation
│   ├── IExportService.cs         # Export interface
│   └── ExportService.cs          # PNG/PDF/JSON/CSV export with SkiaSharp
├── Repositories/                 # Data access layer
│   ├── IIssueRepository.cs       # Issue data interface
│   ├── IssueRepository.cs        # In-memory issue cache
│   ├── ISprintRepository.cs      # Sprint data interface
│   ├── SprintRepository.cs       # In-memory sprint cache
│   ├── IMetricsRepository.cs     # Metrics data interface
│   └── MetricsRepository.cs      # Historical metrics storage
├── Configuration/                # App configuration
│   ├── ICliConfig.cs             # Configuration interface
│   ├── CliConfig.cs              # Configuration implementation
│   └── AppConfigurationProvider.cs # Loads from JSON and env vars
├── Exceptions/                   # Custom exceptions
│   ├── JiraApiException.cs       # API communication errors
│   └── ConfigurationException.cs # Configuration errors
├── Constants/                    # Constants and enums
│   └── Constants.cs              # App-wide constants and enums
├── Utils/                        # Utility extensions
│   ├── DateTimeExtensions.cs     # Date/time helpers
│   ├── ValidationHelpers.cs      # Data validation utilities
│   └── FormattingHelpers.cs      # Output formatting utilities
├── JiraAnalyticsCli.csproj       # Project file (.NET 10)
├── appsettings.json              # Configuration template
├── LICENSE                       # MIT License
└── .gitignore                    # Git ignore patterns
```

## Architecture

### Layered Architecture
- **Presentation Layer**: Program.cs with System.CommandLine CLI
- **Service Layer**: Business logic for analytics and calculations
- **Repository Layer**: Data access and caching
- **Model Layer**: Domain entities with validation
- **Utility Layer**: Helpers for dates, validation, and formatting

### Key Design Patterns
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Repository Pattern**: Abstracted data access with in-memory caching
- **Strategy Pattern**: Multiple export formats handled polymorphically
- **Factory Pattern**: Service instantiation via DI container
- **Async/Await**: Non-blocking API calls and I/O operations

## Installation

### Prerequisites
- .NET 10 SDK or later
- A valid Jira instance (Cloud or Server)
- Jira API token for authentication

### Build from Source

```bash
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli
dotnet build -c Release
dotnet publish -c Release -o ./dist
```

## Configuration

### Environment Variables

```bash
export JIRA_BASE_URL="https://your-jira-instance.atlassian.net"
export JIRA_API_TOKEN="your-api-token-here"
export JIRA_DEFAULT_PROJECT="PROJ"
export CACHE_EXPIRATION_MINUTES=15
export DETAILED_LOGGING=false
export DEFAULT_SPRINT_COUNT=5
export EXPORT_FORMAT=txt
```

### Configuration File (appsettings.json)

```json
{
  "jiraBaseUrl": "https://your-jira-instance.atlassian.net",
  "jiraApiToken": "your-api-token",
  "defaultProject": "PROJ",
  "cacheExpirationMinutes": 15,
  "enableDetailedLogging": false,
  "defaultSprintCount": 5,
  "exportFormat": "txt"
}
```

## Usage

### Analytics Command

Analyze sprint metrics and team performance:

```bash
jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt
```

Options:
- `-p, --project`: Jira project key (required)
- `-s, --sprints`: Number of recent sprints to analyze (default: 5)
- `-o, --output`: Output file path (optional, prints to console if not specified)

### Export Command

Export analytics data in various formats:

```bash
jira-analytics-cli export -p MYPROJECT -f png -o velocity-chart.png
jira-analytics-cli export -p MYPROJECT -f json -o analytics.json
jira-analytics-cli export -p MYPROJECT -f csv -o metrics.csv
```

Options:
- `-p, --project`: Jira project key (required)
- `-f, --format`: Export format: png, jpg, pdf, json, csv (required)
- `-o, --output`: Output file path (required)

### Burndown Command

Generate burndown chart for a specific sprint:

```bash
jira-analytics-cli burndown -p MYPROJECT --sprint-id 123 -o burndown.png
```

Options:
- `-p, --project`: Jira project key (required)
- `--sprint-id`: Jira sprint ID (required)
- `-o, --output`: Output image path (required)

## Model Details

### JiraIssue
- Issue key, ID, summary, description
- Status, type, priority, assignee
- Story points, due date, creation/resolution dates
- Methods: `IsOverdue()`, `IsHighPriority()`, `GetCycleTime()`, `Validate()`

### Sprint
- ID, name, state (Open/Active/Closed)
- Start/end/complete dates, goal
- Collection of issues
- Methods: `GetVelocity()`, `GetCompletedStoryPoints()`, `GetOverdueIssues()`

### Developer
- Key, name, email, join date
- Assigned issues and metrics
- Methods: `GetProductivity()`, `GetCompletionRate()`, `GetAverageCycleTime()`

### SprintMetric
- Planned/completed story points, issue counts
- Quality score, defect rate, risk score
- Methods: `GetVelocity()`, `GetHealthStatus()`, `GetQualityScore()`

### BurndownSnapshot
- Timestamp, remaining/completed story points
- Issue counts and scope changes
- Methods: `GetBurndownPercentage()`, `IsOnTrack()`, `GetProjectedCompletion()`

## Services

### JiraApiService
- Fetches projects, sprints, issues, team members
- Handles Jira REST API v3 communication
- Connection verification with exponential backoff retry

### AnalyticsService
- Analyzes sprint velocity and trends
- Calculates team metrics and performance
- Quality analysis with defect tracking
- Overdue issue detection

### ReportService
- Generates formatted text reports
- Creates HTML reports with styling
- Produces burndown charts using SkiaSharp
- Summary report generation

### ExportService
- PNG/JPG image export with SkiaSharp
- JSON serialization with Newtonsoft.Json
- CSV export with proper escaping
- Burndown chart visualization

## Repository Layer

### IssueRepository
- In-memory caching of issues
- Query by key, project, sprint
- Overdue and high-priority filters
- Batch operations for bulk saves

### SprintRepository
- Sprint lifecycle management (Open/Active/Closed)
- Recent sprint queries
- Active sprint detection
- Historical sprint retrieval

### MetricsRepository
- Historical metrics storage
- Burndown snapshot tracking
- Project and sprint-specific metrics
- Trend analysis data

## Technologies

- **.NET 10**: Latest long-term support framework
- **System.CommandLine 2.0**: Modern CLI argument parsing
- **SkiaSharp 2.88**: High-performance graphics for charts
- **Newtonsoft.Json 13.0**: JSON serialization
- **Microsoft.Extensions**: Logging and dependency injection
- **C# 13**: Latest language features (nullable reference types, records)

## Code Statistics

- **Files**: 32
- **Lines of Code**: 3,880+
- **Classes**: 25+
- **Interfaces**: 8
- **Test Coverage**: Foundation for integration tests

## Error Handling

- **JiraApiException**: API communication failures with HTTP status codes
- **ConfigurationException**: Invalid or missing configuration
- **Validation**: Domain model validation with detailed error messages
- **Logging**: Structured logging with Microsoft.Extensions.Logging

## Performance Considerations

- In-memory caching with configurable expiration
- Concurrent dictionary for thread-safe repositories
- Async/await throughout for non-blocking I/O
- Lazy-loaded relationships on demand
- HTTP client connection pooling

## Security

- API token stored in environment variables, not committed
- HTTPS enforced for Jira API communication
- Input validation on all public methods
- CSV sanitization to prevent injection
- No sensitive data in logs by default

## Future Enhancements (Phase 2+)

- Database persistence (SQL Server, PostgreSQL)
- Real-time webhook integration
- Historical trends and forecasting
- Custom metric definitions
- Web API and dashboard
- Multi-team support
- Advanced filtering and search
- Jira Cloud and Server support

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

## Author

Vladyslav Zaiets
- Website: https://sarmkadan.com
- Email: rutova2@gmail.com

## Contributing

This is a solo open-source project. Contributions via pull requests are welcome for:
- Bug fixes
- Performance improvements
- Documentation enhancements
- Additional export formats

---

**Note**: This project is in active development. Phase 1 focuses on core architecture and APIs.
