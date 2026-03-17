#!/bin/bash
# Example 2: Add Widgets to Dashboard

set -e  # Exit on error

echo "=== Example 2: Add Widgets to Dashboard ==="
echo ""

# Ensure we're in the project directory
cd /tmp/oss-dev2/jira-analytics-cli

# Add a velocity chart widget
echo "Adding velocity chart widget..."
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type velocity-chart \
  --title "Sprint Velocity Trends" \
  --position "0,0" \
  --width 2 \
  --height 1

echo ""

echo "Adding developer load widget..."
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type developer-load \
  --title "Developer Workload Distribution" \
  --position "1,0" \
  --width 2 \
  --height 1

echo ""

echo "Adding burndown chart widget..."
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type burndown-chart \
  --title "Current Sprint Burndown" \
  --position "0,1" \
  --width 2 \
  --height 1

echo ""

echo "Adding sprint summary widget..."
dotnet run -- dashboard add-widget \
  -p MYPROJECT \
  -n "team-overview" \
  --widget-type sprint-summary \
  --title "Sprint Health Overview" \
  --position "1,1" \
  --width 2 \
  --height 1

echo ""

echo "✅ Example 2 completed successfully!"
