using System;
using System.Collections.Generic;
using System.Globalization;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Provides validation helpers for <see cref="ReportService"/> instances.
/// </summary>
public static class ReportServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="ReportService"/> instance.
    /// </summary>
    /// <param name="value">The report service instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ReportService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ReportService"/> instance is valid.
    /// </summary>
    /// <param name="value">The report service instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ReportService value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ReportService"/> instance is valid.
    /// </summary>
    /// <param name="value">The report service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ReportService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "The ReportService instance is invalid. Details: " + string.Join(" ", errors),
                nameof(value));
        }
    }
}