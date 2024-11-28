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
    /// <summary>
    /// Extension methods for testing <see cref="TeamComparisonService"/> functionality.
    /// </summary>
    public static class TeamComparisonServiceTestsExtensions
    {
        /// <summary>
        /// Gets the comparison result as formatted text.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <param name="projectKey1">The first project key to compare.</param>
        /// <param name="projectKey2">The second project key to compare.</param>
        /// <param name="sprintCount">The number of sprints to analyze. Must be greater than zero.</param>
        /// <returns>The formatted comparison result text produced by <see cref="TeamComparisonService.FormatAsText"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sprintCount"/> is less than or equal to zero.</exception>
        public static async Task<string> GetComparisonResultTextAsync(
            this TeamComparisonServiceTests tests,
            string projectKey1,
            string projectKey2,
            int sprintCount = 10)
        {
            ArgumentException.ThrowIfNullOrEmpty(projectKey1);
            ArgumentException.ThrowIfNullOrEmpty(projectKey2);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sprintCount, 0);

            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            return TeamComparisonService.FormatAsText(result);
        }

        /// <summary>
        /// Verifies that team comparison produces the expected snapshot structure.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <param name="projectKey1">The first project key to compare.</param>
        /// <param name="projectKey2">The second project key to compare.</param>
        /// <param name="sprintCount">The number of sprints to analyze. Must be greater than zero.</param>
        /// <returns>A task that completes when the snapshot assertions have been executed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sprintCount"/> is less than or equal to zero.</exception>
        public static async Task VerifyTeamComparisonSnapshot(
            this TeamComparisonServiceTests tests,
            string projectKey1,
            string projectKey2,
            int sprintCount = 10)
        {
            ArgumentException.ThrowIfNullOrEmpty(projectKey1);
            ArgumentException.ThrowIfNullOrEmpty(projectKey2);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sprintCount, 0);

            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            result.Should().NotBeNull();
            result.Teams.Should().HaveCount(2);
            result.Teams[0].ProjectKey.Should().Be(projectKey1);
            result.Teams[1].ProjectKey.Should().Be(projectKey2);
        }

        /// <summary>
        /// Verifies that the fastest team is correctly identified.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <param name="projectKey1">The first project key to compare.</param>
        /// <param name="projectKey2">The second project key to compare.</param>
        /// <param name="sprintCount">The number of sprints to analyze. Must be greater than zero.</param>
        /// <returns>A task that completes when the fastest‑team assertion has been executed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sprintCount"/> is less than or equal to zero.</exception>
        public static async Task VerifyFastestTeamIdentified(
            this TeamComparisonServiceTests tests,
            string projectKey1,
            string projectKey2,
            int sprintCount = 10)
        {
            ArgumentException.ThrowIfNullOrEmpty(projectKey1);
            ArgumentException.ThrowIfNullOrEmpty(projectKey2);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sprintCount, 0);

            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            result.Should().NotBeNull();
            result.FastestTeam.Should().Be(projectKey1);
        }

        /// <summary>
        /// Verifies that the highest quality team is correctly identified.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <param name="projectKey1">The first project key to compare.</param>
        /// <param name="projectKey2">The second project key to compare.</param>
        /// <param name="sprintCount">The number of sprints to analyze. Must be greater than zero.</param>
        /// <returns>A task that completes when the highest‑quality‑team assertion has been executed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="projectKey1"/> or <paramref name="projectKey2"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sprintCount"/> is less than or equal to zero.</exception>
        public static async Task VerifyHighestQualityTeamIdentified(
            this TeamComparisonServiceTests tests,
            string projectKey1,
            string projectKey2,
            int sprintCount = 10)
        {
            ArgumentException.ThrowIfNullOrEmpty(projectKey1);
            ArgumentException.ThrowIfNullOrEmpty(projectKey2);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sprintCount, 0);

            var service = CreateService(tests);
            var result = await service.CompareTeamsAsync(new[] { projectKey1, projectKey2 }, sprintCount);
            result.Should().NotBeNull();
            result.HighestQualityTeam.Should().Be(projectKey2);
        }

        /// <summary>
        /// Creates a new instance of <see cref="TeamComparisonService"/> with mocked dependencies.
        /// </summary>
        /// <param name="tests">The test class instance (unused but maintained for API consistency).</param>
        /// <returns>A <see cref="TeamComparisonService"/> ready for use in unit tests.</returns>
        private static TeamComparisonService CreateService(TeamComparisonServiceTests tests)
        {
            var analyticsMock = new Mock<IAnalyticsService>();
            var loggerMock = new Mock<ILogger<TeamComparisonService>>();
            return new TeamComparisonService(analyticsMock.Object, loggerMock.Object);
        }
    }
}
