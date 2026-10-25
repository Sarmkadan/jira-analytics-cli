// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using System.Text.Json;
using JiraAnalyticsCli.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Exports analytics data to multiple formats: PNG, PDF, JSON, CSV using SkiaSharp for graphics
/// </summary>
public class ExportService : IExportService
{
    private const string PngFormat = "png";
    private const string JpgFormat = "jpg";
    private const string SvgFormat = "svg";
    private const string JsonFormat = "json";
    private const string CsvFormat = "csv";
    private const string MarkdownFormat = "md";
    private const string DefaultTeamName = "unknown";

    private readonly IJiraApiService _jiraService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<ExportService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ExportService(IJiraApiService jiraService, IAnalyticsService analyticsService, ILogger<ExportService> logger, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(jiraService);
        ArgumentNullException.ThrowIfNull(analyticsService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._jiraService = jiraService;
        this._analyticsService = analyticsService;
        this._logger = logger;
        this._serviceProvider = serviceProvider;
    }

    public async Task ExportAnalytics(string projectKey, string format, string outputPath)
    {
        this._logger.LogInformation("Exporting analytics for project {ProjectKey} as {Format}", projectKey, format);

        try
        {
            var analysis = await this._analyticsService.AnalyzeSprints(projectKey, 5);

            await (format.ToLower() switch
            {
                PngFormat or JpgFormat => this.ExportAsImage(analysis, format, outputPath),
                SvgFormat => this.ExportAsImageSvg(analysis, outputPath),
                JsonFormat => this.ExportAsJson(analysis, outputPath),
                CsvFormat => this.ExportAnalyticsAsCsv(analysis, outputPath),
                MarkdownFormat => this.ExportAsMarkdown(analysis, outputPath),
                _ => throw new NotSupportedException($"Format {format} is not supported")
            });

            this._logger.LogInformation("Analytics exported successfully to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error exporting analytics");
            throw;
        }
    }

    public async Task ExportAsMarkdown(SprintAnalysisResult analysis, string outputPath)
    {
        this._logger.LogInformation("Exporting analytics as Markdown to {OutputPath}", outputPath);

        try
        {
            var markdownService = this._serviceProvider.GetRequiredService<IMarkdownReportService>();
            var teamAnalysis = await this._analyticsService.AnalyzeTeam(DefaultTeamName);

            var markdown = markdownService.BuildMarkdown(DefaultTeamName, analysis, teamAnalysis);
            
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(outputPath, markdown, Encoding.UTF8);

            this._logger.LogInformation("Markdown analytics exported successfully to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error exporting analytics as Markdown");
            throw;
        }
    }


    public async Task ExportBurndownChart(int sprintId, string format, string outputPath)
    {
        this._logger.LogInformation("Exporting burndown chart for sprint {SprintId} as {Format}", sprintId, format);

        try
        {
            var sprint = await this._jiraService.GetSprintAsync(sprintId);
            if (sprint == null)
            {
                throw new InvalidOperationException($"Sprint {sprintId} not found");
            }

            var issues = await this._jiraService.GetSprintIssuesAsync(sprintId);
            sprint.Issues.AddRange(issues);

            var burndownData = await this._jiraService.GetBurndownDataAsync(sprintId);

            var fmt = format.ToLower();
            if (fmt is PngFormat or JpgFormat)
            {
                await this.DrawBurndownChart(sprint, burndownData, format, outputPath);
            }
            else if (fmt == SvgFormat)
            {
                await this.DrawBurndownChartSvg(sprint, burndownData, outputPath);
            }
            else if (fmt == JsonFormat)
            {
                await this.ExportAsJson(burndownData, outputPath);
            }

            this._logger.LogInformation("Burndown chart exported to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error exporting burndown chart");
            throw;
        }
    }

    public async Task ExportTeamMetrics(string projectKey, string format, string outputPath)
    {
        this._logger.LogInformation("Exporting team metrics for project {ProjectKey}", projectKey);

        try
        {
            var teamAnalysis = await this._analyticsService.AnalyzeTeam(projectKey);

            await (format.ToLower() switch
            {
                JsonFormat => this.ExportAsJson(teamAnalysis, outputPath),
                CsvFormat => this.ExportTeamAsCsv(teamAnalysis, outputPath),
                _ => throw new NotSupportedException($"Format {format} is not supported")
            });

            this._logger.LogInformation("Team metrics exported to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error exporting team metrics");
            throw;
        }
    }

    public async Task ExportAsJson(object data, string outputPath)
    {
        this._logger.LogInformation("Exporting data as JSON to {OutputPath}", outputPath);

        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error exporting JSON");
            throw;
        }

        await Task.CompletedTask;
    }

    public async Task ExportAsCsv(List<Dictionary<string, object>> data, string outputPath)
    {
        this._logger.LogInformation("Exporting data as CSV to {OutputPath}", outputPath);

        try
        {
            if (!data.Any())
            {
                await File.WriteAllTextAsync(outputPath, string.Empty);
                return;
            }

            var sb = new StringBuilder();
            var headers = data[0].Keys;

            // Write headers
            sb.AppendLine(string.Join(",", headers));

            // Write data rows
            foreach (var row in data)
            {
                var values = headers.Select(h => row.ContainsKey(h) ? this.EscapeCsvValue(FormatCsvCell(row[h])) : string.Empty);
                sb.AppendLine(string.Join(",", values));
            }

            await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error exporting CSV");
            throw;
        }

        await Task.CompletedTask;
    }

    private async Task ExportAsImage(SprintAnalysisResult analysis, string format, string outputPath)
    {
        this._logger.LogInformation("Generating image chart for analytics");

        try
        {
            const int width = 1200;
            const int height = 800;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // White background
            canvas.DrawColor(SKColors.White);

            // Title
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 32,
                IsAntialias = true
            };
            canvas.DrawText("Sprint Velocity Chart", 50, 50, paint);

            // Draw velocity bars
            paint.TextSize = 14;
            paint.Color = new SKColor(52, 152, 219); // Blue

            var barWidth = (width - 100) / (analysis.Metrics.Count + 1);
            var maxVelocity = analysis.Metrics.Any() ? analysis.Metrics.Max(m => m.GetVelocity()) : 1;

            for (int i = 0; i < analysis.Metrics.Count; i++)
            {
                var metric = analysis.Metrics[i];
                var velocity = metric.GetVelocity();
                var barHeight = (velocity / maxVelocity) * (height - 200);

                var x = 50 + (i + 1) * barWidth;
                var y = height - 100 - barHeight;

                paint.Color = new SKColor(52, 152, 219);
                canvas.DrawRect((float)x, (float)y, (float)(barWidth - 10), (float)barHeight, paint);

                paint.Color = SKColors.Black;
                paint.TextSize = 12;
                canvas.DrawText(metric.SprintName, (float)(x + 5), (float)(height - 50), paint);
                canvas.DrawText(velocity.ToString("F1", CultureInfo.InvariantCulture), (float)(x + 5), (float)(y - 10), paint);
            }

            // Save image
            var format_enum = format.ToLower() == PngFormat ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
            using (var image = surface.Snapshot())
            using (var data = image.Encode(format_enum, 80))
            {
                using (var stream = File.OpenWrite(outputPath))
                {
                    data.SaveTo(stream);
                }
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error generating image");
            throw;
        }

        await Task.CompletedTask;
    }

    private async Task DrawBurndownChart(Sprint sprint, List<BurndownSnapshot> burndowns, string format, string outputPath)
    {
        this._logger.LogInformation("Drawing burndown chart for sprint {SprintId}", sprint.Id);

        try
        {
            const int width = 1200;
            const int height = 800;
            const int padding = 80;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Background
            canvas.DrawColor(SKColors.White);

            // Title
            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 28,
                IsAntialias = true
            };
            canvas.DrawText($"Burndown Chart - {sprint.Name}", padding, 40, titlePaint);

            // Draw grid and axes
            this.DrawChartAxes(canvas, padding, width, height);

            // Draw ideal line
            this.DrawIdealBurndownLine(canvas, sprint, padding, width, height);

            // Draw actual burndown
            this.DrawActualBurndown(canvas, burndowns, padding, width, height);

            // Save
            var format_enum = format.ToLower() == PngFormat ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
            using (var image = surface.Snapshot())
            using (var data = image.Encode(format_enum, 80))
            {
                using (var stream = File.OpenWrite(outputPath))
                {
                    data.SaveTo(stream);
                }
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error drawing burndown");
            throw;
        }

        await Task.CompletedTask;
    }

    private void DrawChartAxes(SKCanvas canvas, int padding, int width, int height)
    {
        using var axisPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 2
        };

        // X and Y axes
        canvas.DrawLine(padding, height - padding, width - padding, height - padding, axisPaint);
        canvas.DrawLine(padding, padding, padding, height - padding, axisPaint);
    }

    private void DrawIdealBurndownLine(SKCanvas canvas, Sprint sprint, int padding, int width, int height)
    {
        using var linePaint = new SKPaint
        {
            Color = new SKColor(149, 165, 166), // Gray
            IsAntialias = true,
            StrokeWidth = 2,
            PathEffect = SKPathEffect.CreateDash(new[] { 5f, 5f }, 0)
        };

        var startX = padding;
        var startY = height - padding;
        var endX = width - padding;
        var endY = padding + 100;

        canvas.DrawLine(startX, startY, endX, endY, linePaint);
    }

    private void DrawActualBurndown(SKCanvas canvas, List<BurndownSnapshot> burndowns, int padding, int width, int height)
    {
        if (!burndowns.Any())
        {
            return;
        }

        using var burndownPaint = new SKPaint
        {
            Color = new SKColor(231, 76, 60), // Red
            IsAntialias = true,
            StrokeWidth = 3
        };

        var xScale = (width - 2 * padding) / (double)burndowns.Count;
        var maxPoints = burndowns.Max(b => b.TotalStoryPoints);

        // When no work has been estimated, render a flat line at the baseline
        if (maxPoints <= 0)
        {
            var flatY = (float)(height - padding);
            canvas.DrawLine(padding, flatY, width - padding, flatY, burndownPaint);
            return;
        }

        var yScale = (height - 2 * padding) / (double)maxPoints;

        for (int i = 0; i < burndowns.Count - 1; i++)
        {
            var current = burndowns[i];
            var next = burndowns[i + 1];

            var x1 = padding + (i * xScale);
            var y1 = height - padding - (current.RemainingStoryPoints * yScale);

            var x2 = padding + ((i + 1) * xScale);
            var y2 = height - padding - (next.RemainingStoryPoints * yScale);

            canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, burndownPaint);
        }
    }

    private async Task ExportAsImageSvg(SprintAnalysisResult analysis, string outputPath)
    {
        this._logger.LogInformation("Generating SVG velocity chart for analytics");

        try
        {
            const int width = 1200;
            const int height = 800;

            using var stream = File.OpenWrite(outputPath);
            var bounds = new SKRect(0, 0, width, height);
            using var canvas = SKSvgCanvas.Create(bounds, stream);

            canvas.DrawColor(SKColors.White);

            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 32,
                IsAntialias = true
            };
            canvas.DrawText("Sprint Velocity Chart", 50, 50, paint);

            paint.TextSize = 14;
            paint.Color = new SKColor(52, 152, 219);

            var barWidth = (width - 100) / (analysis.Metrics.Count + 1);
            var maxVelocity = analysis.Metrics.Any() ? analysis.Metrics.Max(m => m.GetVelocity()) : 1;
            if (maxVelocity <= 0)
            {
                maxVelocity = 1;
            }

            for (int i = 0; i < analysis.Metrics.Count; i++)
            {
                var metric = analysis.Metrics[i];
                var velocity = metric.GetVelocity();
                var barHeight = (velocity / maxVelocity) * (height - 200);

                var x = 50 + (i + 1) * barWidth;
                var y = height - 100 - barHeight;

                paint.Color = new SKColor(52, 152, 219);
                canvas.DrawRect((float)x, (float)y, (float)(barWidth - 10), (float)barHeight, paint);

                paint.Color = SKColors.Black;
                paint.TextSize = 12;
                canvas.DrawText(metric.SprintName, (float)(x + 5), (float)(height - 50), paint);
                canvas.DrawText(velocity.ToString("F1", CultureInfo.InvariantCulture), (float)(x + 5), (float)(y - 10), paint);
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error generating SVG image");
            throw;
        }

        await Task.CompletedTask;
    }

    private async Task DrawBurndownChartSvg(Sprint sprint, List<BurndownSnapshot> burndowns, string outputPath)
    {
        this._logger.LogInformation("Drawing SVG burndown chart for sprint {SprintId}", sprint.Id);

        try
        {
            const int width = 1200;
            const int height = 800;
            const int padding = 80;

            using var stream = File.OpenWrite(outputPath);
            var bounds = new SKRect(0, 0, width, height);
            using var canvas = SKSvgCanvas.Create(bounds, stream);

            canvas.DrawColor(SKColors.White);

            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 28,
                IsAntialias = true
            };
            canvas.DrawText($"Burndown Chart - {sprint.Name}", padding, 40, titlePaint);

            this.DrawChartAxes(canvas, padding, width, height);
            this.DrawIdealBurndownLine(canvas, sprint, padding, width, height);
            this.DrawActualBurndown(canvas, burndowns, padding, width, height);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error drawing SVG burndown");
            throw;
        }

        await Task.CompletedTask;
    }


    private async Task ExportAnalyticsAsCsv(SprintAnalysisResult analysis, string outputPath)
    {
        var data = analysis.Metrics.Select(m => new Dictionary<string, object>
        {
            { "Sprint Name", m.SprintName },
            { "Planned Points", m.PlannedStoryPoints },
            { "Completed Points", m.CompletedStoryPoints },
            { "Completion Rate %", m.GetCompletionRate() },
            { "Quality Score", m.GetQualityScore() },
            { "Velocity", m.GetVelocity() },
            { "Health Status", m.GetHealthStatus() }
        }).ToList();

        await this.ExportAsCsv(data, outputPath);
    }

    private async Task ExportTeamAsCsv(TeamAnalysisResult teamAnalysis, string outputPath)
    {
        var data = teamAnalysis.WorkloadDistribution.Select(kvp => new Dictionary<string, object>
        {
            { "Developer", kvp.Key },
            { "Assigned Issues", kvp.Value }
        }).ToList();

        await this.ExportAsCsv(data, outputPath);
    }

    // Formats a CSV cell value using invariant culture so numbers/dates round-trip
    // correctly regardless of the running machine's locale settings.
    private static string FormatCsvCell(object? value) => value switch
    {
        null => string.Empty,
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    public async Task ExportSprintMetricsCsv(IEnumerable<SprintMetric> metrics, string path)
    {
        var csvService = this._serviceProvider.GetRequiredService<ICsvExportService>();
        await csvService.ExportSprintMetrics(metrics, path);
    }

    public async Task ExportTeamMetricsCsv(IEnumerable<KeyValuePair<string, int>> metrics, string path)
    {
        var csvService = this._serviceProvider.GetRequiredService<ICsvExportService>();
        await csvService.ExportTeamMetrics(metrics, path);
    }
}
