// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation methods for <see cref="HtmlReportServiceTests"/> instances.
/// </summary>
namespace JiraAnalyticsCli.Tests.Services;

public static class HtmlReportServiceTestsValidation
{
    /// <summary>
    /// Validates the <see cref="HtmlReportServiceTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HtmlReportServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate private fields are initialized using direct field access via reflection
        // This is more reliable than checking field values directly since the constructor
        // initializes these fields
        try
        {
            var analyticsField = value.GetType()
                .GetField("_analyticsMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loggerField = value.GetType()
                .GetField("_loggerMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sutField = value.GetType()
                .GetField("_sut", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (analyticsField?.GetValue(value) is not object)
            {
                problems.Add("Private field '_analyticsMock' is null");
            }

            if (loggerField?.GetValue(value) is not object)
            {
                problems.Add("Private field '_loggerMock' is null");
            }

            if (sutField?.GetValue(value) is not object)
            {
                problems.Add("Private field '_sut' is null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to validate private fields: {ex.Message}");
        }

        // Validate required public methods exist using nameof for compile-time safety
        var requiredMethods = new[]
        {
            nameof(HtmlReportServiceTests.BuildHtml_WithSprintData_ContainsProjectKeyInTitle),
            nameof(HtmlReportServiceTests.BuildHtml_WithXssCharsInProjectKey_EscapesHtml),
            nameof(HtmlReportServiceTests.BuildHtml_WithNoSprints_StillProducesValidDocument),
            nameof(HtmlReportServiceTests.BuildHtml_WithTopPerformers_IncludesPerformerTable),
            nameof(HtmlReportServiceTests.GenerateReportAsync_WithInvalidSprintCount_ThrowsArgumentOutOfRangeException),
            nameof(HtmlReportServiceTests.GenerateReportAsync_WritesFileWithHtmlContent)
        };

        foreach (var methodName in requiredMethods)
        {
            if (typeof(HtmlReportServiceTests).GetMethod(methodName) is null)
            {
                problems.Add($"Public method '{methodName}' not found");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="HtmlReportServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this HtmlReportServiceTests value) =>
        Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the <see cref="HtmlReportServiceTests"/> instance is valid, throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this HtmlReportServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HtmlReportServiceTests instance is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}