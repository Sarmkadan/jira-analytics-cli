// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the TeamComparisonService class.
/// </summary>
public class TeamComparisonServiceTests
{
    private readonly Mock<IAnalyticsService> _analyticsMock;
    private readonly Mock<ILogger<TeamComparisonService>> _loggerMock;
    private readonly TeamComparisonService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamComparisonServiceTests"/> class.
    /// </summary>
    public TeamComparisonServiceTests()
    {
        _analyticsMock = new Mock<IAnalyticsService>();
        _loggerMock    = new Mock<ILogger<TeamComparisonService>>();
        _sut           = new TeamComparisonService(_analyticsMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Builds a SprintAnalysisResult object with the specified velocity, health, and defects.
    /// </summary>
    /// <param name="velocity">The average velocity of the sprint.</param>
    /// <param name="health">The overall health of the sprint.</param>
    /// <param name="defects">The number of defects in the sprint.</param>
    /// <returns>A SprintAnalysisResult object with the specified properties.</returns>
    private static SprintAnalysisResult BuildAnalysis(double velocity, string health, int defects)
    {
        return new SprintAnalysisResult
        {
            AverageVelocity = velocity,
            OverallHealth   = health,
            Metrics = new List<SprintMetric>
            {
                new()
                {
                    SprintId             = 1,
                    SprintName           = "Sprint 1",
                    StartDate            = DateTime.UtcNow.AddDays(-14),
                    EndDate              = DateTime.UtcNow,
                    PlannedStoryPoints   = 50,
                    CompletedStoryPoints = (int)velocity,
                    TotalIssueCount      = 10,
                    CompletedIssueCount  = 8,
                    DefectsCount         = defects,
                    AverageCycleTime     = 2.5
                }
            }
        };
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method returns both snapshots when given two projects.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("ALPHA", 5)).ReturnsAsync(BuildAnalysis(60, "Excellent", 1));
        _analyticsMock.Setup(a => a.AnalyzeSprints("BETA",  5)).ReturnsAsync(BuildAnalysis(40, "Healthy",   3));

        var report = await _sut.CompareTeamsAsync(new[] { "ALPHA", "BETA" });

        report.Teams.Should().HaveCount(2);
        report.Teams.Select(t => t.ProjectKey).Should().Contain(new[] { "ALPHA", "BETA" });
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method identifies the fastest team correctly.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_IdentifiesFastestTeamCorrectly()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("SLOW", 5)).ReturnsAsync(BuildAnalysis(20, "At Risk", 5));
        _analyticsMock.Setup(a => a.AnalyzeSprints("FAST", 5)).ReturnsAsync(BuildAnalysis(80, "Excellent", 1));

        var report = await _sut.CompareTeamsAsync(new[] { "SLOW", "FAST" });

        report.FastestTeam.Should().Be("FAST");
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method identifies the highest quality team by lowest defect rate.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_IdentifiesHighestQualityTeamByLowestDefectRate()
    {
        _analyticsMock
            .Setup(a => a.AnalyzeSprints("LOWQUAL", 5))
            .ReturnsAsync(BuildAnalysis(50, "At Risk", 8));   // 8 defects / 10 total = 80%
        _analyticsMock
            .Setup(a => a.AnalyzeSprints("HIGHQUAL", 5))
            .ReturnsAsync(BuildAnalysis(50, "Excellent", 0)); // 0 defects = 0%

        var report = await _sut.CompareTeamsAsync(new[] { "LOWQUAL", "HIGHQUAL" });

        report.HighestQualityTeam.Should().Be("HIGHQUAL");
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method deduplicates project keys.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_DeduplicatesProjectKeys()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("PROJ", 5)).ReturnsAsync(BuildAnalysis(30, "Healthy", 2));

        var report = await _sut.CompareTeamsAsync(new[] { "PROJ", "proj", "PROJ" });

        // All three are the same key — deduplication should produce a single entry
        _analyticsMock.Verify(a => a.AnalyzeSprints("PROJ", 5), Times.Once);
        report.Teams.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method throws an ArgumentException when given an empty array of project keys.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.CompareTeamsAsync(Array.Empty<string>()));
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method throws an ArgumentOutOfRangeException when given a sprint count of zero.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.CompareTeamsAsync(new[] { "PROJ" }, sprintCount: 0));
    }

    /// <summary>
    /// Tests that the FormatAsText method returns a string containing both project keys when given a report with two teams.
    /// </summary>
    [Fact]
    public void FormatAsText_WithTwoTeams_ContainsBothProjectKeys()
    {
        var report = new TeamComparisonReport
        {
            FastestTeam        = "ALPHA",
            HighestQualityTeam = "BETA",
            MostConsistentTeam = "ALPHA",
            Teams = new List<TeamProjectSnapshot>
            {
                new() { ProjectKey = "ALPHA", AverageVelocity = 60, AvgCompletionRate = 90, TotalPointsDelivered = 300, TotalDefects = 2, DefectRate = 1.5, AvgCycleTime = 2.1, OverallHealth = "Excellent" },
                new() { ProjectKey = "BETA",  AverageVelocity = 40, AvgCompletionRate = 80, TotalPointsDelivered = 200, TotalDefects = 8, DefectRate = 6.0, AvgCycleTime = 3.5, OverallHealth = "At Risk" }
            }
        };

        var text = TeamComparisonService.FormatAsText(report);

        text.Should().Contain("ALPHA");
        text.Should().Contain("BETA");
        text.Should().Contain("Fastest team");
        text.Should().Contain("Highest quality");
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method handles identical teams by selecting the first one.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_IdenticalTeams_Tie()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("TEAM1", 5)).ReturnsAsync(BuildAnalysis(50, "Healthy", 0));
        _analyticsMock.Setup(a => a.AnalyzeSprints("TEAM2", 5)).ReturnsAsync(BuildAnalysis(50, "Healthy", 0));

        var report = await _sut.CompareTeamsAsync(new[] { "TEAM1", "TEAM2" });

        report.Teams.Should().HaveCount(2);
        // MaxBy returns the first element if multiple have the same maximum value.
        report.FastestTeam.Should().Be("TEAM1");
    }

    /// <summary>
    /// Tests that the CompareTeamsAsync method handles a project with no metrics without throwing divide-by-zero exceptions.
    /// </summary>
    [Fact]
    public async Task CompareTeamsAsync_ProjectWithNoMetrics_HandledWithoutException()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("EMPTY", 5)).ReturnsAsync(new SprintAnalysisResult { Metrics = new List<SprintMetric>() });

        var report = await _sut.CompareTeamsAsync(new[] { "EMPTY" });

        report.Teams.Should().HaveCount(1);
        report.Teams.First().ProjectKey.Should().Be("EMPTY");
        report.Teams.First().OverallHealth.Should().Be("Unknown");
    }
}
