// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text;
using FluentAssertions;
using JiraAnalyticsCli.Configuration;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JiraAnalyticsCli.Tests.Services;

/// <summary>
/// Integration tests for JiraApiService validating HTTP error handling,
/// auth header format, pagination edge cases, and timeout behaviour.
/// </summary>
public class JiraApiServiceTests
{
    private static JiraApiService CreateService(
        HttpMessageHandler handler,
        string? apiToken = "test-token",
        string baseUrl = "https://jira.example.com")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

        // Replicate the header setup performed by the DI container in Program.cs
        if (!string.IsNullOrEmpty(apiToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);
        }
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("jira")).Returns(httpClient);

        var configMock = new Mock<ICliConfig>();
        configMock.Setup(c => c.JiraApiToken).Returns(apiToken ?? string.Empty);
        configMock.Setup(c => c.JiraBaseUrl).Returns(baseUrl);

        var loggerMock = new Mock<ILogger<JiraApiService>>();

        return new JiraApiService(factoryMock.Object, configMock.Object, loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // HTTP status code tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProjectAsync_Returns_Null_On_401_Unauthorized()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        var sut = CreateService(handler);

        var result = await sut.GetProjectAsync("PROJ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectAsync_Returns_Null_On_403_Forbidden()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Forbidden);
        var sut = CreateService(handler);

        var result = await sut.GetProjectAsync("PROJ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectAsync_Returns_Null_On_429_TooManyRequests()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.TooManyRequests);
        var sut = CreateService(handler);

        var result = await sut.GetProjectAsync("PROJ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectAsync_Returns_Null_On_500_InternalServerError()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateService(handler);

        var result = await sut.GetProjectAsync("PROJ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Returns_Empty_On_401_Unauthorized()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        var sut = CreateService(handler);

        var result = await sut.GetSprintIssuesAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Returns_Empty_On_500_InternalServerError()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateService(handler);

        var result = await sut.GetSprintIssuesAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectSprintsAsync_Returns_Empty_On_403_Forbidden()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Forbidden);
        var sut = CreateService(handler);

        var result = await sut.GetProjectSprintsAsync("PROJ");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task VerifyConnectionAsync_Returns_False_On_401_Unauthorized()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        var sut = CreateService(handler);

        var result = await sut.VerifyConnectionAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyConnectionAsync_Returns_True_On_200_OK()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        var sut = CreateService(handler);

        var result = await sut.VerifyConnectionAsync();

        result.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Successful deserialization tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProjectAsync_Returns_Project_On_200_With_Valid_Json()
    {
        const string json = """
            {
                "id": "10001",
                "key": "PROJ",
                "name": "Test Project",
                "description": "A test project",
                "type": "software",
                "created": "2024-01-01T00:00:00.000Z"
            }
            """;

        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var sut = CreateService(handler);

        var result = await sut.GetProjectAsync("PROJ");

        result.Should().NotBeNull();
        result!.Key.Should().Be("PROJ");
        result.Name.Should().Be("Test Project");
        result.Id.Should().Be("10001");
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Returns_Empty_When_Issues_Array_Missing()
    {
        const string json = "{}";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var sut = CreateService(handler);

        var result = await sut.GetSprintIssuesAsync(42);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectSprintsAsync_Returns_Empty_When_Values_Array_Is_Empty()
    {
        const string json = """{ "values": [] }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var sut = CreateService(handler);

        var result = await sut.GetProjectSprintsAsync("PROJ");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Parses_Multiple_Issues_Correctly()
    {
        const string json = """
            {
                "issues": [
                    {
                        "id": "1",
                        "key": "PROJ-1",
                        "fields": {
                            "summary": "Issue one",
                            "status": { "name": "In Progress" },
                            "issuetype": { "name": "Story" },
                            "priority": { "name": "High" },
                            "created": "2024-01-01T00:00:00.000Z",
                            "updated": "2024-01-02T00:00:00.000Z"
                        }
                    },
                    {
                        "id": "2",
                        "key": "PROJ-2",
                        "fields": {
                            "summary": "Issue two",
                            "status": { "name": "Done" },
                            "issuetype": { "name": "Bug" },
                            "priority": { "name": "Low" },
                            "created": "2024-01-03T00:00:00.000Z",
                            "updated": "2024-01-04T00:00:00.000Z"
                        }
                    }
                ]
            }
            """;

        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var sut = CreateService(handler);

        var result = await sut.GetSprintIssuesAsync(10);

        result.Should().HaveCount(2);
        result[0].Key.Should().Be("PROJ-1");
        result[1].Key.Should().Be("PROJ-2");
    }

    // -------------------------------------------------------------------------
    // Pagination: empty next page token
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProjectSprintsAsync_Handles_Empty_Values_Array_As_Last_Page()
    {
        // Simulates a paginated API response where values is empty (end of results)
        const string json = """{ "values": [], "isLast": true, "startAt": 50, "maxResults": 50 }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var sut = CreateService(handler);

        var result = await sut.GetProjectSprintsAsync("PROJ");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Returns_Empty_When_Issues_Array_Is_Empty()
    {
        const string json = """{ "issues": [], "total": 0, "startAt": 0, "maxResults": 100 }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json);
        var sut = CreateService(handler);

        var result = await sut.GetSprintIssuesAsync(5);

        result.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Request timeout handling
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProjectAsync_Returns_Null_When_Request_Times_Out()
    {
        var handler = new TimeoutHttpMessageHandler();
        var sut = CreateService(handler);

        var result = await sut.GetProjectAsync("PROJ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Returns_Empty_When_Request_Times_Out()
    {
        var handler = new TimeoutHttpMessageHandler();
        var sut = CreateService(handler);

        var result = await sut.GetSprintIssuesAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task VerifyConnectionAsync_Returns_False_When_Request_Times_Out()
    {
        var handler = new TimeoutHttpMessageHandler();
        var sut = CreateService(handler);

        var result = await sut.VerifyConnectionAsync();

        result.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Authentication header format validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProjectAsync_Sends_Bearer_Authorization_Header()
    {
        const string expectedToken = "my-api-token-abc123";
        HttpRequestMessage? capturedRequest = null;

        var handler = new CapturingHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
        });

        var sut = CreateService(handler, apiToken: expectedToken);
        await sut.GetProjectAsync("PROJ");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be(expectedToken);
    }

    [Fact]
    public async Task VerifyConnectionAsync_Sends_Accept_Json_Header()
    {
        HttpRequestMessage? capturedRequest = null;

        var handler = new CapturingHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
        });

        var sut = CreateService(handler);
        await sut.VerifyConnectionAsync();

        capturedRequest.Should().NotBeNull();
        var acceptHeader = capturedRequest!.Headers.Accept.ToString();
        acceptHeader.Should().Contain("application/json");
    }

    // -------------------------------------------------------------------------
    // Malformed JSON deserialization
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProjectAsync_Returns_Null_On_Malformed_Json_Response()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{ invalid json !!!");
        var sut = CreateService(handler);

        var result = await sut.GetProjectAsync("PROJ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Returns_Empty_On_Malformed_Json_Response()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{ broken :");
        var sut = CreateService(handler);

        var result = await sut.GetSprintIssuesAsync(1);

        result.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Input validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProjectAsync_Throws_On_Null_ProjectKey()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sut = CreateService(handler);

        await sut.Invoking(s => s.GetProjectAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetProjectAsync_Throws_On_Empty_ProjectKey()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sut = CreateService(handler);

        await sut.Invoking(s => s.GetProjectAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSprintIssuesAsync_Throws_On_Zero_SprintId()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sut = CreateService(handler);

        await sut.Invoking(s => s.GetSprintIssuesAsync(0))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetSprintAsync_Throws_On_Negative_SprintId()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var sut = CreateService(handler);

        await sut.Invoking(s => s.GetSprintAsync(-1))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}

// =============================================================================
// Test helpers
// =============================================================================

/// <summary>Returns a fixed HTTP response for every request.</summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string body = "")
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}

/// <summary>Simulates a network timeout by throwing TaskCanceledException.</summary>
internal sealed class TimeoutHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => throw new TaskCanceledException("Request timed out");
}

/// <summary>Captures the outgoing request and delegates response creation to a factory.</summary>
internal sealed class CapturingHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

    public CapturingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        => _responseFactory = responseFactory;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_responseFactory(request));
}
