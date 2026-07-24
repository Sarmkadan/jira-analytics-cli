// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JiraAnalyticsCli.Utils;

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

    private DateTime _createdDate;
    private DateTime? _resolutionDate;

    /// <summary>
    /// Gets or sets the created date, always normalized to UTC so that subtracting it from
    /// <see cref="ResolutionDate"/> yields a correct elapsed duration regardless of which
    /// time zone or DST offset was in effect when either timestamp was captured.
    /// </summary>
    [JsonConverter(typeof(UtcDateTimeJsonConverter))]
    public DateTime CreatedDate
    {
        get => _createdDate;
        set => _createdDate = NormalizeToUtc(value);
    }

    /// <summary>
    /// Gets or sets the resolution date (if resolved), always normalized to UTC for the same
    /// reason as <see cref="CreatedDate"/>.
    /// </summary>
    [JsonConverter(typeof(UtcDateTimeJsonConverter))]
    public DateTime? ResolutionDate
    {
        get => _resolutionDate;
        set => _resolutionDate = value.HasValue ? NormalizeToUtc(value.Value) : null;
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to an equivalent
    /// UTC value. <see cref="DateTimeKind.Unspecified"/> values are treated as already being
    /// UTC (the convention used throughout this codebase), while <see cref="DateTimeKind.Local"/>
    /// values are converted using the host's time zone rules.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>An equivalent <see cref="DateTime"/> with <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/>.</returns>
    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}
