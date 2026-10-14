// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Diagnostics;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace JiraAnalyticsCli.Tests.Diagnostics;

/// <summary>
/// Tests for <see cref="JiraDoctor"/> against a fake Jira server (WireMock on localhost), covering each check,
/// the skip rules between checks, per-check timeouts, the TLS and DNS failure paths, and the table output.
/// </summary>
public sealed class JiraDoctorTests : IDisposable
{
    private const string Token = "tok-SECRET-456";
    private const string MyselfPath = "/rest/api/3/myself";
    private const string BoardPath = "/rest/agile/1.0/board/42";
    private const string ClosedSprintsPath = "/rest/agile/1.0/board/42/sprint";
    private const string FieldsPath = "/rest/api/3/field";
    private const string AgilePathPrefix = "/rest/agile/";

    private const string HealthyMyself = """{ "accountId": "a1", "displayName": "Ada Lovelace" }""";
    private const string HealthyBoard = """{ "id": 42, "name": "Payments board", "type": "scrum" }""";
    private const string HealthyClosedSprints = """{ "maxResults": 50, "isLast": true, "values": [ { "id": 7, "name": "Sprint 7", "state": "closed" } ] }""";
    private const string HealthyFields = """
        [
          { "id": "summary", "name": "Summary", "custom": false, "schema": { "type": "string" } },
          { "id": "customfield_10016", "name": "Story point estimate", "custom": true, "schema": { "type": "number" } }
        ]
        """;

    private readonly WireMockServer _server;

    public JiraDoctorTests()
    {
        _server = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = new[] { "http://localhost:0" },
            StartAdminInterface = false
        });
    }

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task RunAsync_HealthyJira_EveryCheckPasses()
    {
        StubHealthyJira();

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        results.Select(result => result.Name).Should().Equal(
            JiraDoctor.ConfigurationCheck,
            JiraDoctor.ReachabilityCheck,
            JiraDoctor.AuthenticationCheck,
            JiraDoctor.BoardCheck,
            JiraDoctor.ClosedSprintCheck,
            JiraDoctor.StoryPointsCheck);
        results.Should().OnlyContain(result => result.Status == DoctorStatus.Ok);
        Check(results, JiraDoctor.AuthenticationCheck).Detail.Should().Be("authenticated as Ada Lovelace");
        Check(results, JiraDoctor.BoardCheck).Detail.Should().Be("board 42 \"Payments board\"");
        Check(results, JiraDoctor.ClosedSprintCheck).Detail.Should().Be("1 closed sprint(s) found, e.g. \"Sprint 7\"");
        Check(results, JiraDoctor.StoryPointsCheck).Detail.Should().Be("customfield_10016 \"Story point estimate\"");
        Check(results, JiraDoctor.ReachabilityCheck).Detail.Should().Contain("TLS not checked for http");
        JiraDoctor.HasFailures(results).Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_HealthyJira_SendsBearerTokenToEveryJiraCall()
    {
        StubHealthyJira();

        await JiraDoctor.RunAsync(Jira(), Velocity());

        // Every stub requires the Bearer header, so passing checks also prove the token was sent.
        RequestedPaths().Should().BeEquivalentTo(new[] { MyselfPath, BoardPath, ClosedSprintsPath, FieldsPath });
    }

    [Fact]
    public async Task RunAsync_InvalidSettings_FailsConfigurationAndMakesNoRequests()
    {
        StubHealthyJira();
        var jira = JiraOptionsLoader.Load(new JiraOptionsOverrides(BaseUrl: _server.Urls[0]), _ => null, null);

        var results = await JiraDoctor.RunAsync(jira, Velocity());

        var configuration = Check(results, JiraDoctor.ConfigurationCheck);
        configuration.Status.Should().Be(DoctorStatus.Failed);
        configuration.Detail.Should().Contain("ApiToken");
        configuration.Hint.Should().NotBeNullOrWhiteSpace();
        Check(results, JiraDoctor.AuthenticationCheck).Status.Should().Be(DoctorStatus.Skipped);
        Check(results, JiraDoctor.StoryPointsCheck).Detail.Should().Contain(JiraDoctor.ConfigurationCheck);
        RequestedPaths().Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_InvalidVelocityField_FailsConfiguration()
    {
        var velocity = VelocityOptionsLoader.Load(new VelocityOptionsOverrides(StoryPointsField: "story"), _ => null, null);

        var results = await JiraDoctor.RunAsync(Jira(), velocity);

        var configuration = Check(results, JiraDoctor.ConfigurationCheck);
        configuration.Status.Should().Be(DoctorStatus.Failed);
        configuration.Detail.Should().Contain("StoryPointsField");
    }

    [Fact]
    public async Task RunAsync_TokenRejected_FailsAuthenticationAndSkipsJiraChecks()
    {
        Stub(MyselfPath, HttpStatusCode.Unauthorized);

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var authentication = Check(results, JiraDoctor.AuthenticationCheck);
        authentication.Status.Should().Be(DoctorStatus.Failed);
        authentication.Detail.Should().Be("HTTP 401: the API token was rejected");
        authentication.Hint.Should().Contain("JIRA_API_TOKEN");
        Check(results, JiraDoctor.BoardCheck).Status.Should().Be(DoctorStatus.Skipped);
        Check(results, JiraDoctor.ClosedSprintCheck).Status.Should().Be(DoctorStatus.Skipped);
        Check(results, JiraDoctor.StoryPointsCheck).Status.Should().Be(DoctorStatus.Skipped);
        RequestedPaths().Should().OnlyContain(path => path == MyselfPath);
        JiraDoctor.Format(results).Should().NotContain(Token);
    }

    [Fact]
    public async Task RunAsync_BoardNotConfigured_FailsBoardWithoutCallingAgileApi()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(FieldsPath, HttpStatusCode.OK, HealthyFields);

        var results = await JiraDoctor.RunAsync(Jira(boardId: null), Velocity());

        var board = Check(results, JiraDoctor.BoardCheck);
        board.Status.Should().Be(DoctorStatus.Failed);
        board.Hint.Should().Contain("--board-id");
        Check(results, JiraDoctor.ClosedSprintCheck).Status.Should().Be(DoctorStatus.Skipped);
        Check(results, JiraDoctor.StoryPointsCheck).Status.Should().Be(DoctorStatus.Ok);
        RequestedPaths().Should().NotContain(path => path.StartsWith(AgilePathPrefix, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_BoardNotFound_FailsBoardAndSkipsClosedSprint()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.NotFound);
        Stub(FieldsPath, HttpStatusCode.OK, HealthyFields);

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var board = Check(results, JiraDoctor.BoardCheck);
        board.Status.Should().Be(DoctorStatus.Failed);
        board.Detail.Should().Contain("board 42 does not exist");
        Check(results, JiraDoctor.ClosedSprintCheck).Status.Should().Be(DoctorStatus.Skipped);
        Check(results, JiraDoctor.StoryPointsCheck).Status.Should().Be(DoctorStatus.Ok);
    }

    [Fact]
    public async Task RunAsync_BoardWithoutViewPermission_FailsBoardWithAccessHint()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.Forbidden);
        Stub(FieldsPath, HttpStatusCode.OK, HealthyFields);

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var board = Check(results, JiraDoctor.BoardCheck);
        board.Detail.Should().Be("no access to board 42 (HTTP 403)");
        board.Hint.Should().Contain("View permission");
    }

    [Fact]
    public async Task RunAsync_KanbanBoardRejectsSprintList_FailsClosedSprintWithScrumHint()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.OK, HealthyBoard);
        Stub(ClosedSprintsPath, HttpStatusCode.BadRequest);
        Stub(FieldsPath, HttpStatusCode.OK, HealthyFields);

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var sprint = Check(results, JiraDoctor.ClosedSprintCheck);
        sprint.Status.Should().Be(DoctorStatus.Failed);
        sprint.Detail.Should().Be("board 42 does not support sprints");
        sprint.Hint.Should().Contain("Scrum");
    }

    [Fact]
    public async Task RunAsync_NoClosedSprints_FailsClosedSprint()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.OK, HealthyBoard);
        Stub(ClosedSprintsPath, HttpStatusCode.OK, """{ "maxResults": 50, "isLast": true, "values": [] }""");
        Stub(FieldsPath, HttpStatusCode.OK, HealthyFields);

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var sprint = Check(results, JiraDoctor.ClosedSprintCheck);
        sprint.Status.Should().Be(DoctorStatus.Failed);
        sprint.Detail.Should().Be("board 42 has no closed sprints");
        sprint.Hint.Should().Contain("Complete a sprint");
    }

    [Fact]
    public async Task RunAsync_StoryPointsFieldMissing_SuggestsTheFieldThatExists()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.OK, HealthyBoard);
        Stub(ClosedSprintsPath, HttpStatusCode.OK, HealthyClosedSprints);
        Stub(FieldsPath, HttpStatusCode.OK, """
            [ { "id": "customfield_10100", "name": "Story point estimate", "custom": true, "schema": { "type": "number" } } ]
            """);

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var story = Check(results, JiraDoctor.StoryPointsCheck);
        story.Status.Should().Be(DoctorStatus.Failed);
        story.Detail.Should().Be("customfield_10016 does not exist; customfield_10100 is named \"Story point estimate\"");
        story.Hint.Should().Contain("--story-points-field customfield_10100");
    }

    [Fact]
    public async Task RunAsync_NoStoryPointsFieldAtAll_FailsWithAdminHint()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.OK, HealthyBoard);
        Stub(ClosedSprintsPath, HttpStatusCode.OK, HealthyClosedSprints);
        Stub(FieldsPath, HttpStatusCode.OK, """[ { "id": "summary", "name": "Summary", "custom": false } ]""");

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var story = Check(results, JiraDoctor.StoryPointsCheck);
        story.Status.Should().Be(DoctorStatus.Failed);
        story.Detail.Should().Contain("no story-points field was found");
        story.Hint.Should().Contain("Ask a Jira admin");
    }

    [Fact]
    public async Task RunAsync_ConfiguredStoryPointsFieldIsNotNumeric_Fails()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.OK, HealthyBoard);
        Stub(ClosedSprintsPath, HttpStatusCode.OK, HealthyClosedSprints);
        Stub(FieldsPath, HttpStatusCode.OK, """
            [ { "id": "customfield_10016", "name": "Story point estimate", "custom": true, "schema": { "type": "string" } } ]
            """);

        var results = await JiraDoctor.RunAsync(Jira(), Velocity());

        var story = Check(results, JiraDoctor.StoryPointsCheck);
        story.Status.Should().Be(DoctorStatus.Failed);
        story.Detail.Should().Contain("is a string field, not a number");
    }

    [Fact]
    public async Task RunAsync_CustomStoryPointsFieldFromVelocitySettings_IsTheOneChecked()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.OK, HealthyBoard);
        Stub(ClosedSprintsPath, HttpStatusCode.OK, HealthyClosedSprints);
        Stub(FieldsPath, HttpStatusCode.OK, """
            [ { "id": "customfield_10200", "name": "Story Points", "custom": true, "schema": { "type": "number" } } ]
            """);
        var velocity = VelocityOptionsLoader.Load(new VelocityOptionsOverrides(StoryPointsField: "customfield_10200"), _ => null, null);

        var results = await JiraDoctor.RunAsync(Jira(), velocity);

        Check(results, JiraDoctor.StoryPointsCheck).Detail.Should().Be("customfield_10200 \"Story Points\"");
    }

    [Fact]
    public async Task RunAsync_JiraDoesNotAnswerInTime_TimesOutThatCheck()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself, delay: TimeSpan.FromSeconds(3));
        var stopwatch = Stopwatch.StartNew();

        var results = await JiraDoctor.RunAsync(Jira(timeoutSeconds: 1), Velocity());

        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3), "the 1 s timeout should cut the 3 s response short");
        var authentication = Check(results, JiraDoctor.AuthenticationCheck);
        authentication.Status.Should().Be(DoctorStatus.Failed);
        authentication.Detail.Should().Be("no answer within 1 s");
        authentication.Hint.Should().Contain("--timeout-seconds");
        Check(results, JiraDoctor.BoardCheck).Status.Should().Be(DoctorStatus.Skipped);
    }

    [Fact]
    public async Task RunAsync_HttpsUrlOnPlainHttpServer_FailsTlsHandshake()
    {
        StubHealthyJira();
        var httpsUrl = _server.Urls[0].Replace("http://", "https://", StringComparison.Ordinal);

        var results = await JiraDoctor.RunAsync(Jira(baseUrl: httpsUrl), Velocity());

        var reachability = Check(results, JiraDoctor.ReachabilityCheck);
        reachability.Status.Should().Be(DoctorStatus.Failed);
        reachability.Detail.Should().Contain("TLS handshake");
        reachability.Hint.Should().Contain("https");
        Check(results, JiraDoctor.AuthenticationCheck).Status.Should().Be(DoctorStatus.Skipped);
        RequestedPaths().Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_ClosedPort_FailsTcpConnection()
    {
        var port = FreePort();

        var results = await JiraDoctor.RunAsync(Jira(baseUrl: $"https://localhost:{port}"), Velocity());

        var reachability = Check(results, JiraDoctor.ReachabilityCheck);
        reachability.Status.Should().Be(DoctorStatus.Failed);
        reachability.Detail.Should().Contain($"TCP connection to localhost:{port} failed");
        reachability.Hint.Should().Contain($"TCP port {port}");
        Check(results, JiraDoctor.AuthenticationCheck).Status.Should().Be(DoctorStatus.Skipped);
    }

    [Fact]
    public async Task RunAsync_UnresolvableHost_FailsDnsAndSkipsEverythingElse()
    {
        var results = await JiraDoctor.RunAsync(Jira(baseUrl: "https://doctor-check.invalid", timeoutSeconds: 5), Velocity());

        var reachability = Check(results, JiraDoctor.ReachabilityCheck);
        reachability.Status.Should().Be(DoctorStatus.Failed);
        reachability.Hint.Should().Contain("BaseUrl");
        results.Where(result => result.Name != JiraDoctor.ConfigurationCheck && result.Name != JiraDoctor.ReachabilityCheck)
            .Should().OnlyContain(result => result.Status == DoctorStatus.Skipped);
    }

    [Fact]
    public void Format_ShowsOneRowPerCheckWithStatusAndFixes()
    {
        var text = JiraDoctor.Format(new[]
        {
            DoctorCheckResult.Pass("Configuration", "valid"),
            DoctorCheckResult.Fail("Board", "board 42 does not exist", "Check --board-id."),
            DoctorCheckResult.Skip("Closed sprint", "Board")
        });

        text.Should().Contain("Check           Status  Detail");
        text.Should().Contain("Configuration   OK      valid");
        text.Should().Contain("Board           FAIL    board 42 does not exist");
        text.Should().Contain("Closed sprint   SKIP    not run: Board did not pass");
        text.Should().Contain("How to fix:").And.Contain("  - Board: Check --board-id.");
        text.Should().NotContain("  - Closed sprint:");
        text.Should().EndWith("1 of 3 check(s) failed.");
    }

    [Fact]
    public void Format_AllPassed_ShowsSuccessSummaryWithoutFixes()
    {
        var text = JiraDoctor.Format(new[] { DoctorCheckResult.Pass("Configuration", "valid") });

        text.Should().NotContain("How to fix:");
        text.Should().EndWith("All 1 checks passed.");
    }

    [Fact]
    public void HasFailures_OnlySkippedChecks_ReturnsFalse()
    {
        var results = new[]
        {
            DoctorCheckResult.Pass("Configuration", "valid"),
            DoctorCheckResult.Skip("Board", "Authentication")
        };

        JiraDoctor.HasFailures(results).Should().BeFalse();
    }

    [Fact]
    public void HasFailures_AnyFailedCheck_ReturnsTrue()
    {
        var results = new[]
        {
            DoctorCheckResult.Pass("Configuration", "valid"),
            DoctorCheckResult.Fail("Board", "missing", "Fix it.")
        };

        JiraDoctor.HasFailures(results).Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_NullLoadResult_Throws()
    {
        var act = () => JiraDoctor.RunAsync(null!, Velocity());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private void StubHealthyJira()
    {
        Stub(MyselfPath, HttpStatusCode.OK, HealthyMyself);
        Stub(BoardPath, HttpStatusCode.OK, HealthyBoard);
        Stub(ClosedSprintsPath, HttpStatusCode.OK, HealthyClosedSprints);
        Stub(FieldsPath, HttpStatusCode.OK, HealthyFields);
    }

    /// <summary>Registers a GET stub that answers only when the request carries the Bearer token.</summary>
    private void Stub(string path, HttpStatusCode status, string body = "{}", TimeSpan? delay = null)
    {
        var response = Response.Create()
            .WithStatusCode((int)status)
            .WithHeader("Content-Type", "application/json")
            .WithBody(body);

        if (delay is { } wait)
            response.WithDelay(wait);

        _server.Given(Request.Create()
                .WithPath(path)
                .UsingGet()
                .WithHeader("Authorization", $"Bearer {Token}"))
            .RespondWith(response);
    }

    private JiraOptionsLoadResult Jira(int? boardId = 42, int timeoutSeconds = 5, string? baseUrl = null) =>
        new(new JiraOptions
        {
            BaseUrl = baseUrl ?? _server.Urls[0],
            ApiToken = Token,
            BoardId = boardId,
            TimeoutSeconds = timeoutSeconds
        }, Array.Empty<string>());

    private static VelocityOptionsLoadResult Velocity() =>
        VelocityOptionsLoader.Load(new VelocityOptionsOverrides(), _ => null, null);

    private IEnumerable<string> RequestedPaths() =>
        _server.LogEntries.Select(entry => entry.RequestMessage.Path);

    private static DoctorCheckResult Check(IReadOnlyList<DoctorCheckResult> results, string name) =>
        results.Single(result => result.Name == name);

    /// <summary>Returns a local port that nothing is listening on, by binding and releasing it.</summary>
    private static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
