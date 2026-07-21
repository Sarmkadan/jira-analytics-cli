// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.ComponentModel.DataAnnotations;

namespace JiraAnalyticsCli.Models;


/// <summary>
/// Represents cycle time analysis results for a project
/// </summary>
public class CycleTimeResult
{
    /// <summary>Gets or sets the project key.</summary>
    [Required]
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the average cycle time in days.</summary>
    public double AverageCycleTime { get; set; }

    
    /// <summary>Gets or sets the median cycle time in days.</summary>
    public double MedianCycleTime { get; set; }

    /// <summary>Gets or sets the 50th percentile cycle time in days.</summary>
    public double P50CycleTime { get; set; }

    /// <summary>Gets or sets the 75th percentile cycle time in days.</summary>
    public double P75CycleTime { get; set; }

    /// <summary>Gets or sets the 90th percentile cycle time in days.</summary>
    public double P90CycleTime { get; set; }

    /// <summary>Gets or sets the list of individual issue cycle times.</summary>
    public List<IssueCycleTime> IssueCycleTimes { get; set; } = new();
}

/// <summary>
/// Represents cycle time data for a single issue
/// </summary>
public class IssueCycleTime
{
    /// <summary>Gets or sets the issue key.</summary>
    [Required]
    public string IssueKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the issue summary.</summary>
    [Required]
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets or sets the cycle time in days.</summary>
    public double CycleTimeDays { get; set; }

    /// <summary>Gets or sets the created date.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the resolution date (if resolved).</summary>
    public DateTime? ResolutionDate { get; set; }
}
