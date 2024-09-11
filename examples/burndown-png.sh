#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Generate Burndown Chart as PNG
# Creates a visual burndown chart for a specific sprint.
# Perfect for sharing in retrospectives or status reports.
# =============================================================================

set -euo pipefail

PROJECT="${1:-MYPROJECT}"
SPRINT_ID="${2:-}"
OUTPUT_DIR="${3:-./charts}"

if [ -z "$SPRINT_ID" ]; then
    echo "❌ Usage: $0 PROJECT SPRINT_ID [OUTPUT_DIR]"
    echo ""
    echo "Example: $0 MYPROJECT 42 ./charts"
    echo ""
    echo "To find sprint ID, run:"
    echo "  jira-analytics-cli analytics -p $PROJECT -s 1"
    exit 1
fi

if [ ! -d "$OUTPUT_DIR" ]; then
    mkdir -p "$OUTPUT_DIR"
fi

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
PNG_FILE="$OUTPUT_DIR/${PROJECT}_sprint${SPRINT_ID}_burndown_${TIMESTAMP}.png"

echo "📉 Generating Sprint Burndown Chart"
echo "Project: $PROJECT"
echo "Sprint: $SPRINT_ID"
echo ""

# Generate the burndown chart
jira-analytics-cli burndown -p "$PROJECT" --sprint-id "$SPRINT_ID" -o "$PNG_FILE" || {
    echo "❌ Error: Could not generate burndown chart"
    echo ""
    echo "Troubleshooting:"
    echo "  • Check sprint ID is correct"
    echo "  • Verify project has completed sprints"
    echo "  • Ensure you have permission to view the project"
    exit 1
}

if [ -f "$PNG_FILE" ]; then
    FILE_SIZE=$(du -h "$PNG_FILE" | cut -f1)
    echo "✅ Chart generated successfully"
    echo "   File: $PNG_FILE"
    echo "   Size: $FILE_SIZE"
    echo ""
    echo "💡 View the chart:"

    # Detect OS and open appropriate viewer
    if [[ "$OSTYPE" == "darwin"* ]]; then
        echo "   open '$PNG_FILE'"
        open "$PNG_FILE" &
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        echo "   feh '$PNG_FILE'        # Install: apt-get install feh"
        echo "   eog '$PNG_FILE'        # Install: apt-get install eog"
        echo "   xdg-open '$PNG_FILE'"
    elif [[ "$OSTYPE" == "msys" ]]; then
        echo "   start '$PNG_FILE'      # Windows"
    fi

    echo ""
    echo "📧 Share the chart:"
    echo "   • Email it to team members"
    echo "   • Upload to project wiki"
    echo "   • Include in sprint retrospectives"
    echo "   • Add to status reports"

else
    echo "❌ Failed to generate chart"
    exit 1
fi
