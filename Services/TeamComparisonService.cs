// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Compares sprint and quality metrics across multiple Jira projects by fetching each project's
/// sprint history in parallel and aggregating the results into a ranked side-by-side report.
/// </summary>
public class TeamComparisonService : ITeamComparisonService
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<TeamComparisonService> _logger;

    /// <summary>Initialises a new instance of <see cref="TeamComparisonService"/>.</summary>
    public TeamComparisonService(IAnalyticsService analyticsService, ILogger<TeamComparisonService> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<TeamComparisonReport> CompareTeamsAsync(
        IEnumerable<string> projectKeys,
        int sprintCount = 5,
        CancellationToken cancellationToken = default)
    {
        var keys = (projectKeys ?? throw new ArgumentNullException(nameof(projectKeys)))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (keys.Count == 0)
            throw new ArgumentException("At least one project key must be provided.", nameof(projectKeys));

        if (sprintCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(sprintCount), "Sprint count must be positive.");

        _logger.LogInformation(
            "Comparing {TeamCount} team(s) across {SprintCount} sprint(s): {Keys}",
            keys.Count, sprintCount, string.Join(", ", keys));

        // Fetch analytics for all projects in parallel
        var analysisTasks = keys.Select(k => FetchSnapshotAsync(k, sprintCount)).ToList();
        var snapshots = await Task.WhenAll(analysisTasks);
        cancellationToken.ThrowIfCancellationRequested();

        var teams = snapshots
            .Where(s => s != null)
            .Select(s => s!)
            .ToList();

        var report = new TeamComparisonReport
        {
            Teams              = teams,
            FastestTeam        = teams.MaxBy(t => t.AverageVelocity)?.ProjectKey,
            HighestQualityTeam = teams.MinBy(t => t.DefectRate)?.ProjectKey,
            MostConsistentTeam = teams.MaxBy(t => t.AvgCompletionRate)?.ProjectKey
        };

        _logger.LogInformation(
            "Team comparison complete. Fastest: {Fastest}, Highest quality: {Quality}, Most consistent: {Consistent}",
            report.FastestTeam, report.HighestQualityTeam, report.MostConsistentTeam);

        return report;
    }

    /// <summary>
    /// Formats a <see cref="TeamComparisonReport"/> as a readable console table.
    /// </summary>
    public static string FormatAsText(TeamComparisonReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                  TEAM COMPARISON REPORT                       ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
        sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        if (!report.Teams.Any())
        {
            sb.AppendLine("No team data available.");
            return sb.ToString();
        }

        const int projW   = 12;
        const int velW    = 10;
        const int rateW   = 12;
        const int ptsW    = 10;
        const int defW    = 8;
        const int cycleW  = 10;
        const int healthW = 12;

        sb.AppendLine(
            $"{"Project".PadRight(projW)} {"Avg Vel".PadRight(velW)} {"Completion%".PadRight(rateW)} " +
            $"{"Pts Total".PadRight(ptsW)} {"Defects".PadRight(defW)} {"Cycle(d)".PadRight(cycleW)} Health");
        sb.AppendLine(new string('─', 80));

        foreach (var team in report.Teams.OrderByDescending(t => t.AverageVelocity))
        {
            var awards = BuildAwardIcons(team, report);
            sb.AppendLine(
                $"{team.ProjectKey.PadRight(projW)} " +
                $"{team.AverageVelocity.ToString("F1", CultureInfo.InvariantCulture).PadRight(velW)} " +
                $"{team.AvgCompletionRate.ToString("F1", CultureInfo.InvariantCulture).PadRight(rateW)} " +
                $"{team.TotalPointsDelivered.ToString(CultureInfo.InvariantCulture).PadRight(ptsW)} " +
                $"{team.TotalDefects.ToString(CultureInfo.InvariantCulture).PadRight(defW)} " +
                $"{team.AvgCycleTime.ToString("F1", CultureInfo.InvariantCulture).PadRight(cycleW)} " +
                $"{team.OverallHealth}{awards}");
        }

        sb.AppendLine();
        sb.AppendLine("🏆 Rankings:");
        sb.AppendLine($"  Fastest team         : {report.FastestTeam ?? "N/A"}");
        sb.AppendLine($"  Highest quality      : {report.HighestQualityTeam ?? "N/A"}");
        sb.AppendLine($"  Most consistent      : {report.MostConsistentTeam ?? "N/A"}");

        return sb.ToString();
    }

    /// <summary>
    /// Renders a <see cref="TeamComparisonReport"/> as a GitHub‑flavored markdown table.
    /// Each team is a row and the key metrics are columns.
    /// </summary>
    /// <param name="report">The report to render.</param>
    /// <returns>A markdown formatted string.</returns>
    public static string RenderMarkdownTable(TeamComparisonReport report)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("| Project | Avg Vel | Completion% | Pts Total | Defects | Cycle(d) | Health |");
        sb.AppendLine("|---|---|---|---|---|---|---|");

        if (!report.Teams.Any())
        {
            sb.AppendLine("| *No team data available* | | | | | | |");
            return sb.ToString();
        }

        foreach (var team in report.Teams.OrderByDescending(t => t.AverageVelocity))
        {
            var awards = BuildAwardIcons(team, report);
            sb.AppendLine(
                $"| {team.ProjectKey} " +
                $"| {team.AverageVelocity.ToString("F1", CultureInfo.InvariantCulture)} " +
                $"| {team.AvgCompletionRate.ToString("F1", CultureInfo.InvariantCulture)} " +
                $"| {team.TotalPointsDelivered.ToString(CultureInfo.InvariantCulture)} " +
                $"| {team.TotalDefects.ToString(CultureInfo.InvariantCulture)} " +
                $"| {team.AvgCycleTime.ToString("F1", CultureInfo.InvariantCulture)} " +
                $"| {team.OverallHealth}{awards} |");
        }

        return sb.ToString();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<TeamProjectSnapshot?> FetchSnapshotAsync(string projectKey, int sprintCount)
    {
        try
        {
            _logger.LogDebug("Fetching sprint analytics for project {ProjectKey}", projectKey);

            var analysis = await _analyticsService.AnalyzeSprints(projectKey, sprintCount);

            if (!analysis.Metrics.Any())
            {
                _logger.LogWarning("No sprint data found for project {ProjectKey}", projectKey);
                return new TeamProjectSnapshot
                {
                    ProjectKey    = projectKey,
                    OverallHealth = "Unknown"
                };
            }

            var totalIssues    = analysis.Metrics.Sum(m => m.TotalIssueCount);
            var totalDefects   = analysis.Metrics.Sum(m => m.DefectsCount);
            var defectRate     = totalIssues > 0 ? (totalDefects / (double)totalIssues) * 100 : 0d;
            var avgCompletionRate = analysis.Metrics.Average(m => m.GetCompletionRate());
            var avgCycleTime   = analysis.Metrics.Any(m => m.AverageCycleTime > 0)
                ? analysis.Metrics.Where(m => m.AverageCycleTime > 0).Average(m => m.AverageCycleTime)
                : 0d;

            return new TeamProjectSnapshot
            {
                ProjectKey           = projectKey,
                AverageVelocity      = Math.Round(analysis.AverageVelocity, 2),
                AvgCompletionRate    = Math.Round(avgCompletionRate, 2),
                TotalPointsDelivered = analysis.Metrics.Sum(m => m.CompletedStoryPoints),
                TotalIssuesCompleted = analysis.Metrics.Sum(m => m.CompletedIssueCount),
                TotalDefects         = totalDefects,
                DefectRate           = Math.Round(defectRate, 2),
                AvgCycleTime         = Math.Round(avgCycleTime, 2),
                OverallHealth        = analysis.OverallHealth,
                VelocityTrend        = Math.Round(analysis.TrendPercentage, 2),
                SprintCount          = analysis.Metrics.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching snapshot for project {ProjectKey}", projectKey);
            return null;
        }
    }

    private static string BuildAwardIcons(TeamProjectSnapshot team, TeamComparisonReport report)
    {
        var icons = new List<string>();
        if (team.ProjectKey == report.FastestTeam)        icons.Add("⚡");
        if (team.ProjectKey == report.HighestQualityTeam) icons.Add("✅");
        if (team.ProjectKey == report.MostConsistentTeam) icons.Add("🎯");
        return icons.Count > 0 ? "  " + string.Join(" ", icons) : string.Empty;
    }
}
