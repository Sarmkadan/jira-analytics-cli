using System;
using System.Threading.Tasks;
using FluentAssertions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace jira_analytics_cli.Tests.Services
{
    public static class TeamComparisonServiceTestsExtensions
    {
        public static async Task<string> GetComparisonResultTextAsync(this TeamComparisonServiceTests tests, string projectKey1, string projectKey2, int sprintCount = 10)
        {
            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            return TeamComparisonService.FormatAsText(result);
        }

        public static async Task VerifyTeamComparisonSnapshot(this TeamComparisonServiceTests tests, string projectKey1, string projectKey2, int sprintCount = 10)
        {
            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            result.Should().NotBeNull();
            result.Teams.Should().HaveCount(2);
            result.Teams[0].ProjectKey.Should().Be(projectKey1);
            result.Teams[1].ProjectKey.Should().Be(projectKey2);
        }

        public static async Task VerifyFastestTeamIdentified(this TeamComparisonServiceTests tests, string projectKey1, string projectKey2, int sprintCount = 10)
        {
            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            result.Should().NotBeNull();
            result.FastestTeam.Should().Be(projectKey1);
        }

        public static async Task VerifyHighestQualityTeamIdentified(this TeamComparisonServiceTests tests, string projectKey1, string projectKey2, int sprintCount = 10)
        {
            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            result.Should().NotBeNull();
            result.HighestQualityTeam.Should().Be(projectKey2);
        }

        private static TeamComparisonService CreateService(TeamComparisonServiceTests tests)
        {
            var analyticsMock = new Mock<IAnalyticsService>();
            var loggerMock = new Mock<ILogger<TeamComparisonService>>();
            return new TeamComparisonService(analyticsMock.Object, loggerMock.Object);
        }
    }
}