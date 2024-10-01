using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace JiraAnalyticsCli.Benchmarks
{
    public static class CsvFormatterBenchmarksExtensions
    {
        /// <summary>
        /// Formats the metrics with custom header and ensures consistent column ordering
        /// </summary>
        public static string FormatWithHeaders(this CsvFormatterBenchmarks formatter, string customHeader, bool includeHeader = true)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            if (string.IsNullOrWhiteSpace(customHeader))
                throw new ArgumentException("Header cannot be null or whitespace", nameof(customHeader));

            var formatted = formatter.Format();

            if (!includeHeader)
                return formatted;

            using var reader = new StringReader(formatted);
            var lines = new List<string>();
            string? line;

            // Read and replace the header line
            if ((line = reader.ReadLine()) != null)
            {
                // Parse existing header to get column names
                var existingColumns = line.Split(',').Select(c => c.Trim('"')).ToList();

                // Create new header with custom name but same column structure
                var headerParts = customHeader.Split(',').Select(h => h.Trim()).ToList();
                if (headerParts.Count != existingColumns.Count)
                    throw new InvalidOperationException($"Custom header must have {existingColumns.Count} columns");

                var newHeader = string.Join(",", headerParts.Select(h => $"\"{h}\""));
                lines.Add(newHeader);
            }

            // Add all data lines
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Parses CSV content and returns metrics grouped by sprint name
        /// </summary>
        public static Dictionary<string, List<SprintMetric>> ParseGroupedBySprint(this CsvFormatterBenchmarks formatter, string csvContent)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            if (string.IsNullOrWhiteSpace(csvContent))
                throw new ArgumentException("CSV content cannot be null or whitespace", nameof(csvContent));

            var allMetrics = formatter.Parse(csvContent);

            return allMetrics
                .GroupBy(m => m.SprintName)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Validates that all metrics have valid numeric values
        /// </summary>
        public static bool ValidateMetricsIntegrity(this CsvFormatterBenchmarks formatter, List<SprintMetric> metrics)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            if (metrics == null || metrics.Count == 0)
                return false;

            return metrics.All(m =>
                m.StoryPoints >= 0 &&
                m.CompletedIssues >= 0 &&
                m.TotalIssues >= 0 &&
                m.Velocity >= 0 &&
                !string.IsNullOrWhiteSpace(m.SprintName));
        }

        /// <summary>
        /// Formats metrics as CSV with additional calculated fields
        /// </summary>
        public static string FormatWithCalculations(this CsvFormatterBenchmarks formatter, bool includeCompletionRate = true)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            formatter.Setup();
            var metrics = formatter.Parse(formatter.Format());

            var lines = new List<string>();

            // Header
            var headerParts = new List<string> { "SprintName", "StoryPoints", "CompletedIssues", "TotalIssues", "Velocity" };
            if (includeCompletionRate)
                headerParts.Add("CompletionRate");

            lines.Add(string.Join(",", headerParts.Select(h => $"\"{h}\"")));

            // Data rows
            foreach (var metric in metrics)
            {
                var rowParts = new List<string>
                {
                    $"\"{metric.SprintName}\"",
                    metric.StoryPoints.ToString(CultureInfo.InvariantCulture),
                    metric.CompletedIssues.ToString(CultureInfo.InvariantCulture),
                    metric.TotalIssues.ToString(CultureInfo.InvariantCulture),
                    metric.Velocity.ToString(CultureInfo.InvariantCulture)
                };

                if (includeCompletionRate && metric.TotalIssues > 0)
                {
                    var completionRate = (double)metric.CompletedIssues / metric.TotalIssues * 100;
                    rowParts.Add($"{completionRate:F2}%");
                }

                lines.Add(string.Join(",", rowParts));
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}