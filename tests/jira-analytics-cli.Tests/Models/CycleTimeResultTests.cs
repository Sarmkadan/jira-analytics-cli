using System.Globalization;
using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Verifies the one-line <c>ToString()</c> summary of <see cref="CycleTimeResult"/>.
/// </summary>
public class CycleTimeResultTests
{
    [Fact]
    public void ToString_ShowsProjectKeyMetricsAndIssueCount()
    {
        var result = new CycleTimeResult
        {
            ProjectKey = "PROJ",
            AverageCycleTime = 4.0,
            MedianCycleTime = 3.5,
            P50CycleTime = 3.5,
            P75CycleTime = 6.0,
            P90CycleTime = 9.0,
            IssueCycleTimes = new List<IssueCycleTime>
            {
                new() { IssueKey = "PROJ-1", CycleTimeDays = 2 },
                new() { IssueKey = "PROJ-2", CycleTimeDays = 3.5 },
                new() { IssueKey = "PROJ-3", CycleTimeDays = 6 }
            }
        };

        result.ToString().Should().Be("PROJ: avg 4.0d, median 3.5d, p75 6.0d, p90 9.0d (3 issues)");
    }

    [Fact]
    public void ToString_WithNoIssues_ReportsZeroIssues()
    {
        var result = new CycleTimeResult { ProjectKey = "EMPTY" };

        result.ToString().Should().Be("EMPTY: avg 0.0d, median 0.0d, p75 0.0d, p90 0.0d (0 issues)");
    }

    [Fact]
    public void ToString_UsesInvariantCulture_WhenCurrentCultureUsesComma()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var result = new CycleTimeResult
            {
                ProjectKey = "PROJ",
                AverageCycleTime = 4.5,
                MedianCycleTime = 4.5,
                P75CycleTime = 6.0,
                P90CycleTime = 9.0
            };

            result.ToString().Should().StartWith("PROJ: avg 4.5d, median 4.5d");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
