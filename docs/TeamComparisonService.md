# TeamComparisonService

## Overview
The `TeamComparisonService` compares sprint and quality metrics across multiple Jira projects by fetching each project's sprint history in parallel and aggregating the results into a ranked side-by-side report.

## Purpose
This service is designed to help engineering managers and team leads compare the performance of different teams (represented by Jira projects) based on key agile metrics such as velocity, completion rate, defect rate, and cycle time.

## Key Features
- Fetches sprint analytics for multiple projects in parallel
- Calculates aggregated metrics for each project:
  - Average velocity (story points per sprint)
  - Average completion rate (%)
  - Total story points delivered
  - Total defects count
  - Defect rate (%)
  - Average cycle time (days)
  - Overall health score
- Identifies top-performing teams in three categories:
  - Fastest team (highest average velocity)
  - Highest quality team (lowest defect rate)
  - Most consistent team (highest average completion rate)
- Provides formatted output as plain text table or GitHub-flavored markdown table

## Usage Example
```csharp
// Initialize the service with dependencies
ITeamComparisonService comparisonService = new TeamComparisonService(
    analyticsService, 
    logger
);

// Define projects to compare and number of sprints to analyze
var projectKeys = new[] { "PROJ-A", "PROJ-B", "PROJ-C" };
int sprintCount = 5;

// Run the comparison asynchronously
TeamComparisonReport report = await comparisonService.CompareTeamsAsync(
    projectKeys, 
    sprintCount
);

// Output as formatted text table
Console.WriteLine(TeamComparisonService.FormatAsText(report));

// Output as markdown table (suitable for GitHub README, etc.)
var markdown = TeamComparisonService.RenderMarkdownTable(report);
Console.WriteLine(markdown);
```

## Methods

### CompareTeamsAsync
```csharp
public async Task<TeamComparisonReport> CompareTeamsAsync(
    IEnumerable<string> projectKeys,
    int sprintCount = 5,
    CancellationToken cancellationToken = default)
```
Compares multiple Jira projects based on their sprint analytics.

**Parameters:**
- `projectKeys`: Collection of Jira project keys to compare (null values and empty strings are filtered out)
- `sprintCount`: Number of recent closed sprints to analyze per project (default: 5, must be positive)
- `cancellationToken`: Optional token to cancel the operation

**Returns:**
- `TeamComparisonReport` containing snapshots for each project and identified top performers

**Exceptions:**
- `ArgumentNullException`: If `projectKeys` is null
- `ArgumentException`: If no valid project keys are provided after filtering
- `ArgumentOutOfRangeException`: If `sprintCount` is not positive

### FormatAsText
```csharp
public static string FormatAsText(TeamComparisonReport report)
```
Formats the comparison report as a plain text table with box-drawing characters for console display.

**Parameters:**
- `report`: The team comparison report to format

**Returns:**
- Formatted string suitable for console output

### RenderMarkdownTable
```csharp
public static string RenderMarkdownTable(TeamComparisonReport report)
```
Renders the comparison report as a GitHub-flavored markdown table.

**Parameters:**
- `report`: The team comparison report to render

**Returns:**
- Markdown formatted string suitable for documentation or GitHub

## Data Models

### TeamComparisonReport
Contains the results of a team comparison operation:
- `Teams`: List of `TeamProjectSnapshot` objects for each analyzed project
- `FastestTeam`: Project key of the team with highest average velocity
- `HighestQualityTeam`: Project key of the team with lowest defect rate
- `MostConsistentTeam`: Project key of the team with highest average completion rate
- `GeneratedAt`: Timestamp when the report was generated

### TeamProjectSnapshot
Represents analytics for a single project:
- `ProjectKey`: Jira project key
- `AverageVelocity`: Average story points completed per sprint
- `AvgCompletionRate`: Average percentage of issues completed per sprint
- `TotalPointsDelivered`: Total story points delivered across analyzed sprints
- `TotalIssuesCompleted`: Total number of issues completed
- `TotalDefects`: Total number of defects found
- `DefectRate`: Percentage of issues that were defects
- `AvgCycleTime`: Average time in days for issues to move from "In Progress" to "Done"
- `OverallHealth`: Health assessment string (e.g., "Excellent", "Good", "Fair", "Poor")
- `VelocityTrend`: Percentage change in velocity over the analyzed period
- `SprintCount`: Number of sprints analyzed

## Implementation Details
- Uses parallel processing via `Task.WhenAll` to fetch analytics for all projects concurrently
- Handles missing data gracefully by returning null snapshots for failed fetches
- Applies defensive programming with null checks and argument validation
- Uses `ILogger` for structured logging at appropriate levels (Information, Debug, Warning, Error)
- Formats numbers using `CultureInfo.InvariantCulture` for consistent output across environments
- Builds award icons (⚡, ✅, 🎯) to visually indicate top-performing teams in the formatted output

## Dependencies
- `IAnalyticsService`: Used to fetch and analyze sprint data for individual projects
- `ILogger<TeamComparisonService>`: For logging operations and diagnostics

## Error Handling
- Failed project fetches are logged as errors and excluded from the final report (null snapshots filtered out)
- If no projects have valid data, the report will contain empty teams lists and N/A values for rankings
- All public methods validate their arguments and throw appropriate exceptions for invalid input