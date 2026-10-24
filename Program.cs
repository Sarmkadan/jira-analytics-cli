// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.CommandLine;
using System.CommandLine.Parsing;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Diagnostics;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace JiraAnalyticsCli;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            // --check-config reports every problem itself, so it runs before the fail-fast check below.
            if (!IsHelpOrVersionRequest(args) && args.Contains(CheckConfigFlag))
            {
                return await RunConfigCheckAsync(args);
            }

            // doctor also reports every problem itself (missing settings become check results), so it runs before the fail-fast check.
            if (!IsHelpOrVersionRequest(args) && IsDoctorRequest(args))
            {
                return await RunDoctorAsync(args);
            }

            // Resolve and validate Jira settings before any command runs, so a bad setting fails before any HTTP call.
            // Help and version requests skip validation so they work without a configured token.
            var jiraOptions = new JiraOptions();
            var velocityOptions = new VelocityOptions();
            if (!IsHelpOrVersionRequest(args))
            {
                var loaded = JiraOptionsLoader.Load(ReadJiraFlags(args), Environment.GetEnvironmentVariable, ReadAppSettings());
                if (!loaded.IsValid)
                {
                    Console.Error.WriteLine(JiraOptionsLoader.FormatProblems(loaded.Problems));
                    return 2;
                }

                jiraOptions = loaded.Options;

                // Velocity settings are checked here too, so a bad value stops the run before any Jira call.
                var velocityLoaded = VelocityOptionsLoader.Load(ReadVelocityFlags(args), Environment.GetEnvironmentVariable, ReadAppSettings());
                if (!velocityLoaded.IsValid)
                {
                    Console.Error.WriteLine(VelocityOptionsLoader.FormatProblems(velocityLoaded.Problems));
                    return 2;
                }

                velocityOptions = velocityLoaded.Options!;
            }

            // Initialize dependency injection container via factory
            var serviceProvider = ServiceFactory.CreateServiceProvider(jiraOptions, velocityOptions, IsVerboseRequest(args));

            // Get logger and log startup
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Jira Analytics CLI starting...");

            // Build CLI commands
            var rootCommand = BuildRootCommand(serviceProvider);

            // Execute command
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }


    /// <summary>Root flag that runs the configuration checks and exits.</summary>
    private const string CheckConfigFlag = "--check-config";

    /// <summary>Global flag that logs each Jira request and the summarized analysis inputs at Debug level.</summary>
    private const string VerboseFlag = "--verbose";

    /// <summary>Subcommand that runs the health checks and exits.</summary>
    private const string DoctorCommandName = "doctor";

    /// <summary>
    /// Runs every configuration check, prints one OK/FAIL/SKIP line per item, and returns 0 only when nothing failed.
    /// </summary>
    private static async Task<int> RunConfigCheckAsync(string[] args)
    {
        var loaded = JiraOptionsLoader.Load(ReadJiraFlags(args), Environment.GetEnvironmentVariable, ReadAppSettings());
        var items = await JiraConfigChecker.RunAsync(loaded);

        Console.WriteLine(JiraConfigChecker.Format(items));
        return items.Any(item => item.Status == ConfigCheckStatus.Failed) ? 1 : 0;
    }

    /// <summary>
    /// Runs the <c>doctor</c> checks, prints the status table with remediation hints, and returns 1 when any check failed.
    /// </summary>
    private static async Task<int> RunDoctorAsync(string[] args)
    {
        var jira = JiraOptionsLoader.Load(ReadJiraFlags(args), Environment.GetEnvironmentVariable, ReadAppSettings());
        var velocity = VelocityOptionsLoader.Load(ReadVelocityFlags(args), Environment.GetEnvironmentVariable, ReadAppSettings());
        var results = await JiraDoctor.RunAsync(jira, velocity);

        Console.WriteLine(JiraDoctor.Format(results));
        return JiraDoctor.HasFailures(results) ? 1 : 0;
    }

    /// <summary>
    /// True when the parser selects the <c>doctor</c> subcommand. Parsing, rather than searching for the word,
    /// keeps a value such as <c>-p doctor</c> from being read as the command.
    /// </summary>
    private static bool IsDoctorRequest(string[] args)
    {
        var probe = new RootCommand();
        probe.AddCommand(new Command(DoctorCommandName));
        return probe.Parse(args).CommandResult.Command.Name == DoctorCommandName;
    }

    /// <summary>
    /// Reports a validation problem on an option, so System.CommandLine rejects the command before its handler runs and before any Jira call.
    /// </summary>
    private static void Reject(OptionResult result, string? problem)
    {
        if (problem is not null)
        {
            result.ErrorMessage = problem;
        }
    }

    private static void ValidateFormat(Option<string> format, IReadOnlyCollection<string> supported) =>
        format.AddValidator(result => Reject(result, OutputOptionsValidator.CheckFormat(result.GetValueOrDefault<string>(), supported)));

    private static void ValidateSprintCount(Option<int> sprints) =>
        sprints.AddValidator(result => Reject(result, OutputOptionsValidator.CheckSprintCount(result.GetValueOrDefault<int>())));

    private static void ValidateOutputFile(Option<string> output) =>
        output.AddValidator(result => Reject(result, OutputOptionsValidator.CheckOutputFile(result.GetValueOrDefault<string>())));

    private static void ValidateOutputDirectory(Option<string> outputDir) =>
        outputDir.AddValidator(result => Reject(result, OutputOptionsValidator.CheckOutputDirectory(result.GetValueOrDefault<string>())));

    /// <summary>
    /// Parses only the global Jira flags, so settings can be validated before the command tree is built.
    /// </summary>
    private static JiraOptionsOverrides ReadJiraFlags(string[] args)
    {
        var probe = new RootCommand();
        var flags = JiraGlobalOptions.AddTo(probe);
        return flags.Read(probe.Parse(args));
    }

    /// <summary>
    /// Parses only the global velocity flags, so settings can be validated before the command tree is built.
    /// </summary>
    private static VelocityOptionsOverrides ReadVelocityFlags(string[] args)
    {
        var probe = new RootCommand();
        var flags = VelocityGlobalOptions.AddTo(probe);
        return flags.Read(probe.Parse(args));
    }

    private static string? ReadAppSettings() =>
        File.Exists(JiraOptionsLoader.AppSettingsFileName) ? File.ReadAllText(JiraOptionsLoader.AppSettingsFileName) : null;

    private static bool IsHelpOrVersionRequest(string[] args) =>
        args.Any(arg => arg is "-h" or "-?" or "/?" or "--help" or "--version");

    private static bool IsVerboseRequest(string[] args) => args.Contains(VerboseFlag);

    private static RootCommand BuildRootCommand(IServiceProvider serviceProvider)
    {
        var rootCommand = new RootCommand("Jira Analytics CLI - Advanced sprint and team metrics analysis");
        JiraGlobalOptions.AddTo(rootCommand);
        VelocityGlobalOptions.AddTo(rootCommand);
        // Handled in Main before this tree is used; declared here so it appears in --help.
        rootCommand.AddOption(new Option<bool>(CheckConfigFlag, "Check Jira settings, then the Jira connection (GET /myself), print OK/FAIL per item and exit. Run on its own."));
        // Handled in Main before this tree is used; declared here so it appears in --help.
        rootCommand.AddCommand(new Command(DoctorCommandName, "Check config, DNS/TLS, authentication, board, closed sprint and story-points field. Prints a table with a fix hint per failure and exits 1 if any check fails. Run on its own."));
        // Read in Main before the container is built, so the logger level is set from the start; declared here so it appears in --help and is accepted after a subcommand.
        rootCommand.AddGlobalOption(new Option<bool>(VerboseFlag, "Log each Jira request (method, URL, status, duration) and analysis input counts at Debug level. Secrets are never logged."));

        // Analytics command
        var analyticsCommand = new Command("analytics", "Run analytics and generate reports");
        var analyticsProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key");
        var analyticsSprintsOpt = new Option<int>(new[] { "-s", "--sprints" }, () => 5, "Number of recent sprints to analyze (1-50)");
        var analyticsOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output file path") { IsRequired = false };
        var analyticsOutputDirOpt = new Option<string>(new[] { "--output-dir" }, "Directory to save reports to (created if it does not exist)");
        ValidateSprintCount(analyticsSprintsOpt);
        ValidateOutputFile(analyticsOutputOpt);
        ValidateOutputDirectory(analyticsOutputDirOpt);
        analyticsCommand.AddOption(analyticsProjectOpt);
        analyticsCommand.AddOption(analyticsSprintsOpt);
        analyticsCommand.AddOption(analyticsOutputOpt);
        analyticsCommand.AddOption(analyticsOutputDirOpt);

        analyticsCommand.SetHandler(async (project, sprints, output, outputDir) =>
        {
            var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>();
            var reportService = serviceProvider.GetRequiredService<IReportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Starting analytics for project {Project} with {SprintCount} sprints", project, sprints);
                var analysis = await analyticsService.AnalyzeSprints(project, sprints);
                var report = reportService.GenerateReport(analysis);

                var resolvedOutput = ResolveOutputPath(output, outputDir, "report.txt");
                if (!string.IsNullOrEmpty(resolvedOutput))
                {
                    File.WriteAllText(resolvedOutput, report);
                    logger.LogInformation("Report written to {OutputPath}", resolvedOutput);
                }
                else
                {
                    Console.WriteLine(report);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Analytics command failed");
                throw;
            }
        }, analyticsProjectOpt, analyticsSprintsOpt, analyticsOutputOpt, analyticsOutputDirOpt);

        // Export command
        var exportCommand = new Command("export", "Export analytics data to image or file");
        var exportProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key");
        var exportFormatOpt = new Option<string>(new[] { "-f", "--format" }, () => "json", $"Export format ({string.Join(", ", OutputOptionsValidator.ExportFormats)})");
        var exportOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output file path");
        var exportOutputDirOpt = new Option<string>(new[] { "--output-dir" }, "Directory to save reports to (created if it does not exist)");
        ValidateFormat(exportFormatOpt, OutputOptionsValidator.ExportFormats);
        ValidateOutputFile(exportOutputOpt);
        ValidateOutputDirectory(exportOutputDirOpt);
        exportCommand.AddOption(exportProjectOpt);
        exportCommand.AddOption(exportFormatOpt);
        exportCommand.AddOption(exportOutputOpt);
        exportCommand.AddOption(exportOutputDirOpt);

        exportCommand.SetHandler(async (project, format, output, outputDir) =>
        {
            var exportService = serviceProvider.GetRequiredService<IExportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var resolvedOutput = ResolveOutputPath(output, outputDir, $"report.{format ?? "json"}");
                logger.LogInformation("Exporting analytics as {Format} to {Output}", format, resolvedOutput);
                await exportService.ExportAnalytics(project, format, resolvedOutput);
                logger.LogInformation("Export completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Export command failed");
                throw;
            }
        }, exportProjectOpt, exportFormatOpt, exportOutputOpt, exportOutputDirOpt);

    // CSV export command
    var csvExportCommand = new Command("export-csv", "Export sprint metrics to CSV format");
    var csvExportProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key") { IsRequired = true };
    var csvExportOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output CSV file path") { IsRequired = true };
    ValidateOutputFile(csvExportOutputOpt);
    csvExportCommand.AddOption(csvExportProjectOpt);
    csvExportCommand.AddOption(csvExportOutputOpt);

    csvExportCommand.SetHandler(async (project, output) =>
    {
        var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Exporting sprint metrics for project {Project} to CSV at {Output}", project, output);
            var analysis = await analyticsService.AnalyzeSprints(project, 5);
            var csvService = serviceProvider.GetRequiredService<ICsvExportService>();
            await csvService.ExportSprintMetrics(analysis.Metrics, output);
            logger.LogInformation("Sprint metrics exported successfully to {Output}", output);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CSV export command failed");
            throw;
        }
    }, csvExportProjectOpt, csvExportOutputOpt);

    // Team metrics CSV export command
    var teamCsvExportCommand = new Command("export-team-csv", "Export team metrics to CSV format");
    var teamCsvExportProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key") { IsRequired = true };
    var teamCsvExportOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output CSV file path") { IsRequired = true };
    ValidateOutputFile(teamCsvExportOutputOpt);
    teamCsvExportCommand.AddOption(teamCsvExportProjectOpt);
    teamCsvExportCommand.AddOption(teamCsvExportOutputOpt);

    teamCsvExportCommand.SetHandler(async (project, output) =>
    {
        var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Exporting team metrics for project {Project} to CSV at {Output}", project, output);
            var teamAnalysis = await analyticsService.AnalyzeTeam(project);
            var csvService = serviceProvider.GetRequiredService<ICsvExportService>();
            await csvService.ExportTeamMetrics(teamAnalysis.WorkloadDistribution, output);
            logger.LogInformation("Team metrics exported successfully to {Output}", output);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Team CSV export command failed");
            throw;
        }
    }, teamCsvExportProjectOpt, teamCsvExportOutputOpt);


        // Burndown command
        var burndownCommand = new Command("burndown", "Generate burndown chart for sprint");
        var burndownProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key");
        var burndownSprintIdOpt = new Option<int>(new[] { "--sprint-id" }, "Sprint ID");
        var burndownOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output image path");
        var burndownOutputDirOpt = new Option<string>(new[] { "--output-dir" }, "Directory to save reports to (created if it does not exist)");
        ValidateOutputFile(burndownOutputOpt);
        ValidateOutputDirectory(burndownOutputDirOpt);
        burndownCommand.AddOption(burndownProjectOpt);
        burndownCommand.AddOption(burndownSprintIdOpt);
        burndownCommand.AddOption(burndownOutputOpt);
        burndownCommand.AddOption(burndownOutputDirOpt);

        burndownCommand.SetHandler(async (project, sprintId, output, outputDir) =>
        {
            var reportService = serviceProvider.GetRequiredService<IReportService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var resolvedOutput = ResolveOutputPath(output, outputDir, "burndown.png");
                logger.LogInformation("Generating burndown chart for sprint {SprintId}", sprintId);
                await reportService.GenerateBurndownChart(project, sprintId, resolvedOutput);
                logger.LogInformation("Burndown chart saved to {Output}", resolvedOutput);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Burndown command failed");
                throw;
            }
        }, burndownProjectOpt, burndownSprintIdOpt, burndownOutputOpt, burndownOutputDirOpt);

        rootCommand.AddCommand(analyticsCommand);
        rootCommand.AddCommand(exportCommand);
        rootCommand.AddCommand(burndownCommand);
rootCommand.AddCommand(csvExportCommand);
rootCommand.AddCommand(teamCsvExportCommand);

        // JQL query command
        var jqlCommand         = new Command("jql", "Execute a custom JQL query and display results");
        var jqlQueryOpt        = new Option<string>(new[] { "-q", "--query" }, "JQL query string") { IsRequired = true };
        var jqlMaxResultsOpt   = new Option<int>(new[] { "-n", "--max-results" }, () => 50, "Maximum number of results to return");
        var jqlStartAtOpt      = new Option<int>(new[] { "--start-at" }, () => 0, "Zero-based index of the first result (pagination)");
        var jqlOutputOpt       = new Option<string>(new[] { "-o", "--output" }, "Output file path (omit to print to console)");
        var jqlFormatOpt       = new Option<string>(new[] { "-f", "--format" }, () => "text", "Output format: text or json");
        ValidateFormat(jqlFormatOpt, OutputOptionsValidator.TextOrJsonFormats);
        ValidateOutputFile(jqlOutputOpt);
        jqlCommand.AddOption(jqlQueryOpt);
        jqlCommand.AddOption(jqlMaxResultsOpt);
        jqlCommand.AddOption(jqlStartAtOpt);
        jqlCommand.AddOption(jqlOutputOpt);
        jqlCommand.AddOption(jqlFormatOpt);

        jqlCommand.SetHandler(async (query, maxResults, startAt, output, format) =>
        {
            var jqlService = serviceProvider.GetRequiredService<IJqlQueryService>();
            var logger     = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var result = await jqlService.ExecuteQueryAsync(new JqlQueryRequest
                {
                    Jql        = query,
                    MaxResults = maxResults,
                    StartAt    = startAt
                });

                string content = format.Equals("json", StringComparison.OrdinalIgnoreCase)
                    ? System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
                    : JqlQueryService.FormatAsText(result);

                if (!string.IsNullOrEmpty(output))
                {
                    var dir = Path.GetDirectoryName(output);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    await File.WriteAllTextAsync(output, content, System.Text.Encoding.UTF8);
                    logger.LogInformation("JQL results written to {Output}", output);
                }
                else
                {
                    Console.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "JQL command failed");
                throw;
            }
        }, jqlQueryOpt, jqlMaxResultsOpt, jqlStartAtOpt, jqlOutputOpt, jqlFormatOpt);

        // HTML report command
        var reportCommand        = new Command("report", "Generate a self-contained HTML analytics report");
        var reportProjectOpt     = new Option<string>(new[] { "-p", "--project" }, "Jira project key") { IsRequired = true };
        var reportSprintsOpt     = new Option<int>(new[] { "-s", "--sprints" }, () => 5, "Number of recent sprints to include (1-50)");
        var reportOutputOpt      = new Option<string>(new[] { "-o", "--output" }, "Output HTML file path");
        var reportOutputDirOpt   = new Option<string>(new[] { "--output-dir" }, "Directory to save the report (created if needed)");
        ValidateSprintCount(reportSprintsOpt);
        ValidateOutputFile(reportOutputOpt);
        ValidateOutputDirectory(reportOutputDirOpt);
        reportCommand.AddOption(reportProjectOpt);
        reportCommand.AddOption(reportSprintsOpt);
        reportCommand.AddOption(reportOutputOpt);
        reportCommand.AddOption(reportOutputDirOpt);

        reportCommand.SetHandler(async (project, sprints, output, outputDir) =>
        {
            var htmlService = serviceProvider.GetRequiredService<IHtmlReportService>();
            var logger      = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var resolvedOutput = ResolveOutputPath(output, outputDir, $"{project}-report.html");
                if (string.IsNullOrEmpty(resolvedOutput))
                {
                    resolvedOutput = $"{project}-report.html";
                }

                logger.LogInformation("Generating HTML report for {Project} to {Output}", project, resolvedOutput);
                await htmlService.GenerateReportAsync(project, sprints, resolvedOutput);
                logger.LogInformation("HTML report written to {Output}", resolvedOutput);
                Console.WriteLine($"HTML report saved to: {resolvedOutput}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Report command failed");
                throw;
            }
        }, reportProjectOpt, reportSprintsOpt, reportOutputOpt, reportOutputDirOpt);

        // Team comparison command
        var teamCompareCommand      = new Command("team-compare", "Compare metrics across multiple Jira projects side by side");
        var teamCompareProjectsOpt  = new Option<string>(new[] { "-p", "--projects" }, "Comma-separated list of project keys to compare") { IsRequired = true };
        var teamCompareSprintsOpt   = new Option<int>(new[] { "-s", "--sprints" }, () => 5, "Number of recent sprints per project (1-50)");
        var teamCompareOutputOpt    = new Option<string>(new[] { "-o", "--output" }, "Output file path (omit to print to console)");
        var teamCompareFormatOpt    = new Option<string>(new[] { "-f", "--format" }, () => "text", "Output format: text or json");
        ValidateSprintCount(teamCompareSprintsOpt);
        ValidateOutputFile(teamCompareOutputOpt);
        ValidateFormat(teamCompareFormatOpt, OutputOptionsValidator.TextOrJsonFormats);
        teamCompareCommand.AddOption(teamCompareProjectsOpt);
        teamCompareCommand.AddOption(teamCompareSprintsOpt);
        teamCompareCommand.AddOption(teamCompareOutputOpt);
        teamCompareCommand.AddOption(teamCompareFormatOpt);

        teamCompareCommand.SetHandler(async (projects, sprints, output, format) =>
        {
            var comparisonService = serviceProvider.GetRequiredService<ITeamComparisonService>();
            var logger            = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var keys   = projects.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var report = await comparisonService.CompareTeamsAsync(keys, sprints);

                string content = format.Equals("json", StringComparison.OrdinalIgnoreCase)
                    ? System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
                    : TeamComparisonService.FormatAsText(report);

                if (!string.IsNullOrEmpty(output))
                {
                    var dir = Path.GetDirectoryName(output);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    await File.WriteAllTextAsync(output, content, System.Text.Encoding.UTF8);
                    logger.LogInformation("Team comparison report written to {Output}", output);
                }
                else
                {
                    Console.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "team-compare command failed");
                throw;
            }
        }, teamCompareProjectsOpt, teamCompareSprintsOpt, teamCompareOutputOpt, teamCompareFormatOpt);

        rootCommand.AddCommand(jqlCommand);
        rootCommand.AddCommand(reportCommand);
        rootCommand.AddCommand(teamCompareCommand);

    // Markdown report command
    var markdownReportCommand = new Command("report-md", "Generate a Markdown analytics report");
    var markdownReportProjectOpt = new Option<string>(new[] { "-p", "--project" }, "Jira project key") { IsRequired = true };
    var markdownReportSprintsOpt = new Option<int>(new[] { "-s", "--sprints" }, () => 5, "Number of recent sprints to include (1-50)");
    var markdownReportOutputOpt = new Option<string>(new[] { "-o", "--output" }, "Output Markdown file path");
    var markdownReportOutputDirOpt = new Option<string>(new[] { "--output-dir" }, "Directory to save the report (created if needed)");
    ValidateSprintCount(markdownReportSprintsOpt);
    ValidateOutputFile(markdownReportOutputOpt);
    ValidateOutputDirectory(markdownReportOutputDirOpt);
    markdownReportCommand.AddOption(markdownReportProjectOpt);
    markdownReportCommand.AddOption(markdownReportSprintsOpt);
    markdownReportCommand.AddOption(markdownReportOutputOpt);
    markdownReportCommand.AddOption(markdownReportOutputDirOpt);

    markdownReportCommand.SetHandler(async (project, sprints, output, outputDir) =>
    {
        var markdownService = serviceProvider.GetRequiredService<IMarkdownReportService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var resolvedOutput = ResolveOutputPath(output, outputDir, $"{project}-report.md");
            if (string.IsNullOrEmpty(resolvedOutput))
            {
                resolvedOutput = $"{project}-report.md";
            }

            logger.LogInformation("Generating Markdown report for {Project} to {Output}", project, resolvedOutput);
            await markdownService.GenerateReportAsync(project, sprints, resolvedOutput);
            logger.LogInformation("Markdown report written to {Output}", resolvedOutput);
            Console.WriteLine($"Markdown report saved to: {resolvedOutput}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Markdown report command failed");
            throw;
        }
    }, markdownReportProjectOpt, markdownReportSprintsOpt, markdownReportOutputOpt, markdownReportOutputDirOpt);

    rootCommand.AddCommand(markdownReportCommand);

    // Trend command - analyzes the locally persisted snapshot history for a sprint
    var trendCommand = new Command("trend", "Analyze historical burndown trend for a sprint from locally persisted snapshots");
    var trendSprintIdOpt = new Option<int>(new[] { "--sprint-id" }, "Sprint ID") { IsRequired = true };
    trendCommand.AddOption(trendSprintIdOpt);

    trendCommand.SetHandler(async (int sprintId) =>
    {
        var snapshotStore = serviceProvider.GetRequiredService<ISnapshotStore>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var history = await snapshotStore.LoadAsync(sprintId);
            if (history.Count == 0)
            {
                Console.WriteLine($"No persisted snapshots found for sprint {sprintId}. Run 'burndown --sprint-id {sprintId}' first to start collecting history.");
                return;
            }

            var orderedHistory = history.ToList();
            var latest = orderedHistory[^1];
            var priorSnapshots = orderedHistory.Count > 1
                ? orderedHistory.GetRange(0, orderedHistory.Count - 1)
                : new List<BurndownSnapshot>();

            Console.WriteLine($"Trend analysis for sprint {sprintId} ({history.Count} snapshot(s))");
            Console.WriteLine(new string('-', 70));
            Console.WriteLine(latest.ToStatusString());
            Console.WriteLine($"Velocity trend:       {latest.CalculateVelocityTrend(priorSnapshots):F2} pts/day");
            Console.WriteLine($"Velocity accelerating: {latest.IsVelocityAccelerating(priorSnapshots)}");
            Console.WriteLine($"Burn rate:             {latest.GetBurnRate():F2} pts/day");
            Console.WriteLine($"Scope creep detected:  {latest.HasScopeCreep()}");
            Console.WriteLine($"Completed pts over time: {string.Join(", ", orderedHistory.GetCompletedStoryPointsOverTime())}");
            Console.WriteLine($"Remaining pts over time: {string.Join(", ", orderedHistory.GetRemainingStoryPointsOverTime())}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Trend command failed");
            throw;
        }
    }, trendSprintIdOpt);

    rootCommand.AddCommand(trendCommand);

        return rootCommand;
    }

    /// <summary>
    /// Resolves the effective output file path given an optional explicit output file and an
    /// optional output directory.  Creates the directory if it does not already exist.
    /// </summary>
    /// <param name="output">File name or path supplied via <c>--output</c> (may be null).</param>
    /// <param name="outputDir">Directory supplied via <c>--output-dir</c> (may be null).</param>
    /// <param name="defaultFileName">Fallback file name when <paramref name="output"/> is null.</param>
    /// <returns>
    /// The resolved file path, or <c>null</c> when neither <paramref name="output"/> nor
    /// <paramref name="outputDir"/> was provided (callers should write to stdout in that case).
    /// </returns>
    private static string? ResolveOutputPath(string? output, string? outputDir, string defaultFileName)
    {
        if (string.IsNullOrEmpty(outputDir) && string.IsNullOrEmpty(output))
        {
            return null;
        }

        var fileName = string.IsNullOrEmpty(output)
            ? defaultFileName
            : Path.GetFileName(output);

        if (string.IsNullOrEmpty(outputDir))
        {
            // No output-dir: use the path as-is, but ensure parent directory exists
            var dir = Path.GetDirectoryName(output!);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return output!;
        }

        Directory.CreateDirectory(outputDir);
        return Path.Combine(outputDir, fileName!);
    }
}
