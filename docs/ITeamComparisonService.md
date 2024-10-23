# ITeamComparisonService

A service interface that provides comparative analytics for multiple teams within a Jira project, enabling evaluation of performance metrics such as velocity, quality, and cycle time across teams.

## API

### `public string ProjectKey`
The Jira project key (e.g., "PROJ") for which the comparison data was generated. This value is used to scope all subsequent metrics and team snapshots.

### `public double AverageVelocity`
The average velocity across all teams in the project, measured in story points completed per sprint. This value is calculated as the arithmetic mean of each team's velocity over the analyzed sprints.

### `public double AvgCompletionRate`
The average issue completion rate across all teams, expressed as a percentage. This represents the proportion of issues started that were completed within the analyzed timeframe.

### `public int TotalPointsDelivered`
The total number of story points delivered by all teams combined during the analyzed period. This aggregates the velocity of all teams.

### `public int TotalIssuesCompleted`
The total number of issues completed by all teams during the analyzed period. This aggregates the issue completion counts of all teams.

### `public int TotalDefects`
The total number of defects logged across all teams during the analyzed period. Defects are typically issues marked with a specific type or label indicating a bug.

### `public double DefectRate`
The defect rate across all teams, expressed as a percentage. This is calculated as `(TotalDefects / TotalIssuesCompleted) * 100`.

### `public double AvgCycleTime`
The average cycle time across all teams, measured in days. Cycle time is the duration from issue creation to completion, averaged across all completed issues.

### `public string OverallHealth`
A qualitative health indicator for the project, derived from aggregated metrics such as velocity trend, defect rate, and completion rate. Possible values include "Healthy", "At Risk", or "Needs Attention".

### `public double VelocityTrend`
The trend in team velocity over time, expressed as a percentage change. A positive value indicates increasing velocity; a negative value indicates decreasing velocity.

### `public int SprintCount`
The number of sprints included in the analysis period. This defines the scope of the data used to compute all metrics.

### `public DateTime GeneratedAt`
The timestamp indicating when the comparison data was generated. This ensures data freshness and reproducibility.

### `public List<TeamProjectSnapshot> Teams`
A list of team-specific snapshots containing detailed metrics for each team. Each snapshot includes team-level velocity, quality, and cycle time data.

### `public string? FastestTeam`
The name of the team with the highest average velocity. This is `null` if no teams are present or if metrics are unavailable.

### `public string? HighestQualityTeam`
The name of the team with the lowest defect rate. This is `null` if no teams are present or if metrics are unavailable.

### `public string? MostConsistentTeam`
The name of the team with the lowest standard deviation in cycle time. This is `null` if no teams are present or if metrics are unavailable.

## Usage

### Example 1: Basic Usage
