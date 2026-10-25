# CsvExportService

The `CsvExportService` class writes sprint and team metrics to CSV files. It implements `ICsvExportService`, is registered as a singleton in `ServiceFactory`, and is used by the `export-csv` and `export-team-csv` commands in `Program.cs` as well as by `ExportService`.

Source: `Services/CsvExportService.cs`. Interface: `Services/ICsvExportService.cs`.

## API

### CsvExportService(ILogger<CsvExportService> logger)
Creates a new instance of the service.  
**Parameters:**  
- `logger`: Logger used for information, warning, and error messages. Must not be `null`.  

**Return value:** A ready-to-use service.  
**Exceptions:**  
- `ArgumentNullException` if `logger` is `null`.  

### Task ExportSprintMetrics(IEnumerable<SprintMetric> metrics, string path, int bufferSize = 4096)
Writes sprint metrics to a CSV file at `path`, one row per `SprintMetric`, preceded by a header row. An existing file at `path` is overwritten. The output is produced by streaming the enumerable, so the collection is enumerated once and is not copied into memory.  
**Parameters:**  
- `metrics`: The sprint metrics to export. Must not be `null`. An empty sequence produces a file containing only the header.  
- `path`: Output file path. Resolved with `Path.GetFullPath` and must lie under the current working directory (see Notes).  
- `bufferSize`: Buffer size, in characters, for the underlying `StreamWriter`. Default is `4096`.  

**Return value:** A `Task` that completes when the file has been written.  
**Exceptions:**  
- `ArgumentNullException` if `metrics` or `path` is `null`.  
- `ArgumentException` if `path` is empty or whitespace.  
- `IOException` with the message `Output path is outside the intended output root` if the resolved path is not under the current working directory.  
- Any exception thrown by `StreamWriter` or by enumerating `metrics` (for example `IOException` or `UnauthorizedAccessException`). These are logged at error level and rethrown unchanged.  

**Header (21 columns, in order):**  
`SprintId`, `SprintName`, `StartDate`, `EndDate`, `PlannedStoryPoints`, `CompletedStoryPoints`, `CommittedStoryPoints`, `CompletedIssueCount`, `TotalIssueCount`, `DefectsCount`, `AverageCycleTime`, `OverdueIssueCount`, `TeamSize`, `ScopeChangeCount`, `Velocity`, `CompletionRate%`, `CommitmentAccuracy%`, `QualityScore`, `ProductivityPerTeamMember`, `DailyBurndownRate`, `HealthStatus`

The derived columns (`Velocity`, `CompletionRate%`, `CommitmentAccuracy%`, `QualityScore`, `ProductivityPerTeamMember`, `DailyBurndownRate`, `HealthStatus`) are computed by calling the matching `SprintMetric` methods (`GetVelocity()`, `GetCompletionRate()`, `GetCommitmentAccuracy()`, `GetQualityScore()`, `GetProductivityPerTeamMember()`, `GetDailyBurndownRate()`, `GetHealthStatus()`).

### Task ExportTeamMetrics(IEnumerable<KeyValuePair<string, int>> metrics, string path, int bufferSize = 4096)
Writes team metrics to a CSV file at `path`. Each key is a developer name and each value is the number of assigned issues. The header is `Developer,AssignedIssues`. Overwrite, path-resolution, and buffer-size behaviour match `ExportSprintMetrics`.  
**Parameters:**  
- `metrics`: Developer name to assigned-issue count pairs. Must not be `null`. An empty sequence produces a file containing only the header.  
- `path`: Output file path, resolved as described above.  
- `bufferSize`: Buffer size for the `StreamWriter`. Default is `4096`.  

**Return value:** A `Task` that completes when the file has been written.  
**Exceptions:** Same as `ExportSprintMetrics`.

## Output format

- **Encoding:** UTF-8 (`Encoding.UTF8`, so the file is written with a byte-order mark).  
- **Line endings:** Each line is written with `WriteLineAsync`, so the platform newline is used.  
- **Dates:** `StartDate` and `EndDate` are formatted as `yyyy-MM-dd` with the invariant culture.  
- **Numbers:** Non-null numeric values are formatted with `CultureInfo.InvariantCulture`. `null` becomes an empty cell.  
- **Text sanitising:** Sprint names, health statuses, and developer names that begin with `=`, `+`, `-`, `@`, tab, or carriage return are prefixed with `'` to reduce spreadsheet formula injection. Numeric cells are not prefixed.  
- **Quoting:** A text cell that contains a comma, a double quote, or a line feed is wrapped in double quotes, and embedded double quotes are doubled. Other cells are written as-is.  

## Usage

```csharp
// Resolved through DI in ServiceFactory
var csvService = serviceProvider.GetRequiredService<ICsvExportService>();

var analysis = await analyticsService.AnalyzeSprints("PROJ", 5);
await csvService.ExportSprintMetrics(analysis.Metrics, "reports/sprints.csv");
```

```csharp
var teamAnalysis = await analyticsService.AnalyzeTeam("PROJ");
await csvService.ExportTeamMetrics(teamAnalysis.WorkloadDistribution, "reports/team.csv");
```

Both calls are relative to the current working directory. An absolute path outside that directory fails with `IOException`.

## Notes

- **Output root check:** The check is a case-insensitive string-prefix comparison of the resolved path against `Directory.GetCurrentDirectory()`. It is not a true directory-containment check. For example, when the working directory is `/data/out`, a path resolved to `/data/outside.csv` also passes. The path is resolved (including any `..` segments) before the check.  
- **Logging:** Export start and success are logged at information level. An empty input logs a warning and still writes the header. Failures are logged at error level before being rethrown.  
- **Team values:** `ExportTeamMetrics` converts each `int` value with `ToString()` and no culture argument, so the current culture is used. Other values in the file use the invariant culture.  
- **Concurrency:** Each call opens its own `StreamWriter`. The service holds no mutable state beyond its logger, so it can be called concurrently for different output paths. Concurrent calls to the same path are not coordinated and will overwrite each other.  
- **Helpers:** `ExportSprintMetricsAsync`, `ExportTeamMetricsAsync`, `FormatCsvCell`, `EscapeCsvValue`, `SanitizeCsvValue`, `AppendEscaped`, `GetCanonicalizedPath`, and `IsPathUnderOutputRoot` are private. Tests in `tests/jira-analytics-cli.Tests/Services/CsvExportServiceTests.cs` cover the public methods.
