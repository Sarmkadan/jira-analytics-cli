// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Tests for <see cref="BurndownSnapshot"/> validation functionality
/// </summary>
public class BurndownSnapshotTestsValidation
{
    [Fact]
    public void Validate_WithValidSnapshot_DoesNotThrow()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 25,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.Empty(errors);
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_WithInconsistentStoryPoints_ThrowsOrReturnsError()
    {
        // Arrange - Total != Completed + Remaining (50 != 25 + 30)
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 30, // Inconsistent: 25 + 30 = 55, not 50
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Completed story points plus remaining story points must equal total story points"));
    }

    [Fact]
    public void Validate_WithNegativeCompletedStoryPoints_ReturnsError()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = -5, // Negative value
            RemainingStoryPoints = 55,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Completed story points cannot be negative"));
    }

    [Fact]
    public void Validate_WithNegativeRemainingStoryPoints_ReturnsError()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = -5, // Negative value
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Remaining story points cannot be negative"));
    }

    [Fact]
    public void Validate_WithZeroTotalStoryPoints_ReturnsError()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 0, // Zero value
            CompletedStoryPoints = 0,
            RemainingStoryPoints = 0,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Total story points must be a positive integer"));
    }

    [Fact]
    public void Validate_WithCompletedExceedingTotal_ReturnsError()
    {
        // Arrange - Completed > Total (60 > 50)
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 60, // Exceeds total
            RemainingStoryPoints = 0,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Completed story points cannot exceed total story points"));
    }

    [Fact]
    public void Validate_WithInconsistentIssueCounts_ThrowsOrReturnsError()
    {
        // Arrange - Total != Completed + Remaining for issues
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 25,
            TotalIssueCount = 20,
            CompletedIssueCount = 15,
            RemainingIssueCount = 10, // Inconsistent: 15 + 10 = 25, not 20
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Completed issue count plus remaining issue count must equal total issue count"));
    }

    [Fact]
    public void Validate_WithNegativeSprintId_ReturnsError()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = -1, // Negative ID
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 25,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Sprint ID must be a positive integer"));
    }

    [Fact]
    public void Validate_WithDefaultTimestamp_ReturnsError()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = default, // Default DateTime
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 25,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.Validate(snapshot);
        var isValid = BurndownSnapshotValidation.IsValid(snapshot);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Timestamp must be set to a valid DateTime"));
    }

    [Fact]
    public void EnsureValid_WithValidSnapshot_DoesNotThrow()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 25,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act - No exception should be thrown
        BurndownSnapshotValidation.EnsureValid(snapshot);

        // Assert - No exception thrown means success
    }

    [Fact]
    public void EnsureValid_WithInconsistentSnapshot_ThrowsArgumentException()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 30, // Inconsistent
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => BurndownSnapshotValidation.EnsureValid(snapshot));
        Assert.Contains("Completed story points plus remaining story points must equal total story points", exception.Message);
    }

    [Fact]
    public void GetValidationErrors_WithValidSnapshot_ReturnsEmptyList()
    {
        // Arrange
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = 1,
            TotalStoryPoints = 50,
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 25,
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 10,
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.GetValidationErrors(snapshot);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void GetValidationErrors_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange - Multiple validation errors
        var snapshot = new BurndownSnapshot
        {
            Timestamp = DateTime.UtcNow,
            SprintId = -1, // Negative ID
            TotalStoryPoints = 0, // Zero total
            CompletedStoryPoints = 25,
            RemainingStoryPoints = 30, // Inconsistent
            TotalIssueCount = 20,
            CompletedIssueCount = 10,
            RemainingIssueCount = 15, // Inconsistent
            ScopeChanges = 0
        };

        // Act
        var errors = BurndownSnapshotValidation.GetValidationErrors(snapshot);

        // Assert
        Assert.NotEmpty(errors);
        // Should have 5 errors: Sprint ID, Total story points, story points consistency, issue count consistency, completed exceeds total
        Assert.Equal(5, errors.Count);
        Assert.Contains(errors, e => e.Contains("Sprint ID must be a positive integer"));
        Assert.Contains(errors, e => e.Contains("Total story points must be a positive integer"));
        Assert.Contains(errors, e => e.Contains("Completed story points plus remaining story points"));
        Assert.Contains(errors, e => e.Contains("Completed issue count plus remaining issue count"));
        Assert.Contains(errors, e => e.Contains("Completed story points cannot exceed total story points"));
    }

    [Fact]
    public void Validate_WithNullSnapshot_ThrowsArgumentNullException()
    {
        // Arrange
        BurndownSnapshot? snapshot = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BurndownSnapshotValidation.Validate(snapshot!));
        Assert.Throws<ArgumentNullException>(() => BurndownSnapshotValidation.IsValid(snapshot!));
        Assert.Throws<ArgumentNullException>(() => BurndownSnapshotValidation.GetValidationErrors(snapshot!));
        Assert.Throws<ArgumentNullException>(() => BurndownSnapshotValidation.EnsureValid(snapshot!));
    }
}
