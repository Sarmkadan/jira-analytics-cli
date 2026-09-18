# CLAUDE.md

.NET 10 console CLI (System.CommandLine) that pulls Jira sprint/issue data via REST and produces analytics, burndown charts (SkiaSharp), and HTML/Markdown/CSV reports.

## Build

- SDK: .NET 10.0.100 (`global.json`, rollForward latestMinor)
- `dotnet restore && dotnet build -c Debug` or `make build`
- Release: `dotnet build -c Release` or `make release`
- Publish self-contained: `make publish-linux|publish-windows|publish-macos|publish-all`
- Docker: `make docker`, `docker-compose up`
- Run locally: `dotnet run -- <command> [options]` or `make run`
- Solution: `jira-analytics-cli.sln` (main project + tests). Main csproj excludes `tests/**`, `benchmarks/**`, `examples/**` from compilation.

## Test

- `dotnet test` or `make test` (xUnit + FluentAssertions + Moq)
- Coverage: `make test-coverage` (coverlet/opencover, output in `TestResults/`)
- CI (`.github/workflows/ci.yml`): `dotnet restore`, `dotnet build --no-restore -c Release`, `dotnet test --no-build -c Release`
- Test project `tests/jira-analytics-cli.Tests/` does NOT reference the main project; it includes source files explicitly via `<Compile Include="..\..\...">`. When adding a new class under test, add it to that list in `jira-analytics-cli.Tests.csproj`.
- Benchmarks: `benchmarks/jira-analytics-cli.Benchmarks/` (BenchmarkDotNet, separate csproj, not in the solution).

## Lint / Format

- `dotnet format` or `make format`
- `make lint` = Release build with `/p:EnforceCodeStyleInBuild=true`
- Style rules in `.editorconfig` (4-space indent, PascalCase public members, camelCase locals). `TreatWarningsAsErrors=false`; several nullable warnings suppressed in `Directory.Build.props`.

## Layout

- `Program.cs` - entry point; `BuildRootCommand` wires all CLI commands: `analytics`, `export`, `export-csv`, `export-team-csv`, `burndown`, `jql`, `report`, `team-compare`, `report-md`, `trend`.
- `ServiceFactory.cs` - DI container (`Microsoft.Extensions.DependencyInjection`), logging, named `HttpClient("jira")` with Bearer auth, Polly.
- `Configuration/` - `CliConfig`/`ICliConfig`, `AppConfigurationProvider` (reads `appsettings.json`, then env overrides: `JIRA_URL`/`JIRA_BASE_URL`, `JIRA_TOKEN`/`JIRA_API_TOKEN`, `JIRA_PROJECT`, `CACHE_EXPIRATION_MINUTES`, `DETAILED_LOGGING`).
- `Services/` - business logic: `JiraApiService`, `AnalyticsService`, `ReportService`, `HtmlReportService`, `MarkdownReportService`, `ExportService`, `CsvExportService`, `JqlQueryService`, `TeamComparisonService`, `SnapshotStore`. Each has an `I*` interface.
- `Repositories/` - `IssueRepository`, `SprintRepository`, `MetricsRepository` (+ interfaces) over the API service.
- `Models/` - POCOs: `JiraIssue`, `Sprint`, `SprintMetric`, `Developer`, `JiraProject`, `BurndownSnapshot`, `CycleTimeResult`, `TrendAnalysis`, `TimeSeriesPoint`.
- `Formatters/` - `Json/Csv/Xml/MarkdownFormatter`.
- `Caching/` - `InMemoryCache`, `CacheManager`, `CachePolicy`.
- `Utils/` - `DateTimeExtensions`, `StringExtensions`, `CollectionExtensions`, `FormattingHelpers`, `ValidationHelpers`.
- `Exceptions/` - `JiraApiException`, `ConfigurationException`.
- `docs/` - per-class markdown docs; `examples/` - usage samples (not compiled).
- `Services/*.backup*` files are stale copies; ignore them.

## Conventions

- Namespace = folder: `JiraAnalyticsCli.Services`, `.Models`, `.Repositories`, `.Utils`, etc. File-scoped namespaces preferred (`Program.cs`/`ServiceFactory.cs` are root `JiraAnalyticsCli`).
- Nullable + ImplicitUsings enabled; XML doc comments generated (`GenerateDocumentationFile=true`).
- Interfaces `IFoo` alongside `Foo`; register both in `ServiceFactory`.
- Helper code split into partial/extension files by suffix: `FooExtensions.cs`, `FooJsonExtensions.cs`, `FooValidation.cs`. Tests mirror this: `FooTests.cs`, `FooTestsExtensions.cs`, `FooTestsValidation.cs`.
- Tests live in `tests/jira-analytics-cli.Tests/{Models,Services,Repositories,Utils,Formatters}/`, xUnit `[Fact]`/`[Theory]`, FluentAssertions `.Should()`, Moq for `IJiraApiService`/`ILogger`.
- Argument validation via `ArgumentNullException.ThrowIfNull` / `ValidationHelpers`; tests assert those throws.
- Commit messages: conventional prefixes (`feat:`, `fix:`, `test:`, `docs:`, `chore:`).
- Never commit `.aider*`, `bin/`, `obj/`, `TestResults/`, `appsettings.local.json` (see `.gitignore`).
