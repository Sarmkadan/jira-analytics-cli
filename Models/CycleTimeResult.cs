// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// Returns a one-line summary, e.g. <c>PROJ: avg 4.0d, median 3.5d, p75 6.0d, p90 9.0d (3 issues)</c>.
    /// Numbers are formatted with the invariant culture so the output is stable across locales.
    /// </summary>
    public override string ToString()
    {
        return FormattableString.Invariant($"{this.ProjectKey}: avg {this.AverageCycleTime:F1}d, median {this.MedianCycleTime:F1}d, p75 {this.P75CycleTime:F1}d, p90 {this.P90CycleTime:F1}d ({this.IssueCycleTimes.Count} issues)");
    }
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

    private DateTimeOffset _createdDate;
    private DateTimeOffset? _resolutionDate;

    /// <summary>
    /// Gets or sets the created date, always normalized to UTC so that subtracting it from
    /// <see cref="ResolutionDate"/> yields a correct elapsed duration regardless of which
    /// time zone or DST offset was in effect when either timestamp was captured.
    /// </summary>
    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate
    {
        get => this._createdDate;
        set => this._createdDate = value.ToUniversalTime();
    }

    /// <summary>
    /// Gets or sets the resolution date (if resolved), always normalized to UTC for the same
    /// reason as <see cref="CreatedDate"/>.
    /// </summary>
    [JsonPropertyName("resolutionDate")]
    public DateTimeOffset? ResolutionDate
    {
        get => this._resolutionDate;
        set => this._resolutionDate = value.HasValue ? value.Value.ToUniversalTime() : null;
    }
}
