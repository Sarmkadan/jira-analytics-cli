# Migration Guide: v1.x to v2.0

This guide helps you migrate from **jira-analytics-cli v1.x** to **v2.0**, which introduces the new custom dashboard builder with drag-and-drop widgets and saved layouts.

---

## 📋 Table of Contents

- [What's New in v2.0](#whats-new-in-v20)
- [Breaking Changes](#breaking-changes)
- [Migration Steps](#migration-steps)
- [Configuration Changes](#configuration-changes)
- [Code Examples: Old vs New](#code-examples-old-vs-new)
- [Dashboard Builder API Reference](#dashboard-builder-api-reference)
- [Troubleshooting](#troubleshooting)

---

## ⭐ What's New in v2.0

### 1. Custom Dashboard Builder

**v2.0** introduces a powerful new dashboard builder that allows you to:

- 🎨 **Drag-and-drop widgets** – Arrange widgets visually with simple commands
- 💾 **Save and load layouts** – Persist custom dashboard configurations
- 📊 **Multiple widget types** – Choose from different visualization options
- 🔄 **Dynamic updates** – Modify dashboards without restarting the CLI
- 🎯 **Project-specific dashboards** – Create different layouts per project

### 2. New Widget Types

| Widget Type | Description | Example Use Case |
|------------|-------------|----------------|
| `velocity-chart` | Sprint velocity trends over time | Track team performance across sprints |
| `developer-load` | Developer workload distribution | Identify over/under-utilized team members |
| `burndown-chart` | Sprint burndown visualization | Monitor sprint progress in real-time |
| `issue-metrics` | Issue statistics and trends | Track bug rates and completion metrics |
| `sprint-summary` | Sprint overview with key metrics | Quick sprint health check |

### 3. Dashboard Layout Management

- 📁 **Save layouts** to JSON files for reuse
- 🔄 **Load layouts** from saved configurations
- 🔧 **Modify layouts** dynamically via CLI commands
- 📊 **Export dashboards** as images or PDFs

### 4. Enhanced Export Capabilities

- 🖼️ **Dashboard export** – Export complete dashboards as PNG/PDF
- 📄 **Multi-format support** – Export to JSON, CSV, PNG, JPG, PDF
- 🎯 **Custom output paths** – Save to specific directories

### 5. Performance Improvements

- ⚡ **Faster rendering** – Optimized widget rendering engine
- 💾 **Reduced memory usage** – Efficient layout management
- 🚀 **Parallel processing** – Faster dashboard generation

---

## ⚠️ Breaking Changes

**Good news:** v2.0 maintains backward compatibility for most use cases. However, there are a few changes to be aware of:


### 1. Command Structure Changes

| v1.x Command | v2.0 Equivalent | Notes |
|--------------|-----------------|-------|
| `analytics` | `analytics` | No breaking changes |
| `export` | `export` | No breaking changes |
| `burndown` | `burndown` | No breaking changes |
| `dashboard` | `dashboard` | **NEW COMMAND** – Added for dashboard operations |

### 2. Configuration File Changes

The configuration structure remains the same, but new fields have been added:

```json
{
  "dashboard": {
    "defaultLayout": "team-overview",
    "autoSaveLayout": true,
    "widgetSettings": {
      "velocityChart": {
        "showTrendLine": true,
        "colorScheme": "blue"
      }
    }
  }
}
```

### 3. API Changes

No breaking changes to public APIs. All existing services (`IAnalyticsService`, `IExportService`, etc.) maintain their interfaces.

### 4. Environment Variables

New environment variables added (all optional):

```bash
# Dashboard settings
export DASHBOARD_DEFAULT_LAYOUT="team-overview"
export DASHBOARD_AUTOSAVE=true

# Widget customization
export WIDGET_VELOCITY_COLOR_SCHEME="blue"
export WIDGET_DEVELOPER_LOAD_SHOW_RANKINGS=true
```

---

## 🔄 Migration Steps

### Step 1: Update Your Configuration

1. **Backup your existing configuration**
   ```bash
   cp appsettings.json appsettings.backup.json
   ```

2. **Update appsettings.json** (if you have custom settings)
   ```bash
   # Add dashboard configuration
   {
     "dashboard": {
       "defaultLayout": "team-overview",
       "autoSaveLayout": true
     }
   }
   ```

3. **Verify environment variables**
   ```bash
   # Remove any deprecated variables
   unset OLD_VARIABLE_NAME
   
   # Add new variables (optional)
   export DASHBOARD_DEFAULT_LAYOUT="team-overview"
   ```

### Step 2: Install v2.0

```bash
# Clone the repository (or pull latest)
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli

# Checkout v2.0 branch
git checkout v2.0

# Build the project
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./dist --self-contained
```

### Step 3: Test Existing Commands

Verify that your existing commands still work:

```bash
# Test analytics command
./dist/jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt

# Test export command
dotnet run -- export -p MYPROJECT -f json -o metrics.json

# Test burndown command
dotnet run -- burndown -p MYPROJECT --sprint-id 42 -o burndown.png
```

### Step 4: Try the New Dashboard Features

#### Create a New Dashboard
```bash
# Create a new dashboard with default layout
dotnet run -- dashboard create -p MYPROJECT -n "team-overview" -o dashboard.json

# View the dashboard configuration
cat dashboard.json
```

#### Add Widgets to Your Dashboard
```bash
# Add a velocity chart widget
dotnet run -- dashboard add-widget -p MYPROJECT -n "team-overview" \
  --widget-type velocity-chart \
  --title "Sprint Velocity" \
  --position "0,0"

# Add a developer load widget
dotnet run -- dashboard add-widget -p MYPROJECT -n "team-overview" \
  --widget-type developer-load \
  --title "Developer Workload" \
  --position "1,0"
```

#### Export Your Dashboard
```bash
# Export dashboard as PNG
dotnet run -- dashboard export -p MYPROJECT -n "team-overview" \
  -f png -o dashboard.png

# Export as PDF
dotnet run -- dashboard export -p MYPROJECT -n "team-overview" \
  -f pdf -o dashboard.pdf
```

#### Load and Use a Saved Dashboard
```bash
# Load an existing dashboard
dotnet run -- dashboard load -p MYPROJECT -n "team-overview"

# Generate analytics using the loaded dashboard
dotnet run -- analytics -p MYPROJECT -s 5 --dashboard-name "team-overview"
```

---

## 📝 Configuration Changes

### appsettings.json Changes

#### Before (v1.x)
```json
{
  "jiraConfiguration": {
    "baseUrl": "https://your-instance.atlassian.net",
    "apiToken": "your-token",
    "defaultProject": "MYPROJECT"
  },
  "caching": {
    "expirationMinutes": 15,
    "maxItems": 1000
  }
}
```

#### After (v2.0)
```json
{
  "jiraConfiguration": {
    "baseUrl": "https://your-instance.atlassian.net",
    "apiToken": "your-token",
    "defaultProject": "MYPROJECT"
  },
  "caching": {
    "expirationMinutes": 15,
    "maxItems": 1000
  },
  "dashboard": {
    "defaultLayout": "team-overview",
    "autoSaveLayout": true,
    "widgetSettings": {
      "velocityChart": {
        "showTrendLine": true,
        "colorScheme": "blue"
      },
      "developerLoad": {
        "showRankings": true,
        "showCompletionRates": true
      }
    }
  }
}
```

### New Environment Variables

| Variable | Description | Default Value |
|----------|-------------|---------------|
| `DASHBOARD_DEFAULT_LAYOUT` | Default dashboard layout name | `"team-overview"` |
| `DASHBOARD_AUTOSAVE_LAYOUT` | Auto-save dashboard changes | `true` |
| `WIDGET_VELOCITY_COLOR_SCHEME` | Velocity chart color scheme | `"blue"` |
| `WIDGET_DEVELOPER_LOAD_SHOW_RANKINGS` | Show developer rankings | `true` |
| `WIDGET_BURNDOWN_SHOW_IDEAL_LINE` | Show ideal burndown line | `true` |

---

## 💻 Code Examples: Old vs New

### Example 1: Basic Analytics (No Changes)

#### v1.x
```bash
jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt
```

#### v2.0 (Same Command)
```bash
jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt
```

---

### Example 2: Create and Use a Dashboard

#### v1.x (No Dashboard Support)
```bash
# No equivalent - dashboards didn't exist
```

#### v2.0 (New Dashboard Features)
```bash
# Step 1: Create a new dashboard
jira-analytics-cli dashboard create -p MYPROJECT -n "team-overview" -o dashboard.json

# Step 2: Add widgets to the dashboard
jira-analytics-cli dashboard add-widget -p MYPROJECT -n "team-overview" \
  --widget-type velocity-chart \
  --title "Sprint Velocity" \
  --position "0,0"

jira-analytics-cli dashboard add-widget -p MYPROJECT -n "team-overview" \
  --widget-type developer-load \
  --title "Developer Workload" \
  --position "1,0"

# Step 3: Export the dashboard as an image
jira-analytics-cli dashboard export -p MYPROJECT -n "team-overview" \
  -f png -o dashboard.png

# Step 4: Use the dashboard in analytics
jira-analytics-cli analytics -p MYPROJECT -s 5 --dashboard-name "team-overview"
```

---

### Example 3: Dynamic Dashboard Updates

#### v1.x (Static Reports)
```bash
# Generate a report once
jira-analytics-cli analytics -p MYPROJECT -s 5 -o report.txt
```

#### v2.0 (Dynamic Dashboards)
```bash
# Create a dashboard
jira-analytics-cli dashboard create -p MYPROJECT -n "team-overview" -o dashboard.json

# Continuously update the dashboard (e.g., in a cron job)
while true; do
  # Update dashboard with latest data
  jira-analytics-cli dashboard update -p MYPROJECT -n "team-overview"
  
  # Export as image
  jira-analytics-cli dashboard export -p MYPROJECT -n "team-overview" \
    -f png -o /var/www/dashboard/latest.png
  
  sleep 3600 # Update hourly
end
```

---

### Example 4: Multi-Project Dashboard

#### v1.x (Separate Reports)
```bash
# Generate separate reports for each project
for project in BACKEND FRONTEND DEVOPS; do
  jira-analytics-cli analytics -p "$project" -s 5 -o "${project}-report.txt"
done
```

#### v2.0 (Consolidated Dashboard)
```bash
# Create a dashboard for each project
for project in BACKEND FRONTEND DEVOPS; do
  # Create dashboard
  jira-analytics-cli dashboard create -p "$project" -n "${project}-overview" \
    -o "dashboards/${project}-dashboard.json"
  
  # Add widgets
  jira-analytics-cli dashboard add-widget -p "$project" -n "${project}-overview" \
    --widget-type sprint-summary \
    --title "${project} Sprint Summary" \
    --position "0,0"
  
  # Export as PDF
  jira-analytics-cli dashboard export -p "$project" -n "${project}-overview" \
    -f pdf -o "reports/${project}-dashboard.pdf"
done
```

---

### Example 5: CI/CD Integration with Dashboards

#### v1.x (Basic Analytics)
```yaml
# GitHub Actions example
- name: Generate Jira Analytics
  run: |
    jira-analytics-cli analytics -p ${{ env.JIRA_PROJECT }} -s 5 -o report.txt
    jira-analytics-cli export -p ${{ env.JIRA_PROJECT }} -f json -o metrics.json
```

#### v2.0 (Dashboard with Visualization)
```yaml
# GitHub Actions example with dashboards
- name: Generate Jira Dashboards
  run: |
    # Create dashboard
    jira-analytics-cli dashboard create -p ${{ env.JIRA_PROJECT }} -n "ci-dashboard" \
      -o dashboard.json
    
    # Add widgets
    jira-analytics-cli dashboard add-widget -p ${{ env.JIRA_PROJECT }} -n "ci-dashboard" \
      --widget-type velocity-chart --title "Velocity" --position "0,0"
    
    jira-analytics-cli dashboard add-widget -p ${{ env.JIRA_PROJECT }} -n "ci-dashboard" \
      --widget-type burndown-chart --title "Burndown" --position "0,1"
    
    # Export dashboard
    jira-analytics-cli dashboard export -p ${{ env.JIRA_PROJECT }} -n "ci-dashboard" \
      -f png -o dashboard.png
    
    # Export data for BI tools
    jira-analytics-cli export -p ${{ env.JIRA_PROJECT }} -f json -o metrics.json

- name: Archive Dashboards
  uses: actions/upload-artifact@v3
  with:
    name: jira-dashboards
    path: |
      dashboard.png
      dashboard.json
      metrics.json
```

---

## 📚 Dashboard Builder API Reference

### Commands

| Command | Description | Example |
|---------|-------------|---------|
| `dashboard create` | Create a new dashboard | `dashboard create -p MYPROJECT -n "my-dashboard"` |
| `dashboard load` | Load a dashboard | `dashboard load -p MYPROJECT -n "my-dashboard"` |
| `dashboard save` | Save a dashboard | `dashboard save -p MYPROJECT -n "my-dashboard"` |
| `dashboard add-widget` | Add a widget to dashboard | `dashboard add-widget -p MYPROJECT -n "my-dashboard" --widget-type velocity-chart` |
| `dashboard remove-widget` | Remove a widget | `dashboard remove-widget -p MYPROJECT -n "my-dashboard" --widget-id "vel-1"` |
| `dashboard update` | Update dashboard data | `dashboard update -p MYPROJECT -n "my-dashboard"` |
| `dashboard export` | Export dashboard as image/PDF | `dashboard export -p MYPROJECT -n "my-dashboard" -f png -o dashboard.png` |
| `dashboard list` | List available dashboards | `dashboard list -p MYPROJECT` |
| `dashboard show` | Show dashboard configuration | `dashboard show -p MYPROJECT -n "my-dashboard"` |

---

### Widget Types

| Widget Type | Description | Available Options |
|-------------|-------------|------------------|
| `velocity-chart` | Sprint velocity trends | `showTrendLine`, `colorScheme` |
| `developer-load` | Developer workload | `showRankings`, `showCompletionRates` |
| `burndown-chart` | Sprint burndown | `showIdealLine`, `colorScheme` |
| `issue-metrics` | Issue statistics | `showBreakdown`, `metricsToShow` |
| `sprint-summary` | Sprint overview | `showDetails`, `showHealthScore` |

---

### Create a Dashboard

```bash
jira-analytics-cli dashboard create \
  -p MYPROJECT \
  -n "team-overview" \
  -o dashboard.json \
  --description "Team overview dashboard with velocity and workload metrics"
```

**Options:**
- `-p, --project` (required): Project key
- `-n, --name` (required): Dashboard name
- `-o, --output` (required): Output file path
- `--description`: Optional dashboard description
- `--template`: Template to use (default, team-overview, project-overview)

---

### Add a Widget to Dashboard

```bash
jira-analytics-cli dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type velocity-chart \
  --title "Sprint Velocity Trends" \
  --position "0,0" \
  --width 2 \
  --height 1
```

**Options:**
- `-p, --project` (required): Project key
- `-n, --dashboard-name` (required): Dashboard name
- `--widget-type` (required): Widget type (velocity-chart, developer-load, etc.)
- `--title`: Widget title
- `--position`: Grid position (row,column format, e.g., "0,0")
- `--width`: Widget width (default: 1)
- `--height`: Widget height (default: 1)
- `--config`: Widget-specific configuration as JSON string

**Widget Type Examples:**

```bash
# Velocity Chart
--widget-type velocity-chart --config '{"showTrendLine": true, "colorScheme": "blue"}'

# Developer Load
--widget-type developer-load --config '{"showRankings": true}'

# Burndown Chart
--widget-type burndown-chart --config '{"showIdealLine": true}'
```

---

### Export a Dashboard

```bash
jira-analytics-cli dashboard export \
  -p MYPROJECT \
  -n "team-overview" \
  -f png \
  -o dashboard.png \
  --width 1920 \
  --height 1080
```

**Options:**
- `-p, --project` (required): Project key
- `-n, --name` (required): Dashboard name
- `-f, --format` (required): Output format (png, jpg, pdf, json)
- `-o, --output` (required): Output file path
- `--width`: Output image width (default: 1920)
- `--height`: Output image height (default: 1080)
- `--transparent`: Export with transparent background (PNG only)

---

### Update Dashboard Data

```bash
jira-analytics-cli dashboard update -p MYPROJECT -n "team-overview"
```

Updates all widgets in the dashboard with fresh data from Jira.

---

### Save Dashboard Configuration

```bash
jira-analytics-cli dashboard save -p MYPROJECT -n "team-overview" -o dashboard.json
```

Saves the current dashboard configuration to a file.

---

### List Available Dashboards

```bash
jira-analytics-cli dashboard list -p MYPROJECT
```

Lists all saved dashboards for a project.

---

### Show Dashboard Configuration

```bash
jira-analytics-cli dashboard show -p MYPROJECT -n "team-overview"
```

Displays the dashboard configuration in JSON format.

---

## 🐛 Troubleshooting

### Dashboard Not Saving

**Problem:** Dashboard changes aren't persisted

**Solutions:**
1. Check file permissions: `chmod 644 dashboard.json`
2. Verify output directory exists: `mkdir -p dashboards`
3. Check auto-save setting: `DASHBOARD_AUTOSAVE_LAYOUT=true`


### Widget Not Appearing

**Problem:** Added widget doesn't show up

**Solutions:**
1. Verify widget type is correct (check available types)
2. Check position format: `--position "0,0"` (row,column)
3. Update dashboard data: `dashboard update -p MYPROJECT -n "dashboard-name"`
4. Verify dashboard is loaded: `dashboard load -p MYPROJECT -n "dashboard-name"`

### Export Failing

**Problem:** Dashboard export produces empty or corrupted file

**Solutions:**
1. Check disk space: `df -h`
2. Verify output directory is writable
3. Try simpler format first (JSON before PNG)
4. Check for SkiaSharp/Skia dependencies

### Performance Issues

**Problem:** Dashboard rendering is slow

**Solutions:**
1. Reduce widget complexity
2. Decrease image size: `--width 1200 --height 800`
3. Increase cache expiration: `CACHE_EXPIRATION_MINUTES=30`
4. Disable detailed logging: `ENABLE_DETAILED_LOGGING=false`

---

## 📖 Additional Resources

- [Dashboard Widgets Guide](widgets-guide.md) – Detailed widget configuration
- [Docker Deployment Guide](docker-guide.md) – Running dashboards in containers
- [Examples Directory](../examples/v2-basic-usage/) – Complete v2.0 examples

---

## 🆘 Need Help?

- Check the [FAQ](../docs/faq.md) for common issues
- Review the [examples](../examples/v2-basic-usage/) for working configurations
- Open an issue on GitHub for bugs or feature requests

---

**Migration Complete!** 🎉

Your existing v1.x workflows will continue to work in v2.0. The new dashboard features are completely optional and can be adopted incrementally.
