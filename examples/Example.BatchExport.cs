// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Examples;

/// <summary>
/// Example: Batch Export - Generate reports for multiple projects
/// Demonstrates exporting analytics for multiple projects in different formats
/// and organizing the output for easy distribution.
/// </summary>
public class BatchExportExample
{
    public static async Task Main(string[] args)
    {
        // Example: Export all projects to JSON and CSV
        var projects = new[] { "BACKEND", "FRONTEND", "DEVOPS" };
        var outputDir = "reports";

        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<BatchExportExample>>();
        var exportService = serviceProvider.GetRequiredService<IExportService>();

        // Create output directory
        Directory.CreateDirectory(outputDir);

        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          BATCH EXPORT - MULTI-PROJECT ANALYTICS          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var results = new List<ExportResult>();

        foreach (var project in projects)
        {
            Console.WriteLine($"📊 Processing {project}...");

            try
            {
                // Export as JSON
                var jsonFile = Path.Combine(outputDir, $"{project}-metrics.json");
                await exportService.ExportAnalytics(project, "json", jsonFile);
                results.Add(new ExportResult { Project = project, Format = "JSON", Status = "✅ Success", FilePath = jsonFile });
                Console.WriteLine($"   ✅ JSON exported");

                // Export as CSV
                var csvFile = Path.Combine(outputDir, $"{project}-metrics.csv");
                await exportService.ExportAnalytics(project, "csv", csvFile);
                results.Add(new ExportResult { Project = project, Format = "CSV", Status = "✅ Success", FilePath = csvFile });
                Console.WriteLine($"   ✅ CSV exported");

                // Try to export as PNG (may fail if no data)
                try
                {
                    var pngFile = Path.Combine(outputDir, $"{project}-chart.png");
                    await exportService.ExportAnalytics(project, "png", pngFile);
                    results.Add(new ExportResult { Project = project, Format = "PNG", Status = "✅ Success", FilePath = pngFile });
                    Console.WriteLine($"   ✅ PNG exported");
                }
                catch (Exception ex)
                {
                    results.Add(new ExportResult { Project = project, Format = "PNG", Status = "⚠️  Skipped", FilePath = null });
                    logger.LogWarning("PNG export skipped for {Project}: {Message}", project, ex.Message);
                }
            }
            catch (Exception ex)
            {
                results.Add(new ExportResult { Project = project, Format = "All", Status = "❌ Failed", FilePath = null });
                logger.LogError(ex, "Export failed for project {Project}", project);
            }

            Console.WriteLine();
        }

        // Summary report
        DisplaySummary(results, outputDir);
    }

    private static void DisplaySummary(List<ExportResult> results, string outputDir)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    EXPORT SUMMARY                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var successCount = results.Count(r => r.Status.Contains("✅"));
        var totalCount = results.Count;

        Console.WriteLine($"Total Exports: {successCount}/{totalCount} successful");
        Console.WriteLine();

        // Group by project
        var byProject = results.GroupBy(r => r.Project);
        foreach (var group in byProject)
        {
            Console.WriteLine($"Project: {group.Key}");
            foreach (var result in group)
            {
                Console.WriteLine($"  {result.Format,-6} → {result.Status}");
                if (result.FilePath != null)
                {
                    var size = new FileInfo(result.FilePath).Length;
                    Console.WriteLine($"         {result.FilePath} ({FormatBytes(size)})");
                }
            }
            Console.WriteLine();
        }

        // Output directory info
        var dirInfo = new DirectoryInfo(outputDir);
        var totalSize = dirInfo.GetFiles().Sum(f => f.Length);
        Console.WriteLine($"📁 Output Directory: {Path.GetFullPath(outputDir)}");
        Console.WriteLine($"   Files Created: {dirInfo.GetFiles().Length}");
        Console.WriteLine($"   Total Size: {FormatBytes(totalSize)}");
        Console.WriteLine();

        // Next steps
        Console.WriteLine("💡 Next Steps:");
        Console.WriteLine($"   1. Review files in: {Path.GetFullPath(outputDir)}");
        Console.WriteLine($"   2. Upload to data warehouse or analysis tool");
        Console.WriteLine($"   3. Share CSV files via email or wiki");
        Console.WriteLine($"   4. Include PNG charts in presentations");
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:F1} {sizes[order]}";
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddHttpClient("jira");
        services.AddSingleton<IExportService>(sp => new MockExportService());
        // In real usage, this would be properly configured
    }

    private class ExportResult
    {
        public string Project { get; set; } = "";
        public string Format { get; set; } = "";
        public string Status { get; set; } = "";
        public string? FilePath { get; set; }
    }

    private class MockExportService : IExportService
    {
        public async Task ExportAnalytics(string projectKey, string format, string outputPath)
        {
            // Simulate export by creating file
            await File.WriteAllTextAsync(outputPath, $"Mock export for {projectKey} in {format} format");
            await Task.Delay(100); // Simulate work
        }
    }
}
