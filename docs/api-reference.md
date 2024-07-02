# API Reference

Complete reference for all Jira Analytics CLI commands and options.

## Command-Line Interface

### Root Command

```bash
jira-analytics-cli [COMMAND] [OPTIONS]
```

**Global Options**:
- `--help, -h` - Show help information
- `--version, -v` - Show version number

---

## Analytics Command

Analyze sprint metrics and generate detailed reports.

```bash
jira-analytics-cli analytics [OPTIONS]
```

### Options

| Option | Short | Type | Required | Default | Description |
|--------|-------|------|----------|---------|-------------|
| `--project` | `-p` | string | ✓ | - | Jira project key (e.g., BACKEND, PROJ) |
| `--sprints` | `-s` | int | ✗ | 5 | Number of recent sprints to analyze (1-100) |
| `--output` | `-o` | string | ✗ | stdout | File path for output; prints to console if omitted |

### Examples

```bash
# Analyze default 5 sprints and print to console
jira-analytics-cli analytics -p MYPROJECT

# Analyze specific number of sprints and save to file
jira-analytics-cli analytics -p BACKEND -s 10 -o report.txt

# Using long option names
jira-analytics-cli analytics --project=MYPROJECT --sprints=3 --output=metrics.txt

# Mixed short and long options
jira-analytics-cli analytics -p MYPROJECT -s 5 --output=/tmp/report.txt
```

### Output Format

The command generates a formatted text report including:
- Sprint summary (dates, status, velocity)
- Completed vs. planned story points
- Issue breakdown by status
- Developer productivity
- Quality metrics
- Overdue and at-risk issues
- Trend analysis

---

## Export Command

Export analytics data in various formats for external processing.

```bash
jira-analytics-cli export [OPTIONS]
```

### Options

| Option | Short | Type | Required | Description |
|--------|-------|------|----------|-------------|
| `--project` | `-p` | string | ✓ | Jira project key |
| `--format` | `-f` | string | ✓ | Export format (json, csv, png, jpg, pdf) |
| `--output` | `-o` | string | ✓ | Output file path |

### Supported Formats

#### JSON

```bash
jira-analytics-cli export -p MYPROJECT -f json -o metrics.json
```

**Structure**:
```json
{
  "project": {
    "key": "MYPROJECT",
    "name": "My Project"
  },
  "sprints": [
    {
      "id": 42,
      "name": "Sprint 42",
      "state": "closed",
      "startDate": "2026-05-01",
      "endDate": "2026-05-15",
      "plannedStoryPoints": 50,
      "completedStoryPoints": 48,
      "velocity": 48,
      "completionRate": 0.96,
      "issues": [
        {
          "key": "PROJ-123",
          "summary": "Add authentication",
          "type": "Story",
          "status": "Done",
          "assignee": "john@example.com",
          "storyPoints": 8,
          "cycleTime": "5 days 3 hours"
        }
      ]
    }
  ],
  "developers": [
    {
      "name": "John Doe",
      "email": "john@example.com",
      "completedIssues": 12,
      "completionRate": 0.92,
      "productivity": 9.6,
      "averageCycleTime": "3 days"
    }
  ],
  "metrics": {
    "averageVelocity": 47.5,
    "velocityTrend": "increasing",
    "qualityScore": 0.89,
    "riskScore": 0.12,
    "overdueIssueCount": 2
  }
}
```

**Usage**:
```bash
# Parse with jq
jq '.sprints[0].velocity' metrics.json

# Process with Python
python3 -c "import json; data = json.load(open('metrics.json')); print(data['metrics']['averageVelocity'])"
```

#### CSV

```bash
jira-analytics-cli export -p MYPROJECT -f csv -o metrics.csv
```

**Columns**:
```
Sprint ID,Sprint Name,Start Date,End Date,Planned Points,Completed Points,Velocity,Completion Rate,Quality Score,Risk Score
42,Sprint 42,2026-05-01,2026-05-15,50,48,48,0.96,0.89,0.12
41,Sprint 41,2026-04-17,2026-05-01,55,54,54,0.98,0.91,0.08
...
```

**Developer CSV**:
```
Developer Name,Email,Completed Issues,Completion Rate,Avg Cycle Time
John Doe,john@example.com,12,0.92,3 days
Jane Smith,jane@example.com,14,0.95,2.8 days
...
```

**Usage**:
- Import into Excel or Google Sheets
- Process with pandas: `pd.read_csv('metrics.csv')`
- Use with data visualization tools

#### PNG/JPEG

```bash
jira-analytics-cli export -p MYPROJECT -f png -o velocity-chart.png
jira-analytics-cli export -p MYPROJECT -f jpg -o quality-report.jpg
```

**Generated Visualizations**:
- Sprint velocity chart (line graph)
- Completion rates (bar chart)
- Quality trends (area chart)
- Developer productivity (ranking)

**Usage**:
```bash
# View the image
open velocity-chart.png  # macOS
feh velocity-chart.png   # Linux
```

#### PDF

```bash
jira-analytics-cli export -p MYPROJECT -f pdf -o report.pdf
```

**Content**:
- Title page with project info
- Velocity trends
- Team metrics
- Quality analysis
- Developer rankings
- Export timestamp

---

## Burndown Command

Generate sprint burndown charts for visualization.

```bash
jira-analytics-cli burndown [OPTIONS]
```

### Options

| Option | Short | Type | Required | Description |
|--------|-------|------|----------|-------------|
| `--project` | `-p` | string | ✓ | Jira project key |
| `--sprint-id` | | int | ✓ | Jira sprint ID (numeric) |
| `--output` | `-o` | string | ✓ | Output image file path (.png, .jpg) |

### Examples

```bash
# Generate burndown for a specific sprint
jira-analytics-cli burndown -p MYPROJECT --sprint-id 42 -o burndown.png

# Using equals syntax
jira-analytics-cli burndown --project=BACKEND --sprint-id=100 --output=sprint100_burndown.png
```

### Output Details

The burndown chart includes:
- **Ideal burn line** - Expected linear burndown
- **Actual burn line** - Real sprint progress
- **Sprint timeline** - Start to end date
- **Legend** - Ideal vs. Actual
- **Axis labels** - Days and story points
- **Metadata** - Sprint name, dates, final velocity

### Finding Sprint IDs

To find the sprint ID for a burndown:

```bash
# Get detailed analysis that includes sprint IDs
jira-analytics-cli analytics -p MYPROJECT -s 10

# Look for "Sprint [number]" in the output
```

---

## Configuration Options

### Environment Variables

These override values in appsettings.json:

```bash
# Required
JIRA_BASE_URL=https://your-instance.atlassian.net
JIRA_API_TOKEN=your-api-token

# Optional Jira Configuration
JIRA_DEFAULT_PROJECT=MYPROJECT
JIRA_REQUEST_TIMEOUT_SECONDS=30
JIRA_MAX_RETRIES=3

# Caching
CACHE_EXPIRATION_MINUTES=15
CACHE_MAX_ITEMS=1000

# Features
DEFAULT_SPRINT_COUNT=5
ENABLE_DETAILED_LOGGING=false

# Performance
ENABLE_METRICS_COLLECTION=true
MAX_CONCURRENT_REQUESTS=5
```

### Configuration File (appsettings.json)

```json
{
  "jiraConfiguration": {
    "baseUrl": "https://your-instance.atlassian.net",
    "apiToken": "your-api-token",
    "defaultProject": "MYPROJECT",
    "requestTimeoutSeconds": 30,
    "maxRetries": 3
  },
  "caching": {
    "expirationMinutes": 15,
    "maxItems": 1000
  },
  "features": {
    "defaultSprintCount": 5,
    "enableDetailedLogging": false,
    "exportFormat": "json"
  },
  "performance": {
    "enableMetricsCollection": true,
    "maxConcurrentRequests": 5
  }
}
```

---

## Jira API Integration

This tool uses the **Jira REST API v3**. Understanding the underlying API helps with troubleshooting.

### Endpoints Used

| Endpoint | Purpose | Rate Limit |
|----------|---------|------------|
| `GET /rest/api/3/projects/{projectKeyOrId}` | Get project details | Standard |
| `GET /rest/api/3/board/{boardId}/sprint` | List sprints | Standard |
| `GET /rest/api/3/search` | Search issues (JQL) | Standard |
| `GET /rest/api/3/issue/{issueIdOrKey}` | Get issue details | Standard |
| `GET /rest/api/3/users` | Get team members | Standard |

### Rate Limiting

**Jira Cloud**: 30,000 requests per 10 minutes per workspace.

**Handling Rate Limits**:
- The CLI automatically retries with exponential backoff
- If rate limited, wait and try again in a few minutes
- Reduce sprint count or disable detailed logging to use fewer requests

### Authentication

Uses **Bearer Token** authentication:
```
Authorization: Bearer {JIRA_API_TOKEN}
```

---

## Exit Codes

| Code | Meaning | Example |
|------|---------|---------|
| 0 | Success | Command completed successfully |
| 1 | General error | Unhandled exception |
| 10 | Authentication failed | Invalid or expired API token |
| 11 | Configuration error | Missing required settings |
| 12 | Project not found | Invalid project key |
| 13 | Connection error | Cannot reach Jira instance |
| 20 | Validation error | Invalid input parameters |
| 21 | File error | Cannot write output file |

### Usage in Scripts

```bash
#!/bin/bash
jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt
if [ $? -eq 0 ]; then
    echo "Analysis complete"
    mail -s "Report" team@example.com < report.txt
else
    echo "Analysis failed"
    exit 1
fi
```

---

## Data Models

### Sprint

```json
{
  "id": 42,
  "name": "Sprint 42",
  "state": "closed",
  "startDate": "2026-05-01T00:00:00Z",
  "endDate": "2026-05-15T00:00:00Z",
  "completeDate": "2026-05-15T17:30:00Z",
  "goal": "Complete authentication and payment integration",
  "plannedStoryPoints": 50,
  "completedStoryPoints": 48,
  "velocity": 48,
  "completionRate": 0.96,
  "issueCount": 14,
  "completedIssueCount": 13
}
```

### JiraIssue

```json
{
  "key": "PROJ-123",
  "id": "10001",
  "summary": "Implement OAuth2 authentication",
  "description": "Add OAuth2 support for Google and GitHub",
  "type": "Story",
  "status": "Done",
  "priority": "High",
  "assignee": {
    "name": "John Doe",
    "email": "john@example.com"
  },
  "storyPoints": 8,
  "createdDate": "2026-04-17T10:00:00Z",
  "resolvedDate": "2026-05-12T15:30:00Z",
  "dueDate": "2026-05-15T00:00:00Z",
  "cycleTime": "25 days 5 hours"
}
```

### Developer

```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "joinDate": "2025-01-15T00:00:00Z",
  "assignedIssues": 15,
  "completedIssues": 12,
  "completionRate": 0.92,
  "productivity": 9.6,
  "averageCycleTime": "3 days 2 hours",
  "rank": 1
}
```

### SprintMetric

```json
{
  "sprintId": 42,
  "sprintName": "Sprint 42",
  "plannedStoryPoints": 50,
  "completedStoryPoints": 48,
  "velocity": 48,
  "completionRate": 0.96,
  "totalIssues": 14,
  "completedIssues": 13,
  "qualityScore": 0.89,
  "defectRate": 0.071,
  "riskScore": 0.12,
  "healthStatus": "Healthy",
  "burndownChart": "data:image/png;base64,..."
}
```

---

## Troubleshooting with CLI

### Debug Output

Enable detailed logging:

```bash
export ENABLE_DETAILED_LOGGING=true
jira-analytics-cli analytics -p MYPROJECT -s 1
```

### Validate Configuration

```bash
# Check if Jira instance is reachable
curl -H "Authorization: Bearer $JIRA_API_TOKEN" \
  "$JIRA_BASE_URL/rest/api/3/myself"

# Should return your user profile if authenticated correctly
```

### Test Project Access

```bash
# Verify you can access a specific project
jira-analytics-cli analytics -p TESTPROJECT -s 1
```

---

## Version Information

Current version: **1.2.0**

**Requirements**:
- .NET 10 SDK or later
- Jira Cloud or Server instance
- API token with project access

For detailed version history, see [CHANGELOG.md](../CHANGELOG.md)
