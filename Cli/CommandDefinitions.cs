// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Cli;

/// <summary>
/// Centralized definitions of CLI commands, options, and help text.
/// Separates command metadata from execution logic for reusability and testing.
/// </summary>
public class CommandDefinitions
{
    public static class Verbs
    {
        public const string Analytics    = "analytics";
        public const string Export       = "export";
        public const string Burndown     = "burndown";
        public const string Developer    = "developer";
        public const string Health       = "health";
        public const string Jql          = "jql";
        public const string Report       = "report";
        public const string TeamCompare  = "team-compare";
    }

    public static class Options
    {
        public class ProjectOption
        {
            public const string Short = "-p";
            public const string Long = "--project";
            public const string Description = "Jira project key (e.g., PROJ)";
            public const bool Required = true;
        }

        public class SprintsOption
        {
            public const string Short = "-s";
            public const string Long = "--sprints";
            public const string Description = "Number of sprints to analyze";
            public const int Default = 5;
            public const int Min = 1;
            public const int Max = 52;
        }

        public class FormatOption
        {
            public const string Short = "-f";
            public const string Long = "--format";
            public const string Description = "Output format (json, csv, xml, markdown, png, svg)";
            public const string Default = "json";
            public static readonly string[] ValidValues = { "json", "csv", "xml", "markdown", "png", "svg", "pdf" };
        }

        public class OutputOption
        {
            public const string Short = "-o";
            public const string Long = "--output";
            public const string Description = "Output file or image path";
            public const bool Required = true;
        }

        public class DetailedOption
        {
            public const string Short = "-d";
            public const string Long = "--detailed";
            public const string Description = "Include detailed analysis data";
        }

        public class OutputDirOption
        {
            public const string Long = "--output-dir";
            public const string Description = "Directory to save reports to (created if it does not exist; defaults to current directory)";
        }

        public class VerboseOption
        {
            public const string Short = "-v";
            public const string Long = "--verbose";
            public const string Description = "Enable verbose logging";
        }

        public class ConfigOption
        {
            public const string Short = "-c";
            public const string Long = "--config";
            public const string Description = "Configuration file path";
        }
    }

    public static class Commands
    {
        public class AnalyticsCommand
        {
            public const string Name = "analytics";
            public const string Description = "Run sprint velocity and metrics analysis";
            public const string HelpText = @"
Analyze Jira sprint metrics including velocity, story points, developer load.

Examples:
  jira-analytics analytics -p PROJ -s 5
  jira-analytics analytics -p PROJ -s 10 -o report.json -d
";
        }

        public class ExportCommand
        {
            public const string Name = "export";
            public const string Description = "Export analytics data in multiple formats";
            public const string HelpText = @"
Export sprint analytics in JSON, CSV, XML, or Markdown format.

Examples:
  jira-analytics export -p PROJ -f json -o report.json
  jira-analytics export -p PROJ -f csv -o metrics.csv
";
        }

        public class BurndownCommand
        {
            public const string Name = "burndown";
            public const string Description = "Generate burndown chart image";
            public const string HelpText = @"
Generate a burndown chart for a specific sprint as PNG/PDF image.

Examples:
  jira-analytics burndown -p PROJ -s 42 -o burndown.png
  jira-analytics burndown -p PROJ -s 42 -o burndown.pdf
";
        }

        public class DeveloperCommand
        {
            public const string Name = "developer";
            public const string Description = "Analyze individual developer metrics";
            public const string HelpText = @"
Show per-developer metrics including issues assigned, completed, load distribution.

Examples:
  jira-analytics developer -p PROJ -d john.doe
  jira-analytics developer -p PROJ --days 30
";
        }

        public class HealthCommand
        {
            public const string Name = "health";
            public const string Description = "Check Jira API connectivity";
            public const string HelpText = @"
Verify Jira instance connectivity and authentication status.

Examples:
  jira-analytics health
";
        }

        public class JqlCommand
        {
            public const string Name = "jql";
            public const string Description = "Execute a custom JQL query and display results";
            public const string HelpText = @"
Run any JQL (Jira Query Language) query and display or export matching issues.
Supports pagination via --start-at and output as plain text or JSON.

Examples:
  jira-analytics jql -q ""project = PROJ AND status = 'In Progress'""
  jira-analytics jql -q ""assignee = john.doe AND sprint in openSprints()"" -n 20
  jira-analytics jql -q ""priority = Critical ORDER BY created DESC"" -f json -o critical.json
  jira-analytics jql -q ""project = PROJ"" -n 50 --start-at 50
";
        }

        public class ReportCommand
        {
            public const string Name = "report";
            public const string Description = "Generate a self-contained HTML analytics report";
            public const string HelpText = @"
Generate a responsive, self-contained HTML report for a project covering sprint velocity,
completion rates, team workload, and top performers.  The output file can be opened
directly in any browser without additional dependencies.

Examples:
  jira-analytics report -p PROJ -o report.html
  jira-analytics report -p PROJ -s 10 -o reports/sprint-report.html
  jira-analytics report -p PROJ --output-dir ./reports
";
        }

        public class TeamCompareCommand
        {
            public const string Name = "team-compare";
            public const string Description = "Compare metrics across multiple Jira projects side by side";
            public const string HelpText = @"
Compare sprint velocity, completion rates, defect rates, and cycle times across
two or more Jira projects.  Results are ranked to highlight the fastest, highest
quality, and most consistent team.

Examples:
  jira-analytics team-compare -p PROJ1,PROJ2
  jira-analytics team-compare -p ALPHA,BETA,GAMMA -s 10
  jira-analytics team-compare -p PROJ1,PROJ2 -f json -o comparison.json
";
        }
    }

    public static class ValidationRules
    {
        public const int MaxProjectKeyLength = 10;
        public const int MinProjectKeyLength = 2;
        public const int MaxSprintCount = 52;
        public const int MinSprintCount = 1;
        public const int MaxDeveloperNameLength = 255;
        public const int DefaultTimeoutSeconds = 30;
    }

    public static class ErrorMessages
    {
        public const string ProjectKeyRequired = "Project key is required";
        public const string InvalidProjectKey = "Project key must be 2-10 alphanumeric characters";
        public const string InvalidSprintCount = "Sprint count must be 1-52";
        public const string InvalidFormat = "Invalid format. Supported: json, csv, xml, markdown";
        public const string OutputPathRequired = "Output file path is required";
        public const string InvalidOutputPath = "Output path is not writable";
        public const string ConfigurationMissing = "Configuration file not found";
        public const string ApiConnectionFailed = "Failed to connect to Jira API";
    }
}
