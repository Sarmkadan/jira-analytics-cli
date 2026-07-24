// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Text.Json;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Tests proving that <see cref="BurndownSnapshot.Timestamp"/> stays UTC-normalized end to
/// end, so duration math (e.g. burn-rate points/hour) stays correct across a DST boundary
/// even when the source timestamps carry different UTC offsets, as Jira's API does.
/// </summary>
public class BurndownSnapshotTimestampUtcTests
{
    /// <summary>
    /// On 2026-03-29 Central European time springs forward from +01:00 to +02:00 at 02:00
    /// local (01:00 UTC). A snapshot taken at 01:30+01:00 (00:30 UTC) and another taken at
    /// 03:30+02:00 (01:30 UTC) are exactly one real hour apart, even though their local
    /// wall-clock values are two hours apart. If the two timestamps were deserialized with
    /// the default DateTime converter (which preserves the local-converted Kind), the naive
    /// difference would drift depending on the host's time zone; normalizing to UTC keeps it
    /// exactly correct regardless of where the code runs.
    /// </summary>
    [Fact]
    public void Timestamp_DeserializedAcrossDstBoundary_YieldsCorrectRealDuration()
    {
        // Arrange
        var beforeDst = JsonSerializer.Deserialize<BurndownSnapshot>(
            """
            {"timestamp":"2026-03-29T01:30:00+01:00","sprintId":1,"remainingStoryPoints":40,"completedStoryPoints":60,"totalStoryPoints":100,"remainingIssueCount":4,"completedIssueCount":6,"totalIssueCount":10,"scopeChanges":0}
            """);
        var afterDst = JsonSerializer.Deserialize<BurndownSnapshot>(
            """
            {"timestamp":"2026-03-29T03:30:00+02:00","sprintId":1,"remainingStoryPoints":30,"completedStoryPoints":70,"totalStoryPoints":100,"remainingIssueCount":3,"completedIssueCount":7,"totalIssueCount":10,"scopeChanges":0}
            """);

        // Act
        var elapsed = afterDst!.Timestamp - beforeDst!.Timestamp;
        var pointsBurned = afterDst.CompletedStoryPoints - beforeDst.CompletedStoryPoints;
        var burnRatePointsPerHour = pointsBurned / elapsed.TotalHours;

        // Assert
        Assert.Equal(DateTimeKind.Utc, beforeDst.Timestamp.Kind);
        Assert.Equal(DateTimeKind.Utc, afterDst.Timestamp.Kind);
        Assert.Equal(new DateTime(2026, 3, 29, 0, 30, 0, DateTimeKind.Utc), beforeDst.Timestamp);
        Assert.Equal(new DateTime(2026, 3, 29, 1, 30, 0, DateTimeKind.Utc), afterDst.Timestamp);
        Assert.Equal(1.0, elapsed.TotalHours, precision: 6);
        Assert.Equal(10.0, burnRatePointsPerHour, precision: 6);
    }

    /// <summary>
    /// Assigning a <see cref="DateTimeKind.Local"/> or <see cref="DateTimeKind.Unspecified"/>
    /// value directly (bypassing JSON) is also normalized to UTC by the property setter, so
    /// callers that build snapshots in memory (e.g. from <c>DateTime.Now</c> or a bare
    /// <c>new DateTime(...)</c>) cannot silently corrupt burn-rate math either.
    /// </summary>
    [Fact]
    public void Timestamp_AssignedDirectly_IsAlwaysNormalizedToUtc()
    {
        // Arrange
        var unspecified = new DateTime(2026, 7, 10, 14, 30, 0, DateTimeKind.Unspecified);

        // Act
        var snapshot = new BurndownSnapshot { Timestamp = unspecified };

        // Assert
        Assert.Equal(DateTimeKind.Utc, snapshot.Timestamp.Kind);
        Assert.Equal(new DateTime(2026, 7, 10, 14, 30, 0, DateTimeKind.Utc), snapshot.Timestamp);
    }
}
