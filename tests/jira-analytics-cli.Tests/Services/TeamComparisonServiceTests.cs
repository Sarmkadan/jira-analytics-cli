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

namespace JiraAnalyticsCli.Tests.Services;

public class TeamComparisonServiceTests
{
    private readonly Mock<IAnalyticsService> _analyticsMock;
    private readonly Mock<ILogger<TeamComparisonService>> _loggerMock;
    private readonly TeamComparisonService _sut;

    public TeamComparisonServiceTests()
    {
        _analyticsMock = new Mock<IAnalyticsService>();
        _loggerMock    = new Mock<ILogger<TeamComparisonService>>();
        _sut           = new TeamComparisonService(_analyticsMock.Object, _loggerMock.Object);
    }

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

    [Fact]
    public async Task CompareTeamsAsync_WithTwoProjects_ReturnsBothSnapshots()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("ALPHA", 5)).ReturnsAsync(BuildAnalysis(60, "Excellent", 1));
        _analyticsMock.Setup(a => a.AnalyzeSprints("BETA",  5)).ReturnsAsync(BuildAnalysis(40, "Healthy",   3));

        var report = await _sut.CompareTeamsAsync(new[] { "ALPHA", "BETA" });

        report.Teams.Should().HaveCount(2);
        report.Teams.Select(t => t.ProjectKey).Should().Contain(new[] { "ALPHA", "BETA" });
    }

    [Fact]
    public async Task CompareTeamsAsync_IdentifiesFastestTeamCorrectly()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("SLOW", 5)).ReturnsAsync(BuildAnalysis(20, "At Risk", 5));
        _analyticsMock.Setup(a => a.AnalyzeSprints("FAST", 5)).ReturnsAsync(BuildAnalysis(80, "Excellent", 1));

        var report = await _sut.CompareTeamsAsync(new[] { "SLOW", "FAST" });

        report.FastestTeam.Should().Be("FAST");
    }

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

    [Fact]
    public async Task CompareTeamsAsync_DeduplicatesProjectKeys()
    {
        _analyticsMock.Setup(a => a.AnalyzeSprints("PROJ", 5)).ReturnsAsync(BuildAnalysis(30, "Healthy", 2));

        var report = await _sut.CompareTeamsAsync(new[] { "PROJ", "proj", "PROJ" });

        // All three are the same key — deduplication should produce a single entry
        _analyticsMock.Verify(a => a.AnalyzeSprints("PROJ", 5), Times.Once);
        report.Teams.Should().HaveCount(1);
    }

    [Fact]
    public async Task CompareTeamsAsync_WithEmptyProjectKeys_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.CompareTeamsAsync(Array.Empty<string>()));
    }

    [Fact]
    public async Task CompareTeamsAsync_WithZeroSprintCount_ThrowsArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.CompareTeamsAsync(new[] { "PROJ" }, sprintCount: 0));
    }

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
}
