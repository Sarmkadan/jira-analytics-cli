# Getting Started with Jira Analytics CLI

This guide will walk you through setting up and using Jira Analytics CLI in 5 minutes.

## Prerequisites

- .NET 10 SDK ([download](https://dotnet.microsoft.com/download/dotnet/10.0))
- A Jira Cloud or Server instance
- Access to create API tokens in Jira
- 30 MB disk space for installation

## Step 1: Get Your Jira API Token

1. Open [Jira Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Click **Create API Token**
3. Name it "Analytics CLI"
4. Copy the token to a safe location
5. You'll also need your Jira workspace URL (e.g., `https://your-workspace.atlassian.net`)

## Step 2: Install the Tool

### Option A: Build from Source (Recommended)

```bash
# Clone repository
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli

# Build and publish
dotnet publish -c Release -o ./dist --self-contained -p:PublishSingleFile=true

# Create a symlink for easy access (macOS/Linux)
sudo ln -s "$(pwd)/dist/jira-analytics-cli" /usr/local/bin/jira-analytics-cli

# Test installation
jira-analytics-cli --version
```

### Option B: Docker (No Dependencies)

```bash
docker build -t jira-analytics-cli .
alias jira-analytics-cli="docker run --rm jira-analytics-cli"
```

## Step 3: Configure Your Credentials

### Quick Setup (Environment Variables)

```bash
# Add to your ~/.bashrc or ~/.zshrc
export JIRA_BASE_URL="https://your-workspace.atlassian.net"
export JIRA_API_TOKEN="your-api-token"

# Reload shell
source ~/.bashrc  # or source ~/.zshrc
```

### Permanent Setup (Configuration File)

Create `~/.jira-analytics/appsettings.json`:

```json
{
  "jiraConfiguration": {
    "baseUrl": "https://your-workspace.atlassian.net",
    "apiToken": "your-api-token",
    "defaultProject": "MYPROJECT"
  }
}
```

## Step 4: Verify Connection

```bash
jira-analytics-cli analytics -p MYPROJECT -s 1
```

You should see a report with sprint metrics. If you get an error:

- **401 Unauthorized**: Check your API token
- **Connection refused**: Verify JIRA_BASE_URL is correct
- **Not found**: Verify project key is correct

## Step 5: Run Your First Analysis

### Generate a Text Report

```bash
jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt
cat report.txt
```

### Export as JSON

```bash
jira-analytics-cli export -p MYPROJECT -f json -o metrics.json
# View with jq
jq '.sprints[0]' metrics.json
```

### Create a Burndown Chart

```bash
# First, find a sprint ID
jira-analytics-cli analytics -p MYPROJECT -s 1

# Then generate chart (replace 42 with your sprint ID)
jira-analytics-cli burndown -p MYPROJECT --sprint-id 42 -o burndown.png
```

## Common Use Cases

### Use Case 1: Weekly Team Report

```bash
#!/bin/bash
# Save as weekly-report.sh

PROJECT="MYPROJECT"
DATE=$(date +%Y%m%d)
REPORT="reports/weekly_${DATE}.txt"

mkdir -p reports
jira-analytics-cli analytics -p "$PROJECT" -s 4 -o "$REPORT"
echo "Report saved to $REPORT"
cat "$REPORT"
```

Run with: `bash weekly-report.sh`

### Use Case 2: Export Metrics to Excel

```bash
jira-analytics-cli export -p MYPROJECT -f csv -o metrics.csv
open metrics.csv  # macOS
# or import into Excel/Google Sheets
```

### Use Case 3: Monitor Burndown

```bash
#!/bin/bash
# Monitor a specific sprint

while true; do
  jira-analytics-cli burndown -p MYPROJECT --sprint-id 42 \
    -o "burndown_$(date +%s).png"
  sleep 3600  # Update every hour
done
```

### Use Case 4: Developer Performance Analysis

```bash
# Export full metrics
jira-analytics-cli export -p MYPROJECT -f json -o metrics.json

# Analyze with Python
python3 << 'EOF'
import json
with open('metrics.json') as f:
    data = json.load(f)
    developers = sorted(data['developers'], 
                       key=lambda x: x['completionRate'], 
                       reverse=True)
    for dev in developers:
        print(f"{dev['name']}: {dev['completionRate']:.1%}")
EOF
```

## Troubleshooting

### "Project not found"

- Verify project key: `PROJ` not `Project Name`
- Check you have view permission in Jira
- Ensure project has active sprints

### "No sprints available"

- Project may have no completed sprints yet
- Try reducing sprint count: `-s 1`
- Check project board in Jira

### "Command not found"

Ensure the tool is in your PATH:

```bash
# Find where it was installed
which jira-analytics-cli

# Or add to PATH manually
export PATH="$PATH:/path/to/dist"
```

### "Permission denied" on macOS

```bash
chmod +x dist/jira-analytics-cli
```

## Next Steps

- Read [Architecture Guide](./architecture.md) to understand the design
- Check [API Reference](./api-reference.md) for detailed command options
- See [Deployment Guide](./deployment.md) for production setups
- Browse [FAQ](./faq.md) for more questions and answers

## Support

Having issues? Check:

1. Error message matches something in [FAQ](./faq.md)
2. Your Jira instance is accessible: `curl https://your-instance.atlassian.net`
3. API token is valid and not expired
4. .NET 10 is installed: `dotnet --version`

## Getting Help

- **GitHub Issues**: Report bugs or request features
- **Documentation**: Check docs/ directory
- **Examples**: See examples/ directory for real-world usage

---

You're all set! Start with: `jira-analytics-cli analytics -p MYPROJECT -s 5`
