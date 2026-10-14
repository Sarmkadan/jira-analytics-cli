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

- `JIRA_URL` (or `JIRA_BASE_URL`) is the base URL of your Jira instance.
- `JIRA_API_TOKEN` (or `JIRA_TOKEN`) is sent as a Bearer token. The CLI does not read `JIRA_EMAIL`; see [Use JiraClient from C#](#6-use-jiraclient-from-c) for the case where it is needed.

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
