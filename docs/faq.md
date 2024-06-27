# Frequently Asked Questions

## Installation & Setup

### Q: Do I need .NET installed to use the CLI?

**A:** Only if you're building from source. The published releases are self-contained executables that include .NET. Just download and run!

### Q: What versions of .NET are supported?

**A:** .NET 10 and later. The project uses the latest .NET features, so older versions are not supported. You can download .NET 10 from https://dotnet.microsoft.com/download/dotnet/10.0

### Q: Can I use this with Jira Server (on-premise)?

**A:** Yes! The tool works with both Jira Cloud and Jira Server. Just set `JIRA_BASE_URL` to your server's address instead of `.atlassian.net`.

### Q: How do I set up the API token?

**A:** For Jira Cloud:
1. Go to https://id.atlassian.com/manage-profile/security/api-tokens
2. Click "Create API Token"
3. Copy it and set `export JIRA_API_TOKEN="your-token"`

For Jira Server, create the token in your Jira user profile settings.

### Q: Is the API token stored securely?

**A:** The tool reads the token from environment variables or configuration files, not from a hardcoded location. It's never logged or output. Always use environment variables or secrets management systems in production.

---

## Configuration

### Q: Can I use a configuration file instead of environment variables?

**A:** Yes! Create `appsettings.json`:

```json
{
  "jiraConfiguration": {
    "baseUrl": "https://your-instance.atlassian.net",
    "apiToken": "your-token"
  }
}
```

Environment variables override file settings, so use variables for sensitive data.

### Q: How do I change the default cache expiration?

**A:** Set the environment variable:

```bash
export CACHE_EXPIRATION_MINUTES=30
```

Or in `appsettings.json`:

```json
{
  "caching": {
    "expirationMinutes": 30
  }
}
```

### Q: Can I disable logging output?

**A:** Set detailed logging to false:

```bash
export ENABLE_DETAILED_LOGGING=false
```

### Q: How do I use the CLI behind a corporate proxy?

**A:** Set HTTP proxy environment variables:

```bash
export HTTP_PROXY="http://proxy.company.com:3128"
export HTTPS_PROXY="https://proxy.company.com:3128"
export NO_PROXY="localhost,127.0.0.1,.company.com"
```

---

## Usage & Features

### Q: What's the difference between analytics, export, and burndown commands?

**Analytics**: Generates a text report with metrics summary
**Export**: Exports full data in specified format (JSON, CSV, PNG, etc.)
**Burndown**: Creates a burndown chart for a specific sprint

### Q: Can I analyze more than 100 sprints?

**A:** Technically yes, but Jira's API has pagination limits. For practical purposes, analyzing 20-30 recent sprints gives the best signal-to-noise ratio.

### Q: Why does the burndown chart look different from Jira's?

**A:** Our burndown uses actual task point changes throughout the sprint. Jira's chart may include scope changes and other adjustments. Both are correct, just calculated differently.

### Q: Can I export to Excel directly?

**A:** Export as CSV and open in Excel:

```bash
jira-analytics-cli export -p MYPROJECT -f csv -o metrics.csv
open metrics.csv
```

### Q: How do I generate reports for multiple projects?

**A:** Use a loop:

```bash
for project in BACKEND FRONTEND DEVOPS; do
  jira-analytics-cli analytics -p "$project" -s 5 -o "${project}-report.txt"
done
```

### Q: Can I schedule reports to run automatically?

**A:** Yes! Use cron (Linux/macOS):

```bash
0 9 * * 1 /usr/local/bin/jira-analytics-cli analytics -p MYPROJECT -s 5 -o /reports/weekly.txt
```

Or GitHub Actions, Azure Pipelines, etc. See [Deployment Guide](./deployment.md#scheduled-execution).

---

## Data & Metrics

### Q: What's included in the velocity calculation?

**A:** Velocity = completed story points in a sprint. Only issues marked as "Done" are counted, and they must have been completed during that sprint.

### Q: How is cycle time calculated?

**A:** Cycle time = issue resolution date - issue creation date. This measures how long an issue stayed in the system.

### Q: What's the quality score based on?

**A:** Quality score factors in:
- Defect rate (% of issues that are bugs)
- Overdue issues (past due date)
- High-priority items closed quickly
- Completion rate

### Q: Can I export custom metrics?

**A:** The CLI calculates standard metrics. For custom metrics, export as JSON and process with Python or your analysis tool:

```bash
jira-analytics-cli export -p MYPROJECT -f json -o data.json

# Then analyze with Python
python3 -c "import json; data = json.load(open('data.json')); ..."
```

### Q: Why are some developers not showing up in the report?

**A:** Developers must have completed at least one issue in the analyzed sprints to appear in the report.

---

## Performance & Troubleshooting

### Q: The command is running slowly. How do I speed it up?

**A:** Try these optimizations:

1. **Reduce sprints analyzed**: `-s 3` instead of `-s 10`
2. **Increase cache expiration**: `export CACHE_EXPIRATION_MINUTES=60`
3. **Disable detailed logging**: `export ENABLE_DETAILED_LOGGING=false`
4. **Reduce concurrent requests**: `export MAX_CONCURRENT_REQUESTS=2`

### Q: I get "401 Unauthorized" error

**A:** Your API token is invalid or expired:

```bash
# Check the token is set
echo $JIRA_API_TOKEN

# Regenerate in Jira settings
# https://id.atlassian.com/manage-profile/security/api-tokens
```

### Q: I get "Connection refused" error

**A:** Jira instance is unreachable:

```bash
# Check URL is correct
echo $JIRA_BASE_URL

# Test connectivity
curl -I "$JIRA_BASE_URL"

# Check firewall/proxy settings
```

### Q: I get "Project not found" error

**A:** Project key is invalid or you don't have permission:

```bash
# Verify project key (not project name)
# PROJ-123 uses key "PROJ"

# Check permission in Jira
# You need at least "Browse" permission
```

### Q: I get "No sprints available" error

**A:** Project has no completed sprints:

```bash
# Try reducing sprint count
jira-analytics-cli analytics -p MYPROJECT -s 1

# Check project has active sprints in Jira
```

### Q: Out of memory error

**A:** Too many issues or too many sprints:

```bash
# Reduce sprint count
jira-analytics-cli analytics -p MYPROJECT -s 2

# Reduce cache size
export CACHE_MAX_ITEMS=500
```

### Q: Command timed out

**A:** Request took too long:

```bash
# Increase timeout
export JIRA_REQUEST_TIMEOUT_SECONDS=60

# Or reduce scope
jira-analytics-cli analytics -p MYPROJECT -s 1
```

---

## Docker & Deployment

### Q: How do I run this in Docker?

**A:** Build and run:

```bash
docker build -t jira-analytics-cli .
docker run --rm \
  -e JIRA_BASE_URL="https://your-instance.atlassian.net" \
  -e JIRA_API_TOKEN="your-token" \
  jira-analytics-cli analytics -p MYPROJECT -s 5
```

### Q: Can I run this in Kubernetes?

**A:** Yes! See [Deployment Guide - Kubernetes](./deployment.md#4-kubernetes).

### Q: How do I pass the API token securely in Docker?

**A:** Use Docker secrets or environment variable files:

```bash
# Option 1: Secret
echo "your-token" | docker secret create jira_token -

# Option 2: .env file
echo "JIRA_API_TOKEN=your-token" > .env
docker run --env-file .env jira-analytics-cli
```

### Q: Can I save reports to a volume?

**A:** Yes, mount a volume:

```bash
docker run --rm \
  -v $(pwd)/reports:/app/reports \
  -e JIRA_BASE_URL="https://..." \
  -e JIRA_API_TOKEN="..." \
  jira-analytics-cli analytics -p MYPROJECT -s 5 -o /app/reports/report.txt
```

---

## Development & Contributing

### Q: Can I modify the code for my use case?

**A:** Absolutely! The MIT license allows commercial and personal use. See the LICENSE file.

### Q: How do I build the project myself?

**A:**
```bash
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli
dotnet build -c Release
```

### Q: Can I add my own metrics or export formats?

**A:** Yes! The architecture supports extensibility. See the ExportService implementation in the code.

### Q: How do I run tests?

**A:** Tests aren't included yet, but the architecture supports unit tests with dependency injection. Contributions welcome!

### Q: Can I contribute improvements?

**A:** Yes! Pull requests are welcome for:
- Bug fixes
- Performance improvements
- New export formats
- Documentation
- Features

---

## Licensing & Legal

### Q: What's the license?

**A:** MIT License. You can use this commercially, modify it, distribute it, and keep your modifications private.

### Q: Can I use this commercially?

**A:** Yes! The MIT license permits commercial use.

### Q: Do I need to credit you if I use this?

**A:** Not required by the license, but appreciated!

### Q: Is there a warranty?

**A:** No. The software is provided "as-is". See LICENSE file for full terms.

---

## Getting Help

### Q: Where do I report bugs?

**A:** Create an issue on GitHub: https://github.com/sarmkadan/jira-analytics-cli/issues

### Q: Where do I request features?

**A:** Open a GitHub discussion or issue describing your use case.

### Q: How do I contact the author?

**A:** Visit https://sarmkadan.com or reach out on GitHub.

---

## Glossary

**Velocity**: Story points completed in a sprint (measure of team capacity)

**Cycle Time**: Days from issue creation to resolution (measure of efficiency)

**Completion Rate**: Percentage of issues marked as Done (measure of plan accuracy)

**Quality Score**: Aggregate metric including defect rate and overdue items (0-100)

**Risk Score**: Indicator of project health based on blockers and overdue items

**Burndown**: Chart showing remaining work over sprint duration (visual progress)

**Sprint**: Time-boxed iteration, typically 2 weeks

**Story Point**: Unit of work complexity estimation

---

Still have questions? Check the [full documentation](../README.md) or open an issue on GitHub!
