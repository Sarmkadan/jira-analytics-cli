#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Velocity Trend Report
# Analyzes velocity trends across multiple projects and generates a
# comparison report useful for management and planning discussions.
# =============================================================================

set -euo pipefail

# Projects to analyze
PROJECTS=("${@:-BACKEND FRONTEND MOBILE}")
SPRINT_COUNT=8
OUTPUT_FILE="velocity-comparison-$(date +%Y%m%d).txt"

{
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║          VELOCITY TREND COMPARISON REPORT                   ║"
    echo "║      Generated: $(date '+%Y-%m-%d %H:%M:%S')"
    echo "╚════════════════════════════════════════════════════════════╝"
    echo ""
    echo "Analysis Period: Last $SPRINT_COUNT sprints"
    echo "Projects Analyzed: ${PROJECTS[*]}"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""

    # Process each project
    for project in $PROJECTS; do
        echo "📊 Project: $project"
        echo ""

        # Create temporary JSON output
        temp_json="/tmp/${project}_metrics.json"

        # Export metrics
        jira-analytics-cli export -p "$project" -f json -o "$temp_json" 2>/dev/null || {
            echo "  ⚠️  Could not retrieve metrics for $project"
            echo ""
            continue
        }

        # Parse and display metrics
        if command -v jq &> /dev/null; then
            echo "  Sprints analyzed: $(jq '.sprints | length' "$temp_json")"

            # Calculate average velocity
            avg_velocity=$(jq '[.sprints[].velocity] | add / length' "$temp_json" 2>/dev/null)
            echo "  Average Velocity: ${avg_velocity%.*} points"

            # Completion rate
            completion=$(jq '[.sprints[].completionRate] | add / length' "$temp_json" 2>/dev/null)
            echo "  Avg Completion: $(printf "%.1f" "$completion")%"

            # Quality score
            quality=$(jq '[.sprints[].qualityScore] | add / length' "$temp_json" 2>/dev/null)
            echo "  Avg Quality: $(printf "%.1f" "$quality")/100"

            # Trend
            latest_velocity=$(jq '.sprints[-1].velocity' "$temp_json")
            first_velocity=$(jq '.sprints[0].velocity' "$temp_json")
            if (( $(echo "$latest_velocity > $first_velocity" | bc -l) )); then
                trend="📈 Increasing"
            elif (( $(echo "$latest_velocity < $first_velocity" | bc -l) )); then
                trend="📉 Decreasing"
            else
                trend="➡️  Stable"
            fi
            echo "  Trend: $trend"
        else
            # Fallback without jq
            jira-analytics-cli analytics -p "$project" -s "$SPRINT_COUNT" 2>/dev/null | grep -E "Velocity|Completion|Quality" || true
        fi

        # Cleanup
        rm -f "$temp_json"
        echo ""
    done

    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "Notes:"
    echo "  • Velocity is measured in story points"
    echo "  • Completion Rate indicates how well the team estimated"
    echo "  • Quality Score considers defects and overdue items"
    echo "  • Trend shows if velocity is improving or declining"
    echo ""
    echo "Report generated: $(date '+%Y-%m-%d %H:%M:%S')"

} | tee "$OUTPUT_FILE"

echo ""
echo "✅ Report saved to: $OUTPUT_FILE"
