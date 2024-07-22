// =============================================================================
// Advanced Usage Example for jira-analytics-cli
// Demonstrates advanced configuration, custom options, and error handling
// =============================================================================

using System;
using System.IO;
using System.Threading.Tasks;

// This example shows advanced usage patterns including:
// - Custom configuration files
// - Multiple output formats
// - Error handling
// - Working with output directories
// - Advanced JQL queries

class AdvancedUsage
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Advanced Usage Examples ===\n");

        // Example 1: Custom configuration file
        Console.WriteLine("Example 1: Using custom appsettings.json configuration");
        Console.WriteLine("Create a custom configuration file (e.g., appsettings.custom.json):");
        Console.WriteLine(@"{
  "JiraBaseUrl": "https://your-instance.atlassian.net",
  "JiraApiToken": "your-api-token",
  "JiraUsername": "your-email@example.com",
  "LogLevel": "Debug",
  "CacheEnabled": true,
  "CacheDurationMinutes": 30
}");
        Console.WriteLine("\nRun with custom configuration:");
        Console.WriteLine("  dotnet run -- --configuration custom analytics -p YOUR_PROJECT\n");

        // Example 2: Export to multiple formats
        Console.WriteLine("Example 2: Exporting data to different formats");
        Console.WriteLine("JSON format (structured data for analysis):");
        Console.WriteLine("  dotnet run -- export -p YOUR_PROJECT -f json -o metrics.json");
        Console.WriteLine("\nCSV format (spreadsheet-friendly):");
        Console.WriteLine("  dotnet run -- export -p YOUR_PROJECT -f csv -o metrics.csv");
        Console.WriteLine("\nPNG chart image:");
        Console.WriteLine("  dotnet run -- export -p YOUR_PROJECT -f png -o velocity-chart.png\n");

        // Example 3: Working with output directories
        Console.WriteLine("Example 3: Organizing reports in directories");
        Console.WriteLine("Save to specific directory:");
        Console.WriteLine("  dotnet run -- analytics -p YOUR_PROJECT --output-dir ./reports");
        Console.WriteLine("\nSave to specific file in directory:");
        Console.WriteLine("  dotnet run -- analytics -p YOUR_PROJECT -o ./reports/velocity.txt");
        Console.WriteLine("\nCreate dated report directories:");
        Console.WriteLine("  dotnet run -- report -p YOUR_PROJECT --output-dir ./reports/2026-06\n");

        // Example 4: Advanced JQL queries
        Console.WriteLine("Example 4: Complex JQL queries");
        Console.WriteLine("Filter by sprint:");
        Console.WriteLine("  dotnet run -- jql -q \"project = YOUR_PROJECT AND Sprint = 'Sprint 42'\"");
        Console.WriteLine("\nFind overdue issues:");
        Console.WriteLine("  dotnet run -- jql -q \"project = YOUR_PROJECT AND status != Done AND due <= now()\"");
        Console.WriteLine("\nFind high priority issues:");
        Console.WriteLine("  dotnet run -- jql -q \"project = YOUR_PROJECT AND priority = High ORDER BY created DESC\"");
        Console.WriteLine("\nPagination with start-at:");
        Console.WriteLine("  dotnet run -- jql -q \"project = YOUR_PROJECT\" -n 100 --start-at 100\n");

        // Example 5: Custom sprint analysis
        Console.WriteLine("Example 5: Analyzing specific number of sprints");
        Console.WriteLine("Analyze last 10 sprints:");
        Console.WriteLine("  dotnet run -- analytics -p YOUR_PROJECT -s 10");
        Console.WriteLine("\nAnalyze last 3 sprints and save to file:");
        Console.WriteLine("  dotnet run -- analytics -p YOUR_PROJECT -s 3 -o last-3-sprints.txt\n");

        // Example 6: HTML reports with customization
        Console.WriteLine("Example 6: Generating HTML reports");
        Console.WriteLine("Basic HTML report:");
        Console.WriteLine("  dotnet run -- report -p YOUR_PROJECT -o report.html");
        Console.WriteLine("\nHTML report with 10 sprints:");
        Console.WriteLine("  dotnet run -- report -p YOUR_PROJECT -s 10 -o detailed-report.html");
        Console.WriteLine("\nHTML report to specific directory:");
        Console.WriteLine("  dotnet run -- report -p YOUR_PROJECT --output-dir ./reports/html\n");

        // Example 7: Team comparison with custom options
        Console.WriteLine("Example 7: Team comparisons");
        Console.WriteLine("Compare 2 teams:");
        Console.WriteLine("  dotnet run -- team-compare -p BACKEND,FRONTEND");
        Console.WriteLine("\nCompare 3 teams with 10 sprints, output to JSON:");
        Console.WriteLine("  dotnet run -- team-compare -p TEAM_A,TEAM_B,TEAM_C -s 10 -f json -o comparison.json\n");

        // Example 8: Burndown charts
        Console.WriteLine("Example 8: Burndown charts");
        Console.WriteLine("Generate burndown for specific sprint:");
        Console.WriteLine("  dotnet run -- burndown -p YOUR_PROJECT --sprint-id 42 -o burndown.png");
        Console.WriteLine("\nSave burndown to reports directory:");
        Console.WriteLine("  dotnet run -- burndown -p YOUR_PROJECT --sprint-id 42 --output-dir ./reports\n");

        // Example 9: Error handling and validation
        Console.WriteLine("Example 9: Error handling best practices");
        Console.WriteLine("Check if project exists:");
        Console.WriteLine("  dotnet run -- analytics -p NON_EXISTENT_PROJECT 2>&1 | grep -i error");
        Console.WriteLine("\nValidate JQL query syntax:");
        Console.WriteLine("  dotnet run -- jql -q \"project = INVALID_QUERY\" 2>&1 | head -20");
        Console.WriteLine("\nHandle API rate limits:");
        Console.WriteLine("  # Add retry logic in your scripts or wait before retrying");
        Console.WriteLine("  sleep 60 && dotnet run -- analytics -p YOUR_PROJECT\n");

        // Example 10: Automation with shell scripts
        Console.WriteLine("Example 10: Automation scripts");
        Console.WriteLine("Create a weekly report script (weekly-report.sh):");
        Console.WriteLine(@"#!/bin/bash
# Weekly Jira analytics report generator

PROJECT_KEY=\"YOUR_PROJECT\"
DATE=$(date +%Y-%m-%d)
OUTPUT_DIR=\"./weekly-reports/\"

mkdir -p \"$OUTPUT_DIR\"

# Generate analytics
echo \"Generating weekly analytics...\"
dotnet run -- analytics -p \"$PROJECT_KEY\" --output-dir \"$OUTPUT_DIR\" -o \"$OUTPUT_DIR/analytics-$DATE.txt\"

# Generate HTML report
echo \"Generating HTML report...\"
dotnet run -- report -p \"$PROJECT_KEY\" --output-dir \"$OUTPUT_DIR\" -o \"$OUTPUT_DIR/report-$DATE.html\"

# Generate team comparison
echo \"Generating team comparison...\"
dotnet run -- team-compare -p BACKEND,FRONTEND -f json --output-dir \"$OUTPUT_DIR\" -o \"$OUTPUT_DIR/team-comparison-$DATE.json\"

echo \"Reports generated in $OUTPUT_DIR\"");
        Console.WriteLine("\nMake it executable and run:");
        Console.WriteLine("  chmod +x weekly-report.sh");
        Console.WriteLine("  ./weekly-report.sh\n");

        // Example 11: Docker usage
        Console.WriteLine("Example 11: Using Docker container");
        Console.WriteLine("Build Docker image:");
        Console.WriteLine("  docker build -t jira-analytics-cli .");
        Console.WriteLine("\nRun in container (Linux/macOS):");
        Console.WriteLine(@"  docker run --rm \\
  -e JIRA_BASE_URL=\"https://your-instance.atlassian.net\" \\
  -e JIRA_API_TOKEN=\"your-token\" \\
  -e JIRA_USERNAME=\"your-email@example.com\" \\
  -v \"$(pwd)/output:/app/output\" \\
  jira-analytics-cli analytics -p YOUR_PROJECT -o /app/output/report.txt");
        Console.WriteLine("\nRun in container (Windows PowerShell):");
        Console.WriteLine(@"  docker run --rm `$`");
        Console.WriteLine(@"  -e JIRA_BASE_URL=\"https://your-instance.atlassian.net\" `$`");
        Console.WriteLine(@"  -e JIRA_API_TOKEN=\"your-token\" `$`");
        Console.WriteLine(@"  -e JIRA_USERNAME=\"your-email@example.com\" `$`");
        Console.WriteLine(@"  -v ${PWD}/output:/app/output `$`");
        Console.WriteLine(@"  jira-analytics-cli analytics -p YOUR_PROJECT -o /app/output/report.txt");

        // Example 12: CI/CD integration
        Console.WriteLine("\nExample 12: CI/CD pipeline integration");
        Console.WriteLine("GitHub Actions example (.github/workflows/jira-analytics.yml):");
        Console.WriteLine(@"name: Jira Analytics Report
on:
  schedule:
    - cron: '0 9 * * 1'  # Every Monday at 9 AM
  workflow_dispatch:

jobs:
  report:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Generate report
        env:
          JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
          JIRA_API_TOKEN: ${{ secrets.JIRA_API_TOKEN }}
          JIRA_USERNAME: ${{ secrets.JIRA_USERNAME }}
        run: |
          mkdir -p reports
          dotnet run -- analytics -p YOUR_PROJECT --output-dir ./reports -o ./reports/weekly-report.txt

      - name: Upload report
        uses: actions/upload-artifact@v4
        with:
          name: jira-analytics-report
          path: reports/");

        Console.WriteLine("\n=== Tips ===");
        Console.WriteLine("1. Store API tokens in environment variables or secrets, never in code");
        Console.WriteLine("2. Use --output-dir to organize multiple reports");
        Console.WriteLine("3. Combine commands in scripts for automation");
        Console.WriteLine("4. Validate JQL queries in Jira web interface first");
        Console.WriteLine("5. Monitor API rate limits when running automated reports");
    }
}
