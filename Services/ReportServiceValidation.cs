using System;
using System.Collections.Generic;
using System.Globalization;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Provides validation helpers for <see cref="ReportService"/> instances and their method parameters.
/// Validates constructor dependencies and method parameters to ensure non-null, non-empty,
/// and within reasonable bounds before report generation.
/// </summary>
public static class ReportServiceValidation
{
	/// <summary>
	/// Validates the specified <see cref="ReportService"/> instance.
	/// Note: Dependency validation is handled by the DI container; this only checks for null instance.
	/// </summary>
	/// <param name="value">The report service instance to validate.</param>
	/// <returns>A list of validation errors; empty if valid.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this ReportService? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		// Instance is valid if not null (dependencies are validated by DI container)
		return Array.Empty<string>();
	}

	/// <summary>
	/// Determines whether the specified <see cref="ReportService"/> instance is valid.
	/// </summary>
	/// <param name="value">The report service instance to check.</param>
	/// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
	public static bool IsValid(this ReportService? value)
	{
		return value?.Validate().Count == 0;
	}

	/// <summary>
	/// Ensures that the specified <see cref="ReportService"/> instance is valid, throwing an exception
	/// with detailed validation messages if it is not.
	/// </summary>
	/// <param name="value">The report service instance to validate.</param>
	/// <exception cref="ArgumentException">Thrown if the instance is null or has validation problems</exception>
	public static void EnsureValid(this ReportService? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = value.Validate();

		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"ReportService instance is invalid. Problems: {string.Join("; ", problems)}");
		}
	}

	/// <summary>
	/// Validates the <see cref="SprintAnalysisResult"/> parameter used in report generation methods.
	/// Ensures the analysis data is suitable for report generation.
	/// </summary>
	/// <param name="analysis">The sprint analysis result to validate.</param>
	/// <returns>A list of validation problems (empty if valid).</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="analysis"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this SprintAnalysisResult? analysis)
	{
		ArgumentNullException.ThrowIfNull(analysis);

		var problems = new List<string>();

		if (analysis.Metrics is null)
		{
			problems.Add("SprintAnalysisResult.Metrics collection cannot be null");
		}
		else if (analysis.Metrics.Count == 0)
		{
			problems.Add("SprintAnalysisResult.Metrics collection cannot be empty");
		}
		else
		{
			// Validate each metric
			for (var i = 0; i < analysis.Metrics.Count; i++)
			{
				var metric = analysis.Metrics[i];
				if (metric is null)
				{
					problems.Add($"SprintAnalysisResult.Metrics[{i}] cannot be null");
				}
			}
		}

		// Validate AverageVelocity
		if (double.IsNaN(analysis.AverageVelocity))
		{
			problems.Add("SprintAnalysisResult.AverageVelocity cannot be NaN");
		}
		else if (double.IsInfinity(analysis.AverageVelocity))
		{
			problems.Add("SprintAnalysisResult.AverageVelocity cannot be infinite");
		}
		else if (analysis.AverageVelocity < 0)
		{
			problems.Add($"SprintAnalysisResult.AverageVelocity cannot be negative (was {analysis.AverageVelocity.ToString(CultureInfo.InvariantCulture)})");
		}

		// Validate TrendPercentage
		if (double.IsNaN(analysis.TrendPercentage))
		{
			problems.Add("SprintAnalysisResult.TrendPercentage cannot be NaN");
		}
		else if (double.IsInfinity(analysis.TrendPercentage))
		{
			problems.Add("SprintAnalysisResult.TrendPercentage cannot be infinite");
		}

		// Validate OverallHealth
		if (string.IsNullOrWhiteSpace(analysis.OverallHealth))
		{
			problems.Add("SprintAnalysisResult.OverallHealth cannot be null, empty, or whitespace");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates the <see cref="TeamAnalysisResult"/> parameter used in HTML report generation.
	/// Ensures the team analysis data is suitable for report generation.
	/// </summary>
	/// <param name="teamAnalysis">The team analysis result to validate.</param>
	/// <returns>A list of validation problems (empty if valid).</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="teamAnalysis"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this TeamAnalysisResult? teamAnalysis)
	{
		ArgumentNullException.ThrowIfNull(teamAnalysis);

		var problems = new List<string>();

		if (teamAnalysis.TopPerformers is null)
		{
			problems.Add("TeamAnalysisResult.TopPerformers collection cannot be null");
		}
		else if (teamAnalysis.TopPerformers.Count == 0)
		{
			// Empty top performers is acceptable (no data available)
		}

		if (teamAnalysis.LowPerformers is null)
		{
			problems.Add("TeamAnalysisResult.LowPerformers collection cannot be null");
		}

		if (teamAnalysis.WorkloadDistribution is null)
		{
			problems.Add("TeamAnalysisResult.WorkloadDistribution dictionary cannot be null");
		}

		// Validate AverageProductivity
		if (double.IsNaN(teamAnalysis.AverageProductivity))
		{
			problems.Add("TeamAnalysisResult.AverageProductivity cannot be NaN");
		}
		else if (double.IsInfinity(teamAnalysis.AverageProductivity))
		{
			problems.Add("TeamAnalysisResult.AverageProductivity cannot be infinite");
		}
		else if (teamAnalysis.AverageProductivity < 0)
		{
			problems.Add($"TeamAnalysisResult.AverageProductivity cannot be negative (was {teamAnalysis.AverageProductivity.ToString(CultureInfo.InvariantCulture)})");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates a project key parameter for report generation methods.
	/// Project keys should be non-null, non-empty strings containing only alphanumeric characters and hyphens.
	/// </summary>
	/// <param name="projectKey">The project key to validate.</param>
	/// <returns>A list of validation problems (empty if valid).</returns>
	public static IReadOnlyList<string> ValidateProjectKey(string? projectKey)
	{
		var problems = new List<string>();

		if (string.IsNullOrWhiteSpace(projectKey))
		{
			problems.Add("Project key cannot be null, empty, or whitespace");
			return problems.AsReadOnly();
		}

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
	/// Validates a sprint ID parameter for burndown chart generation.
	/// Sprint IDs should be positive integers representing valid Jira sprint identifiers.
	/// </summary>
	/// <param name="sprintId">The sprint ID to validate.</param>
	/// <returns>A list of validation problems (empty if valid).</returns>
	public static IReadOnlyList<string> ValidateSprintId(int sprintId)
	{
		var problems = new List<string>();

		if (sprintId <= 0)
		{
			problems.Add("Sprint ID must be a positive integer");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates an output file path parameter for report generation.
	/// Paths should be non-null, non-empty strings with valid directory and file name components.
	/// </summary>
	/// <param name="outputPath">The output path to validate.</param>
	/// <returns>A list of validation problems (empty if valid).</returns>
	public static IReadOnlyList<string> ValidateOutputPath(string? outputPath)
	{
		var problems = new List<string>();

		if (string.IsNullOrWhiteSpace(outputPath))
		{
			problems.Add("Output path cannot be null, empty, or whitespace");
			return problems.AsReadOnly();
		}

		var path = outputPath.Trim();

		// Check for invalid path characters (platform-independent)
		var invalidChars = System.IO.Path.GetInvalidPathChars();
		if (path.IndexOfAny(invalidChars) >= 0)
		{
			problems.Add("Output path contains invalid characters");
		}

		// Check for invalid file name characters
		var fileName = System.IO.Path.GetFileName(path);
		if (string.IsNullOrEmpty(fileName))
		{
			problems.Add("Output path must include a file name");
		}
		else
		{
			var invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
			if (fileName.IndexOfAny(invalidFileNameChars) >= 0)
			{
				problems.Add("Output file name contains invalid characters");
			}
		}

		// Check path length (reasonable limit for file systems)
		if (path.Length > 260)
		{
			problems.Add("Output path is too long (maximum 260 characters)");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Validates a format parameter for report generation.
	/// Format should be a non-null, non-empty string representing a supported export format.
	/// </summary>
	/// <param name="format">The format to validate.</param>
	/// <returns>A list of validation problems (empty if valid).</returns>
	public static IReadOnlyList<string> ValidateFormat(string? format)
	{
		var problems = new List<string>();

		if (string.IsNullOrWhiteSpace(format))
		{
			problems.Add("Format cannot be null, empty, or whitespace");
			return problems.AsReadOnly();
		}

		var normalized = format.Trim().ToLowerInvariant();
		if (normalized != "png" && normalized != "html" && normalized != "txt" && normalized != "csv")
		{
			problems.Add("Format must be one of: png, html, txt, csv");
		}

		return problems.AsReadOnly();
	}
}