## Getting Started

Requires the .NET 10 SDK.

### 1. Install as a global tool

The package isn't published to NuGet, so build it from source and install the local package:

```bash
git clone https://github.com/sarmkadan/jira-analytics-cli.git
cd jira-analytics-cli
dotnet pack JiraAnalyticsCli.csproj -c Release -o ./nupkg
dotnet tool install --global --add-source ./nupkg jira-analytics-cli
jira-analytics-cli --help
```

To upgrade after pulling changes, run `dotnet pack` again, then:

```bash
dotnet tool update --global --add-source ./nupkg jira-analytics-cli
```

### 2. Configure credentials

```bash
export JIRA_URL="https://your-company.atlassian.net"
export JIRA_API_TOKEN="your-api-token"
```

PowerShell: `$env:JIRA_URL = "https://your-company.atlassian.net"` and `$env:JIRA_API_TOKEN = "your-api-token"`.

- `JIRA_URL` (or `JIRA_BASE_URL`) is the base URL of your Jira instance. It must be an absolute `https://` URI.
- `JIRA_API_TOKEN` (or `JIRA_TOKEN`) is sent as a Bearer token. The CLI does not read `JIRA_EMAIL` for authentication; see [Use JiraClient from C#](#6-use-jiraclient-from-c) for the case where it is needed.
- `JIRA_BOARD_ID` is optional. When set, it must be a positive integer.
- `JIRA_API_TIMEOUT_SECONDS` sets the HTTP timeout for Jira requests. It must be between 1 and 300 (default 30).

Each setting can also be given as a CLI flag (`--jira-url`, `--jira-email`, `--jira-token`, `--board-id`, `--timeout-seconds`) or as a key in `appsettings.json` (`jiraBaseUrl`, `jiraEmail`, `jiraApiToken`, `jiraBoardId`, `jiraApiTimeoutSeconds`). Precedence, highest first:

1. CLI flag
2. Environment variable
3. `appsettings.json`
4. Built-in default

An empty environment variable or JSON value counts as unset. `-p/--project` on a command overrides the default project set by `JIRA_PROJECT` or `defaultProject`.

Settings are validated before any command runs and before any HTTP call. If anything is wrong, the CLI prints every problem in one message, names each offending setting and where to set it, and exits with code 2. Help and version requests skip this check. Passing a token as a flag puts it in the process list, so prefer `JIRA_API_TOKEN`.

To check your setup without running a command, use `--check-config`. It checks each setting, then sends one authenticated `GET /rest/api/3/myself` to Jira. It prints `OK`, `FAIL` or `SKIP` for each item and exits with 0 only when nothing failed. The token is never printed.

```bash
jira-analytics-cli --check-config
```

Output options are checked before any Jira call. An unsupported `--format` is rejected and the error lists the supported values. `--sprints` must be between 1 and 50. `--output` and `--output-dir` must point to a path whose directory already exists or can be created. `--check-config` does not check output options, because it takes no output flags.

### 3. Sprint report

`analytics` summarizes the most recent sprints for a project and prints a text report (use `--output` to write it to a file instead):

```bash
jira-analytics-cli analytics --project MYPROJ --sprints 5
```

Example output (abbreviated; the values are illustrative):

```
╔════════════════════════════════════════════════════════════════╗
║       JIRA ANALYTICS REPORT - SPRINT PERFORMANCE SUMMARY       ║
╚════════════════════════════════════════════════════════════════╝

📊 OVERALL METRICS
──────────────────────────────────────────────────────────────────────
Average Velocity:      41.20 pts/sprint
Velocity Trend:        ↑ 8.3%
Overall Health:        Good

📈 SPRINT BREAKDOWN
──────────────────────────────────────────────────────────────────────
Sprint: MYPROJ Sprint 41
  Planned:       45 pts
  Completed:     40 pts (88.9%)
  Issues:        18/21
  Quality:       82.5/100
  Cycle Time:    3.4 days
  Status:        ...
```

### 4. Burndown chart

`burndown` renders a PNG for one sprint (default output is `burndown.png`). Each run also appends a snapshot for that sprint, which `trend` reads.

```bash
jira-analytics-cli burndown --project MYPROJ --sprint-id 4321 --output charts/burndown-4321.png
```

Example output (log lines):

```
info: JiraAnalyticsCli.Program[0]
      Jira Analytics CLI starting...
...
info: JiraAnalyticsCli.Program[0]
      Burndown chart saved to charts/burndown-4321.png
```

### 5. Velocity

There is no `velocity` command. Velocity is reported in two places:

- `analytics` (above) prints the average velocity and trend across sprints.
- `trend` prints velocity for a single sprint, using the snapshots that earlier `burndown` runs recorded. Run `burndown --sprint-id` at least once first.

```bash
jira-analytics-cli trend --sprint-id 4321
```

Example output (values are illustrative):

```
Trend analysis for sprint 4321 (3 snapshot(s))
----------------------------------------------------------------------
Sprint 4321 @ 2026-10-13 17:00 | 28/50 pts (56.0%) | 14/20 issues | Projected: 88.0% in 7 days
Velocity trend:       3.40 pts/day
Velocity accelerating: True
Burn rate:             4.10 pts/day
Scope creep detected:  False
Completed pts over time: 6, 17, 28
Remaining pts over time: 44, 33, 22
```

### 6. Use JiraClient from C#

`JiraClient` (namespace `JiraAnalyticsCli.Services`) is a low-level REST helper. It has no sprint-specific methods, so the snippet calls the endpoints directly. Unlike the CLI, it authenticates with Basic auth (`JIRA_EMAIL` plus the API token), so set all three variables.

```bash
export JIRA_EMAIL="you@example.com"
mkdir SprintExplorer && cd SprintExplorer
dotnet new console
dotnet add reference ../jira-analytics-cli/JiraAnalyticsCli.csproj
```

Replace `Program.cs` with:

```csharp
using System.Text.Json;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

var sprintId = 4321;

var options = Options.Create(new JiraClientOptions
{
    BaseUrl = Environment.GetEnvironmentVariable("JIRA_URL")!,
    Username = Environment.GetEnvironmentVariable("JIRA_EMAIL")!,
    ApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN")!
});

var client = new JiraClient(new HttpClient(), options, NullLogger<JiraClient>.Instance);

if (!await client.TestConnectionAsync())
{
    Console.Error.WriteLine("Cannot reach Jira. Check JIRA_URL and credentials.");
    return 1;
}

// Fetch the sprint
var sprint = await client.GetAsync<JsonElement>($"/rest/api/3/sprints/{sprintId}");
Console.WriteLine($"{sprint.GetProperty("name").GetString()} [{sprint.GetProperty("state").GetString()}]");

// List the sprint's issues (all pages)
var jql = Uri.EscapeDataString($"sprint = {sprintId} ORDER BY created DESC");
var issues = await client.GetAllPaginatedAsync<JsonElement>($"/rest/api/3/search?jql={jql}", resultsProperty: "issues");
foreach (var issue in issues)
{
    Console.WriteLine($"{issue.GetProperty("key").GetString()}  {issue.GetProperty("fields").GetProperty("summary").GetString()}");
}

return 0;
```

Run it with `dotnet run`.

### 7. Reports and Output Formats

The CLI supports multiple output formats for reports and analytics data. Below are examples of each format with sample commands and truncated output.

#### Console Table (Default Text Report)
The `analytics` command produces a formatted text table by default:

```bash
jira-analytics-cli analytics --project MYPROJ --sprints 3
```

Sample output:
```
╔════════════════════════════════════════════════════════════════╗
║       JIRA ANALYTICS REPORT - SPRINT PERFORMANCE SUMMARY       ║
╚════════════════════════════════════════════════════════════════╝

📊 OVERALL METRICS
──────────────────────────────────────────────────────────────────────
Average Velocity:      42.50 pts/sprint
Velocity Trend:        ↑ 5.2%
Overall Health:        Good

📈 SPRINT BREAKDOWN
──────────────────────────────────────────────────────────────────────
Sprint: MYPROJ Sprint 41
  Planned:       45 pts
  Completed:     38 pts (84.4%)
  Issues:        15/18
  Quality:       78.2/100
  Cycle Time:    3.2 days
  Status:        Healthy
```

#### Markdown Report
Generate a Markdown report using the `report-md` command:

```bash
jira-analytics-cli report-md --project MYPROJ --sprints 3 --output reports/sprint-report.md
```

Sample content of `sprint-report.md`:
```markdown
# Jira Analytics Report

**Generated:** 2026-10-14 14:30:00 UTC

## Sprint Metrics

| Sprint | Planned | Completed | Completion % | Quality | Health |
|--------|---------|-----------|--------------|---------|--------|
| MYPROJ Sprint 41 | 45 | 38 | 84.4% | 78.2 | Healthy |
| MYPROJ Sprint 40 | 50 | 47 | 94.0% | 85.6 | Excellent |
| MYPROJ Sprint 39 | 42 | 35 | 83.3% | 72.1 | Healthy |

## Summary
- **Sprints Analyzed:** 3
- **Average Velocity:** 42.50 pts/sprint
- **Trend:** Positive (5.2%)
- **Health Status:** Good
```

#### HTML Report
Generate a self-contained HTML report using the `report` command:

```bash
jira-analytics-cli report --project MYPROJ --sprints 3 --output reports/sprint-report.html
```

The HTML report includes styled KPI cards, sprint breakdown tables, and team performance sections. Open the file in any browser to view the responsive report.

#### JSON Export
Export analytics data as JSON using the `export` command:

```bash
jira-analytics-cli export --project MYPROJ --format json --output reports/analytics.json
```

Sample output (truncated):
```json
{
  "AverageVelocity": 42.5,
  "TrendPercentage": 5.2,
  "OverallHealth": "Good",
  "Metrics": [
    {
      "SprintName": "MYPROJ Sprint 41",
      "PlannedStoryPoints": 45,
      "CompletedStoryPoints": 38,
      "TotalIssueCount": 18,
      "CompletedIssueCount": 15,
      "AverageCycleTime": 3.2,
      "DefectsCount": 3
    }
  ]
}
```

You can pipe JSON output to `jq` for filtering:
```bash
jira-analytics-cli export --project MYPROJ --format json --output - | jq '.Metrics[] | select(.CompletedStoryPoints > 40)'
```

#### CSV Export
Export sprint metrics to CSV using the `export-csv` command:

```bash
jira-analytics-cli export-csv --project MYPROJ --output reports/sprint-metrics.csv
```

Sample content of `sprint-metrics.csv`:
```csv
Sprint Name,Planned Points,Completed Points,Completion Rate %,Quality Score,Velocity,Health Status
MYPROJ Sprint 41,45,38,84.4,78.2,7.6,Healthy
MYPROJ Sprint 40,50,47,94.0,85.6,9.4,Excellent
MYPROJ Sprint 39,42,35,83.3,72.1,7.0,Healthy
```

Import CSV into a spreadsheet application (Excel, Google Sheets, etc.) for further analysis.

#### Image Charts
Generate visual charts as PNG or JPG using the `export` command:

```bash
jira-analytics-cli export --project MYPROJ --format png --output charts/velocity-chart.png
```

Supported image formats: `png`, `jpg`, `svg`.

#### Burndown Charts
Generate burndown charts for a specific sprint:

```bash
jira-analytics-cli burndown --project MYPROJ --sprint-id 4321 --output charts/burndown-4321.png
```

Burndown charts support formats: `png`, `jpg`, `svg`, `json`.

### 8. Using in CI

Example GitHub Actions workflow step that generates a sprint report as a job artifact:

```yaml
name: Generate Sprint Report

on:
  workflow_dispatch:
  schedule:
    - cron: '0 9 * * MON'  # Every Monday at 9 AM

jobs:
  generate-report:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Configure Jira Credentials
        run: |
          echo "JIRA_URL=${{ secrets.JIRA_URL }}" >> $GITHUB_ENV
          echo "JIRA_API_TOKEN=${{ secrets.JIRA_API_TOKEN }}" >> $GITHUB_ENV
      
      - name: Build and Publish
        run: |
          dotnet build -c Release
          dotnet pack JiraAnalyticsCli.csproj -c Release -o ./nupkg
          dotnet tool install --global --add-source ./nupkg jira-analytics-cli
      
      - name: Generate Sprint Report
        run: |
          jira-analytics-cli report --project MYPROJ --sprints 5 --output jira-report.html
      
      - name: Upload Report Artifact
        uses: actions/upload-artifact@v4
        with:
          name: sprint-report
          path: jira-report.html
```

## SprintMetricExtensionsTests

The CLI supports multiple output formats for reports and analytics data. Below are examples of each format with sample commands and truncated output.

#### Console Table (Default Text Report)
The `analytics` command produces a formatted text table by default:

```bash
jira-analytics-cli analytics --project MYPROJ --sprints 3
```

Sample output:
```
╔════════════════════════════════════════════════════════════════╗
║       JIRA ANALYTICS REPORT - SPRINT PERFORMANCE SUMMARY       ║
╚════════════════════════════════════════════════════════════════╝

📊 OVERALL METRICS
──────────────────────────────────────────────────────────────────────
Average Velocity:      42.50 pts/sprint
Velocity Trend:        ↑ 5.2%
Overall Health:        Good

📈 SPRINT BREAKDOWN
──────────────────────────────────────────────────────────────────────
Sprint: MYPROJ Sprint 41
  Planned:       45 pts
  Completed:     38 pts (84.4%)
  Issues:        15/18
  Quality:       78.2/100
  Cycle Time:    3.2 days
  Status:        Healthy
```

#### Markdown Report
Generate a Markdown report using the `report-md` command:

```bash
jira-analytics-cli report-md --project MYPROJ --sprints 3 --output reports/sprint-report.md
```

Sample content of `sprint-report.md`:
```markdown
# Jira Analytics Report

**Generated:** 2026-10-14 14:30:00 UTC

## Sprint Metrics

| Sprint | Planned | Completed | Completion % | Quality | Health |
|--------|---------|-----------|--------------|---------|--------|
| MYPROJ Sprint 41 | 45 | 38 | 84.4% | 78.2 | Healthy |
| MYPROJ Sprint 40 | 50 | 47 | 94.0% | 85.6 | Excellent |
| MYPROJ Sprint 39 | 42 | 35 | 83.3% | 72.1 | Healthy |

## Summary
- **Sprints Analyzed:** 3
- **Average Velocity:** 42.50 pts/sprint
- **Trend:** Positive (5.2%)
- **Health Status:** Good
```

#### HTML Report
Generate a self-contained HTML report using the `report` command:

```bash
jira-analytics-cli report --project MYPROJ --sprints 3 --output reports/sprint-report.html
```

The HTML report includes styled KPI cards, sprint breakdown tables, and team performance sections. Open the file in any browser to view the responsive report.

#### JSON Export
Export analytics data as JSON using the `export` command:

```bash
jira-analytics-cli export --project MYPROJ --format json --output reports/analytics.json
```

Sample output (truncated):
```json
{
  "AverageVelocity": 42.5,
  "TrendPercentage": 5.2,
  "OverallHealth": "Good",
  "Metrics": [
    {
      "SprintName": "MYPROJ Sprint 41",
      "PlannedStoryPoints": 45,
      "CompletedStoryPoints": 38,
      "TotalIssueCount": 18,
      "CompletedIssueCount": 15,
      "AverageCycleTime": 3.2,
      "DefectsCount": 3
    }
  ]
}
```

You can pipe JSON output to `jq` for filtering:
```bash
jira-analytics-cli export --project MYPROJ --format json --output - | jq '.Metrics[] | select(.CompletedStoryPoints > 40)'
```

#### CSV Export
Export sprint metrics to CSV using the `export-csv` command:

```bash
jira-analytics-cli export-csv --project MYPROJ --output reports/sprint-metrics.csv
```

Sample content of `sprint-metrics.csv`:
```csv
Sprint Name,Planned Points,Completed Points,Completion Rate %,Quality Score,Velocity,Health Status
MYPROJ Sprint 41,45,38,84.4,78.2,7.6,Healthy
MYPROJ Sprint 40,50,47,94.0,85.6,9.4,Excellent
MYPROJ Sprint 39,42,35,83.3,72.1,7.0,Healthy
```

Import CSV into a spreadsheet application (Excel, Google Sheets, etc.) for further analysis.

#### Image Charts
Generate visual charts as PNG or JPG using the `export` command:

```bash
jira-analytics-cli export --project MYPROJ --format png --output charts/velocity-chart.png
```

Supported image formats: `png`, `jpg`, `svg`.

#### Burndown Charts
Generate burndown charts for a specific sprint:

```bash
jira-analytics-cli burndown --project MYPROJ --sprint-id 4321 --output charts/burndown-4321.png
```

### 8. Using in CI

Example GitHub Actions workflow step that generates a sprint report as a job artifact:

```yaml
name: Generate Sprint Report

on:
  workflow_dispatch:
  schedule:
    - cron: '0 9 * * MON'  # Every Monday at 9 AM

jobs:
  generate-report:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Configure Jira Credentials
        run: |
          echo "JIRA_URL=${{ secrets.JIRA_URL }}" >> $GITHUB_ENV
          echo "JIRA_API_TOKEN=${{ secrets.JIRA_API_TOKEN }}" >> $GITHUB_ENV
      
      - name: Build and Publish
        run: |
          dotnet build -c Release
          dotnet pack JiraAnalyticsCli.csproj -c Release -o ./nupkg
          dotnet tool install --global --add-source ./nupkg jira-analytics-cli
      
      - name: Generate Sprint Report
        run: |
          jira-analytics-cli report --project MYPROJ --sprints 5 --output jira-report.html
      
      - name: Upload Report Artifact
        uses: actions/upload-artifact@v4
        with:
          name: sprint-report
          path: jira-report.html
```

## SprintMetricExtensionsTests

The `SprintMetricExtensionsTests` class contains a comprehensive suite of unit tests for the `SprintMetricExtensions` class. It validates critical functionalities such as progress percentage calculations, sprint completion status checks, and average daily progress metrics, including robust handling of edge cases and null inputs.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Models;
using System;
using Xunit;

// Instantiate the test class
var tests = new SprintMetricExtensionsTests();

// These methods are typically executed by a test runner like Xunit.
// The following shows how to manually invoke the tests.
try
{
    tests.GetProgressPercentage_ShouldReturnCorrectPercentage();
    tests.GetProgressPercentage_ShouldThrowDivideByZeroException_WhenPlannedPointsIsZero();
    tests.GetProgressPercentage_ShouldThrowArgumentNullException_WhenMetricIsNull();
    tests.IsSprintComplete_ShouldReturnTrue_WhenEndDateIsInPast();
    tests.IsSprintComplete_ShouldReturnFalse_WhenEndDateIsInFuture();
    tests.IsSprintComplete_ShouldThrowArgumentNullException_WhenMetricIsNull();
    tests.GetAverageDailyProgress_ShouldReturnCorrectAverage();
    tests.GetAverageDailyProgress_ShouldThrowDivideByZeroException_WhenDurationIsZero();
    tests.GetAverageDailyProgress_ShouldThrowArgumentNullException_WhenMetricIsNull();
    
    Console.WriteLine("All tests executed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Test execution failed: {ex.Message}");
}
```

## BurndownSnapshotExtensions

The `BurndownSnapshotExtensions` class provides extension methods for the `BurndownSnapshot` model that enhance burndown chart analysis and reporting. It includes methods for calculating velocity trends, detecting acceleration/deceleration, computing burn rates, creating delta comparisons, formatting status strings, detecting scope creep, and extracting time-series data for charting.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using System;
using System.Collections.Generic;

// Create a sample burndown snapshot
var snapshot = new BurndownSnapshot
{
    Timestamp = new DateTime(2026, 7, 10, 14, 30, 0),
    SprintId = "PROJ-2026-Q3-SPR1",
    TotalStoryPoints = 50,
    CompletedStoryPoints = 25,
    RemainingStoryPoints = 25,
    TotalIssueCount = 20,
    CompletedIssueCount = 10,
    RemainingIssueCount = 10,
    ScopeChanges = 2
};

// Create historical snapshots for trend analysis
var historicalSnapshots = new List<BurndownSnapshot>
{
    new BurndownSnapshot
    {
        Timestamp = new DateTime(2026, 7, 9, 14, 30, 0),
        SprintId = "PROJ-2026-Q3-SPR1",
        TotalStoryPoints = 50,
        CompletedStoryPoints = 20,
        RemainingStoryPoints = 30,
        TotalIssueCount = 20,
        CompletedIssueCount = 8,
        RemainingIssueCount = 12,
        ScopeChanges = 1
    },
    new BurndownSnapshot
    {
        Timestamp = new DateTime(2026, 7, 8, 14, 30, 0),
        SprintId = "PROJ-2026-Q3-SPR1",
        TotalStoryPoints = 50,
        CompletedStoryPoints = 15,
        RemainingStoryPoints = 35,
        TotalIssueCount = 20,
        CompletedIssueCount = 6,
        RemainingIssueCount = 14,
        ScopeChanges = 0
    }
};

// Calculate velocity trend over time
var velocityTrend = snapshot.CalculateVelocityTrend(historicalSnapshots);
Console.WriteLine($"Velocity Trend: {velocityTrend:F2} story points/day");

// Check if velocity is accelerating
var isAccelerating = snapshot.IsVelocityAccelerating(historicalSnapshots);
Console.WriteLine($"Is Accelerating: {isAccelerating}");

// Get burn rate for the sprint
var burnRate = snapshot.GetBurnRate(daysInSprint: 14);
Console.WriteLine($"Burn Rate: {burnRate:F2} story points/day");

// Create a delta snapshot for comparison
var previousSnapshot = historicalSnapshots[0];
var deltaSnapshot = snapshot.CreateDeltaSnapshot(previousSnapshot);
if (deltaSnapshot != null)
{
    Console.WriteLine($"Delta - Completed: {deltaSnapshot.CompletedStoryPoints}, " +
                     $"Remaining: {deltaSnapshot.RemainingStoryPoints}");
}

// Format as status string
var statusString = snapshot.ToStatusString();
Console.WriteLine(statusString);
// Output: Sprint PROJ-2026-Q3-SPR1 @ 2026-07-10 14:30 | 25/50 pts (50.0%) | 10/20 issues

// Check for scope creep
var hasScopeCreep = snapshot.HasScopeCreep(threshold: 3);
Console.WriteLine($"Has Scope Creep: {hasScopeCreep}");

// Extract time-series data for charting
var completedOverTime = historicalSnapshots.Append(snapshot).ToList()
    .GetCompletedStoryPointsOverTime();
var remainingOverTime = historicalSnapshots.Append(snapshot).ToList()
    .GetRemainingStoryPointsOverTime();

Console.WriteLine($"Completed over time: [{string.Join(", ", completedOverTime)}]");
Console.WriteLine($"Remaining over time: [{string.Join(", ", remainingOverTime)}]");
```

## ReportServiceValidation
## CycleTimeResult

The `CycleTimeResult` type represents the outcome of cycle time analysis for a set of issues, providing insights into the average, median, and percentile cycle times. It also includes detailed cycle time information for each issue. Here's an example of how to use it:

```csharp
var result = new CycleTimeResult
{
    ProjectKey = "MYPROJECT",
    AverageCycleTime = 10.5,
    MedianCycleTime = 9.2,
    P50CycleTime = 8.1,
    P75CycleTime = 12.5,
    P90CycleTime = 15.8,
    IssueCycleTimes = new List<IssueCycleTime>
    {
        new IssueCycleTime { IssueKey = "MYISSUE-1", Summary = "Issue 1", CycleTimeDays = 7 },
        new IssueCycleTime { IssueKey = "MYISSUE-2", Summary = "Issue 2", CycleTimeDays = 14 },
    }
};

Console.WriteLine($"Average cycle time: {result.AverageCycleTime} days");
Console.WriteLine($"Median cycle time: {result.MedianCycleTime} days");
Console.WriteLine($"Issue cycle times:");
foreach (var issue in result.IssueCycleTimes)
{
    Console.WriteLine($"  {issue.IssueKey}: {issue.CycleTimeDays} days");
}

The `ReportServiceValidation` class provides extension methods for validating `ReportService` instances and their parameters used in report generation. It includes methods for validating service instances, checking validity, ensuring validity with exceptions, and validating various report parameters like project keys, sprint IDs, output paths, and formats.


### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using System;
using System.Collections.Generic;

// Create a ReportService instance (typically injected via DI)
var reportService = new ReportService(
    // dependencies would be injected here
);

// Validate the ReportService instance
var validationErrors = reportService.Validate();
Console.WriteLine($"Validation errors count: {validationErrors.Count}");

// Check if the ReportService instance is valid
var isValid = reportService.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Ensure the ReportService instance is valid (throws if invalid)
try
{
    reportService.EnsureValid();
    Console.WriteLine("ReportService instance is valid");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a project key for report generation
var projectKeyErrors = ReportServiceValidation.ValidateProjectKey("PROJ-2026");
if (projectKeyErrors.Count == 0)
{
    Console.WriteLine("Project key is valid");
}

// Validate a sprint ID for burndown chart generation
var sprintIdErrors = ReportServiceValidation.ValidateSprintId(123);
if (sprintIdErrors.Count == 0)
{
    Console.WriteLine("Sprint ID is valid");
}

// Validate an output path for report generation
var outputPathErrors = ReportServiceValidation.ValidateOutputPath("./reports/sprint-2026-q3.html");
if (outputPathErrors.Count == 0)
{
    Console.WriteLine("Output path is valid");
}

// Validate a format for report generation
var formatErrors = ReportServiceValidation.ValidateFormat("html");
if (formatErrors.Count == 0)
{
    Console.WriteLine("Format is valid");
}
```

## AnalyticsService

`AnalyticsService` retrieves Jira project data, calculates delivery, team, quality, and flow metrics, and returns structured result models for reporting or further processing. It depends on `IJiraApiService` for Jira data, `IMetricsRepository`, and `ILogger<AnalyticsService>`; these dependencies are normally supplied through dependency injection.

Its public methods are:

- `AnalyzeSprints(projectKey, sprintCount)` — analyzes recent closed sprints, including velocity, completion, defects, overdue work, trends, and overall health. For each sprint, it fetches associated issues to calculate defects count, overdue issue count, team size, and average cycle time.
- `AnalyzeTeam(projectKey)` — calculates developer productivity and workload distribution by assigning issues to team members based on assignee, then computing per-developer productivity metrics and identifying top/low performers.
- `AnalyzeQuality(projectKey)` — calculates defect totals and rates across all closed sprints by counting issues with IssueType == "Bug", and identifies high-risk components (those with the highest concentration of bugs).
- `AnalyzeVelocityTrend(projectKey, sprintCount)` — returns per-sprint velocities for the specified number of most recent closed sprints and classifies the recent trend as increasing, decreasing, or stable by comparing velocity averages between the first and second halves of the date range.
- `AnalyzeOverdueIssues(projectKey)` — returns overdue issues with total count, critical count (high-priority overdue issues), and average days overdue calculated from issues past their due date.
- `AnalyzeCycleTime(projectKey)` — calculates average, median, P50, P75, and P90 cycle times for resolved issues (those with ResolutionDate) and includes a per-issue breakdown with issue key, summary, and timeline dates.

All methods are asynchronous. Invalid or empty project keys are rejected before analysis; Jira or calculation failures are logged and produce an empty or partial result rather than propagating the caught exception.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;

// Dependencies are typically provided by the application's DI container.
IAnalyticsService analytics = new AnalyticsService(
    jiraApiService,
    metricsRepository,
    logger);

var sprintAnalysis = await analytics.AnalyzeSprints("PROJ", sprintCount: 5);

Console.WriteLine($"Health: {sprintAnalysis.OverallHealth}");
Console.WriteLine($"Average velocity: {sprintAnalysis.AverageVelocity:F1}");

foreach (var metric in sprintAnalysis.Metrics)
{
    Console.WriteLine($"{metric.SprintName}: {metric.GetVelocity():F1}");
}
```

## AnalyticsServiceValidation

`AnalyticsServiceValidation` offers a set of static helper methods that validate an `AnalyticsService` instance and the parameters of its public API. The methods ensure that inputs such as project keys, sprint counts, dates, numeric values, and collections meet basic business rules before the service performs any calculations.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using JiraAnalyticsCli.Services;

// Assume an AnalyticsService instance is available (dependencies would normally be injected)
var analyticsService = new AnalyticsService(
    // constructor arguments omitted for brevity
);

// Validate the service instance itself
IReadOnlyList<string> instanceProblems = analyticsService.Validate();
Console.WriteLine($"Instance validation problems: {instanceProblems.Count}");

// Quick validity check
bool isValid = analyticsService.IsValid();
Console.WriteLine($"Service is valid: {isValid}");

// Throw if the instance is not valid
analyticsService.EnsureValid();

// Validate a project key
IReadOnlyList<string> keyProblems = AnalyticsServiceValidation.ValidateProjectKey("PROJ-2026");
Console.WriteLine($"Project key problems: {keyProblems.Count}");

// Validate sprint count
IReadOnlyList<string> sprintProblems = AnalyticsServiceValidation.ValidateSprintCount(5);
Console.WriteLine($"Sprint count problems: {sprintProblems.Count}");

// Validate a date parameter
IReadOnlyList<string> dateProblems = AnalyticsServiceValidation.ValidateDate(DateTime.UtcNow, nameof(dateProblems));
Console.WriteLine($"Date problems: {dateProblems.Count}");

// Validate a numeric value
IReadOnlyList<string> numberProblems = AnalyticsServiceValidation.ValidateNumber(42.7, "velocity");
Console.WriteLine($"Number problems: {numberProblems.Count}");

// Validate a collection
var ids = new List<int> { 1, 2, 3 };
IReadOnlyList<string> collectionProblems = AnalyticsServiceValidation.ValidateCollection(ids, "ids");
Console.WriteLine($"Collection problems: {collectionProblems.Count}");
```

## CollectionExtensionsValidation

The `CollectionExtensionsValidation` class provides validation extension methods for parameters used by `CollectionExtensions` operations. It includes methods for validating collection sources, checking validity, ensuring validity with exceptions, and validating batch sizes, key selectors, and list indices to prevent common collection-related exceptions.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

// Create a sample collection
var issues = new List<string> { "ISSUE-1", "ISSUE-2", "ISSUE-3" };

// Validate a collection source
var collectionErrors = issues.Validate();
Console.WriteLine($"Collection validation errors: {collectionErrors.Count}");

// Quick validity check
bool isValid = issues.IsValid();
Console.WriteLine($"Collection is valid: {isValid}");

// Ensure the collection is valid (throws if invalid)
try
{
    issues.EnsureValid();
    Console.WriteLine("Collection is valid");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate an empty collection
var emptyCollection = new List<string>();
var emptyErrors = emptyCollection.Validate();
Console.WriteLine($"Empty collection errors: {string.Join(", ", emptyErrors)}");

// Validate a batch size for batching operations
var batchSizeErrors = 10.Validate();
Console.WriteLine($"Batch size validation errors: {batchSizeErrors.Count}");

// Check if batch size is valid
bool batchSizeValid = 5.IsValid();
Console.WriteLine($"Batch size 5 is valid: {batchSizeValid}");

// Ensure batch size is valid (throws if invalid)
try
{
    (-1).EnsureValid();
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine("Negative batch size validation failed as expected");
}

// Validate a key selector function for GroupByMultiple operations
Func<string, string> keySelector = s => s.Split('-')[0];
var selectorErrors = keySelector.Validate();
Console.WriteLine($"Key selector validation errors: {selectorErrors.Count}");

// Check if key selector is valid
bool selectorValid = keySelector.IsValid();
Console.WriteLine($"Key selector is valid: {selectorValid}");

// Ensure key selector is valid (throws if invalid)
try
{
    ((Func<string, string>)null!).EnsureValid();
}
catch (ArgumentNullException)
{
    Console.WriteLine("Null key selector validation failed as expected");
}

// Validate a list and index for GetAtIndexOrDefault operations
var list = new List<int> { 10, 20, 30 };
var indexErrors = list.Validate(1);
Console.WriteLine($"List index validation errors: {indexErrors.Count}");

// Check if list and index are valid
bool indexValid = list.IsValid(2);
Console.WriteLine($"List index 2 is valid: {indexValid}");

// Ensure list and index are valid (throws if invalid)
try
{
    list.EnsureValid(-1);
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine("Negative index validation failed as expected");
}
```

## SprintRepositoryJsonExtensions

The `SprintRepositoryJsonExtensions` class provides extension methods for serializing and deserializing `SprintRepository` instances to and from JSON format. It includes methods for converting a repository to a JSON string, parsing JSON back into a repository, safe parsing with error handling, retrieving all sprints for serialization, and loading sprints into a repository.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using System;
using System.Collections.Generic;

// Create a sample sprint repository with sprints
var repository = new SprintRepository(null!);
var sprints = new List<Sprint>
{
    new Sprint
    {
        Id = 1,
        Name = "Sprint 1",
        Goal = "Complete initial features",
        StartDate = new DateTime(2026, 7, 1),
        EndDate = new DateTime(2026, 7, 14),
        State = "Active",
        CompletedIssues = 15,
        TotalIssues = 20,
        CompletedStoryPoints = 45,
        TotalStoryPoints = 50
    },
    new Sprint
    {
        Id = 2,
        Name = "Sprint 2",
        Goal = "Implement advanced features",
        StartDate = new DateTime(2026, 7, 15),
        EndDate = new DateTime(2026, 7, 28),
        State = "Planned",
        CompletedIssues = 0,
        TotalIssues = 25,
        CompletedStoryPoints = 0,
        TotalStoryPoints = 60
    }
};
repository.LoadSprints(sprints);

// Serialize the repository to JSON (compact format)
var json = repository.ToJson();
Console.WriteLine(json);
// Output: [{"id":1,"name":"Sprint 1","goal":"Complete initial features",...}]

// Serialize with indentation for readability
var prettyJson = repository.ToJson(indented: true);
Console.WriteLine(prettyJson);

// Deserialize JSON back to a repository
var jsonData = """[
    {"id":1,"name":"Sprint 1","goal":"Complete initial features","startDate":"2026-07-01T00:00:00","endDate":"2026-07-14T00:00:00","state":"Active","completedIssues":15,"totalIssues":20,"completedStoryPoints":45,"totalStoryPoints":50},
    {"id":2,"name":"Sprint 2","goal":"Implement advanced features","startDate":"2026-07-15T00:00:00","endDate":"2026-07-28T00:00:00","state":"Planned","completedIssues":0,"totalIssues":25,"completedStoryPoints":0,"totalStoryPoints":60}
]""";
var deserializedRepo = SprintRepositoryJsonExtensions.FromJson(jsonData);
Console.WriteLine($"Deserialized repository has {deserializedRepo?.GetAllSprints().Count} sprints");

// Safe deserialization with error handling
if (SprintRepositoryJsonExtensions.TryFromJson(jsonData, out var safeRepo))
{
    Console.WriteLine("Successfully deserialized using TryFromJson");
}
else
{
    Console.WriteLine("Failed to deserialize JSON");
}

// Load sprints into an empty repository
var emptyRepo = new SprintRepository(null!);
emptyRepo.LoadSprints(allSprints);
Console.WriteLine($"Empty repository now has {emptyRepo.GetAllSprints().Count} sprints");
```

## CsvExportServiceTests

The `CsvExportServiceTests` class provides a comprehensive test suite for the `CsvExportService` to verify correct CSV export behavior. It covers edge cases such as empty datasets, escaping of special characters in sprint names, invariant culture formatting for numbers and dates, and proper header/data generation for team metrics.

### Usage Example

```csharp
using JiraAnalyticsCli.Tests.Services;
using System;
using System.Threading.Tasks;
using Xunit;

// Instantiate the test class
var tests = new CsvExportServiceTests();

// These methods are typically executed by a test runner like Xunit.
// The following shows how to manually invoke the tests.
try
{
    await tests.ExportSprintMetrics_ShouldWriteHeaderOnly_WhenMetricsIsEmpty();
    await tests.ExportSprintMetrics_ShouldEscapeSpecialCharactersInSprintName();
    await tests.ExportSprintMetrics_ShouldUseInvariantCultureForNumbersAndDates();
    await tests.ExportTeamMetrics_ShouldWriteHeaderAndData();
    
    Console.WriteLine("All tests executed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Test execution failed: {ex.Message}");
}
```

## JqlQueryService

The `JqlQueryService` executes custom JQL queries via the Jira API and returns structured, paginated results. It also provides static helpers for building common JQL clauses and formatting results as text.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using System;
using System.Threading.Tasks;

// Example: query issues in a project updated in the last week
var service = new JqlQueryService(jiraApiService, logger); // dependencies injected
var projectJql = JqlQueryService.BuildProjectJql("PROJ");
var dateJql = JqlQueryService.BuildDateRangeJql("updated", DateTime.UtcNow.AddDays(-7), null);
var jql = $"{projectJql} AND {dateJql} ORDER BY updated DESC";

JqlQueryResult result = await service.ExecuteQueryAsync(jql, maxResults: 20);
Console.WriteLine(JqlQueryService.FormatAsText(result));
```

## TeamComparisonService

`TeamComparisonService` compares delivery and quality metrics across Jira projects. It analyzes each project's recent closed sprints in parallel, creates a `TeamProjectSnapshot` for every project with available results, and identifies the teams with the highest average velocity, lowest defect rate, and highest average completion rate.

Its public methods are:

- `CompareTeamsAsync(projectKeys, sprintCount = 5, cancellationToken = default)` — returns a `TeamComparisonReport` containing the per-project snapshots and ranking winners. Blank project keys are ignored and duplicate keys are removed case-insensitively. At least one project key and a positive sprint count are required.
- `FormatAsText(report)` — formats a report as a console-friendly table followed by its ranking winners. This method is static.
- `RenderMarkdownTable(report)` — renders the per-project metrics as a GitHub-flavored Markdown table. This method is static.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;

// Dependencies are typically supplied by the application's DI container.
ITeamComparisonService comparisonService = new TeamComparisonService(
    analyticsService,
    logger);

var report = await comparisonService.CompareTeamsAsync(
    new[] { "PLATFORM", "MOBILE", "WEB" },
    sprintCount: 5);

Console.WriteLine(TeamComparisonService.FormatAsText(report));

var markdown = TeamComparisonService.RenderMarkdownTable(report);
await File.WriteAllTextAsync("team-comparison.md", markdown);
```

## MarkdownReportService

`MarkdownReportService` generates Markdown reports from sprint and team analytics data, as well as cycle time reports. The output is a plain text Markdown file that can be viewed in any Markdown viewer.

Its public methods are:

- `GenerateReportAsync(projectKey, sprintCount, outputPath)` — generates a Markdown report containing sprint and team analytics for the specified project and number of sprints, writing it to the given output path.
- `GenerateCycleTimeReportAsync(projectKey, cycleTimeResult, outputPath)` — generates a Markdown report for cycle time analysis, writing it to the given output path.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using System.Threading.Tasks;

// Dependencies are typically provided by the application's DI container.
IMarkdownReportService reportService = new MarkdownReportService(
    analyticsService,
    logger);

// Generate a sprint and team analytics report
await reportService.GenerateReportAsync(
    "PROJ",
    sprintCount: 5,
    outputPath: "./reports/sprint-report.md");

// Generate a cycle time report
var cycleTimeResult = await analyticsService.AnalyzeCycleTime("PROJ");
await reportService.GenerateCycleTimeReportAsync(
    "PROJ",
    cycleTimeResult,
    outputPath: "./reports/cycle-time-report.md");
```

## HtmlReportService

`HtmlReportService` generates self-contained, responsive HTML reports with embedded CSS from sprint and team analytics data. The output file requires no external resources and can be opened directly in any browser.

Its public methods are:

- `GenerateReportAsync(projectKey, sprintCount, outputPath)` — runs sprint and team analysis in parallel, builds a styled HTML report, and writes it to the given output path (creating the directory if needed). Blank project keys or output paths are rejected, and a non-positive sprint count throws `ArgumentOutOfRangeException`.
- `BuildHtml(projectKey, sprintAnalysis, teamAnalysis)` — builds the HTML string from already-computed `SprintAnalysisResult` and `TeamAnalysisResult` data without performing any analysis. This method is synchronous and static-free; it throws `ArgumentNullException` if either result is null.

The generated report includes KPI cards (average velocity, velocity trend, overall health, issues delivered, bugs found, overdue issues), a sprint breakdown table, a team workload distribution section, and a top performers section. All dynamic values are HTML-encoded to prevent injection in generated reports.

### Usage Example

```csharp
using JiraAnalyticsCli.Services;
using System.Threading.Tasks;

// Dependencies are typically provided by the application's DI container.
IHtmlReportService reportService = new HtmlReportService(
    analyticsService,
    logger);

// Generate a full HTML report (runs sprint and team analysis internally)
await reportService.GenerateReportAsync(
    "PROJ",
    sprintCount: 5,
    outputPath: "./reports/sprint-report.html");

// Build HTML from already-computed analysis results
var sprintAnalysis = await analyticsService.AnalyzeSprints("PROJ", sprintCount: 5);
var teamAnalysis = await analyticsService.AnalyzeTeam("PROJ");
var html = reportService.BuildHtml("PROJ", sprintAnalysis, teamAnalysis);
await File.WriteAllTextAsync("./reports/custom-report.html", html);
```

## SnapshotStore

`SnapshotStore` is a file-backed implementation of `ISnapshotStore` that persists `BurndownSnapshot` instances across CLI invocations so historical trend analysis can be performed between separate runs of the tool. It keeps one JSON-lines (`.jsonl`) file per sprint under a configurable data directory; each line is an independently parseable JSON-serialized `BurndownSnapshot`, keyed for lookup by the file's `SprintId` and ordered within the file by `Timestamp`.

Appending is append-only, so a run that is killed mid-write can at most corrupt its own last line, which `LoadAsync` skips over with a logged warning rather than failing the whole load. The data directory resolves in this order: the `dataDirectory` constructor argument, then the `JIRA_SNAPSHOT_DIR` environment variable, then a `snapshots` directory relative to the current working directory.

Its public methods are:

- `AppendAsync(snapshot, cancellationToken = default)` — validates the snapshot and appends it as a new JSON line to the file for its `SprintId`, creating the data directory if needed. Writes are serialized through a process-wide lock. Throws `ArgumentNullException` when the snapshot is null and `ArgumentException` when it fails validation.
- `LoadAsync(sprintId, cancellationToken = default)` — loads the persisted snapshot series for a sprint, ordered chronologically (oldest first). Corrupt, empty, or invalid lines are skipped with a logged warning; returns an empty list when no file exists yet. Throws `ArgumentOutOfRangeException` when `sprintId` is not positive.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using System;
using System.Threading.Tasks;

// Dependencies are typically provided by the application's DI container.
ISnapshotStore store = new SnapshotStore(logger);

// Capture a point-in-time burndown snapshot and persist it
var snapshot = new BurndownSnapshot
{
    Timestamp = DateTime.UtcNow,
    SprintId = 123,
    TotalStoryPoints = 50,
    CompletedStoryPoints = 25,
    RemainingStoryPoints = 25,
    TotalIssueCount = 20,
    CompletedIssueCount = 10,
    RemainingIssueCount = 10,
    ScopeChanges = 0
};
await store.AppendAsync(snapshot);

// Load the full history for the sprint, oldest first
var history = await store.LoadAsync(sprintId: 123);
Console.WriteLine($"Loaded {history.Count} snapshots for sprint 123");
foreach (var entry in history)
{
    Console.WriteLine($"{entry.Timestamp:O} | {entry.CompletedStoryPoints}/{entry.TotalStoryPoints} pts");
}
```

## JsonFormatter

`JsonFormatter` (namespace `JiraAnalyticsCli.Formatters`) formats analytics data as JSON using `System.Text.Json`. It is a standalone, generic formatter: the `export` command does not use it, so the file layout shown in [JSON Export](#json-export) comes from the export pipeline, not from this class. The constructor accepts an `ILogger<JsonFormatter>` and an optional `prettyPrint` flag (default `true`) that controls indentation in `Format` and `FormatFiltered`.

Its public methods are:

- `Format(object data)` - serializes any object to JSON. Null properties are omitted, and circular references are not followed (the repeated reference is written as `null`). Passing a string does not parse it: the result is a JSON string literal. Use `Prettify` to re-indent JSON text. Throws `ArgumentNullException` when `data` is null and `InvalidOperationException` when serialization fails.
- `FormatWithMetadata(object data, string title, string version = "1.0.0")` - wraps `data` in an envelope with a `metadata` object (`title`, `version`, `generatedAt`, `generatedBy`) and a `data` property. `generatedAt` is the current UTC time, and `generatedBy` is always `jira-analytics-cli`. Throws `ArgumentNullException` when `data` is null, and `ArgumentException` when `title` or `version` is null or empty.
- `FormatFiltered(object data, string[] includedFields)` - writes only the requested fields. Each entry is a dotted path such as `Name` or `Team.Lead`, and the path itself becomes the output key: `Team.Lead` is written as a flat key `"Team.Lead"`, not as a nested object. Paths that do not exist are skipped. Names are matched as serialized with default options, so C# property names are case-sensitive. Null values that are requested are kept as `null`, unlike `Format`. Throws `ArgumentNullException` when an argument is null and `InvalidOperationException` on failure, including when `data` does not serialize to a JSON object.
- `Prettify(string json)` - re-indents a JSON string. The output is always indented, regardless of `prettyPrint`. Throws `ArgumentException` when `json` is null or empty and `InvalidOperationException` when it is not valid JSON.
- `Validate(string json)` - returns `(bool IsValid, string[] Errors)`. Invalid JSON returns `false` and one error message of the form `JSON parsing error at line N: ...`. The line number is zero-based, as reported by `JsonException.LineNumber`. Malformed JSON does not throw. Null or empty input does throw `ArgumentException`.

### Usage Example

```csharp
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging.Abstractions;

var formatter = new JsonFormatter(NullLogger<JsonFormatter>.Instance);
var compact = new JsonFormatter(NullLogger<JsonFormatter>.Instance, prettyPrint: false);

var sprint = new
{
    Name = "PROJ Sprint 41",
    Velocity = 42.5,
    Notes = (string?)null,
    Team = new { Lead = "A", Size = 5 },
};

// Indented JSON; the null Notes property is omitted
Console.WriteLine(formatter.Format(sprint));

// Single-line JSON
Console.WriteLine(compact.Format(sprint));
// {"Name":"PROJ Sprint 41","Velocity":42.5,"Team":{"Lead":"A","Size":5}}

// Wrap the data with generation metadata
var report = formatter.FormatWithMetadata(sprint, "Sprint report");

// Keep only selected fields; dotted paths become flat keys
var filtered = formatter.FormatFiltered(sprint, new[] { "Name", "Team.Lead", "Missing" });
Console.WriteLine(filtered);
// {
//   "Name": "PROJ Sprint 41",
//   "Team.Lead": "A"
// }

// Re-indent existing JSON text
Console.WriteLine(compact.Prettify("{\"a\":1,\"b\":[2]}"));

// Validate JSON text
var (isValid, errors) = formatter.Validate("{\"a\": }");
if (!isValid)
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

## XmlFormatter

`XmlFormatter` (namespace `JiraAnalyticsCli.Formatters`) formats analytics data as XML with proper structure and validation. It supports nested objects, dictionaries, and collections with customizable root element names. The constructor accepts an `ILogger<XmlFormatter>` and an optional `includeXmlDeclaration` flag (default `true`).

Its public methods are:

- `Format(object data, string rootElement = "data")` - serializes an object to an indented XML string. Dictionaries become child elements keyed by entry key, collections become `<item index="N">` elements, and plain objects are reflected by their public properties. Throws `InvalidOperationException` on failure.
- `Validate(string xml)` - checks whether an XML string is well-formed. Returns a tuple `(bool IsValid, string? Error)` without throwing on invalid input.
- `Prettify(string xml)` - re-formats an XML string with consistent two-space indentation. Throws `InvalidOperationException` on failure.
- `SelectValues(string xml, string xpathExpression)` - evaluates an XPath expression against the XML and returns matching element values as a `List<string>`. Returns an empty list on error instead of throwing.

Element names are sanitized to contain only letters, digits, underscores, and hyphens; names that do not start with a letter or underscore are prefixed with `_`. Null property values are represented as an element with a `null="true"` attribute. `DateTime` values are formatted in round-trip ("O") format.

### Usage Example

```csharp
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging.Abstractions;

var formatter = new XmlFormatter(NullLogger<XmlFormatter>.Instance);

// Format an object as XML
var data = new Dictionary<string, object>
{
    ["ProjectKey"] = "PROJ",
    ["SprintCount"] = 5,
    ["Velocity"] = 42.5
};
var xml = formatter.Format(data, rootElement: "analytics");
Console.WriteLine(xml);
// <?xml version="1.0" encoding="utf-8" standalone="yes"?>
// <analytics>
//   <ProjectKey>PROJ</ProjectKey>
//   <SprintCount>5</SprintCount>
//   <Velocity>42.5</Velocity>
// </analytics>

// Validate XML
var (isValid, error) = formatter.Validate(xml);
Console.WriteLine($"Valid: {isValid}"); // Valid: True

// Prettify existing XML
var ugly = "<root><a>1</a><b>2</b></root>";
Console.WriteLine(formatter.Prettify(ugly));

// Extract values with XPath
var values = formatter.SelectValues(xml, "//Velocity");
Console.WriteLine($"Velocity: {values.FirstOrDefault()}"); // Velocity: 42.5
```

## CsvFormatter

`CsvFormatter` (namespace `JiraAnalyticsCli.Formatters`) converts a sequence of objects into CSV text. It is a standalone, generic formatter: the `export-csv` command does not use it, so the column layout shown in [CSV Export](#csv-export) comes from the export pipeline, not from this class. The constructor accepts an `ILogger<CsvFormatter>`.

Its public methods are:

- `Format<T>(IEnumerable<T> data)` - writes one column per public instance property of `T`, with a header row first. Throws `ArgumentNullException` when `data` is null. For an empty sequence it logs a warning and returns an empty string, so no header is written.
- `FormatWithMapping<T>(IEnumerable<T> data, Dictionary<string, Func<T, string>> columnMapping)` - writes one column per dictionary key, in the dictionary's enumeration order, with each cell produced by the mapper for that key. Throws `ArgumentNullException` when either argument is null. Unlike `Format`, the header row is written even when `data` is empty.
- `Parse<T>(string csv)` - reads CSV text back into a `List<T>` where `T` is a class with a parameterless constructor. Returns an empty list for null or empty input, or when there are no data rows.

### Output format

**Layout**

- The delimiter is `,`.
- Every row, including the last, ends with `Environment.NewLine` (`\n` on Linux and macOS, `\r\n` on Windows).
- Header names are the property names (for `Format`) or the mapping keys (for `FormatWithMapping`), written unchanged.
- `Format` column order follows reflection order. In practice this is declaration order, but the .NET API does not guarantee it.

**Quoting and escaping**

- A field is wrapped in double quotes only when it contains a comma, a double quote, or a line feed (`\n`). Other fields are written as-is.
- Inside a quoted field, every `"` is doubled (`"` becomes `""`).
- Null and empty values are written as an empty field (for example `a,,c`).
- Leading and trailing spaces are written unchanged and are not quoted.
- A value that contains a carriage return (`\r`) without a line feed is not quoted. Remove bare `\r` characters from text values before formatting.

**Value formatting**

| Value type | Written as |
|------------|------------|
| `null` | empty field |
| `DateTime` | `yyyy-MM-dd HH:mm:ss` (for example `2026-10-09 17:00:00`). No milliseconds, no time zone, and `DateTimeKind` is not converted. The pattern is formatted with the current culture. |
| Other `IFormattable` (numbers, `bool`, `Guid`, enums, and so on) | `ToString(null, InvariantCulture)`. Decimals use `.`, there are no group separators, and booleans are written as `True` / `False`. |
| Any other object | `ToString()`, or an empty field if that returns `null` |

**Parsing (`Parse<T>`)**

- Input is split into lines on `\r\n`, `\r`, or `\n` before quoted fields are processed. A quoted field that contains a line break therefore does not round-trip.
- Blank lines are skipped. Header names are matched to properties case-insensitively. Columns with no matching property are ignored.
- Surrounding whitespace is trimmed from every field.
- Values are converted with `Convert.ChangeType`. A value that cannot be converted is logged as a warning and left at the property's default.

### Usage Example

```csharp
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging.Abstractions;

var formatter = new CsvFormatter(NullLogger<CsvFormatter>.Instance);

var sprints = new[]
{
    new Sprint { Name = "PROJ Sprint 41", Planned = 45, Completed = 38.5, End = new DateTime(2026, 10, 9, 17, 0, 0) },
    new Sprint { Name = "Login, \"SSO\" fixes", Planned = 12, Completed = 12.0, End = new DateTime(2026, 9, 25, 17, 0, 0) },
};

// Every public property becomes a column
var csv = formatter.Format(sprints);
Console.Write(csv);
// Name,Planned,Completed,End
// PROJ Sprint 41,45,38.5,2026-10-09 17:00:00
// "Login, ""SSO"" fixes",12,12,2026-09-25 17:00:00

// Explicit columns, with the header names and cell values chosen by the mapping
var mapping = new Dictionary<string, Func<Sprint, string>>
{
    ["Sprint"] = s => s.Name,
    ["Planned Points"] = s => s.Planned.ToString(),
};
Console.Write(formatter.FormatWithMapping(sprints, mapping));
// Sprint,Planned Points
// PROJ Sprint 41,45
// "Login, ""SSO"" fixes",12

// Read CSV text back into objects
var parsed = formatter.Parse<Sprint>(csv);
Console.WriteLine(parsed.Count); // 2

public class Sprint
{
    public string Name { get; set; } = string.Empty;
    public int Planned { get; set; }
    public double Completed { get; set; }
    public DateTime End { get; set; }
}
```

## InMemoryCache

`InMemoryCache` (namespace `JiraAnalyticsCli.Caching`) is a thread-safe, process-local cache for analytics and API responses. Values are serialized to JSON when stored and deserialized on every `Get`, so callers always receive a fresh copy and never a reference to the cached instance. Each entry carries a `CachePolicy` that decides when it expires. The constructor accepts an `ILogger<InMemoryCache>`; pass `NullLogger<InMemoryCache>.Instance` when logging is not needed.

Expiration is evaluated lazily: an expired entry is removed the next time it is read, checked, or swept by `CleanupExpired()`. An entry is treated as expired when any of these hold:

- its `AbsoluteExpiration` has passed since creation,
- its `SlidingExpiration` has passed since the last successful `Get` (each hit refreshes the window),
- its `Condition` delegate returns `false`.

`MaxSize` on `CachePolicy` is stored but not enforced by `InMemoryCache`; the cache does not cap the number or total size of entries.

Its public methods are:

- `Set<T>(key, value, policy)` - serializes `value` and stores it under `key`, overwriting any existing entry. Throws `ArgumentException` when `key` is null or empty and `ArgumentNullException` when `value` or `policy` is null.
- `Get<T>(key, defaultValue = default)` - returns the deserialized value, or `defaultValue` when the key is missing, expired, or cannot be deserialized into `T` (a deserialization failure also evicts the entry and logs an error).
- `Contains(key)` - returns `true` when the key exists and has not expired.
- `Remove(key)` - removes a single entry if present.
- `RemoveByPattern(pattern)` - removes every entry whose key matches a wildcard pattern (`*` matches any run of characters, `?` matches one character) and returns the number removed.
- `Clear()` - removes all entries.
- `CleanupExpired()` - removes all expired entries and returns how many were removed. Call it periodically from a background task if memory reclamation matters; otherwise expired entries are removed only when they are next accessed.
- `GetStatistics()` - returns a `CacheStatistics` record with `Hits`, `Misses`, `Evictions`, `EntryCount`, and a computed `HitRate` (0 when no lookups have occurred).

Statistics count a read of an expired entry as both an eviction and a miss. `Remove`, `RemoveByPattern`, and `Clear` do not increment the eviction counter.

### Usage Example

```csharp
using JiraAnalyticsCli.Caching;
using Microsoft.Extensions.Logging.Abstractions;
using System;

// Example type: public record SprintSummary(int SprintId, double Velocity);

var cache = new InMemoryCache(NullLogger<InMemoryCache>.Instance);

// Cache a sprint summary for 10 minutes, refreshed on each read
var policy = CachePolicy.WithSlidingExpiration("sprint:123", TimeSpan.FromMinutes(10));
cache.Set("sprint:123", new SprintSummary(123, 42.5), policy);

// Read it back; a miss returns the supplied default
var summary = cache.Get<SprintSummary>("sprint:123");
if (summary is null)
{
    // Recompute and store again
}

// Remove every cached sprint entry at once
var removed = cache.RemoveByPattern("sprint:*");
Console.WriteLine($"Removed {removed} entries");

// Inspect cache effectiveness
var stats = cache.GetStatistics();
Console.WriteLine($"Hit rate: {stats.HitRate:P0} ({stats.Hits} hits, {stats.Misses} misses)");
```

## SprintRepository

`SprintRepository` (namespace `JiraAnalyticsCli.Repositories`) is the in-memory store for `Sprint` entities behind `ISprintRepository`. Sprints are kept in a thread-safe `ConcurrentDictionary` keyed by the Jira sprint `Id`, so saving a sprint with an existing `Id` replaces the stored copy (upsert). Nothing is written to disk: the contents live only as long as the instance, and `Clear()` or a new instance starts empty. The constructor accepts an `ILogger<SprintRepository>`; pass `NullLogger<SprintRepository>.Instance` when logging is not needed.

Its public methods are:

- `GetByIdAsync(sprintId)` - returns the stored `Sprint` or `null` when the ID is unknown. Errors are logged and swallowed, so it returns `null` rather than throwing.
- `GetByProjectAsync(projectKey)` - returns every stored sprint whose `ProjectKey` matches exactly (case-sensitive), ordered by `EndDate` descending. Sprints without an end date sort last. Errors are logged and an empty list is returned.
- `GetActiveSprints()` - returns the sprints for which `Sprint.IsActive()` is true, across all projects: either the state is `"Active"`, or the current UTC time falls inside the start/end date window. No particular order is guaranteed.
- `GetRecentClosedSprints(count)` - returns up to `count` sprints for which `Sprint.IsClosed()` is true (state `"Closed"` or a `CompleteDate` is set), ordered by `CompleteDate` descending. A non-positive `count` yields an empty list.
- `SaveAsync(sprint)` - validates the sprint with `Sprint.Validate()` and stores it (insert or replace). Throws `ArgumentNullException` when the sprint is null and `ArgumentException` when validation fails; both are logged and rethrown.
- `SaveRangeAsync(sprints)` - validates and stores each sprint in turn. Throws `ArgumentException` when the list is null or empty. The batch is not atomic: if a sprint fails validation, the exception is rethrown and the sprints saved before it remain in the repository.
- `GetCount()` - returns the number of distinct sprints currently stored.
- `Clear()` - removes all stored sprints and logs an informational message.

Sprint validation rules (from `Sprint.Validate()`): `Id` must be positive, `Name` must not be empty or whitespace, and when both dates are set `EndDate` must be after `StartDate`.

### Usage Example

```csharp
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

var repository = new SprintRepository(NullLogger<SprintRepository>.Instance);

// Store a sprint that is currently running
await repository.SaveAsync(new Sprint
{
    Id = 123,
    Name = "Sprint 42",
    ProjectKey = "PROJ",
    State = "Active",
    StartDate = DateTime.UtcNow.AddDays(-7),
    EndDate = DateTime.UtcNow.AddDays(7)
});

// Look up one sprint, or list sprints by project
var sprint = await repository.GetByIdAsync(123);
var projectSprints = await repository.GetByProjectAsync("PROJ");
Console.WriteLine($"{projectSprints.Count} sprints in PROJ, stored total: {repository.GetCount()}");

// Query active and recently closed sprints
var active = await repository.GetActiveSprints();
var recentlyClosed = await repository.GetRecentClosedSprints(count: 5);

// Reset the in-memory store
repository.Clear();
```

## AppConfigurationProvider

`AppConfigurationProvider` (namespace `JiraAnalyticsCli.Configuration`) implements `IConfigurationProvider` and builds the `ICliConfig` used by the rest of the application. `LoadConfiguration()` reads the sources in order, validates the result, and returns a new `CliConfig`. `ServiceFactory` registers the provider as a singleton and resolves `ICliConfig` by calling `LoadConfiguration()` once.

The constructor takes an `ILogger<AppConfigurationProvider>` and a `JiraOptions` instance. Both arguments must be non-null (`ArgumentNullException` otherwise). The `JiraOptions` argument carries the Jira settings already resolved from CLI flags, environment variables and `appsettings.json` (see [2. Configure credentials](#2-configure-credentials)).

Sources are applied in this order, and each later source overwrites the earlier one:

1. Built-in defaults from `CliConfig`.
2. `appsettings.json` in the current working directory (a relative path, not the executable's folder). The keys are camelCase, for example `cacheExpirationMinutes`. A file that is missing is skipped without a warning.
3. Environment variables. Each one is listed in the table below.
4. The `JiraOptions` values for the base URL, API token, timeout and default project. These are always copied in, so `JIRA_URL`, `JIRA_API_TOKEN`, `JIRA_PROJECT` and `JIRA_API_TIMEOUT_SECONDS` take effect through `JiraOptions`. The provider still reads them and rejects bad values in step 3.

Environment variables with an unset or empty value are ignored. Numeric variables also ignore whitespace-only values. Numeric variables must be whole numbers within the range shown below. Any other value causes a `ConfigurationException` whose `PropertyName` is the variable name. The message does not repeat the value, because the value may be a credential pasted into the wrong variable.

| Environment variable | `appsettings.json` key | Allowed values | Default |
|---|---|---|---|
| `JIRA_URL`, `JIRA_BASE_URL` | `jiraBaseUrl` | Any string, replaced by `JiraOptions.BaseUrl` | `https://jira.atlassian.net` |
| `JIRA_TOKEN`, `JIRA_API_TOKEN` | `jiraApiToken` | Any string, replaced by `JiraOptions.ApiToken` | empty |
| `JIRA_PROJECT`, `JIRA_DEFAULT_PROJECT` | `defaultProject` | Any string, replaced by `JiraOptions.ProjectKey` | none |
| `JIRA_API_TIMEOUT_SECONDS` | `jiraApiTimeoutSeconds` | 1 to 300 (env only); replaced by `JiraOptions.TimeoutSeconds` | 30 |
| `CACHE_EXPIRATION_MINUTES` | `cacheExpirationMinutes` | 0 to 10080 (env only) | 15 |
| `DETAILED_LOGGING` | `enableDetailedLogging` | `true` or `false`, case-insensitive (`bool.TryParse`); JSON must be a boolean | `false` |
| `DEFAULT_SPRINT_COUNT` | `defaultSprintCount` | 1 to 100 (env only) | 5 |
| `EXPORT_FORMAT` | `exportFormat` | Any string, not checked here | `txt` |
| `JIRA_API_MAX_RETRY_ATTEMPTS` | `jiraApiMaxRetryAttempts` | 1 to 10 (env only) | 5 |
| `JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD` | `jiraApiCircuitBreakerFailureThreshold` | 1 to 100 (env only) | 5 |
| `JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS` | `jiraApiCircuitBreakerDurationSeconds` | 1 to 3600 (env only) | 30 |
| `JIRA_API_MAX_RESULTS_PER_REQUEST` | `jiraApiMaxResultsPerRequest` | 1 to 1000 (env only) | 1000 |
| `JIRA_API_MAX_PAGES` | `jiraApiMaxPages` | 1 to 1000 (env only) | 100 |
| `JIRA_API_MAX_PAGE_SIZE` | `jiraApiMaxPageSize` | 1 to 1000 (env only) | 200 |

For JSON, numeric keys must be JSON integers. A value of the wrong type is ignored and the default or earlier value is kept. A string key whose value is not a JSON string stops the rest of the file from being read, and the remaining keys fall back to their defaults.

If `appsettings.json` is not valid JSON, the provider logs a warning with the line and byte position and continues with the environment variables and defaults. The warning does not quote the bad text, because it may contain a token.

After all sources are applied, `CliConfig.Validate()` runs. It requires a non-empty absolute `http` or `https` base URL, a non-empty API token, and positive values for every numeric setting. It throws `InvalidOperationException` with a message that names the setting. The provider logs the error and rethrows it.

The provider never logs or throws the token. Its log messages name variables and keys, and report only parsed numbers.

### Usage Example

```csharp
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using System;

// Settings already resolved from CLI flags, environment and appsettings.json
var jiraOptions = new JiraOptions
{
    BaseUrl = "https://your-company.atlassian.net",
    ApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN") ?? string.Empty,
    ProjectKey = "PROJ",
    TimeoutSeconds = 30
};

var provider = new AppConfigurationProvider(NullLogger<AppConfigurationProvider>.Instance, jiraOptions);

try
{
    // Reads appsettings.json, applies environment overrides, then validates
    var config = provider.LoadConfiguration();
    Console.WriteLine($"Cache: {config.CacheExpirationMinutes} min, page size: {config.JiraApiMaxPageSize}");
}
catch (ConfigurationException ex)
{
    // A numeric environment variable is out of range or not a whole number
    Console.Error.WriteLine($"Fix {ex.PropertyName} and try again.");
}
catch (InvalidOperationException ex)
{
    // Validation failed, for example when the API token is empty
    Console.Error.WriteLine(ex.Message);
}
```

## FormattingHelpers

`FormattingHelpers` (namespace `JiraAnalyticsCli.Utils`) is a static class of text formatting helpers for console output and reports. Numbers, dates and percentages are formatted with `CultureInfo.InvariantCulture`, so the output does not depend on the machine's locale.

Its public methods are:

- `FormatPercentage(double value, int decimalPlaces = 1)` - formats `value` with a fixed number of decimals and a `%` suffix. The value is not multiplied by 100, so pass `84.4` to get `84.4%`. A negative `decimalPlaces` is not rejected and produces an invalid result such as `F-1%`; pass 0 or more.
- `FormatNumber(int value)` - adds thousands separators and no decimals: `1234567` becomes `1,234,567`.
- `FormatDecimal(double value, int decimalPlaces = 2)` - fixed decimals without a suffix: `42.5` becomes `42.50`.
- `FormatDate(DateTime date, string format = "yyyy-MM-dd")` - formats a date with a custom format string. Throws `ArgumentNullException` when `format` is null and `ArgumentException` when it is empty.
- `FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")` - the same as `FormatDate`, with a date and time default.
- `FormatBytes(long bytes)` - converts a byte count to a size in steps of 1024 up to `TB`, with at most two decimals and no trailing zeros (`1536` becomes `1.5 KB`). Sizes above 1024 TB stay in `TB` (3 PB is shown as `3072 TB`). Negative values are not handled specially.
- `CreateTable(string[] headers, List<string[]> rows)` - builds a Markdown-style pipe table. Each column is as wide as its longest header or cell, measured by `string.Length`, so emoji and wide CJK characters can misalign. A row with fewer cells than headers is padded with empty cells; extra cells are ignored. Returns an empty string when there are no headers or no rows. Throws `ArgumentNullException` for a null argument. Each line ends with a trailing space before the last `|`.
- `ColorText(string text, ConsoleColor color)` - wraps `text` in an ANSI escape sequence (`ESC[<n>m` ... `ESC[0m`), where `<n>` is the integer value of `color`. The enum values are not the ANSI foreground codes, so `ConsoleColor.Red` (12) emits `ESC[12m`, which most terminals do not show as red. Do not rely on the colors until this is changed. Throws for null or empty `text`.
- `FormatStatus(string status)` - prefixes a Jira status with an emoji. The match is exact and case-sensitive: `Done` and `Closed` get `✅`, `In Progress` and `In Review` get `🔄`, `Open` gets `📋`, `Blocked` gets `🚫`, `On Hold` gets `⏸️`. Any other value gets `❓`. Throws for null or empty input.
- `RepeatChar(char character, int count)` - returns `character` repeated `count` times, for drawing lines. A negative `count` throws `ArgumentOutOfRangeException`.
- `Indent(string text, int spaces = 2)` - prepends `spaces` spaces to `text`. It does not split lines, so a multi-line string gets indented only at its start. Throws for null or empty `text`.
- `CenterText(string text, int width)` - pads `text` with spaces on both sides to reach `width`. When the padding is odd, the extra space goes on the right. Returns `text` unchanged when it is already at least `width` long. Throws for null or empty `text`.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;
using System;
using System.Collections.Generic;

var headers = new[] { "Sprint", "Planned", "Completed", "Completion" };
var rows = new List<string[]>
{
    new[] { "MYPROJ Sprint 41", FormattingHelpers.FormatNumber(45), "38", FormattingHelpers.FormatPercentage(84.4) },
    new[] { "MYPROJ Sprint 40", FormattingHelpers.FormatNumber(1200), "1,147", FormattingHelpers.FormatPercentage(95.6) }
};

Console.WriteLine(FormattingHelpers.CenterText("SPRINT SUMMARY", 40));
Console.WriteLine(FormattingHelpers.RepeatChar('-', 40));
Console.Write(FormattingHelpers.CreateTable(headers, rows));
Console.WriteLine(FormattingHelpers.FormatDate(new DateTime(2026, 10, 14)));
Console.WriteLine(FormattingHelpers.FormatBytes(2_621_440));
Console.WriteLine(FormattingHelpers.FormatStatus("In Progress"));
Console.WriteLine(FormattingHelpers.Indent("MYPROJ-101 Fix login"));
```

Output:

```
             SPRINT SUMMARY             
----------------------------------------
| Sprint           | Planned | Completed | Completion | 
|------------------|---------|-----------|------------|
| MYPROJ Sprint 41 | 45      | 38        | 84.4%      | 
| MYPROJ Sprint 40 | 1,200   | 1,147     | 95.6%      | 
2026-10-14
2.5 MB
🔄 In Progress
  MYPROJ-101 Fix login
```

## ValidationHelpers

`ValidationHelpers` (namespace `JiraAnalyticsCli.Utils`) is a static class of validation and small text helpers. The `Is*` methods return `bool` and do not throw; the string checks return `false` for null, empty or whitespace input. The text helpers accept null and return an empty string where noted below.

Its public methods are:

- `IsValidJiraIssueKey(string? issueKey)` - matches `PROJ-123`: an uppercase letter, at least one more uppercase letter or digit, a `-`, then digits. The project part must be at least two characters, so `P-1` is rejected. Lowercase is rejected.
- `IsValidProjectKey(string? projectKey)` - uppercase letter followed by uppercase letters or digits, at most 10 characters in total. A single letter such as `A` is accepted.
- `IsValidUrl(string? url)` - accepts only absolute `http` and `https` URIs. `ftp://...` and relative paths are rejected.
- `IsValidEmail(string? email)` - uses `System.Net.Mail.MailAddress` and then requires the parsed `Address` to equal the input exactly. Plain `user@example.com` passes; the display-name form `Name <user@example.com>` is rejected.
- `IsValidSprintId(int sprintId)` - true for any value greater than 0.
- `IsValidStoryPoints(int? storyPoints)` - true for `null` or any value of 0 or more. Fractional story points are not checked here because the parameter is an `int`.
- `IsValidDateRange(DateTime startDate, DateTime endDate)` - true only when `startDate` is strictly earlier than `endDate`. Equal dates return `false`.
- `IsValidPercentage(double value)` - true for values from 0 to 100 inclusive. `NaN` returns `false`.
- `TruncateWithEllipsis(string? value, int maxLength = 50)` - returns null or empty input as `""`, and returns `value` unchanged when it fits. Otherwise it keeps the first `maxLength - 3` characters and appends `...`, so the result is exactly `maxLength` long. A `maxLength` below 3 throws `ArgumentOutOfRangeException` when the value is too long to fit (`maxLength = 2` with `"abcdefghij"` throws; `maxLength = 3` returns `"..."`).
- `SanitizeForCsv(string? value)` - removes commas, double quotes, `\r` and `\n` from the value. It does not escape them, so the text changes: `a,"b"\nc` becomes `abc`. Null or empty input returns `""`. Use it only where losing those characters is acceptable.
- `ToProgressBar(double percentage, int length = 20)` - builds a text bar such as `[█████░░░░░]`, with `█` for the filled part and `░` for the rest. The filled count is `(int)(percentage / 100 * length)`, rounded down. A percentage outside 0-100, or `NaN`, is treated as 0. A negative `length` throws `ArgumentOutOfRangeException`.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;
using System;

Console.WriteLine(ValidationHelpers.IsValidJiraIssueKey("MYPROJ-101"));  // True
Console.WriteLine(ValidationHelpers.IsValidProjectKey("myproj"));        // False
Console.WriteLine(ValidationHelpers.IsValidUrl("https://example.atlassian.net")); // True
Console.WriteLine(ValidationHelpers.IsValidDateRange(
    new DateTime(2026, 10, 1), new DateTime(2026, 10, 14)));             // True

Console.WriteLine(ValidationHelpers.TruncateWithEllipsis("Refactor the login flow", 12)); // Refactor ...
Console.WriteLine(ValidationHelpers.SanitizeForCsv("Fix \"login\", retry"));            // Fix login retry
Console.WriteLine(ValidationHelpers.ToProgressBar(50, 10));              // [█████░░░░░]
Console.WriteLine(ValidationHelpers.ToProgressBar(150, 10));             // [░░░░░░░░░░]
```

## License

This project is licensed under the [MIT License](LICENSE).
