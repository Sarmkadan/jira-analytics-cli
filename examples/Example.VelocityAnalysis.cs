// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraAnalyticsCli.Services;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Examples;

/// <summary>
/// Example: Velocity Analysis - Calculates trends and forecasts
/// This program demonstrates how to programmatically use the analytics
/// service to analyze velocity trends across multiple sprints.
/// </summary>
public class VelocityAnalysisExample
{
    // Entry point for running this example
    public static async Task Main(string[] args)
    {
        // Example usage:
        // dotnet run Example.VelocityAnalysis MYPROJECT 8

        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Example.VelocityAnalysis <PROJECT_KEY> [SPRINT_COUNT]");
            return;
        }

        string projectKey = args[0];
        int sprintCount = args.Length > 1 ? int.Parse(args[1]) : 5;

        // Setup DI container (same as Program.cs)
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<VelocityAnalysisExample>>();
        var analyticsService = serviceProvider.GetRequiredService<IAnalyticsService>();

        try
        {
            logger.LogInformation("Starting velocity analysis for {Project}", projectKey);

            // Analyze sprints
            var analysis = await analyticsService.AnalyzeSprints(projectKey, sprintCount).ConfigureAwait(false);

            // Display velocity trend
            DisplayVelocityTrend(analysis);

            // Calculate forecast
            CalculateVelocityForecast(analysis);

            // Performance insights
            ProvideInsights(analysis);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during analysis");
        }
    }

    /// <summary>
    /// Display velocity trend across sprints with visual representation
    /// </summary>
    private static void DisplayVelocityTrend(ProjectAnalysis analysis)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              VELOCITY TREND ANALYSIS                     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        if (analysis.SprintMetrics.Count == 0)
        {
            Console.WriteLine("No sprint data available");
            return;
        }

        // Find min and max for scaling the chart
        int minVelocity = analysis.SprintMetrics.Min(m => m.Velocity);
        int maxVelocity = analysis.SprintMetrics.Max(m => m.Velocity);
        int range = maxVelocity - minVelocity + 1;

        // Display each sprint as a bar
        foreach (var metric in analysis.SprintMetrics)
        {
            // Calculate bar length (max 40 characters)
            int barLength = (int)((metric.Velocity - minVelocity) * 40 / range);
            string bar = new string('█', barLength);

            Console.WriteLine($"{metric.SprintName,-15} │{bar,-40} {metric.Velocity} pts");
        }

        Console.WriteLine();
        Console.WriteLine($"Range: {minVelocity} - {maxVelocity} story points");
        Console.WriteLine($"Average: {analysis.SprintMetrics.Average(m => m.Velocity):F1} story points");
    }

    /// <summary>
    /// Calculate velocity forecast for next sprint using trend analysis
    /// </summary>
    private static void CalculateVelocityForecast(ProjectAnalysis analysis)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              VELOCITY FORECAST                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var metrics = analysis.SprintMetrics;

        if (metrics.Count < 2)
        {
            Console.WriteLine("Need at least 2 sprints for trend analysis");
            return;
        }

        // Calculate trend using linear regression
        double avgVelocity = metrics.Average(m => m.Velocity);

        // Simple trend: compare recent vs older sprints
        var recent = metrics.TakeLast(3).Average(m => m.Velocity);
        var older = metrics.Take(Math.Max(1, metrics.Count - 3)).Average(m => m.Velocity);
        double trendChange = ((recent - older) / older) * 100;

        Console.WriteLine($"Current Average Velocity: {avgVelocity:F1} story points");
        Console.WriteLine($"Recent Average (last 3):  {recent:F1} story points");
        Console.WriteLine($"Trend Change:             {trendChange:+0.0;-0.0;0.0}%");
        Console.WriteLine();

        // Forecast next sprint
        double forecastedVelocity = recent * (1 + (trendChange / 100));
        Console.WriteLine($"📊 Forecasted Next Sprint Velocity: {forecastedVelocity:F0} story points");
        Console.WriteLine();

        // Confidence assessment
        if (Math.Abs(trendChange) < 10)
        {
            Console.WriteLine("📈 High Confidence: Velocity is stable");
        }
        else if (trendChange > 0)
        {
            Console.WriteLine("📈 Moderate Confidence: Velocity is improving");
        }
        else
        {
            Console.WriteLine("📉 Warning: Velocity is declining");
        }
    }

    /// <summary>
    /// Provide actionable insights based on the analysis
    /// </summary>
    private static void ProvideInsights(ProjectAnalysis analysis)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              INSIGHTS & RECOMMENDATIONS                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var metrics = analysis.SprintMetrics;

        // Check completion rate
        var avgCompletion = metrics.Average(m => m.CompletionRate);
        if (avgCompletion < 0.8)
        {
            Console.WriteLine("⚠️  Low Completion Rate ({avgCompletion:P0})");
            Console.WriteLine("   Recommendation: Improve estimation or reduce sprint scope");
        }
        else if (avgCompletion > 1.0)
        {
            Console.WriteLine("✅ Strong Completion Rate ({avgCompletion:P0})");
            Console.WriteLine("   Team is consistently exceeding planned capacity");
        }

        // Check quality
        var avgQuality = metrics.Average(m => m.QualityScore);
        if (avgQuality < 0.70)
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  Quality Score is Low ({avgQuality:F1}/100)");
            Console.WriteLine("   Recommendation: Increase code review, add test automation");
        }

        // Check for declining velocity
        var recent = metrics.TakeLast(2).Average(m => m.Velocity);
        var previous = metrics.Take(metrics.Count - 2).Average(m => m.Velocity);
        if (recent < previous * 0.9)
        {
            Console.WriteLine();
            Console.WriteLine("📉 Velocity Declining");
            Console.WriteLine("   Investigate: Team blockers, increased complexity, or burnout");
        }

        // Team composition
        var topDeveloper = analysis.Developers.OrderByDescending(d => d.CompletionRate).First();
        Console.WriteLine();
        Console.WriteLine($"🏆 Top Performer: {topDeveloper.Name} ({topDeveloper.CompletionRate:P0} completion)");

        // Overdue items
        var overdueCount = analysis.OverdueIssues.Count;
        if (overdueCount > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"⚠️  {overdueCount} Overdue Issues");
            Console.WriteLine("   Action: Schedule review meeting to resolve blockers");
        }

        Console.WriteLine();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Same as Program.cs - setup DI container
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddSingleton<ICliConfig>(new CliConfig
        {
            JiraBaseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL") ?? "",
            JiraApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN") ?? ""
        });
        // ... other services would be configured here
    }
}

// Placeholder models (would be real models in actual code)
public class ProjectAnalysis
{
    public List<SprintMetric> SprintMetrics { get; set; } = new();
    public List<Developer> Developers { get; set; } = new();
    public List<JiraIssue> OverdueIssues { get; set; } = new();
}
