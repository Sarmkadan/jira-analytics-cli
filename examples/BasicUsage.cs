// =============================================================================
// Basic Usage Example for jira-analytics-cli
// Demonstrates minimal setup and basic command execution
// =============================================================================

using System;
using System.CommandLine;
using System.Threading.Tasks;

// This example shows the simplest way to use jira-analytics-cli
// It assumes you have already set up your environment variables:
// export JIRA_BASE_URL="https://your-instance.atlassian.net"
// export JIRA_API_TOKEN="your-api-token"

class BasicUsage
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== jira-analytics-cli Basic Usage Examples ===\n");

        // Example 1: Basic analytics report for a project
        Console.WriteLine("Example 1: Generate analytics report for a project");
        Console.WriteLine("Command: jira-analytics-cli analytics -p YOUR_PROJECT_KEY\n");

        // Example 2: Export data to JSON
        Console.WriteLine("Example 2: Export analytics data to JSON file");
        Console.WriteLine("Command: jira-analytics-cli export -p YOUR_PROJECT_KEY -f json -o metrics.json\n");

        // Example 3: Generate HTML report
        Console.WriteLine("Example 3: Generate HTML report");
        Console.WriteLine("Command: jira-analytics-cli report -p YOUR_PROJECT_KEY -o report.html\n");

        // Example 4: Run a custom JQL query
        Console.WriteLine("Example 4: Execute a JQL query");
        Console.WriteLine("Command: jira-analytics-cli jql -q \"project = YOUR_PROJECT AND status = \\\"In Progress\\\"\"\n");

        // Example 5: Compare multiple teams
        Console.WriteLine("Example 5: Compare metrics across teams");
        Console.WriteLine("Command: jira-analytics-cli team-compare -p TEAM_A,TEAM_B,TEAM_C\n");

        Console.WriteLine("=== Configuration Setup ===\n");
        Console.WriteLine("Before running these commands, set up your environment:");
        Console.WriteLine("1. Set JIRA_BASE_URL (e.g., https://your-company.atlassian.net)");
        Console.WriteLine("2. Set JIRA_API_TOKEN (create at https://id.atlassian.com/manage-profile/security/api-tokens)");
        Console.WriteLine("3. Set JIRA_USERNAME if required by your instance\n");

        Console.WriteLine("=== Environment Variables ===\n");
        Console.WriteLine("# Linux/macOS");
        Console.WriteLine("export JIRA_BASE_URL=\"https://your-instance.atlassian.net\"");
        Console.WriteLine("export JIRA_API_TOKEN=\"your-api-token-here\"");
        Console.WriteLine("export JIRA_USERNAME=\"your-email@example.com\"\n");

        Console.WriteLine("# Windows (PowerShell)");
        Console.WriteLine("$env:JIRA_BASE_URL=\"https://your-instance.atlassian.net\"");
        Console.WriteLine("$env:JIRA_API_TOKEN=\"your-api-token-here\"");
        Console.WriteLine("$env:JIRA_USERNAME=\"your-email@example.com\"\n");

        Console.WriteLine("=== Quick Start ===");
        Console.WriteLine("After setting up environment variables, try:");
        Console.WriteLine("  dotnet run -- analytics -p YOUR_PROJECT_KEY");
        Console.WriteLine("  dotnet run -- report -p YOUR_PROJECT_KEY -o my-report.html");
        Console.WriteLine("  dotnet run -- jql -q \"project = YOUR_PROJECT\"");
    }
}