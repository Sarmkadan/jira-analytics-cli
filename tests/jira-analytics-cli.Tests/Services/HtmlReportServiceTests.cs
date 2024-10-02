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
/// Tests for the HtmlReportService class.
/// </summary>
namespace JiraAnalyticsCli.Tests.Services;

public class HtmlReportServiceTests
{
    private readonly Mock<IAnalyticsService> _analyticsMock;
    private readonly Mock<ILogger<HtmlReportService>> _loggerMock;
    private readonly HtmlReportService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlReportServiceTests"/> class.
    /// </summary>
    public HtmlReportServiceTests()
    {
        _analyticsMock = new Mock<IAnalyticsService>();
        _loggerMock    = new Mock<ILogger<HtmlReportService>>();
        _sut           = new HtmlReportService(_analyticsMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that the BuildHtml method produces HTML that contains the project key in the title.
    /// </summary>
    [Fact]
    public void BuildHtml_WithSprintData_ContainsProjectKeyInTitle()
    {
        var sprintResult = new SprintAnalysisResult
        {
            AverageVelocity  = 42.5,
            TrendPercentage  = 5.2,
            OverallHealth    = "Healthy",
            Metrics = new List<SprintMetric>
            {
                new()
                {
                    SprintId              = 1,
                    SprintName            = "Sprint 1",
                    StartDate             = DateTime.UtcNow.AddDays(-28),
                    EndDate               = DateTime.UtcNow.AddDays(-14),
                    PlannedStoryPoints    = 40,
                    CompletedStoryPoints  = 38,
                    CompletedIssueCount   = 10,
                    TotalIssueCount       = 11,
                    DefectsCount          = 1,
                    AverageCycleTime      = 3.2
                }
            }
        };
        var teamResult = new TeamAnalysisResult
        {
            WorkloadDistribution = new Dictionary<string, int> { { "alice", 5 }, { "bob", 3 } }
        };

        var html = _sut.BuildHtml("MYPROJ", sprintResult, teamResult);

        html.Should().Contain("MYPROJ");
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("Sprint 1");
        html.Should().Contain("42.5");
    }

    /// <summary>
    /// Verifies that the BuildHtml method escapes HTML characters in the project key.
    /// </summary>
    [Fact]
    public void BuildHtml_WithXssCharsInProjectKey_EscapesHtml()
    {
        var sprintResult = new SprintAnalysisResult { OverallHealth = "Healthy" };
        var teamResult   = new TeamAnalysisResult();

        var html = _sut.BuildHtml("<script>alert(1)</script>", sprintResult, teamResult);

        html.Should().NotContain("<script>alert(1)</script>");
        html.Should().Contain("&lt;script&gt;");
    }

    /// <summary>
    /// Verifies that the BuildHtml method produces a valid HTML document even when there are no sprints.
    /// </summary>
    [Fact]
    public void BuildHtml_WithNoSprints_StillProducesValidDocument()
    {
        var sprintResult = new SprintAnalysisResult
        {
            AverageVelocity = 0,
            OverallHealth   = "Unknown"
        };
        var teamResult = new TeamAnalysisResult();

        var html = _sut.BuildHtml("EMPTY", sprintResult, teamResult);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("</html>");
        html.Should().Contain("EMPTY");
    }

    /// <summary>
    /// Verifies that the BuildHtml method includes the top performers table in the HTML.
    /// </summary>
    [Fact]
    public void BuildHtml_WithTopPerformers_IncludesPerformerTable()
    {
        var dev = new Developer
        {
            Key         = "jdoe",
            Name        = "jdoe",
            DisplayName = "Jane Doe",
            JoinDate    = DateTime.UtcNow.AddDays(-90)
        };
        dev.AssignIssue(new JiraIssue
        {
            Key = "P-1", Id = "1", Summary = "Task A",
            Status = "Done", IssueType = "Task", Priority = "Medium",
            CreatedDate = DateTime.UtcNow.AddDays(-10),
            ResolutionDate = DateTime.UtcNow.AddDays(-1),
            StoryPoints = 5
        });

        var sprintResult = new SprintAnalysisResult { OverallHealth = "Healthy" };
        var teamResult   = new TeamAnalysisResult
        {
            TopPerformers = new List<Developer> { dev }
        };

        var html = _sut.BuildHtml("PROJ", sprintResult, teamResult);

        html.Should().Contain("Jane Doe");
        html.Should().Contain("Top Performers");
    }

    /// <summary>
    /// Verifies that the GenerateReportAsync method throws an ArgumentOutOfRangeException when the sprint count is invalid.
    /// </summary>
    [Fact]
    public async Task GenerateReportAsync_WithInvalidSprintCount_ThrowsArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.GenerateReportAsync("PROJ", 0, "/tmp/report.html"));
    }

    /// <summary>
    /// Verifies that the GenerateReportAsync method writes the report to the specified file.
    /// </summary>
    [Fact]
    public async Task GenerateReportAsync_WritesFileWithHtmlContent()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            _analyticsMock
                .Setup(a => a.AnalyzeSprints("PROJ", 3))
                .ReturnsAsync(new SprintAnalysisResult { OverallHealth = "Healthy" });

            _analyticsMock
                .Setup(a => a.AnalyzeTeam("PROJ"))
                .ReturnsAsync(new TeamAnalysisResult());

            await _sut.GenerateReportAsync("PROJ", 3, tempFile);

            var content = await File.ReadAllTextAsync(tempFile);
            content.Should().Contain("<!DOCTYPE html>");
            content.Should().Contain("PROJ");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
