// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Tests for <see cref="BurndownSnapshotExtensions"/> methods
/// </summary>
public class BurndownSnapshotTestsExtensions
{
    [Fact]
    public void CalculateVelocityTrend_WithValidSnapshots_ReturnsCorrectValue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var snapshot = new BurndownSnapshot
        {
            Timestamp = now,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 40,
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 0
        };

        var historicalSnapshots = new List<BurndownSnapshot>
        {
            new BurndownSnapshot
            {
                Timestamp = now.AddDays(-3),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 30,
                RemainingStoryPoints = 70,
                TotalIssueCount = 20,
                CompletedIssueCount = 6,
                RemainingIssueCount = 14,
                ScopeChanges = 0
            },
            new BurndownSnapshot
            {
                Timestamp = now.AddDays(-2),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 40,
                RemainingStoryPoints = 60,
                TotalIssueCount = 20,
                CompletedIssueCount = 8,
                RemainingIssueCount = 12,
                ScopeChanges = 0
            }
        };

        // Act
        var velocityTrend = snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        Assert.True(velocityTrend > 0);
        // (60 - 40) / 3 days = ~6.67 story points per day
        Assert.Equal(6.66667, velocityTrend, 5);
    }

    [Fact]
    public void CalculateVelocityTrend_WithInvalidSnapshot_ThrowsArgumentException()
    {
        // Arrange - Inconsistent snapshot
        var now = DateTime.UtcNow;
        var snapshot = new BurndownSnapshot
        {
            Timestamp = now,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 50, // Inconsistent: 60 + 50 = 110, not 100
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 0
        };

        var historicalSnapshots = new List<BurndownSnapshot>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => snapshot.CalculateVelocityTrend(historicalSnapshots));
    }

    [Fact]
    public void CalculateVelocityTrend_WithNullSnapshot_ThrowsArgumentNullException()
    {
        // Arrange
        var historicalSnapshots = new List<BurndownSnapshot>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BurndownSnapshotExtensions.CalculateVelocityTrend(null!, historicalSnapshots));
    }

    [Fact]
    public void CalculateVelocityTrend_WithNullHistoricalSnapshots_ThrowsArgumentNullException()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 50,
            RemainingStoryPoints = 50,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => snapshot.CalculateVelocityTrend(null!));
    }

    [Fact]
    public void IsVelocityAccelerating_WithValidSnapshots_ReturnsBoolean()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var snapshot = new BurndownSnapshot
        {
            Timestamp = now,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 40,
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 0
        };

        var historicalSnapshots = new List<BurndownSnapshot>
        {
            new BurndownSnapshot
            {
                Timestamp = now.AddDays(-3),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 30,
                RemainingStoryPoints = 70,
                TotalIssueCount = 20,
                CompletedIssueCount = 6,
                RemainingIssueCount = 14,
                ScopeChanges = 0
            }
        };

        // Act
        var isAccelerating = snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        Assert.IsType<bool>(isAccelerating);
    }

    [Fact]
    public void IsVelocityAccelerating_WithInvalidSnapshot_ThrowsArgumentException()
    {
        // Arrange - Inconsistent snapshot
        var now = DateTime.UtcNow;
        var snapshot = new BurndownSnapshot
        {
            Timestamp = now,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 50, // Inconsistent
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 0
        };

        var historicalSnapshots = new List<BurndownSnapshot>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => snapshot.IsVelocityAccelerating(historicalSnapshots));
    }

    [Fact]
    public void GetBurnRate_WithValidSnapshot_ReturnsCorrectValue()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 50,
            RemainingStoryPoints = 50,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var burnRate = snapshot.GetBurnRate(14); // 2-week sprint

        // Assert
        Assert.True(burnRate > 0);
        Assert.Equal(50.0 / 14, burnRate, 5);
    }

    [Fact]
    public void GetBurnRate_WithInvalidSnapshot_ThrowsArgumentException()
    {
        // Arrange - Inconsistent snapshot
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 50,
            RemainingStoryPoints = 60, // Inconsistent: 50 + 60 = 110, not 100
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => snapshot.GetBurnRate(14));
    }

    [Fact]
    public void GetBurnRate_WithNegativeDaysInSprint_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 50,
            RemainingStoryPoints = 50,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetBurnRate(-1));
    }

    [Fact]
    public void CreateDeltaSnapshot_WithValidSnapshots_ReturnsDeltaSnapshot()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var current = new BurndownSnapshot
        {
            Timestamp = now,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 40,
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 5
        };

        var previous = new BurndownSnapshot
        {
            Timestamp = now.AddDays(-1),
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 40,
            RemainingStoryPoints = 60,
            TotalIssueCount = 20,
            CompletedIssueCount = 8,
            RemainingIssueCount = 12,
            ScopeChanges = 2
        };

        // Act
        var delta = current.CreateDeltaSnapshot(previous);

        // Assert
        Assert.NotNull(delta);
        Assert.Equal(-20, delta.RemainingStoryPoints); // 40 - 60 = -20
        Assert.Equal(20, delta.CompletedStoryPoints); // 60 - 40 = 20
        Assert.Equal(0, delta.TotalStoryPoints); // 100 - 100 = 0
        Assert.Equal(-4, delta.RemainingIssueCount); // 8 - 12 = -4
        Assert.Equal(4, delta.CompletedIssueCount); // 12 - 8 = 4
        Assert.Equal(0, delta.TotalIssueCount); // 20 - 20 = 0
        Assert.Equal(3, delta.ScopeChanges); // 5 - 2 = 3
    }

    [Fact]
    public void CreateDeltaSnapshot_WithInvalidCurrentSnapshot_ThrowsArgumentException()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var current = new BurndownSnapshot
        {
            Timestamp = now,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 50, // Inconsistent
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 0
        };

        var previous = new BurndownSnapshot
        {
            Timestamp = now.AddDays(-1),
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 40,
            RemainingStoryPoints = 60,
            TotalIssueCount = 20,
            CompletedIssueCount = 8,
            RemainingIssueCount = 12,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => current.CreateDeltaSnapshot(previous));
    }

    [Fact]
    public void CreateDeltaSnapshot_WithInvalidPreviousSnapshot_ThrowsArgumentException()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var current = new BurndownSnapshot
        {
            Timestamp = now,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 40,
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 0
        };

        var previous = new BurndownSnapshot
        {
            Timestamp = now.AddDays(-1),
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 40,
            RemainingStoryPoints = 70, // Inconsistent
            TotalIssueCount = 20,
            CompletedIssueCount = 8,
            RemainingIssueCount = 12,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => current.CreateDeltaSnapshot(previous));
    }

    [Fact]
    public void CreateDeltaSnapshot_WithNullCurrent_ThrowsArgumentNullException()
    {
        // Arrange
        var previous = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 50,
            RemainingStoryPoints = 50,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BurndownSnapshotExtensions.CreateDeltaSnapshot(null!, previous));
    }

    [Fact]
    public void CreateDeltaSnapshot_WithNullPrevious_ThrowsArgumentNullException()
    {
        // Arrange
        var current = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 50,
            RemainingStoryPoints = 50,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => current.CreateDeltaSnapshot(null!));
    }

    [Fact]
    public void ToStatusString_WithValidSnapshot_ReturnsFormattedString()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = new DateTime(2024, 1, 15, 10, 30, 0),
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 45,
            RemainingStoryPoints = 55,
            TotalIssueCount = 20,
            CompletedIssueCount = 9,
            RemainingIssueCount = 11,
            ScopeChanges = 3
        };

        // Act
        var statusString = snapshot.ToStatusString();

        // Assert
        Assert.NotNull(statusString);
        Assert.Contains("Sprint 1 @ 2024-01-15 10:30", statusString);
        Assert.Contains("45/100 pts", statusString);
        Assert.Contains("9/20 issues", statusString);
        Assert.Contains("Scope change: +3", statusString);
    }

    [Fact]
    public void ToStatusString_WithInvalidSnapshot_ThrowsArgumentException()
    {
        // Arrange - Inconsistent snapshot
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 50, // Inconsistent
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 0
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => snapshot.ToStatusString());
    }

    [Fact]
    public void HasScopeCreep_WithValidSnapshot_ReturnsBoolean()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 50,
            RemainingStoryPoints = 50,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 5 // Above default threshold of 3
        };

        // Act
        var hasCreep = snapshot.HasScopeCreep();

        // Assert
        Assert.True(hasCreep);
    }

    [Fact]
    public void HasScopeCreep_WithInvalidSnapshot_ThrowsArgumentException()
    {
        // Arrange - Inconsistent snapshot
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 100,
            CompletedStoryPoints = 60,
            RemainingStoryPoints = 50, // Inconsistent
            TotalIssueCount = 20,
            CompletedIssueCount = 12,
            RemainingIssueCount = 8,
            ScopeChanges = 5
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => snapshot.HasScopeCreep());
    }

    [Fact]
    public void GetCompletedStoryPointsOverTime_WithValidSnapshots_ReturnsSequence()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow.AddDays(-2),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 20,
                RemainingStoryPoints = 80,
                TotalIssueCount = 20,
                CompletedIssueCount = 4,
                RemainingIssueCount = 16,
                ScopeChanges = 0
            },
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow.AddDays(-1),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 40,
                RemainingStoryPoints = 60,
                TotalIssueCount = 20,
                CompletedIssueCount = 8,
                RemainingIssueCount = 12,
                ScopeChanges = 0
            },
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow,
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 60,
                RemainingStoryPoints = 40,
                TotalIssueCount = 20,
                CompletedIssueCount = 12,
                RemainingIssueCount = 8,
                ScopeChanges = 0
            }
        };

        // Act
        var completedPoints = snapshots.GetCompletedStoryPointsOverTime();

        // Assert
        Assert.NotNull(completedPoints);
        var list = Assert.IsType<List<int>>(completedPoints);
        Assert.Equal(3, list.Count);
        Assert.Equal(20, list[0]);
        Assert.Equal(40, list[1]);
        Assert.Equal(60, list[2]);
    }

    [Fact]
    public void GetCompletedStoryPointsOverTime_WithInvalidSnapshot_ThrowsArgumentException()
    {
        // Arrange - One invalid snapshot in the list
        var snapshots = new List<BurndownSnapshot>
        {
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow.AddDays(-1),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 40,
                RemainingStoryPoints = 60,
                TotalIssueCount = 20,
                CompletedIssueCount = 8,
                RemainingIssueCount = 12,
                ScopeChanges = 0
            },
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow,
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 60,
                RemainingStoryPoints = 50, // Inconsistent
                TotalIssueCount = 20,
                CompletedIssueCount = 12,
                RemainingIssueCount = 8,
                ScopeChanges = 0
            }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => snapshots.GetCompletedStoryPointsOverTime().ToList());
    }

    [Fact]
    public void GetRemainingStoryPointsOverTime_WithValidSnapshots_ReturnsSequence()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow.AddDays(-2),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 20,
                RemainingStoryPoints = 80,
                TotalIssueCount = 20,
                CompletedIssueCount = 4,
                RemainingIssueCount = 16,
                ScopeChanges = 0
            },
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow.AddDays(-1),
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 40,
                RemainingStoryPoints = 60,
                TotalIssueCount = 20,
                CompletedIssueCount = 8,
                RemainingIssueCount = 12,
                ScopeChanges = 0
            },
            new BurndownSnapshot
            {
                Timestamp = DateTime.UtcNow,
                SprintId = 1,
                TotalStoryPoints = 100,
                CompletedStoryPoints = 60,
                RemainingStoryPoints = 40,
                TotalIssueCount = 20,
                CompletedIssueCount = 12,
                RemainingIssueCount = 8,
                ScopeChanges = 0
            }
        };

        // Act
        var remainingPoints = snapshots.GetRemainingStoryPointsOverTime();

        // Assert
        Assert.NotNull(remainingPoints);
        var list = Assert.IsType<List<int>>(remainingPoints);
        Assert.Equal(3, list.Count);
        Assert.Equal(80, list[0]);
        Assert.Equal(60, list[1]);
        Assert.Equal(40, list[2]);
    }

    [Fact]
    public void GetRemainingStoryPointsOverTime_WithNullSnapshots_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BurndownSnapshotExtensions.GetRemainingStoryPointsOverTime(null!));
    }
}
