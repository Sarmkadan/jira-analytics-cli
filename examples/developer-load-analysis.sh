#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Developer Load Analysis
# Analyzes individual developer workload and productivity metrics.
# Useful for identifying overloaded team members and recognizing high performers.
# =============================================================================

set -euo pipefail

PROJECT="${1:-MYPROJECT}"
SPRINT_COUNT="${2:-5}"

echo "👥 Developer Load Analysis"
echo "Project: $PROJECT"
echo "Sprints: $SPRINT_COUNT"
echo ""

# Create temporary JSON file
temp_json="/tmp/dev_load_${PROJECT}.json"

# Export metrics
echo "Fetching metrics..."
jira-analytics-cli export -p "$PROJECT" -f json -o "$temp_json" || {
    echo "Error: Could not fetch metrics for $PROJECT"
    exit 1
}

# Check if jq is available for JSON parsing
if command -v jq &> /dev/null; then
    echo ""
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║           DEVELOPER WORKLOAD & PRODUCTIVITY                ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    echo ""

    # Get total developers
    dev_count=$(jq '.developers | length' "$temp_json")
    echo "Total Team Members: $dev_count"
    echo ""

    # Create a formatted table
    printf "%-25s %-15s %-12s %-15s\n" "Developer" "Issues Done" "Completion" "Avg Cycle"
    printf "%-25s %-15s %-12s %-15s\n" "-" "-" "-" "-"

    # Process each developer
    jq -r '.developers[] | "\(.name)|\(.completedIssues)|\(.completionRate)|\(.averageCycleTime)"' "$temp_json" | \
    while IFS='|' read -r name issues completion cycle; do
        # Calculate completion percentage
        completion_pct=$(printf "%.1f%%" $(echo "$completion * 100" | bc -l))
        printf "%-25s %-15s %-12s %-15s\n" "$name" "$issues" "$completion_pct" "$cycle"
    done

    echo ""
    echo "Analysis:"
    echo ""

    # Find top performer
    top_dev=$(jq -r '.developers | max_by(.completionRate) | .name' "$temp_json")
    top_rate=$(jq -r '.developers | max_by(.completionRate) | .completionRate' "$temp_json")
    echo "🏆 Top Performer: $top_dev ($(printf "%.1f%%" $(echo "$top_rate * 100" | bc -l)))"

    # Find most productive
    most_productive=$(jq -r '.developers | max_by(.completedIssues) | .name' "$temp_json")
    most_issues=$(jq -r '.developers | max_by(.completedIssues) | .completedIssues' "$temp_json")
    echo "🚀 Most Productive: $most_productive ($most_issues issues)"

    # Find potentially overloaded (if data available)
    avg_issues=$(jq '[.developers[].completedIssues] | add / length' "$temp_json")
    overloaded=$(jq -r ".developers[] | select(.completedIssues > $avg_issues * 1.3) | .name" "$temp_json" | head -3)
    if [ -n "$overloaded" ]; then
        echo "⚠️  Potentially Overloaded:"
        echo "$overloaded" | sed 's/^/   - /'
    fi

    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "Metrics Explanation:"
    echo "  • Completion Rate: % of assigned issues completed"
    echo "  • Avg Cycle Time: Average time to complete an issue"
    echo "  • Issues Done: Total issues completed in period"
    echo ""

else
    echo ""
    echo "Note: Install 'jq' for formatted output:"
    echo "  brew install jq          # macOS"
    echo "  apt-get install jq       # Debian/Ubuntu"
    echo "  choco install jq         # Windows"
    echo ""
    echo "Raw metrics exported to: $temp_json"
fi

# Cleanup
rm -f "$temp_json"
