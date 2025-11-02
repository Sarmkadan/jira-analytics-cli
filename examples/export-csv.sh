#!/bin/bash

# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Example: Export to CSV and Analyze with Spreadsheet Tools
# Exports metrics in CSV format for import into Excel, Google Sheets, or data analysis tools.
# =============================================================================

set -euo pipefail

PROJECT="${1:-MYPROJECT}"
OUTPUT_DIR="${2:-.}"

if [ ! -d "$OUTPUT_DIR" ]; then
    mkdir -p "$OUTPUT_DIR"
fi

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
CSV_FILE="$OUTPUT_DIR/${PROJECT}_metrics_${TIMESTAMP}.csv"

echo "📊 Exporting metrics to CSV"
echo "Project: $PROJECT"
echo ""

# Export as CSV
jira-analytics-cli export -p "$PROJECT" -f csv -o "$CSV_FILE" || {
    echo "Error: Could not export metrics"
    exit 1
}

if [ -f "$CSV_FILE" ]; then
    FILE_SIZE=$(du -h "$CSV_FILE" | cut -f1)
    LINES=$(wc -l < "$CSV_FILE")

    echo "✅ Export complete"
    echo "   File: $CSV_FILE"
    echo "   Size: $FILE_SIZE"
    echo "   Rows: $LINES"
    echo ""
    echo "📋 Preview (first 5 rows):"
    echo "---"
    head -5 "$CSV_FILE"
    echo "---"
    echo ""
    echo "💡 Next steps:"
    echo "   1. Open with Excel: open $CSV_FILE"
    echo "   2. Import to Google Sheets (File > Import)"
    echo "   3. Analyze with Python:"
    echo "      python3 -c \"import pandas as pd; df = pd.read_csv('$CSV_FILE'); print(df.describe())\""
    echo "      # Install pandas if needed: pip install pandas"
    echo ""
    echo "📈 Sample Python analysis:"
    cat << 'EOF'

import pandas as pd
import numpy as np

# Load the CSV
df = pd.read_csv('metrics.csv')

# Display statistics
print("Sprint Metrics Summary:")
print(df.describe())

# Calculate averages
print("\nAverage Metrics:")
print(f"  Average Velocity: {df['Velocity'].mean():.1f} points")
print(f"  Average Completion: {df['Completion Rate'].mean():.1%}")
print(f"  Average Quality: {df['Quality Score'].mean():.1f}/100")

# Find trends
print("\nTrends:")
if df['Velocity'].iloc[-1] > df['Velocity'].iloc[0]:
    print("  📈 Velocity is increasing")
else:
    print("  📉 Velocity is decreasing")

# Save as Excel
df.to_excel('metrics.xlsx', index=False)
print("\n✅ Saved as metrics.xlsx")
EOF

else
    echo "❌ Export failed"
    exit 1
fi
