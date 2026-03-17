#!/bin/bash
# Example 1: Create Your First Dashboard

set -e  # Exit on error

echo "=== Example 1: Create Your First Dashboard ==="
echo ""

# Ensure we're in the project directory
cd /tmp/oss-dev2/jira-analytics-cli

# Create output directory
mkdir -p dashboards

# Create a new dashboard
echo "Creating dashboard 'team-overview'..."
dotnet run -- dashboard create \
  -p MYPROJECT \
  -n "team-overview" \
  -o dashboards/team-overview.json \
  --description "Team overview with velocity and workload metrics"

echo ""
echo "Dashboard created successfully!"
echo "File: dashboards/team-overview.json"
echo ""

# Show the created dashboard
echo "Dashboard content:"
cat dashboards/team-overview.json

echo ""
echo "✅ Example 1 completed successfully!"
