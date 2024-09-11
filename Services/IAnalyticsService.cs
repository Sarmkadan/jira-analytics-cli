// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Interface for analytics calculations and metrics generation
/// </summary>
public interface IAnalyticsService
{
    Task<SprintAnalysisResult> AnalyzeSprints(string projectKey, int sprintCount);
    Task<TeamAnalysisResult> AnalyzeTeam(string projectKey);
    Task<QualityMetricsResult> AnalyzeQuality(string projectKey);
    Task<VelocityTrendResult> AnalyzeVelocityTrend(string projectKey, int sprintCount);
    Task<OverdueIssuesResult> AnalyzeOverdueIssues(string projectKey);
}

public class SprintAnalysisResult
{
    public List<SprintMetric> Metrics { get; set; } = new();
    public double AverageVelocity { get; set; }
    public double TrendPercentage { get; set; }
    public string OverallHealth { get; set; } = "Unknown";
}

public class TeamAnalysisResult
{
    public List<Developer> TopPerformers { get; set; } = new();
    public List<Developer> LowPerformers { get; set; } = new();
    public double AverageProductivity { get; set; }
    public Dictionary<string, int> WorkloadDistribution { get; set; } = new();
}

public class QualityMetricsResult
{
    public double AverageQualityScore { get; set; }
    public int TotalDefects { get; set; }
    public double DefectRate { get; set; }
    public List<string> HighRiskAreas { get; set; } = new();
}

public class VelocityTrendResult
{
    public List<(string SprintName, double Velocity)> Velocities { get; set; } = new();
    public double TrendSlope { get; set; }
    public string Trend { get; set; } = "Stable"; // Increasing, Decreasing, Stable
}

public class OverdueIssuesResult
{
    public int TotalOverdueCount { get; set; }
    public int CriticalCount { get; set; }
    public List<JiraIssue> Issues { get; set; } = new();
    public double AverageDaysOverdue { get; set; }
}
