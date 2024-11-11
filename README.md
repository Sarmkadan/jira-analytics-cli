# jira-analytics-cli

![Build](https://github.com/sarmkadan/jira-analytics-cli/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/jira-analytics-cli)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)

.NET 9 CLI for Jira sprint analytics. Pulls data via Jira REST API v3 and outputs
velocity reports, burndown charts, HTML dashboards, and team comparison tables.

## Requirements

- .NET 9 SDK
- Jira Cloud or Server instance with API access
- API token from [id.atlassian.com](https://id.atlassian.com/manage-profile/security/api-tokens)

## Install

```bash
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli
dotnet publish -c Release -o ./dist --self-contained -p:PublishSingleFile=true -r linux-x64
```

## Configuration

Set environment variables (override `appsettings.json`):

```bash
export JIRA_BASE_URL="https://your-instance.atlassian.net"
export JIRA_API_TOKEN="your-api-token"
```

Or edit `appsettings.json` directly.

## Commands

### analytics

Sprint velocity and metrics for recent sprints.

```bash
jira-analytics-cli analytics -p MYPROJECT
jira-analytics-cli analytics -p MYPROJECT -s 10 -o report.txt
jira-analytics-cli analytics -p MYPROJECT -s 5 --output-dir ./reports
```

Options: `-p/--project`, `-s/--sprints` (default 5), `-o/--output`, `--output-dir`

### export

Export analytics data to file.

```bash
jira-analytics-cli export -p MYPROJECT -f json -o metrics.json
jira-analytics-cli export -p MYPROJECT -f csv -o metrics.csv
```

Options: `-p/--project`, `-f/--format` (json, csv), `-o/--output`, `--output-dir`

### burndown

Generate burndown chart for a sprint.

```bash
jira-analytics-cli burndown -p MYPROJECT --sprint-id 42 -o burndown.png
```

Options: `-p/--project`, `--sprint-id`, `-o/--output`, `--output-dir`

### report

Generate self-contained HTML report.

```bash
jira-analytics-cli report -p MYPROJECT -o report.html
jira-analytics-cli report -p MYPROJECT -s 10 --output-dir ./reports
```

Options: `-p/--project`, `-s/--sprints` (default 5), `-o/--output`, `--output-dir`

### jql

Run a JQL query and display or export results.

```bash
jira-analytics-cli jql -q "project = PROJ AND status != Done"
jira-analytics-cli jql -q "assignee = john.doe" -f json -o issues.json
jira-analytics-cli jql -q "project = PROJ ORDER BY created DESC" -n 50 --start-at 50
```

Options: `-q/--query` (required), `-n/--max-results` (default 50), `--start-at`, `-f/--format` (text|json), `-o/--output`

### team-compare

Compare sprint metrics across multiple projects.

```bash
jira-analytics-cli team-compare -p BACKEND,FRONTEND
jira-analytics-cli team-compare -p ALPHA,BETA,GAMMA -s 10 -f json -o comparison.json
```

Options: `-p/--projects` (comma-separated, required), `-s/--sprints` (default 5), `-f/--format` (text|json), `-o/--output`

## Docker

Build and run the CLI using Docker:

### Build the Docker image

```bash
# Build the image
docker build -t jira-analytics-cli .

# Or use docker-compose for easier management
docker-compose build
```

### Run with environment variables

```bash
# Quick run - outputs to console
docker run --rm \
  -e JIRA_BASE_URL="https://your-instance.atlassian.net" \
  -e JIRA_API_TOKEN="your-api-token" \
  jira-analytics-cli analytics -p MYPROJECT

# Run with output file
docker run --rm \
  -e JIRA_BASE_URL="https://your-instance.atlassian.net" \
  -e JIRA_API_TOKEN="your-api-token" \
  -v $(pwd)/output:/app/output \
  jira-analytics-cli analytics -p MYPROJECT -o /app/output/report.txt
```

### Using docker-compose

```bash
# Start the service (creates output directory automatically)
docker-compose up -d

# Run analytics command
docker-compose run --rm app analytics -p MYPROJECT -s 5 -o /app/output/report.txt

# Run export command
docker-compose run --rm app export -p MYPROJECT -f json -o /app/output/metrics.json

# Run burndown chart generation
docker-compose run --rm app burndown -p MYPROJECT --sprint-id 42 -o /app/output/burndown.png

# Run HTML report generation
docker-compose run --rm app report -p MYPROJECT -o /app/output/report.html

# Run JQL query
docker-compose run --rm app jql -q "project = MYPROJECT AND status = Done" -n 50

# Run team comparison
docker-compose run --rm app team-compare -p PROJ1,PROJ2,PROJ3 -s 5
```

### Configuration

Create a `.env` file for persistent configuration:

```bash
# .env
JIRA_BASE_URL=https://your-instance.atlassian.net
JIRA_API_TOKEN=your-api-token
JIRA_DEFAULT_PROJECT=MYPROJECT
DEFAULT_SPRINT_COUNT=5
ENABLE_DETAILED_LOGGING=true
```

Then run:

```bash
docker-compose --env-file .env up -d
```

### Available Commands in Docker

All CLI commands work the same way in Docker as they do when installed directly:

- `analytics` - Sprint velocity and metrics
- `export` - Export data to JSON, CSV, or images
- `burndown` - Generate burndown charts
- `report` - Generate HTML reports
- `jql` - Execute custom JQL queries
- `team-compare` - Compare metrics across projects

### Notes

- The container runs as a non-root user for security
- Output files are saved to the `./output` directory (mounted as volume)
- The image uses Alpine-based runtime for minimal size (~50MB)
- Self-contained build ensures no external dependencies needed
- Health checks verify the CLI is functioning properly

## Usage Examples

The repository includes practical usage examples in the `/examples` directory:

- **BasicUsage.cs** - Minimal setup and first commands
- **AdvancedUsage.cs** - Configuration, custom options, and error handling
- **IntegrationExample.cs** - ASP.NET Core dependency injection integration


These examples demonstrate real-world usage patterns and can be adapted for your projects.

## Development


```bash
dotnet restore
dotnet build
dotnet test
```

## Benchmarks

Performance benchmarks are implemented using [BenchmarkDotNet](https://benchmarkdotnet.org/) to track critical path performance and memory allocations.

### Running Benchmarks

```bash
dotnet run -c Release --project benchmarks/jira-analytics-cli.Benchmarks/jira-analytics-cli.Benchmarks.csproj
```

### StringBenchmarks

The `StringBenchmarks` class measures performance of common string operations used throughout the CLI, including whitespace removal, truncation with ellipsis, slug generation, common prefix detection, and pattern matching. These operations are critical for processing Jira keys, issue summaries, and generating URL-friendly identifiers.


```csharp
var benchmarks = new StringBenchmarks();

// Remove whitespace from Jira keys and issue summaries
string cleanKey = "PROJ-1234 sprint-planning".RemoveWhitespace(); // "PROJ-1234sprint-planning"

// Truncate long issue summaries for display
string truncated = "Implement OAuth2 token refresh with sliding-window expiry".TruncateWithEllipsis(30); // "Implement OAuth2 token refr..."

// Convert issue titles to URL-friendly slugs
string slug = "Hello World! This is a Jira Sprint".ToSlug(); // "hello-world-this-is-a-jira-sprint"

// Find common prefix between sprint labels
string commonPrefix = "MYPROJECT-SPRINT-2024-Q1-WEEK-03".GetCommonPrefix(
    "MYPROJECT-SPRINT-2024-Q2-WEEK-01"); // "MYPROJECT-SPRINT-2024-"

// Check if a string matches a wildcard pattern
bool matches = "PROJ-1234".MatchesPattern("*PROJ-12*"); // true
```

### Current Results

| Method | Mean | Error | StdDev | Gen0 | Allocated |
| :--- | :--- | :--- | :--- | :--- | :--- |
| CacheSet — JSON serialise + ConcurrentDictionary write | 1,582.68 ns | 31.172 ns | 45.692 ns | 0.1621 | 1368 B |
| CacheGet (hit) — expiry check + JSON deserialise | 1,962.95 ns | 48.701 ns | 142.831 ns | 0.1640 | 1376 B |
| CacheContains — expiry check only, no deserialise | 49.26 ns | 0.933 ns | 0.827 ns | - | - |

## JiraApiException

The `JiraApiException` is a custom exception thrown when communication with the Jira REST API fails. It extends the standard `Exception` class with additional properties to capture HTTP status codes and response content from failed API requests, making it easier to diagnose issues like authentication failures, rate limiting, or invalid queries.

This exception is particularly useful for implementing robust error handling in CLI commands that interact with Jira's API, allowing you to provide meaningful error messages to users based on the specific failure scenario.

### Usage Example

```csharp
try
{
    var client = new JiraClient("https://your-instance.atlassian.net", "your-api-token");
    var sprints = await client.GetSprintsAsync("PROJ", 5);
}
catch (JiraApiException ex) when (ex.StatusCode == 401)
{
    Console.Error.WriteLine($"Authentication failed: {ex.Message}");
    Console.Error.WriteLine($"Status Code: {ex.StatusCode}");
    Console.Error.WriteLine($"Response: {ex.ResponseContent}");
    return 1;
}
catch (JiraApiException ex)
{
    Console.Error.WriteLine($"Jira API request failed: {ex.Message}");
    if (ex.StatusCode.HasValue)
    {
        Console.Error.WriteLine($"HTTP Status: {ex.StatusCode}");
    }
    if (!string.IsNullOrEmpty(ex.ResponseContent))
    {
        Console.Error.WriteLine($"Response Content:\n{ex.ResponseContent}");
    }
    return 1;
}
```

## JiraIssueTests

The `JiraIssueTests` class provides unit tests for the `JiraIssue` model, verifying critical business logic methods that calculate issue metrics and priorities. These tests validate the behavior of overdue detection, priority classification, and cycle time calculation to ensure accurate analytics reporting.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using FluentAssertions;

// Create a test issue with a past due date and open status
var overdueIssue = new JiraIssue
{
    Key = "PROJ-123",
    Summary = "Fix critical authentication bug",
    Status = "In Progress",
    Priority = "Critical",
    DueDate = DateTime.UtcNow.AddDays(-5),
    CreatedDate = DateTime.UtcNow.AddDays(-30),
    ResolutionDate = null,
    StoryPoints = 8
};

// Test overdue detection
bool isOverdue = overdueIssue.IsOverdue(); // Returns true

// Test high priority detection
bool isHighPriority = overdueIssue.IsHighPriority(); // Returns true

// Create a resolved issue for cycle time testing
var resolvedIssue = new JiraIssue
{
    Key = "PROJ-456",
    Summary = "Implement new API endpoint",
    Status = "Done",
    Priority = "Medium",
    DueDate = DateTime.UtcNow.AddDays(-10),
    CreatedDate = DateTime.UtcNow.AddDays(-15),
    ResolutionDate = DateTime.UtcNow.AddDays(-2),
    StoryPoints = 5
};

// Test cycle time calculation
double cycleTime = resolvedIssue.GetCycleTime(); // Returns ~13 days

// Verify assertions using FluentAssertions
resolvedIssue.IsOverdue().Should().BeFalse();
resolvedIssue.IsHighPriority().Should().BeFalse();
```

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
