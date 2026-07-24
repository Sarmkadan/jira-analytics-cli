using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

public class BurndownSnapshotExtensionsTests
{
    // Helper method to create a valid BurndownSnapshot
    private BurndownSnapshot CreateValidSnapshot(
        int sprintId = 1,
        int completedStoryPoints = 50,
        int totalStoryPoints = 100,
        int remainingStoryPoints = -1, // -1 means calculate from completed + total
        int remainingIssueCount = -1, // -1 means calculate from completed + total
        int completedIssueCount = 10,
        int totalIssueCount = 20,
        int scopeChanges = 0,
        DateTime? timestamp = null)
    {
        if (remainingStoryPoints == -1)
        {
            remainingStoryPoints = totalStoryPoints - completedStoryPoints;
        }

        if (remainingIssueCount == -1)
        {
            remainingIssueCount = totalIssueCount - completedIssueCount;
        }

        // Ensure timestamp is not in the future (validation rule)
        timestamp ??= DateTime.UtcNow.AddMinutes(-1);

        return new BurndownSnapshot
        {
            SprintId = sprintId,
            Timestamp = timestamp.Value,
            RemainingStoryPoints = remainingStoryPoints,
            CompletedStoryPoints = completedStoryPoints,
            TotalStoryPoints = totalStoryPoints,
            RemainingIssueCount = remainingIssueCount,
            CompletedIssueCount = completedIssueCount,
            TotalIssueCount = totalIssueCount,
            ScopeChanges = scopeChanges
        };
    }

    #region Velocity Trend Tests

    [Fact]
    public void CalculateVelocityTrend_ShouldReturnZero_WhenHistoricalSnapshotsIsEmpty()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();
        var historicalSnapshots = new List<BurndownSnapshot>();

        // Act
        var result = snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateVelocityTrend_ShouldReturnZero_WhenNoPreviousDaySnapshots()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(timestamp: snapshot.Timestamp.AddDays(1)) // Future timestamp
        };

        // Act
        var result = snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateVelocityTrend_ShouldReturnCorrectValue_WhenFlatTrend()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date.AddDays(-10);
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 80,
            timestamp: baseTime.AddDays(10)
        );

        // Create historical snapshots with flat velocity (2 points completed per day)
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 60, timestamp: baseTime.AddDays(0)),
            CreateValidSnapshot(completedStoryPoints: 62, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 64, timestamp: baseTime.AddDays(2)),
            CreateValidSnapshot(completedStoryPoints: 66, timestamp: baseTime.AddDays(3)),
            CreateValidSnapshot(completedStoryPoints: 68, timestamp: baseTime.AddDays(4)),
            CreateValidSnapshot(completedStoryPoints: 70, timestamp: baseTime.AddDays(5)),
            CreateValidSnapshot(completedStoryPoints: 72, timestamp: baseTime.AddDays(6)),
            CreateValidSnapshot(completedStoryPoints: 74, timestamp: baseTime.AddDays(7)),
            CreateValidSnapshot(completedStoryPoints: 76, timestamp: baseTime.AddDays(8)),
            CreateValidSnapshot(completedStoryPoints: 78, timestamp: baseTime.AddDays(9))
        };

        // Act
        var result = snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        // Should be approximately 2 points per day (80-60)/10 = 2
        result.Should().BeApproximately(2.0, 0.1);
    }

    [Fact]
    public void CalculateVelocityTrend_ShouldReturnPositiveValue_WhenIncreasingTrend()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date.AddDays(-10);
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 100,
            timestamp: baseTime.AddDays(10)
        );

        // Create historical snapshots with increasing velocity
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 20, timestamp: baseTime.AddDays(0)),
            CreateValidSnapshot(completedStoryPoints: 25, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 35, timestamp: baseTime.AddDays(2)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(3)),
            CreateValidSnapshot(completedStoryPoints: 70, timestamp: baseTime.AddDays(4)),
            CreateValidSnapshot(completedStoryPoints: 80, timestamp: baseTime.AddDays(5)),
            CreateValidSnapshot(completedStoryPoints: 85, timestamp: baseTime.AddDays(6)),
            CreateValidSnapshot(completedStoryPoints: 90, timestamp: baseTime.AddDays(7)),
            CreateValidSnapshot(completedStoryPoints: 95, timestamp: baseTime.AddDays(8)),
            CreateValidSnapshot(completedStoryPoints: 98, timestamp: baseTime.AddDays(9))
        };

        // Act
        var result = snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        // Should be positive and significant
        result.Should().BeGreaterThan(5.0);
    }

    [Fact]
    public void CalculateVelocityTrend_ShouldReturnNegativeValue_WhenDecreasingTrend()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date.AddDays(-10);
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 50,
            timestamp: baseTime.AddDays(10)
        );

        // Create historical snapshots with decreasing velocity
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 90, timestamp: baseTime.AddDays(0)),
            CreateValidSnapshot(completedStoryPoints: 88, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 85, timestamp: baseTime.AddDays(2)),
            CreateValidSnapshot(completedStoryPoints: 80, timestamp: baseTime.AddDays(3)),
            CreateValidSnapshot(completedStoryPoints: 75, timestamp: baseTime.AddDays(4)),
            CreateValidSnapshot(completedStoryPoints: 70, timestamp: baseTime.AddDays(5)),
            CreateValidSnapshot(completedStoryPoints: 65, timestamp: baseTime.AddDays(6)),
            CreateValidSnapshot(completedStoryPoints: 60, timestamp: baseTime.AddDays(7)),
            CreateValidSnapshot(completedStoryPoints: 55, timestamp: baseTime.AddDays(8)),
            CreateValidSnapshot(completedStoryPoints: 52, timestamp: baseTime.AddDays(9))
        };

        // Act
        var result = snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        // Should be negative
        result.Should().BeNegative();
    }

    [Fact]
    public void CalculateVelocityTrend_ShouldThrowArgumentNullException_WhenSnapshotIsNull()
    {
        // Arrange
        BurndownSnapshot snapshot = null!;
        var historicalSnapshots = new List<BurndownSnapshot> { CreateValidSnapshot() };

        // Act
        Action act = () => snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateVelocityTrend_ShouldThrowArgumentNullException_WhenHistoricalSnapshotsIsNull()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();
        IReadOnlyList<BurndownSnapshot> historicalSnapshots = null!;

        // Act
        Action act = () => snapshot.CalculateVelocityTrend(historicalSnapshots);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Velocity Acceleration Tests

    [Fact]
    public void IsVelocityAccelerating_ShouldReturnFalse_WhenHistoricalSnapshotsIsEmpty()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();
        var historicalSnapshots = new List<BurndownSnapshot>();

        // Act
        var result = snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVelocityAccelerating_ShouldReturnFalse_WhenInsufficientDataForOverallVelocity()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(completedStoryPoints: 10);
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 5, timestamp: snapshot.Timestamp.AddSeconds(-1))
        };

        // Act
        var result = snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVelocityAccelerating_ShouldReturnFalse_WhenVelocityTrendIsZero()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date.AddDays(-5);
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 50,
            timestamp: baseTime.AddDays(5)
        );

        // Create historical snapshots with zero velocity trend (flat)
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(0)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(2)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(3)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(4))
        };

        // Act
        var result = snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVelocityAccelerating_ShouldReturnTrue_WhenVelocityIsAccelerating()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date.AddDays(-10);
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 100,
            timestamp: baseTime.AddDays(10)
        );

        // Create historical snapshots with low initial velocity but recent acceleration
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10, timestamp: baseTime.AddDays(0)),
            CreateValidSnapshot(completedStoryPoints: 15, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 20, timestamp: baseTime.AddDays(2)),
            CreateValidSnapshot(completedStoryPoints: 25, timestamp: baseTime.AddDays(3)),
            CreateValidSnapshot(completedStoryPoints: 30, timestamp: baseTime.AddDays(4)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(5)), // Jump
            CreateValidSnapshot(completedStoryPoints: 70, timestamp: baseTime.AddDays(6)), // Jump
            CreateValidSnapshot(completedStoryPoints: 85, timestamp: baseTime.AddDays(7)),
            CreateValidSnapshot(completedStoryPoints: 95, timestamp: baseTime.AddDays(8)),
            CreateValidSnapshot(completedStoryPoints: 98, timestamp: baseTime.AddDays(9))
        };

        // Act
        var result = snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsVelocityAccelerating_ShouldReturnFalse_WhenVelocityIsDecelerating()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date.AddDays(-10);
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 50,
            timestamp: baseTime.AddDays(10)
        );

        // Create historical snapshots with high initial velocity but recent deceleration
        var historicalSnapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 90, timestamp: baseTime.AddDays(0)),
            CreateValidSnapshot(completedStoryPoints: 85, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 80, timestamp: baseTime.AddDays(2)),
            CreateValidSnapshot(completedStoryPoints: 75, timestamp: baseTime.AddDays(3)),
            CreateValidSnapshot(completedStoryPoints: 70, timestamp: baseTime.AddDays(4)),
            CreateValidSnapshot(completedStoryPoints: 60, timestamp: baseTime.AddDays(5)), // Drop
            CreateValidSnapshot(completedStoryPoints: 55, timestamp: baseTime.AddDays(6)),
            CreateValidSnapshot(completedStoryPoints: 52, timestamp: baseTime.AddDays(7)),
            CreateValidSnapshot(completedStoryPoints: 51, timestamp: baseTime.AddDays(8)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(9))
        };

        // Act
        var result = snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsVelocityAccelerating_ShouldThrowArgumentNullException_WhenSnapshotIsNull()
    {
        // Arrange
        BurndownSnapshot snapshot = null!;
        var historicalSnapshots = new List<BurndownSnapshot> { CreateValidSnapshot() };

        // Act
        Action act = () => snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsVelocityAccelerating_ShouldThrowArgumentNullException_WhenHistoricalSnapshotsIsNull()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();
        IReadOnlyList<BurndownSnapshot> historicalSnapshots = null!;

        // Act
        Action act = () => snapshot.IsVelocityAccelerating(historicalSnapshots);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Burn Rate Tests

    [Fact]
    public void GetBurnRate_ShouldReturnZero_WhenRemainingStoryPointsIsZero()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 100,
            remainingStoryPoints: 0, // Already complete
            totalStoryPoints: 100
        );

        // Act
        var result = snapshot.GetBurnRate();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetBurnRate_ShouldReturnZero_WhenCompletedStoryPointsIsZero()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 0,
            totalStoryPoints: 100
        );

        // Act
        var result = snapshot.GetBurnRate();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetBurnRate_ShouldReturnCorrectValue_WhenNormalCase()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 70,
            remainingStoryPoints: 30,
            totalStoryPoints: 100
        );
        // Timestamp is set to now, so burn rate should be 70/14 = 5 points per day for 2-week sprint

        // Act
        var result = snapshot.GetBurnRate();

        // Assert
        result.Should().Be(5.0);
    }

    [Fact]
    public void GetBurnRate_ShouldReturnCorrectValue_WhenCustomDaysInSprint()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(
            completedStoryPoints: 60,
            remainingStoryPoints: 40,
            totalStoryPoints: 100
        );
        // Timestamp is set to now, so for 10-day sprint: 60/10 = 6 points per day

        // Act
        var result = snapshot.GetBurnRate(10);

        // Assert
        result.Should().Be(6.0);
    }

    [Fact]
    public void GetBurnRate_ShouldThrowArgumentNullException_WhenSnapshotIsNull()
    {
        // Arrange
        BurndownSnapshot snapshot = null!;

        // Act
        Action act = () => snapshot.GetBurnRate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetBurnRate_ShouldThrowArgumentOutOfRangeException_WhenDaysInSprintIsZero()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();

        // Act
        Action act = () => snapshot.GetBurnRate(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetBurnRate_ShouldThrowArgumentOutOfRangeException_WhenDaysInSprintIsNegative()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();

        // Act
        Action act = () => snapshot.GetBurnRate(-5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Delta Snapshot Tests

    [Fact]
    public void CreateDeltaSnapshot_ShouldReturnNull_WhenTimestampsAreNotChronological()
    {
        // Arrange
        var previous = CreateValidSnapshot(timestamp: DateTime.UtcNow.AddDays(-1));
        var current = CreateValidSnapshot(timestamp: DateTime.UtcNow.AddDays(-2)); // Earlier timestamp

        // Act
        var result = current.CreateDeltaSnapshot(previous);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateDeltaSnapshot_ShouldReturnNull_WhenTimestampsAreEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var previous = CreateValidSnapshot(timestamp: timestamp);
        var current = CreateValidSnapshot(timestamp: timestamp); // Same timestamp

        // Act
        var result = current.CreateDeltaSnapshot(previous);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CreateDeltaSnapshot_ShouldReturnCorrectDelta_WhenTimestampsAreChronological()
    {
        // Arrange
        var previous = CreateValidSnapshot(
            completedStoryPoints: 30,
            remainingStoryPoints: 70,
            totalStoryPoints: 100,
            completedIssueCount: 5,
            remainingIssueCount: 15,
            totalIssueCount: 20,
            scopeChanges: 2
        );

        var current = CreateValidSnapshot(
            completedStoryPoints: 50,
            remainingStoryPoints: 50,
            totalStoryPoints: 100,
            completedIssueCount: 10,
            remainingIssueCount: 10,
            totalIssueCount: 20,
            scopeChanges: 3
        );

        // Act
        var result = current.CreateDeltaSnapshot(previous);

        // Assert
        result.Should().NotBeNull();
        result!.SprintId.Should().Be(current.SprintId);
        result.Timestamp.Should().Be(current.Timestamp);
        result.CompletedStoryPoints.Should().Be(20); // 50 - 30
        result.RemainingStoryPoints.Should().Be(-20); // 50 - 70
        result.TotalStoryPoints.Should().Be(0); // 100 - 100
        result.CompletedIssueCount.Should().Be(5); // 10 - 5
        result.RemainingIssueCount.Should().Be(-5); // 10 - 15
        result.TotalIssueCount.Should().Be(0); // 20 - 20
        result.ScopeChanges.Should().Be(1); // 3 - 2
    }

    [Fact]
    public void CreateDeltaSnapshot_ShouldThrowArgumentNullException_WhenCurrentIsNull()
    {
        // Arrange
        BurndownSnapshot current = null!;
        var previous = CreateValidSnapshot();

        // Act
        Action act = () => current.CreateDeltaSnapshot(previous);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateDeltaSnapshot_ShouldThrowArgumentNullException_WhenPreviousIsNull()
    {
        // Arrange
        var current = CreateValidSnapshot();
        BurndownSnapshot previous = null!;

        // Act
        Action act = () => current.CreateDeltaSnapshot(previous);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Scope Creep Tests

    [Fact]
    public void HasScopeCreep_ShouldReturnFalse_WhenScopeChangesBelowThreshold()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(scopeChanges: 2); // Below default threshold of 3

        // Act
        var result = snapshot.HasScopeCreep();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasScopeCreep_ShouldReturnTrue_WhenScopeChangesAtThreshold()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(scopeChanges: 3); // At default threshold of 3

        // Act
        var result = snapshot.HasScopeCreep();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasScopeCreep_ShouldReturnTrue_WhenScopeChangesAboveThreshold()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(scopeChanges: 5); // Above default threshold of 3

        // Act
        var result = snapshot.HasScopeCreep();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasScopeCreep_ShouldReturnFalse_WhenScopeChangesIsZero()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(scopeChanges: 0);

        // Act
        var result = snapshot.HasScopeCreep();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasScopeCreep_ShouldReturnFalse_WhenScopeChangesIsNegative()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(scopeChanges: -2); // Removed scope

        // Act
        var result = snapshot.HasScopeCreep();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasScopeCreep_ShouldUseCustomThreshold_WhenProvided()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(scopeChanges: 5);

        // Act
        var result = snapshot.HasScopeCreep(10); // Custom threshold of 10

        // Assert
        result.Should().BeFalse(); // 5 < 10
    }

    [Fact]
    public void HasScopeCreep_ShouldReturnTrue_WhenCustomThresholdIsMet()
    {
        // Arrange
        var snapshot = CreateValidSnapshot(scopeChanges: 15);

        // Act
        var result = snapshot.HasScopeCreep(10); // Custom threshold of 10

        // Assert
        result.Should().BeTrue(); // 15 >= 10
    }

    [Fact]
    public void HasScopeCreep_ShouldThrowArgumentNullException_WhenSnapshotIsNull()
    {
        // Arrange
        BurndownSnapshot snapshot = null!;

        // Act
        Action act = () => snapshot.HasScopeCreep();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasScopeCreep_ShouldThrowArgumentOutOfRangeException_WhenThresholdIsNegative()
    {
        // Arrange
        var snapshot = CreateValidSnapshot();

        // Act
        Action act = () => snapshot.HasScopeCreep(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Time Series Extraction Tests

    [Fact]
    public void GetCompletedStoryPointsOverTime_ShouldReturnEmptySequence_WhenInputIsEmpty()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>();

        // Act
        var result = snapshots.GetCompletedStoryPointsOverTime();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetCompletedStoryPointsOverTime_ShouldReturnSingleValue_WhenInputHasOneElement()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 42)
        };

        // Act
        var result = snapshots.GetCompletedStoryPointsOverTime();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void GetCompletedStoryPointsOverTime_ShouldReturnCorrectSequence_WhenInputHasMultipleElements()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10),
            CreateValidSnapshot(completedStoryPoints: 20),
            CreateValidSnapshot(completedStoryPoints: 30)
        };

        // Act
        var result = snapshots.GetCompletedStoryPointsOverTime();

        // Assert
        result.Should().ContainInOrder(10, 20, 30);
    }

    [Fact]
    public void GetCompletedStoryPointsOverTime_ShouldThrowArgumentNullException_WhenSnapshotsIsNull()
    {
        // Arrange
        IReadOnlyList<BurndownSnapshot> snapshots = null!;

        // Act
        Action act = () => snapshots.GetCompletedStoryPointsOverTime();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetRemainingStoryPointsOverTime_ShouldReturnEmptySequence_WhenInputIsEmpty()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>();

        // Act
        var result = snapshots.GetRemainingStoryPointsOverTime();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetRemainingStoryPointsOverTime_ShouldReturnSingleValue_WhenInputHasOneElement()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(remainingStoryPoints: 58)
        };

        // Act
        var result = snapshots.GetRemainingStoryPointsOverTime();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(58);
    }

    [Fact]
    public void GetRemainingStoryPointsOverTime_ShouldReturnCorrectSequence_WhenInputHasMultipleElements()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(remainingStoryPoints: 90),
            CreateValidSnapshot(remainingStoryPoints: 80),
            CreateValidSnapshot(remainingStoryPoints: 70)
        };

        // Act
        var result = snapshots.GetRemainingStoryPointsOverTime();

        // Assert
        result.Should().ContainInOrder(90, 80, 70);
    }

    [Fact]
    public void GetRemainingStoryPointsOverTime_ShouldThrowArgumentNullException_WhenSnapshotsIsNull()
    {
        // Arrange
        IReadOnlyList<BurndownSnapshot> snapshots = null!;

        // Act
        Action act = () => snapshots.GetRemainingStoryPointsOverTime();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToCompletedStoryPointsSeries_ShouldReturnEmptyList_WhenInputIsEmpty()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>();

        // Act
        var result = snapshots.ToCompletedStoryPointsSeries();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToCompletedStoryPointsSeries_ShouldReturnSingleTimeSeriesPoint_WhenInputHasOneElement()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 42, timestamp: timestamp)
        };

        // Act
        var result = snapshots.ToCompletedStoryPointsSeries();

        // Assert
        result.Should().ContainSingle();
        result[0].Timestamp.Should().Be(timestamp);
        result[0].Value.Should().Be(42);
    }

    [Fact]
    public void ToCompletedStoryPointsSeries_ShouldReturnCorrectList_WhenInputHasMultipleElements()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10, timestamp: baseTime.AddDays(-2)),
            CreateValidSnapshot(completedStoryPoints: 20, timestamp: baseTime.AddDays(-1)),
            CreateValidSnapshot(completedStoryPoints: 30, timestamp: baseTime)
        };

        // Act
        var result = snapshots.ToCompletedStoryPointsSeries();

        // Assert
        result.Should().HaveCount(3);
        result[0].Timestamp.Should().Be(baseTime.AddDays(-2));
        result[0].Value.Should().Be(10);
        result[1].Timestamp.Should().Be(baseTime.AddDays(-1));
        result[1].Value.Should().Be(20);
        result[2].Timestamp.Should().Be(baseTime);
        result[2].Value.Should().Be(30);
    }

    [Fact]
    public void ToCompletedStoryPointsSeries_ShouldThrowArgumentNullException_WhenSnapshotsIsNull()
    {
        // Arrange
        IReadOnlyList<BurndownSnapshot> snapshots = null!;

        // Act
        Action act = () => snapshots.ToCompletedStoryPointsSeries();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Trend Analysis Delegation Tests

    [Fact]
    public void GetCompletedStoryPointsTrendSlope_ShouldReturnZero_WhenInputIsEmpty()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>();

        // Act
        var result = snapshots.GetCompletedStoryPointsTrendSlope();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCompletedStoryPointsTrendSlope_ShouldReturnZero_WhenInputHasOneElement()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 50)
        };

        // Act
        var result = snapshots.GetCompletedStoryPointsTrendSlope();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCompletedStoryPointsTrendSlope_ShouldReturnCorrectSlope_WhenInputHasMultipleElements()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date;
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10, timestamp: baseTime.AddDays(0)),
            CreateValidSnapshot(completedStoryPoints: 20, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 30, timestamp: baseTime.AddDays(2))
        };

        // Act
        var result = snapshots.GetCompletedStoryPointsTrendSlope();

        // Assert
        // Should be approximately 10 points per day (linear increase from 10 to 30 over 2 days)
        result.Should().BeApproximately(10.0, 0.1);
    }

    [Fact]
    public void GetCompletedStoryPointsTrendSlope_ShouldThrowArgumentNullException_WhenSnapshotsIsNull()
    {
        // Arrange
        IReadOnlyList<BurndownSnapshot> snapshots = null!;

        // Act
        Action act = () => snapshots.GetCompletedStoryPointsTrendSlope();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetCompletedStoryPointsAcceleration_ShouldReturnZero_WhenInputHasLessThanFourElements()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10),
            CreateValidSnapshot(completedStoryPoints: 20),
            CreateValidSnapshot(completedStoryPoints: 30)
        };

        // Act
        var result = snapshots.GetCompletedStoryPointsAcceleration();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCompletedStoryPointsAcceleration_ShouldReturnCorrectValue_WhenInputHasSufficientElements()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.Date;
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10, timestamp: baseTime.AddDays(0)),    // First half: 10->20 (slope 10)
            CreateValidSnapshot(completedStoryPoints: 20, timestamp: baseTime.AddDays(1)),
            CreateValidSnapshot(completedStoryPoints: 30, timestamp: baseTime.AddDays(2)),    // Second half: 30->70 (slope 40)
            CreateValidSnapshot(completedStoryPoints: 40, timestamp: baseTime.AddDays(3)),
            CreateValidSnapshot(completedStoryPoints: 50, timestamp: baseTime.AddDays(4)),
            CreateValidSnapshot(completedStoryPoints: 60, timestamp: baseTime.AddDays(5)),
            CreateValidSnapshot(completedStoryPoints: 70, timestamp: baseTime.AddDays(6)),    // Second half: 70->100 (slope 30)
            CreateValidSnapshot(completedStoryPoints: 80, timestamp: baseTime.AddDays(7)),
            CreateValidSnapshot(completedStoryPoints: 90, timestamp: baseTime.AddDays(8)),
            CreateValidSnapshot(completedStoryPoints: 100, timestamp: baseTime.AddDays(9))
        };

        // Act
        var result = snapshots.GetCompletedStoryPointsAcceleration();

        // Assert
        // First half slope: (20-10)/1 = 10
        // Second half slope: (100-70)/3 = 10
        // Acceleration should be (10-10)/3.5 = 0 (since midpoints are 3.5 days apart)
        // Actually let me recalculate: first half points 0-3 (4 points), second half 4-9 (6 points)
        // First half slope: (40-10)/3 = 10
        // Second half slope: (100-60)/5 = 8
        // Midpoint difference: (7.5 - 1.5) = 6 days
        // Acceleration: (8-10)/6 = -0.33
        result.Should().BeApproximately(-0.33, 0.1);
    }

    [Fact]
    public void GetCompletedStoryPointsAcceleration_ShouldThrowArgumentNullException_WhenSnapshotsIsNull()
    {
        // Arrange
        IReadOnlyList<BurndownSnapshot> snapshots = null!;

        // Act
        Action act = () => snapshots.GetCompletedStoryPointsAcceleration();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetCompletedStoryPointsMovingAverage_ShouldReturnEmptyList_WhenInputIsEmpty()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>();

        // Act
        var result = snapshots.GetCompletedStoryPointsMovingAverage(3);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetCompletedStoryPointsMovingAverage_ShouldReturnCorrectValues_WhenInputHasElements()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10),
            CreateValidSnapshot(completedStoryPoints: 20),
            CreateValidSnapshot(completedStoryPoints: 30),
            CreateValidSnapshot(completedStoryPoints: 40)
        };

        // Act
        var result = snapshots.GetCompletedStoryPointsMovingAverage(2);

        // Assert
        result.Should().HaveCount(4);
        result[0].Should().Be(10); // (10)
        result[1].Should().Be(15); // (10+20)/2
        result[2].Should().Be(25); // (20+30)/2
        result[3].Should().Be(35); // (30+40)/2
    }

    [Fact]
    public void GetCompletedStoryPointsMovingAverage_ShouldThrowArgumentNullException_WhenSnapshotsIsNull()
    {
        // Arrange
        IReadOnlyList<BurndownSnapshot> snapshots = null!;

        // Act
        Action act = () => snapshots.GetCompletedStoryPointsMovingAverage(3);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetCompletedStoryPointsMovingAverage_ShouldThrowArgumentOutOfRangeException_WhenWindowSizeIsZero()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10)
        };

        // Act
        Action act = () => snapshots.GetCompletedStoryPointsMovingAverage(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetCompletedStoryPointsMovingAverage_ShouldThrowArgumentOutOfRangeException_WhenWindowSizeIsNegative()
    {
        // Arrange
        var snapshots = new List<BurndownSnapshot>
        {
            CreateValidSnapshot(completedStoryPoints: 10)
        };

        // Act
        Action act = () => snapshots.GetCompletedStoryPointsMovingAverage(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}