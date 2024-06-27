# v2.0 Basic Usage Examples

This directory contains minimal, runnable examples demonstrating the new v2.0 features of jira-analytics-cli, particularly the custom dashboard builder.

---

## 📋 Table of Contents

- [Example 1: Create Your First Dashboard](#example-1-create-your-first-dashboard)
- [Example 2: Add Widgets to Dashboard](#example-2-add-widgets-to-dashboard)
- [Example 3: Export Dashboard as Image](#example-3-export-dashboard-as-image)
- [Example 4: Update Dashboard with Fresh Data](#example-4-update-dashboard-with-fresh-data)
- [Example 5: Load and Use a Saved Dashboard](#example-5-load-and-use-a-saved-dashboard)
- [Running the Examples](#running-the-examples)

---

## 🚀 Example 1: Create Your First Dashboard

This example shows how to create a new dashboard configuration file.

### Command
```bash
# Create a new dashboard for your project
dotnet run -- dashboard create \
  -p MYPROJECT \
  -n "team-overview" \
  -o dashboards/team-overview.json \
  --description "Team overview with velocity and workload metrics"
```

### Expected Output
```json
{
  "name": "team-overview",
  "project": "MYPROJECT",
  "description": "Team overview with velocity and workload metrics",
  "widgets": [],
  "createdAt": "2026-05-18T10:00:00Z",
  "updatedAt": "2026-05-18T10:00:00Z"
}
```

### What This Does
- Creates a new dashboard configuration file
- Sets the project key
- Adds an optional description
- Initializes an empty widgets array

---

## 📊 Example 2: Add Widgets to Dashboard

This example demonstrates adding different types of widgets to your dashboard.

### Velocity Chart Widget
```bash
# Add a velocity chart widget to track sprint velocity trends
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type velocity-chart \
  --title "Sprint Velocity Trends" \
  --position "0,0" \
  --width 2 \
  --height 1
```

### Developer Load Widget
```bash
# Add a developer load widget to monitor team workload
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type developer-load \
  --title "Developer Workload Distribution" \
  --position "1,0" \
  --width 2 \
  --height 1
```

### Burndown Chart Widget
```bash
# Add a burndown chart widget for sprint tracking
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type burndown-chart \
  --title "Current Sprint Burndown" \
  --position "0,1" \
  --width 2 \
  --height 1
```

### Sprint Summary Widget
```bash
# Add a sprint summary widget for quick health check
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type sprint-summary \
  --title "Sprint Health Overview" \
  --position "1,1" \
  --width 2 \
  --height 1
```

### Expected Output (Updated Dashboard JSON)
```json
{
  "name": "team-overview",
  "project": "MYPROJECT",
  "description": "Team overview with velocity and workload metrics",
  "widgets": [
    {
      "id": "vel-1",
      "type": "velocity-chart",
      "title": "Sprint Velocity Trends",
      "position": "0,0",
      "width": 2,
      "height": 1,
      "config": {}
    },
    {
      "id": "dev-1",
      "type": "developer-load",
      "title": "Developer Workload Distribution",
      "position": "1,0",
      "width": 2,
      "height": 1,
      "config": {}
    },
    {
      "id": "burndown-1",
      "type": "burndown-chart",
      "title": "Current Sprint Burndown",
      "position": "0,1",
      "width": 2,
      "height": 1,
      "config": {}
    },
    {
      "id": "summary-1",
      "type": "sprint-summary",
      "title": "Sprint Health Overview",
      "position": "1,1",
      "width": 2,
      "height": 1,
      "config": {}
    }
  ],
  "createdAt": "2026-05-18T10:00:00Z",
  "updatedAt": "2026-05-18T10:00:00Z"
}
```

---

## 🖼️ Example 3: Export Dashboard as Image

This example shows how to export your dashboard as a PNG image.

### Command
```bash
# Export the dashboard as a PNG image
dotnet run -- dashboard export \
  -p MYPROJECT \
  -n "team-overview" \
  -f png \
  -o dashboards/team-overview.png \
  --width 1920 \
  --height 1080
```

### Options
- `-f, --format`: Output format (png, jpg, pdf, json)
- `-o, --output`: Output file path
- `--width`: Image width in pixels (default: 1920)
- `--height`: Image height in pixels (default: 1080)
- `--transparent`: Export with transparent background (PNG only)

### Expected Output
- A PNG file at `dashboards/team-overview.png`
- The image will contain all configured widgets arranged in a grid

---

## 🔄 Example 4: Update Dashboard with Fresh Data

This example demonstrates updating your dashboard with the latest Jira data.

### Command
```bash
# Update the dashboard with fresh data from Jira
dotnet run -- dashboard update -p MYPROJECT -n "team-overview"
```

### What This Does
- Fetches the latest sprint data from Jira
- Updates all widgets with current metrics
- Preserves your widget layout and configuration
- Updates the dashboard's `updatedAt` timestamp

### When to Use
- Before generating reports
- On a schedule (e.g., hourly or daily)
- After Jira data changes
- Before sharing dashboards with stakeholders

---

## 📂 Example 5: Load and Use a Saved Dashboard

This example shows how to load a saved dashboard and use it with analytics commands.

### Command
```bash
# Load an existing dashboard
dotnet run -- dashboard load -p MYPROJECT -n "team-overview"
```

### Using Dashboard with Analytics
```bash
# Generate analytics using the loaded dashboard
dotnet run -- analytics -p MYPROJECT -s 5 --dashboard-name "team-overview" -o reports/analytics-with-dashboard.txt
```

### Using Dashboard with Export
```bash
# Export data with dashboard context
dotnet run -- export -p MYPROJECT -f json -o metrics-with-dashboard.json --dashboard-name "team-overview"
```

### Benefits
- Consistent dashboard configuration across runs
- Easy to share dashboards with team members
- Version-controlled dashboard layouts
- Reusable across different projects

---

## 🏃 Running the Examples

### Prerequisites

1. **Build the project**:
   ```bash
   dotnet build -c Release
   ```

2. **Set up Jira credentials**:
   ```bash
   export JIRA_BASE_URL="https://your-instance.atlassian.net"
   export JIRA_API_TOKEN="your-api-token"
   export JIRA_DEFAULT_PROJECT="MYPROJECT"
   ```

3. **Create output directories**:
   ```bash
   mkdir -p dashboards reports
   ```

### Running All Examples

```bash
# Create dashboard
./examples/v2-basic-usage/example-1-create-dashboard.sh

# Add widgets
./examples/v2-basic-usage/example-2-add-widgets.sh

# Export as image
./examples/v2-basic-usage/example-3-export-dashboard.sh

# Update with fresh data
./examples/v2-basic-usage/example-4-update-dashboard.sh

# Use in analytics
./examples/v2-basic-usage/example-5-use-dashboard.sh
```

### Individual Commands

You can run each example individually:

```bash
# Example 1: Create dashboard
dotnet run -- dashboard create -p MYPROJECT -n "team-overview" -o dashboards/team-overview.json

# Example 2: Add widgets
dotnet run -- dashboard add-widget -p MYPROJECT -n "team-overview" --widget-type velocity-chart --title "Velocity" --position "0,0"
dotnet run -- dashboard add-widget -p MYPROJECT -n "team-overview" --widget-type developer-load --title "Workload" --position "1,0"

# Example 3: Export
dotnet run -- dashboard export -p MYPROJECT -n "team-overview" -f png -o dashboards/team-overview.png

# Example 4: Update
dotnet run -- dashboard update -p MYPROJECT -n "team-overview"

# Example 5: Use in analytics
dotnet run -- analytics -p MYPROJECT -s 5 --dashboard-name "team-overview" -o reports/analytics.txt
```

---

## 📝 Notes

- All examples assume you have a valid Jira API token and project key
- Replace `MYPROJECT` with your actual Jira project key
- Dashboard files are saved as JSON for easy version control
- Widget positions use a grid system (row,column format)
- You can have multiple dashboards per project

---

## 🎯 Next Steps

After mastering these basic examples, try:

1. **Create multiple dashboards** for different stakeholders (managers, developers, product owners)
2. **Customize widget configurations** with colors and metrics to display
3. **Automate dashboard updates** with cron jobs or CI/CD pipelines
4. **Share dashboards** with your team by committing them to version control
5. **Export to PDF** for presentations and reports

---

## 📚 Related Documentation

- [Migration Guide](../docs/migration-guide-v2.md) – Migrating from v1.x to v2.0
- [Dashboard Widgets Guide](../docs/widgets-guide.md) – Detailed widget configuration
- [CLI Reference](../docs/api-reference.md) – Complete command documentation
