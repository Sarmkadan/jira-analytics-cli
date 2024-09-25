// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Generates self-contained, responsive HTML reports with embedded CSS from sprint and team data.
/// The output file requires no external resources and can be opened directly in any browser.
/// </summary>
public class HtmlReportService : IHtmlReportService
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<HtmlReportService> _logger;

    /// <summary>Initialises a new instance of <see cref="HtmlReportService"/>.</summary>
    public HtmlReportService(IAnalyticsService analyticsService, ILogger<HtmlReportService> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task GenerateReportAsync(string projectKey, int sprintCount, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectKey, nameof(projectKey));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath, nameof(outputPath));

        if (sprintCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(sprintCount), "Sprint count must be positive.");

        _logger.LogInformation(
            "Generating HTML report for project {ProjectKey} ({SprintCount} sprints) → {OutputPath}",
            projectKey, sprintCount, outputPath);

        try
        {
            var sprintTask = _analyticsService.AnalyzeSprints(projectKey, sprintCount);
            var teamTask   = _analyticsService.AnalyzeTeam(projectKey);

            await Task.WhenAll(sprintTask, teamTask);

            var html = BuildHtml(projectKey, sprintTask.Result, teamTask.Result);

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(outputPath, html, Encoding.UTF8);

            _logger.LogInformation("HTML report written to {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HTML report for project {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public string BuildHtml(string projectKey, SprintAnalysisResult sprintAnalysis, TeamAnalysisResult teamAnalysis)
    {
        ArgumentNullException.ThrowIfNull(sprintAnalysis);
        ArgumentNullException.ThrowIfNull(teamAnalysis);

        var sb = new StringBuilder();
        var generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var trendSymbol = sprintAnalysis.TrendPercentage >= 0 ? "▲" : "▼";
        var trendClass  = sprintAnalysis.TrendPercentage >= 0 ? "positive" : "negative";
        var healthClass = sprintAnalysis.OverallHealth.ToLower().Replace(" ", "-");

        sb.Append($$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <title>Jira Analytics Report – {{H(projectKey)}}</title>
              <style>
                *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                body  { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
                        background: #f0f2f5; color: #2c3e50; line-height: 1.6; }
                .container { max-width: 1100px; margin: 0 auto; padding: 24px; }
                header { background: linear-gradient(135deg, #1a252f, #2c3e50);
                         color: #fff; padding: 32px 24px; border-radius: 8px; margin-bottom: 28px; }
                header h1 { font-size: 1.8rem; font-weight: 700; }
                header .meta { font-size: .85rem; color: #bdc3c7; margin-top: 6px; }
                .kpi-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
                            gap: 16px; margin-bottom: 28px; }
                .kpi-card { background: #fff; border-radius: 8px; padding: 20px;
                            box-shadow: 0 1px 4px rgba(0,0,0,.08); }
                .kpi-card .value { font-size: 2rem; font-weight: 700; color: #2980b9; }
                .kpi-card .label { font-size: .78rem; color: #7f8c8d; text-transform: uppercase;
                                   letter-spacing: .05em; margin-top: 4px; }
                .kpi-card .value.positive { color: #27ae60; }
                .kpi-card .value.negative { color: #e74c3c; }
                .kpi-card .value.excellent { color: #27ae60; }
                .kpi-card .value.healthy   { color: #f39c12; }
                .kpi-card .value.at-risk   { color: #e67e22; }
                .kpi-card .value.critical  { color: #e74c3c; }
                section { background: #fff; border-radius: 8px; padding: 24px;
                          box-shadow: 0 1px 4px rgba(0,0,0,.08); margin-bottom: 24px; }
                section h2 { font-size: 1.1rem; font-weight: 600; margin-bottom: 16px;
                             padding-bottom: 8px; border-bottom: 2px solid #ecf0f1; }
                table  { width: 100%; border-collapse: collapse; font-size: .9rem; }
                thead tr { background: #34495e; color: #fff; }
                th, td { padding: 10px 14px; text-align: left; }
                tbody tr:nth-child(even) { background: #f8f9fa; }
                tbody tr:hover { background: #eaf4fb; }
                .badge { display: inline-block; padding: 2px 10px; border-radius: 12px;
                         font-size: .78rem; font-weight: 600; }
                .badge-excellent { background: #d5f5e3; color: #1e8449; }
                .badge-healthy   { background: #fef9e7; color: #b7950b; }
                .badge-at-risk   { background: #fdebd0; color: #935116; }
                .badge-critical  { background: #fadbd8; color: #922b21; }
                .progress-bar { height: 8px; background: #ecf0f1; border-radius: 4px; overflow: hidden; }
                .progress-fill { height: 100%; border-radius: 4px; background: #3498db; }
                footer { text-align: center; color: #95a5a6; font-size: .8rem; padding: 16px 0; }
              </style>
            </head>
            <body>
            <div class="container">
              <header>
                <h1>Jira Analytics Report — {{H(projectKey)}}</h1>
                <div class="meta">Generated {{generatedAt}} UTC &nbsp;|&nbsp; {{sprintAnalysis.Metrics.Count}} sprint(s) analysed</div>
              </header>

              <div class="kpi-grid">
                <div class="kpi-card">
                  <div class="value">{{sprintAnalysis.AverageVelocity:F1}}</div>
                  <div class="label">Avg velocity (pts/sprint)</div>
                </div>
                <div class="kpi-card">
                  <div class="value {{trendClass}}">{{trendSymbol}} {{Math.Abs(sprintAnalysis.TrendPercentage):F1}}%</div>
                  <div class="label">Velocity trend</div>
                </div>
                <div class="kpi-card">
                  <div class="value {{healthClass}}">{{H(sprintAnalysis.OverallHealth)}}</div>
                  <div class="label">Overall health</div>
                </div>
                <div class="kpi-card">
                  <div class="value">{{sprintAnalysis.Metrics.Sum(m => m.CompletedIssueCount)}}</div>
                  <div class="label">Issues delivered</div>
                </div>
                <div class="kpi-card">
                  <div class="value">{{sprintAnalysis.Metrics.Sum(m => m.DefectsCount)}}</div>
                  <div class="label">Bugs found</div>
                </div>
                <div class="kpi-card">
                  <div class="value">{{sprintAnalysis.Metrics.Sum(m => m.OverdueIssueCount)}}</div>
                  <div class="label">Overdue issues</div>
                </div>
              </div>
            """);

        // Sprint metrics table
        sb.Append("""
              <section>
                <h2>Sprint Breakdown</h2>
                <table>
                  <thead>
                    <tr>
                      <th>Sprint</th>
                      <th>Period</th>
                      <th>Planned pts</th>
                      <th>Completed pts</th>
                      <th>Completion</th>
                      <th>Cycle time</th>
                      <th>Health</th>
                    </tr>
                  </thead>
                  <tbody>
            """);

        foreach (var m in sprintAnalysis.Metrics.OrderBy(x => x.EndDate))
        {
            var rate       = m.GetCompletionRate();
            var badgeClass = HealthBadgeClass(m.GetHealthStatus());
            sb.Append($$"""
                      <tr>
                        <td><strong>{{H(m.SprintName)}}</strong></td>
                        <td>{{m.StartDate:yyyy-MM-dd}} – {{m.EndDate:yyyy-MM-dd}}</td>
                        <td>{{m.PlannedStoryPoints}}</td>
                        <td>{{m.CompletedStoryPoints}}</td>
                        <td>
                          <div style="display:flex;align-items:center;gap:8px">
                            <div class="progress-bar" style="width:120px">
                              <div class="progress-fill" style="width:{{Math.Min(rate, 100):F0}}%"></div>
                            </div>
                            {{rate:F1}}%
                          </div>
                        </td>
                        <td>{{m.AverageCycleTime:F1}} d</td>
                        <td><span class="badge {{badgeClass}}">{{H(m.GetHealthStatus())}}</span></td>
                      </tr>
                """);
        }

        sb.Append("    </tbody>\n  </table>\n</section>\n");

        // Team section
        if (teamAnalysis.WorkloadDistribution.Any())
        {
            sb.Append("""
                  <section>
                    <h2>Team Workload Distribution</h2>
                    <table>
                      <thead>
                        <tr><th>Developer</th><th>Assigned issues</th><th>Load</th></tr>
                      </thead>
                      <tbody>
                """);

            var maxLoad = teamAnalysis.WorkloadDistribution.Values.DefaultIfEmpty(1).Max();
            if (maxLoad == 0) maxLoad = 1;

            foreach (var (dev, count) in teamAnalysis.WorkloadDistribution.OrderByDescending(kv => kv.Value))
            {
                var pct = (count / (double)maxLoad) * 100;
                sb.Append($$"""
                          <tr>
                            <td>{{H(dev)}}</td>
                            <td>{{count}}</td>
                            <td>
                              <div style="display:flex;align-items:center;gap:8px">
                                <div class="progress-bar" style="width:160px">
                                  <div class="progress-fill" style="width:{{pct:F0}}%;background:#9b59b6"></div>
                                </div>
                                {{pct:F0}}%
                              </div>
                            </td>
                          </tr>
                    """);
            }

            sb.Append("      </tbody>\n    </table>\n  </section>\n");
        }

        if (teamAnalysis.TopPerformers.Any())
        {
            sb.Append("""
                  <section>
                    <h2>Top Performers</h2>
                    <table>
                      <thead>
                        <tr><th>Developer</th><th>Completed issues</th><th>Story points</th><th>Productivity score</th></tr>
                      </thead>
                      <tbody>
                """);

            foreach (var dev in teamAnalysis.TopPerformers)
            {
                sb.Append($$"""
                          <tr>
                            <td>{{H(dev.DisplayName ?? dev.Name)}}</td>
                            <td>{{dev.GetCompletedIssues()}}</td>
                            <td>{{dev.GetCompletedStoryPoints()}}</td>
                            <td>{{dev.GetProductivity():F2}}</td>
                          </tr>
                    """);
            }

            sb.Append("      </tbody>\n    </table>\n  </section>\n");
        }

        sb.Append($$"""
              <footer>Jira Analytics CLI &nbsp;|&nbsp; Report generated {{generatedAt}} UTC</footer>
            </div>
            </body>
            </html>
            """);

        return sb.ToString();
    }

    // HTML-encodes a string to prevent XSS in generated reports.
    private static string H(string? value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);

    private static string HealthBadgeClass(string health) => health.ToLower() switch
    {
        "excellent" => "badge-excellent",
        "healthy"   => "badge-healthy",
        "at risk"   => "badge-at-risk",
        _           => "badge-critical"
    };
}
