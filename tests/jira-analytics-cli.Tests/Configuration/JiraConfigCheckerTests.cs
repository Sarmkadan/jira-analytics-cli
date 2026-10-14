// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using FluentAssertions;
using JiraAnalyticsCli.Configuration;
using Xunit;

namespace JiraAnalyticsCli.Tests.Configuration;

/// <summary>
/// Unit tests for <see cref="JiraConfigChecker"/>: one item per setting, the authenticated /myself request,
/// skipping the request when settings are invalid, and a report that never prints the token.
/// </summary>
public class JiraConfigCheckerTests
{
    private const string Token = "tok-SECRET-123";
    private const string BaseUrl = "https://jira.example.com";

    private static readonly Func<string, string?> NoEnv = _ => null;

    private static JiraOptionsLoadResult ValidLoad() =>
        JiraOptionsLoader.Load(new JiraOptionsOverrides(BaseUrl: BaseUrl, ApiToken: Token), NoEnv, null);

    private static ConfigCheckItem Item(IReadOnlyList<ConfigCheckItem> items, string name) =>
        items.Single(item => item.Name == name);

    [Fact]
    public async Task RunAsync_ValidSettingsAndAuthenticated_AllItemsOk()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """{ "displayName": "Ada Lovelace" }""");

        var items = await JiraConfigChecker.RunAsync(ValidLoad(), handler);

        items.Should().OnlyContain(item => item.Status == ConfigCheckStatus.Ok);
        Item(items, "Jira connection").Detail.Should().Be("authenticated as Ada Lovelace");
    }

    [Fact]
    public async Task RunAsync_ValidSettings_CallsMyselfWithBearerToken()
    {
        var handler = new StubHandler(HttpStatusCode.OK);

        await JiraConfigChecker.RunAsync(ValidLoad(), handler);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri.Should().Be(new Uri("https://jira.example.com/rest/api/3/myself"));
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization!.Parameter.Should().Be(Token);
    }

    [Fact]
    public async Task RunAsync_TokenRejected_ReportsFailureWithoutTokenValue()
    {
        var handler = new StubHandler(HttpStatusCode.Unauthorized);

        var items = await JiraConfigChecker.RunAsync(ValidLoad(), handler);

        var connection = Item(items, "Jira connection");
        connection.Status.Should().Be(ConfigCheckStatus.Failed);
        connection.Detail.Should().Be("HTTP 401 from jira.example.com: the API token was rejected");
        JiraConfigChecker.Format(items).Should().NotContain(Token);
    }

    [Fact]
    public async Task RunAsync_UnexpectedStatus_ReportsCode()
    {
        var handler = new StubHandler(HttpStatusCode.InternalServerError);

        var items = await JiraConfigChecker.RunAsync(ValidLoad(), handler);

        Item(items, "Jira connection").Detail.Should().StartWith("HTTP 500 from jira.example.com");
    }

    [Fact]
    public async Task RunAsync_HostUnreachable_ReportsFailure()
    {
        var handler = new StubHandler(toThrow: new HttpRequestException("name not resolved"));

        var items = await JiraConfigChecker.RunAsync(ValidLoad(), handler);

        var connection = Item(items, "Jira connection");
        connection.Status.Should().Be(ConfigCheckStatus.Failed);
        connection.Detail.Should().Contain("could not reach jira.example.com").And.Contain("name not resolved");
    }

    [Fact]
    public async Task RunAsync_InvalidSetting_SkipsConnectionWithoutCallingJira()
    {
        var loaded = JiraOptionsLoader.Load(new JiraOptionsOverrides(BaseUrl: "http://insecure.example.com", ApiToken: Token), NoEnv, null);
        var handler = new StubHandler(HttpStatusCode.OK);

        var items = await JiraConfigChecker.RunAsync(loaded, handler);

        Item(items, "BaseUrl").Status.Should().Be(ConfigCheckStatus.Failed);
        Item(items, "BaseUrl").Detail.Should().Contain("must be an absolute https URI");
        Item(items, "Jira connection").Status.Should().Be(ConfigCheckStatus.Skipped);
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task RunAsync_MissingToken_ReportsApiTokenFailure()
    {
        var loaded = JiraOptionsLoader.Load(new JiraOptionsOverrides(BaseUrl: BaseUrl), NoEnv, null);

        var items = await JiraConfigChecker.RunAsync(loaded, new StubHandler(HttpStatusCode.OK));

        var token = Item(items, "ApiToken");
        token.Status.Should().Be(ConfigCheckStatus.Failed);
        token.Detail.Should().Contain("must not be empty");
        Item(items, "Jira connection").Status.Should().Be(ConfigCheckStatus.Skipped);
    }

    [Fact]
    public async Task RunAsync_BadBoardIdAndTimeout_EachGetOwnItem()
    {
        var loaded = JiraOptionsLoader.Load(
            new JiraOptionsOverrides(BaseUrl: BaseUrl, ApiToken: Token, BoardId: "abc", TimeoutSeconds: "0"),
            NoEnv,
            null);

        var items = await JiraConfigChecker.RunAsync(loaded);

        Item(items, "BoardId").Status.Should().Be(ConfigCheckStatus.Failed);
        Item(items, "TimeoutSeconds").Status.Should().Be(ConfigCheckStatus.Failed);
        items.Should().NotContain(item => item.Name == "Other settings");
    }

    [Fact]
    public async Task RunAsync_BrokenAppSettings_ReportedAsItsOwnItem()
    {
        var loaded = JiraOptionsLoader.Load(new JiraOptionsOverrides(BaseUrl: BaseUrl, ApiToken: Token), NoEnv, "{ not json");

        var items = await JiraConfigChecker.RunAsync(loaded);

        var appSettings = Item(items, "appsettings.json");
        appSettings.Status.Should().Be(ConfigCheckStatus.Failed);
        appSettings.Detail.Should().Contain("is not valid JSON");
        Item(items, "Jira connection").Status.Should().Be(ConfigCheckStatus.Skipped);
    }

    [Fact]
    public async Task RunAsync_OptionalBoardIdNotSet_ReportsOk()
    {
        var items = await JiraConfigChecker.RunAsync(ValidLoad(), new StubHandler(HttpStatusCode.OK));

        Item(items, "BoardId").Detail.Should().Be("not set (optional)");
    }

    [Fact]
    public async Task RunAsync_NullLoadResult_Throws()
    {
        var act = () => JiraConfigChecker.RunAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Format_AllPassed_ShowsOkLinesAndSuccessSummary()
    {
        var text = JiraConfigChecker.Format(new[]
        {
            new ConfigCheckItem("BaseUrl", ConfigCheckStatus.Ok, "https://jira.example.com"),
            new ConfigCheckItem("Jira connection", ConfigCheckStatus.Ok, "authenticated")
        });

        text.Should().Contain("[OK] BaseUrl");
        text.Should().Contain("[OK] Jira connection");
        text.Should().EndWith("All configuration checks passed.");
    }

    [Fact]
    public void Format_Failures_CountsOnlyFailedItems()
    {
        var text = JiraConfigChecker.Format(new[]
        {
            new ConfigCheckItem("BaseUrl", ConfigCheckStatus.Failed, "bad"),
            new ConfigCheckItem("ApiToken", ConfigCheckStatus.Ok, "set (value hidden)"),
            new ConfigCheckItem("Jira connection", ConfigCheckStatus.Skipped, "skipped")
        });

        text.Should().Contain("[FAIL] BaseUrl");
        text.Should().Contain("[SKIP] Jira connection");
        text.Should().EndWith("1 configuration check(s) failed.");
    }

    /// <summary>Returns a fixed response, or throws, and records every request it receives.</summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;
        private readonly Exception? _toThrow;

        public StubHandler(HttpStatusCode status = HttpStatusCode.OK, string body = "{}", Exception? toThrow = null)
        {
            _status = status;
            _body = body;
            _toThrow = toThrow;
        }

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (_toThrow is not null)
                return Task.FromException<HttpResponseMessage>(_toThrow);

            return Task.FromResult(new HttpResponseMessage(_status) { Content = new StringContent(_body) });
        }
    }
}
