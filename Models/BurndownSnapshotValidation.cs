// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace JiraAnalyticsCli.Models;

/// <summary>
/// Provides validation helpers for <see cref="BurndownSnapshot"/> instances
/// </summary>
public static class BurndownSnapshotValidation
{
    /// <summary>
    /// Validates the burndown snapshot using the instance Validate() method.
    /// </summary>
    /// <param name="value">The burndown snapshot to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this BurndownSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);
        try
        {
            value.Validate();
            return Array.Empty<string>();
        }
        catch (ArgumentException ex)
        {
            return new[] { ex.Message };
        }
    }


    /// <summary>
    /// Determines whether the specified burndown snapshot is valid.
    /// </summary>
    /// <param name="value">The burndown snapshot to check</param>
    /// <returns>True if the snapshot is valid; otherwise, false</returns>
    public static bool IsValid(this BurndownSnapshot value)
    {
        return value is not null && value.GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Validates the specified burndown snapshot and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The burndown snapshot to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> GetValidationErrors(this BurndownSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate SprintId
        if (value.SprintId <= 0)
        {
            errors.Add("Sprint ID must be a positive integer");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a valid DateTime");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddHours(1))
        {
            errors.Add("Timestamp cannot be in the future");
        }

        // Validate story points
        if (value.RemainingStoryPoints < 0)
        {
            errors.Add("Remaining story points cannot be negative");
        }

        if (value.CompletedStoryPoints < 0)
        {
            errors.Add("Completed story points cannot be negative");
        }

        if (value.TotalStoryPoints <= 0)
        {
            errors.Add("Total story points must be a positive integer");
        }
        else if (value.TotalStoryPoints > 1000000)
        {
            errors.Add("Total story points value is unreasonably large");
        }

        // Validate story points consistency
        if (value.CompletedStoryPoints + value.RemainingStoryPoints != value.TotalStoryPoints)
        {
            errors.Add("Completed story points plus remaining story points must equal total story points");
        }

        // Validate issue counts
        if (value.RemainingIssueCount < 0)
        {
            errors.Add("Remaining issue count cannot be negative");
        }

        if (value.CompletedIssueCount < 0)
        {
            errors.Add("Completed issue count cannot be negative");
        }

        if (value.TotalIssueCount <= 0)
        {
            errors.Add("Total issue count must be a positive integer");
        }
        else if (value.TotalIssueCount > 1000000)
        {
            errors.Add("Total issue count value is unreasonably large");
        }

        // Validate issue count consistency
        if (value.CompletedIssueCount + value.RemainingIssueCount != value.TotalIssueCount)
        {
            errors.Add("Completed issue count plus remaining issue count must equal total issue count");
        }

        // Validate scope changes
        if (value.ScopeChanges < -1000 || value.ScopeChanges > 1000)
        {
            errors.Add("Scope changes must be between -1000 and 1000");
        }

        // Validate that completed doesn't exceed total
        if (value.CompletedStoryPoints > value.TotalStoryPoints)
        {
            errors.Add("Completed story points cannot exceed total story points");
        }

        if (value.CompletedIssueCount > value.TotalIssueCount)
        {
            errors.Add("Completed issue count cannot exceed total issue count");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the specified burndown snapshot and throws an exception if invalid.
    /// </summary>
    /// <param name="value">The burndown snapshot to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the snapshot contains validation errors</exception>
    public static void EnsureValid(this BurndownSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.GetValidationErrors();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BurndownSnapshot validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)
                }");
        }
    }
}