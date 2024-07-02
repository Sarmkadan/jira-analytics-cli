#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Analyze Recent Sprints
# This script generates a comprehensive analysis of the last N sprints
# and saves the report with a timestamp for easy reference.
# =============================================================================

set -euo pipefail

# Configuration
PROJECT="${1:-MYPROJECT}"
SPRINT_COUNT="${2:-5}"
OUTPUT_DIR="${3:-.}"

if [ ! -d "$OUTPUT_DIR" ]; then
    mkdir -p "$OUTPUT_DIR"
fi

# Generate filename with timestamp
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
REPORT_FILE="$OUTPUT_DIR/sprint-analysis_${PROJECT}_${TIMESTAMP}.txt"

echo "📊 Analyzing $SPRINT_COUNT sprints for project: $PROJECT"
echo "📁 Output directory: $OUTPUT_DIR"
echo ""

# Run the analytics command
jira-analytics-cli analytics \
    -p "$PROJECT" \
    -s "$SPRINT_COUNT" \
    -o "$REPORT_FILE"

# Check if report was created
if [ -f "$REPORT_FILE" ]; then
    FILE_SIZE=$(du -h "$REPORT_FILE" | cut -f1)
    LINES=$(wc -l < "$REPORT_FILE")

    echo ""
    echo "✅ Report generated successfully"
    echo "   File: $REPORT_FILE"
    echo "   Size: $FILE_SIZE"
    echo "   Lines: $LINES"
    echo ""
    echo "📋 Report preview (first 20 lines):"
    echo "---"
    head -20 "$REPORT_FILE"
    echo "---"
    echo ""
    echo "💡 Tip: View the full report with:"
    echo "   cat $REPORT_FILE"
    echo "   less $REPORT_FILE"
else
    echo "❌ Failed to generate report"
    exit 1
fi
