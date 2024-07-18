# jira-analytics-cli

![CI](https://github.com/sarmkadan/jira-analytics-cli/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/jira-analytics-cli)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

.NET 10 CLI for Jira sprint analytics. Pulls data via Jira REST API v3 and outputs
velocity reports, burndown charts, HTML dashboards, and team comparison tables.

## Requirements

- .NET 10 SDK
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

```bash
docker build -t jira-analytics-cli .
docker run --rm \
  -e JIRA_BASE_URL="https://your-instance.atlassian.net" \
  -e JIRA_API_TOKEN="your-token" \
  -v $(pwd)/output:/app/output \
  jira-analytics-cli analytics -p MYPROJECT -o /app/output/report.txt
```

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets
