// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Cli;

/// <summary>
/// Central command parser for CLI argument validation and routing.
/// Handles complex argument parsing with custom validators and transformations.
/// </summary>
public class CommandParser
{
    private readonly ILogger<CommandParser> _logger;
    private readonly CommandDefinitions _definitions;

    public CommandParser(ILogger<CommandParser> logger, CommandDefinitions definitions)
    {
        _logger = logger;
        _definitions = definitions;
    }

    /// <summary>
    /// Validates command arguments against schema rules and returns parsed result.
    /// Performs early validation to catch user errors before service execution.
    /// </summary>
    public ParseResult ParseAndValidate(string[] args)
    {
        _logger.LogDebug("Parsing command line arguments: {ArgCount} total", args.Length);

        var rootCommand = BuildRootCommand();
        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Any())
        {
            _logger.LogWarning("Parse errors detected: {ErrorCount}", parseResult.Errors.Count);
            foreach (var error in parseResult.Errors)
            {
                _logger.LogWarning("Parse error: {Error}", error);
            }
        }

        return parseResult;
    }

    /// <summary>
    /// Builds the root command with all subcommands and option validators.
    /// Centralizes command definition for maintainability and consistency.
    /// </summary>
    private RootCommand BuildRootCommand()
    {
        var root = new RootCommand("Jira Analytics CLI - Advanced metrics and reporting");

        // Global options
        var verbosityOption = new Option<bool>(
            new[] { "-v", "--verbose" },
            "Enable verbose logging output");

        var configOption = new Option<string>(
            new[] { "-c", "--config" },
            "Path to configuration file");

        root.AddGlobalOption(verbosityOption);
        root.AddGlobalOption(configOption);

        // Analytics command
        root.AddCommand(CreateAnalyticsCommand());
        root.AddCommand(CreateExportCommand());
        root.AddCommand(CreateBurndownCommand());
        root.AddCommand(CreateDeveloperCommand());
        root.AddCommand(CreateHealthCheckCommand());

        return root;
    }

    private Command CreateAnalyticsCommand()
    {
        var cmd = new Command("analytics", "Run sprint velocity and metrics analysis");

        var projectOption = new Option<string>(
            new[] { "-p", "--project" },
            "Jira project key (required)") { IsRequired = true };

        var sprintsOption = new Option<int>(
            new[] { "-s", "--sprints" },
            () => 5,
            "Number of sprints to analyze (1-52)");

        var outputOption = new Option<string>(
            new[] { "-o", "--output" },
            "Output file path (optional)");

        var detailedOption = new Option<bool>(
            new[] { "-d", "--detailed" },
            "Include detailed burndown data");

        // Add validators
        projectOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(projectOption);
            if (string.IsNullOrWhiteSpace(value))
                result.ErrorMessage = "Project key cannot be empty";
            if (value?.Length > 10)
                result.ErrorMessage = "Project key seems invalid (max 10 chars)";
        });

        sprintsOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(sprintsOption);
            if (value < 1 || value > 52)
                result.ErrorMessage = "Sprint count must be between 1 and 52";
        });

        cmd.AddOption(projectOption);
        cmd.AddOption(sprintsOption);
        cmd.AddOption(outputOption);
        cmd.AddOption(detailedOption);

        return cmd;
    }

    private Command CreateExportCommand()
    {
        var cmd = new Command("export", "Export analytics data in multiple formats");

        var projectOption = new Option<string>(
            new[] { "-p", "--project" },
            "Jira project key") { IsRequired = true };

        // Fix: Add project key validation for consistency
        projectOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(projectOption);
            if (string.IsNullOrWhiteSpace(value))
                result.ErrorMessage = "Project key cannot be empty";
            if (value?.Length > 10)
                result.ErrorMessage = "Project key seems invalid (max 10 chars)";
        });

        var formatOption = new Option<string>(
            new[] { "-f", "--format" },
            () => "json",
            "Export format: json, csv, xml, markdown");

        var outputOption = new Option<string>(
            new[] { "-o", "--output" },
            "Output file path") { IsRequired = true };

        formatOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(formatOption);
            var validFormats = new[] { "json", "csv", "xml", "markdown" };
            if (!validFormats.Contains(value?.ToLower()))
                result.ErrorMessage = $"Invalid format. Valid options: {string.Join(", ", validFormats)}";
        });

        cmd.AddOption(projectOption);
        cmd.AddOption(formatOption);
        cmd.AddOption(outputOption);

        return cmd;
    }

    private Command CreateBurndownCommand()
    {
        var cmd = new Command("burndown", "Generate burndown chart for sprint");

        var projectOption = new Option<string>(
            new[] { "-p", "--project" },
            "Jira project key") { IsRequired = true };

        // Fix: Add project key validation for consistency
        projectOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(projectOption);
            if (string.IsNullOrWhiteSpace(value))
                result.ErrorMessage = "Project key cannot be empty";
            if (value?.Length > 10)
                result.ErrorMessage = "Project key seems invalid (max 10 chars)";
        });

        var sprintOption = new Option<int>(
            new[] { "-s", "--sprint-id" },
            "Sprint ID") { IsRequired = true };

        var outputOption = new Option<string>(
            new[] { "-o", "--output" },
            "Output image path") { IsRequired = true };

        sprintOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(sprintOption);
            if (value <= 0)
                result.ErrorMessage = "Sprint ID must be positive";
        });

        cmd.AddOption(projectOption);
        cmd.AddOption(sprintOption);
        cmd.AddOption(outputOption);

        return cmd;
    }

    private Command CreateDeveloperCommand()
    {
        var cmd = new Command("developer", "Analyze individual developer load and contributions");

        var projectOption = new Option<string>(
            new[] { "-p", "--project" },
            "Jira project key") { IsRequired = true };

        // Fix: Add project key validation for consistency
        projectOption.AddValidator(result =>
        {
            var value = result.GetValueForOption(projectOption);
            if (string.IsNullOrWhiteSpace(value))
                result.ErrorMessage = "Project key cannot be empty";
            if (value?.Length > 10)
                result.ErrorMessage = "Project key seems invalid (max 10 chars)";
        });

        var developerOption = new Option<string>(
            new[] { "-d", "--developer" },
            "Developer username or email");

        var periodOption = new Option<int>(
            new[] { "--days" },
            () => 30,
            "Analysis period in days");

        cmd.AddOption(projectOption);
        cmd.AddOption(developerOption);
        cmd.AddOption(periodOption);

        return cmd;
    }

    private Command CreateHealthCheckCommand()
    {
        var cmd = new Command("health", "Check Jira API connectivity and configuration");

        return cmd;
    }
}
