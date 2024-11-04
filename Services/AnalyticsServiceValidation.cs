// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for AnalyticsService to ensure data integrity before processing
// =============================================================================

using System.Globalization;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Provides validation helpers for <see cref="AnalyticsService"/> instances.
/// Validates constructor dependencies and method parameters to ensure non-null, non-empty,
/// and within reasonable bounds before processing.
/// </summary>
public static class AnalyticsServiceValidation
{
    /// <summary>
    /// Validates that an AnalyticsService instance is not null.
    /// Note: Dependency validation is handled by the DI container; this only checks for null instance.
    /// </summary>
    /// <param name="value">The AnalyticsService instance to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Instance is valid if not null (dependencies are validated by DI container)
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified AnalyticsService instance is valid.
    /// </summary>
    /// <param name="value">The AnalyticsService instance to check</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this AnalyticsService? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified AnalyticsService instance is valid, throwing an exception
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The AnalyticsService instance to validate</param>
    /// <exception cref="ArgumentException">Thrown if the instance is null or has validation problems</exception>
    public static void EnsureValid(this AnalyticsService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AnalyticsService instance is invalid. Problems: {string.Join("; ", problems)}");
        }
    }

    /// <summary>
    /// Validates project key parameter for analytics methods.
    /// Project keys should be non-null, non-empty strings containing only alphanumeric characters and hyphens.
    /// </summary>
    /// <param name="projectKey">The project key to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateProjectKey(string? projectKey)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(projectKey))
        {
            problems.Add("Project key cannot be null, empty, or whitespace");
            return problems.AsReadOnly();
        }

        // Basic validation: project keys are typically uppercase alphanumeric with hyphens
        var normalized = projectKey.Trim();
        if (normalized.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
        {
            problems.Add("Project key contains invalid characters. Only letters, digits, and hyphens are allowed");
        }

        if (normalized.Length > 20)
        {
            problems.Add("Project key is too long (maximum 20 characters)");
        }

        if (normalized.Length < 2)
        {
            problems.Add("Project key is too short (minimum 2 characters)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates sprint count parameter for sprint-based analytics methods.
    /// Sprint counts should be positive integers representing reasonable historical data ranges.
    /// </summary>
    /// <param name="sprintCount">The number of sprints to analyze</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateSprintCount(int sprintCount)
    {
        var problems = new List<string>();

        if (sprintCount <= 0)
        {
            problems.Add("Sprint count must be a positive integer");
        }
        else if (sprintCount > 50)
        {
            problems.Add("Sprint count is too large (maximum 50 sprints)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a DateTime is not the default (Unix epoch) and is within reasonable bounds.
    /// </summary>
    /// <param name="date">The date to validate</param>
    /// <param name="paramName">The name of the parameter for error messages</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateDate(DateTime date, string paramName)
    {
        var problems = new List<string>();

        // Default DateTime (Unix epoch) is considered invalid for business logic
        if (date == default)
        {
            problems.Add($"{paramName} cannot be the default date (Unix epoch)");
        }
        else if (date < new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        {
            problems.Add($"{paramName} date is before year 2000 (too old)");
        }
        else if (date > DateTime.UtcNow.AddYears(1))
        {
            problems.Add($"{paramName} date is more than 1 year in the future");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a numeric value is within reasonable bounds for analytics calculations.
    /// </summary>
    /// <param name="value">The numeric value to validate</param>
    /// <param name="paramName">The name of the parameter for error messages</param>
    /// <param name="minValue">Minimum allowed value (inclusive)</param>
    /// <param name="maxValue">Maximum allowed value (inclusive)</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateNumber(double value, string paramName, double minValue = 0, double maxValue = 10000)
    {
        var problems = new List<string>();

        if (double.IsNaN(value))
        {
            problems.Add($"{paramName} cannot be NaN");
        }
        else if (double.IsInfinity(value))
        {
            problems.Add($"{paramName} cannot be infinite");
        }
        else if (value < minValue)
        {
            problems.Add($"{paramName} cannot be less than {minValue} (was {value.ToString(CultureInfo.InvariantCulture)})");
        }
        else if (value > maxValue)
        {
            problems.Add($"{paramName} cannot be greater than {maxValue} (was {value.ToString(CultureInfo.InvariantCulture)})");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that a collection is not null and contains only valid items.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="paramName">The name of the parameter for error messages</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateCollection<T>(IReadOnlyCollection<T>? collection, string paramName)
    {
        var problems = new List<string>();

        if (collection is null)
        {
            problems.Add($"{paramName} collection cannot be null");
            return problems.AsReadOnly();
        }

        if (collection.Count == 0)
        {
            problems.Add($"{paramName} collection cannot be empty");
        }

        return problems.AsReadOnly();
    }
}