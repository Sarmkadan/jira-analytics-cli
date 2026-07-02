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

### Current Results

| Method | Mean | Error | StdDev | Gen0 | Allocated |
| :--- | :--- | :--- | :--- | :--- | :--- |
| CacheSet — JSON serialise + ConcurrentDictionary write | 1,582.68 ns | 31.172 ns | 45.692 ns | 0.1621 | 1368 B |
| CacheGet (hit) — expiry check + JSON deserialise | 1,962.95 ns | 48.701 ns | 142.831 ns | 0.1640 | 1376 B |
| CacheContains — expiry check only, no deserialise | 49.26 ns | 0.933 ns | 0.827 ns | - | - |

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
