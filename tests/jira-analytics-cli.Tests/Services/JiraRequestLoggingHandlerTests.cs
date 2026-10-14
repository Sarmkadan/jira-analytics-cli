// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using FluentAssertions;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace JiraAnalyticsCli.Tests.Services;

/// <summary>
/// Verifies the Jira request log line and that credentials never reach the log.
/// </summary>
public class JiraRequestLoggingHandlerTests
{
    private const string Token = "super-secret-token-123";

    [Fact]
    public async Task SendAsync_LogsMethodUrlStatusAndDuration()
    {
        var logger = new CapturingLogger<JiraRequestLoggingHandler>();
        using var invoker = CreateInvoker(logger, HttpStatusCode.OK);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://jira.example.com/rest/api/3/myself");

        await invoker.SendAsync(request, CancellationToken.None);

        var line = logger.Messages.Should().ContainSingle().Subject;
        line.Level.Should().Be(LogLevel.Debug);
        line.Message.Should().StartWith("Jira request GET https://jira.example.com/rest/api/3/myself returned 200 in ");
        line.Message.Should().EndWith(" ms");
    }

    [Fact]
    public async Task SendAsync_NeverLogsAuthorizationHeader()
    {
        var logger = new CapturingLogger<JiraRequestLoggingHandler>();
        using var invoker = CreateInvoker(logger, HttpStatusCode.Unauthorized);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://jira.example.com/rest/api/3/myself");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        await invoker.SendAsync(request, CancellationToken.None);

        logger.Messages.Should().NotBeEmpty();
        logger.Messages.Should().OnlyContain(m => !m.Message.Contains(Token));
    }

    [Fact]
    public async Task SendAsync_LogsFailureWithoutTokenWhenTransportThrows()
    {
        var logger = new CapturingLogger<JiraRequestLoggingHandler>();
        var handler = new JiraRequestLoggingHandler(logger) { InnerHandler = new ThrowingHandler() };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://jira.example.com/rest/api/3/search?jql=project%3DX");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        var act = () => invoker.SendAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
        var line = logger.Messages.Should().ContainSingle().Subject;
        line.Message.Should().StartWith("Jira request GET https://jira.example.com/rest/api/3/search?jql=project%3DX failed after ");
        line.Message.Should().NotContain(Token);
    }

    [Theory]
    [InlineData("https://user:pa55word@jira.example.com/rest/api/3/myself", "https://jira.example.com/rest/api/3/myself")]
    [InlineData("https://jira.example.com/rest/api/3/search?jql=sprint%3D1#frag", "https://jira.example.com/rest/api/3/search?jql=sprint%3D1")]
    public void RedactUrl_RemovesUserInfoAndFragment(string input, string expected)
    {
        JiraRequestLoggingHandler.RedactUrl(new Uri(input)).Should().Be(expected);
    }

    [Fact]
    public void RedactUrl_HandlesNullAndRelativeUris()
    {
        JiraRequestLoggingHandler.RedactUrl(null).Should().Be("<no url>");
        JiraRequestLoggingHandler.RedactUrl(new Uri("/rest/api/3/myself", UriKind.Relative)).Should().Be("<relative url>");
    }

    private static HttpMessageInvoker CreateInvoker(ILogger<JiraRequestLoggingHandler> logger, HttpStatusCode status)
    {
        var handler = new JiraRequestLoggingHandler(logger)
        {
            InnerHandler = new StatusHandler(status)
        };
        return new HttpMessageInvoker(handler);
    }

    private sealed class StatusHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new HttpRequestException("connection refused");
    }
}
