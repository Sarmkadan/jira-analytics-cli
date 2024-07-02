# Jira Analytics CLI Examples

This directory contains practical examples demonstrating how to use the Jira Analytics CLI in real-world scenarios.

## Shell Script Examples

These bash/shell scripts show common use cases and can be run on macOS and Linux.

### 1. analyze-recent-sprints.sh

**Purpose**: Generate a timestamped analysis report of recent sprints.

**Usage**:
```bash
bash examples/analyze-recent-sprints.sh MYPROJECT [SPRINT_COUNT] [OUTPUT_DIR]
```

**Example**:
```bash
bash examples/analyze-recent-sprints.sh BACKEND 5 ./reports
```

**Output**: Text report with sprint metrics, development team performance, and quality metrics.

---

### 2. velocity-report.sh

**Purpose**: Compare velocity trends across multiple projects.

**Usage**:
```bash
bash examples/velocity-report.sh [PROJECT1 PROJECT2 ...]
```

**Example**:
```bash
bash examples/velocity-report.sh BACKEND FRONTEND MOBILE
```

**Output**: Formatted comparison table showing velocity, completion rate, quality score, and trends for each project.

**Requirements**: `jq` for formatted JSON output (optional, but recommended)
```bash
# Install jq
brew install jq          # macOS
apt-get install jq       # Debian/Ubuntu
```

---

### 3. developer-load-analysis.sh

**Purpose**: Analyze individual developer workload and productivity.

**Usage**:
```bash
bash examples/developer-load-analysis.sh PROJECT [SPRINT_COUNT]
```

**Example**:
```bash
bash examples/developer-load-analysis.sh BACKEND 5
```

**Output**:
- Developer productivity rankings
- Completion rates
- Cycle time metrics
- Identification of overloaded team members
- Recognition of top performers

**Tips**:
- Use to identify burnout risks
- Recognize high performers
- Balance team workload
- Plan capacity for future sprints

---

### 4. export-csv.sh

**Purpose**: Export metrics to CSV for spreadsheet analysis.

**Usage**:
```bash
bash examples/export-csv.sh PROJECT [OUTPUT_DIR]
```

**Example**:
```bash
bash examples/export-csv.sh MYPROJECT ./exports
```

**Output**: CSV file ready to import into:
- Excel
- Google Sheets
- Data analysis tools (Python pandas, R, etc.)

**Post-processing Example**:
```python
import pandas as pd
df = pd.read_csv('MYPROJECT_metrics.csv')
print(df.describe())
df.to_excel('metrics.xlsx')
```

---

### 5. burndown-png.sh

**Purpose**: Generate a visual burndown chart for a sprint.

**Usage**:
```bash
bash examples/burndown-png.sh PROJECT SPRINT_ID [OUTPUT_DIR]
```

**Example**:
```bash
bash examples/burndown-png.sh BACKEND 42 ./charts
```

**Output**: PNG image showing:
- Sprint name and dates
- Actual vs. ideal burn line
- Story points remaining
- Final velocity

**Use For**:
- Retrospective meetings
- Status reports
- Project dashboards
- Client presentations
- Wiki/documentation

---

## C# Examples

These examples demonstrate programmatic usage of the Jira Analytics CLI services.

### 1. Example.VelocityAnalysis.cs

**Purpose**: Analyze velocity trends and forecast future capacity.

**Topics Covered**:
- Using the AnalyticsService
- Dependency Injection setup
- Data analysis and trend calculation
- Forecasting with linear regression
- Actionable insights generation

**Key Features**:
- Visualize velocity trends with ASCII charts
- Calculate velocity forecasts
- Identify team performance patterns
- Generate recommendations

**Run**:
```bash
cd /path/to/jira-analytics-cli
dotnet run --project examples/Example.VelocityAnalysis.cs -- MYPROJECT 8
```

---

### 2. Example.BatchExport.cs

**Purpose**: Export analytics for multiple projects in bulk.

**Topics Covered**:
- Using the ExportService
- Batch processing
- Error handling
- Result aggregation and reporting
- File size and metadata tracking

**Key Features**:
- Export multiple projects simultaneously
- Support for multiple formats (JSON, CSV, PNG)
- Summary reporting with file sizes
- Graceful error handling

**Run**:
```bash
cd /path/to/jira-analytics-cli
dotnet run --project examples/Example.BatchExport.cs
```

---

## Usage Patterns

### Pattern 1: Weekly Reporting

Generate reports every Monday morning for distribution:

```bash
#!/bin/bash
# weekly-report.sh

# Run analytics for multiple projects
for project in BACKEND FRONTEND DEVOPS; do
  bash examples/analyze-recent-sprints.sh "$project" 4 "weekly_reports/"
done

# Email the results
for report in weekly_reports/*.txt; do
  cat "$report" | mail -s "Weekly Report: $(basename $report)" team@example.com
done
```

### Pattern 2: Data Analysis Pipeline

Export data and analyze with Python:

```bash
#!/bin/bash
# analysis-pipeline.sh

PROJECT="MYPROJECT"

# Export metrics
bash examples/export-csv.sh "$PROJECT" "data/"

# Analyze with Python
python3 << 'EOF'
import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_csv('data/MYPROJECT_metrics.csv')

# Plot trends
df.plot(x='SprintName', y='Velocity', title='Velocity Trend')
plt.savefig('velocity_trend.png')

# Calculate statistics
print("Velocity Statistics:")
print(df['Velocity'].describe())
EOF
```

### Pattern 3: Dashboard Updates

Generate charts for a dashboard:

```bash
#!/bin/bash
# dashboard-update.sh

PROJECT="MYPROJECT"

# Generate multiple visualizations
for sprint_id in 40 41 42 43; do
  bash examples/burndown-png.sh "$PROJECT" "$sprint_id" "dashboard/charts/"
done

# Copy to web server
cp dashboard/charts/*.png /var/www/dashboards/jira/
```

### Pattern 4: Performance Monitoring

Track team performance over time:

```bash
#!/bin/bash
# monitor-performance.sh

PROJECT="MYPROJECT"
ARCHIVE_DIR="performance_history"

mkdir -p "$ARCHIVE_DIR"

# Daily snapshot
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
bash examples/developer-load-analysis.sh "$PROJECT" 5 > "$ARCHIVE_DIR/load_$TIMESTAMP.txt"

# Keep last 30 days
find "$ARCHIVE_DIR" -mtime +30 -delete
```

---

## Integration Examples

### GitHub Actions

```yaml
# .github/workflows/jira-analytics.yml
name: Jira Analytics
on:
  schedule:
    - cron: '0 9 * * 1'  # Every Monday at 9 AM

jobs:
  analytics:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Analyze
        env:
          JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
          JIRA_API_TOKEN: ${{ secrets.JIRA_API_TOKEN }}
        run: |
          bash examples/velocity-report.sh BACKEND FRONTEND
      - uses: actions/upload-artifact@v3
        with:
          name: reports
          path: velocity-comparison*.txt
```

### GitLab CI

```yaml
jira_analytics:
  schedule:
    - "0 9 * * 1"
  script:
    - bash examples/analyze-recent-sprints.sh $JIRA_PROJECT 5
  artifacts:
    paths:
      - sprint-analysis*.txt
```

---

## Troubleshooting Examples

### Example: "Project not found"

```bash
# Verify project key is correct
bash examples/analyze-recent-sprints.sh MYPROJ 1

# Check with correct key
bash examples/analyze-recent-sprints.sh MYPROJECT 1
```

### Example: "No sprints available"

```bash
# Try with fewer sprints
bash examples/analyze-recent-sprints.sh MYPROJECT 1

# Check project has completed sprints in Jira
```

### Example: Permission denied

```bash
# Make scripts executable
chmod +x examples/*.sh

# Then run
bash examples/analyze-recent-sprints.sh MYPROJECT 5
```

---

## Contributing Examples

Have a useful example? Share it!

- Create a new shell script or C# file in `examples/`
- Add documentation to this README
- Follow the author header format
- Keep examples focused and well-commented

---

## Resources

- [Main README](../README.md) - Project overview
- [Getting Started Guide](../docs/getting-started.md) - Setup instructions
- [API Reference](../docs/api-reference.md) - Command reference
- [Architecture Guide](../docs/architecture.md) - Design details
- [FAQ](../docs/faq.md) - Common questions

---

**Last Updated**: 2026-01-15

**Examples Maintained By**: [Vladyslav Zaiets](https://sarmkadan.com)
