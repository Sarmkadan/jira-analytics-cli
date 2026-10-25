# ExportService

The `ExportService` class exports Jira sprint analytics, burndown charts, and team metrics to files. Output formats are PNG/JPG and SVG (charts drawn with SkiaSharp), Markdown, JSON, and CSV. It implements `IExportService`.

Source: `Services/ExportService.cs`. Interface: `Services/IExportService.cs`.

## Dependencies

The constructor takes four arguments, all of which must be non-null:

- `IJiraApiService` - fetches sprints, sprint issues, and burndown data.
- `IAnalyticsService` - produces `SprintAnalysisResult` and `TeamAnalysisResult`.
- `ILogger<ExportService>` - information and error logging.
- `IServiceProvider` - resolves `IMarkdownReportService` and `ICsvExportService` on demand.

Every public method logs at information level on entry and at error level on failure, then rethrows the original exception.

## API

### Task ExportAnalytics(string projectKey, string format, string outputPath)
Analyses the five most recent sprints for `projectKey` (`AnalyzeSprints(projectKey, 5)`) and writes the result in `format`. The format is matched case-insensitively with `ToLower()`.  

| `format` | Output | Notes |
|---|---|---|
| `png`, `jpg` | Sprint velocity bar chart, 1200×800 | JPEG is encoded at quality 80. |
| `svg` | Same velocity chart as SVG | Bar heights are clamped so a maximum velocity of zero or less becomes `1`. |
| `json` | The `SprintAnalysisResult`, indented | Written by `ExportAsJson`. |
| `csv` | One row per sprint | Written by `ExportAnalyticsAsCsv`. Columns: `Sprint Name`, `Planned Points`, `Completed Points`, `Completion Rate %`, `Quality Score`, `Velocity`, `Health Status`. |
| `md` | Markdown report | Written by `ExportAsMarkdown`. |

**Exceptions:**  
- `NotSupportedException` with the message `Format {format} is not supported` for any other value.  
- Exceptions from `IAnalyticsService` and from the image, file, or CSV writers are propagated.  

### Task ExportBurndownChart(int sprintId, string format, string outputPath)
Exports a burndown chart for one sprint. The sprint is fetched with `GetSprintAsync`, its issues are added to `sprint.Issues`, and the burndown series comes from `GetBurndownDataAsync`.  

| `format` | Output |
|---|---|
| `png`, `jpg` | Raster chart, 1200×800, JPEG at quality 80 for `jpg`. |
| `svg` | Same chart as SVG. |
| `json` | The list of `BurndownSnapshot` values, indented. |

The chart has a title (`Burndown Chart - {sprint name}`), X and Y axes, a dashed grey "ideal" line from the bottom-left corner to a fixed point near the top-right, and a red line for actual remaining story points. The actual line is scaled to the largest `TotalStoryPoints` value. If no snapshot has positive total points, a flat line is drawn at the baseline. The ideal line is not calculated from the sprint dates.

**Exceptions:**  
- `InvalidOperationException` with the message `Sprint {sprintId} not found` if `GetSprintAsync` returns `null`.  
- Exceptions from the Jira service and from the writers are propagated.  

**Note:** Any other `format` is accepted without error. Nothing is written to `outputPath`, and the method still logs success.

### Task ExportTeamMetrics(string projectKey, string format, string outputPath)
Analyses the team for `projectKey` (`AnalyzeTeam`) and writes the result.  

| `format` | Output |
|---|---|
| `json` | The `TeamAnalysisResult`, indented. |
| `csv` | Columns `Developer` and `Assigned Issues`, one row per entry in `WorkloadDistribution`. |

**Exceptions:** `NotSupportedException` with the message `Format {format} is not supported` for any other value. Image and `md` formats are not available here.

### Task ExportAsMarkdown(SprintAnalysisResult analysis, string outputPath)
Public on the class only; it is not part of `IExportService`. It calls `AnalyzeTeam("unknown")` and passes the result to `IMarkdownReportService.BuildMarkdown("unknown", analysis, teamAnalysis)`. The project name in the report is therefore always `unknown`.  
Creates the parent directory if it is missing, then writes the report as UTF-8 (`Encoding.UTF8`, with a byte-order mark).

### Task ExportAsJson(object data, string outputPath)
Serialises `data` with `JsonSerializer` using `WriteIndented = true` and writes it as UTF-8. Does not create missing directories.

### Task ExportAsCsv(List<Dictionary<string, object>> data, string outputPath)
Writes a generic CSV from a list of dictionaries.  
- An empty list produces an empty file.  
- The header is the key list of the **first** dictionary, in insertion order. Keys that appear only in later rows are dropped.  
- A row that lacks a header key gets an empty cell.  
- Values are formatted with `FormatCsvCell` (`IFormattable` values use the invariant culture; `null` becomes empty) and then escaped. A value that contains a comma, a double quote, or a line feed is wrapped in double quotes, with embedded quotes doubled.  
- Lines end with `AppendLine`, so the platform newline is used. The file is written as UTF-8.  
- Header names are written without escaping.

**Exceptions:** Exceptions from `File.WriteAllTextAsync` are propagated.

### Task ExportSprintMetricsCsv(IEnumerable<SprintMetric> metrics, string path)
Resolves `ICsvExportService` from the service provider and calls its `ExportSprintMetrics`. See [CsvExportService](CsvExportService.md) for the header and escaping rules.

### Task ExportTeamMetricsCsv(IEnumerable<KeyValuePair<string, int>> metrics, string path)
Resolves `ICsvExportService` and calls its `ExportTeamMetrics`. See [CsvExportService](CsvExportService.md).

## Behaviour notes

- **Directories:** Only `ExportAsMarkdown` creates the output directory. Image, SVG, JSON, and CSV writers expect it to exist.  
- **Overwrite:** Image and SVG writers open the file with `File.OpenWrite`, which does not truncate. Exporting over a longer existing file can leave stale trailing bytes. JSON, CSV, and Markdown writers replace the file.  
- **Empty data:** With no sprint metrics the raster chart uses a maximum velocity of `1`, so the bar area is empty but a file is produced. The raster and SVG versions differ here: the SVG version clamps a non-positive maximum to `1`, while the raster version does not.  
- **Fixed sizes:** Chart dimensions (1200×800), padding, colours, and the sprint count (5) are constants in the source.  
- **Validation:** The methods do not validate `projectKey`, `format`, or `outputPath`. A `null` `format` causes a `NullReferenceException` when `ToLower()` is called.  
- **DI registration:** `IExportService` is not registered in `ServiceFactory.cs`, which registers only `ICsvExportService` for this area. Code that resolves `IExportService` from that container (the `export` command in `Program.cs` and `ReportService`) will fail at resolution time unless it is registered elsewhere. `examples/IntegrationExample.cs` shows a registration: `AddSingleton<IExportService, ExportService>()`.

## Usage

```csharp
// Registration used in the examples project
builder.Services.AddSingleton<IExportService, ExportService>();

// Resolve and export the five-sprint analytics report as Markdown
var exportService = serviceProvider.GetRequiredService<IExportService>();
await exportService.ExportAnalytics("PROJ", "md", "output/report.md");

// Export a burndown chart for sprint 42 as SVG
await exportService.ExportBurndownChart(42, "svg", "output/sprint-42-burndown.svg");

// Export team workload as CSV
await exportService.ExportTeamMetrics("PROJ", "csv", "output/team.csv");
```
