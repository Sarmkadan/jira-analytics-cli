using System.Text.Json;
using FluentAssertions;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace JiraAnalyticsCli.Tests.Services;

public class JiraClientTests : IAsyncLifetime
{
    private WireMockServer _wireMock = null!;
    private const string BaseUrl = "http://localhost:9090";

    public Task InitializeAsync()
    {
        this._wireMock = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = new[] { BaseUrl },
            StartAdminInterface = false
        });
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        this._wireMock.Stop();
        return Task.CompletedTask;
    }

    private JiraClient CreateClient()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        var options = Options.Create(new JiraClientOptions
        {
            BaseUrl = BaseUrl,
            Username = "testuser",
            ApiToken = "testtoken"
        });
        var logger = LoggerFactory.Create(builder =>
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter("LoggingConsoleApp", LogLevel.Debug))
            .CreateLogger<JiraClient>();
        return new JiraClient(httpClient, options, logger);
    }

    [Fact]
    public async Task GetAllPaginatedAsync_MergesPagesWithoutDuplicates()
    {
        // Arrange
        var endpoint = "/rest/api/3/search";
        var pageSize = 2;

        // Page 1
        this._wireMock.Given(Request.Create()
                .WithPath($"/rest/api/3/search*")
                .WithParam("startAt", "0")
                .WithParam("maxResults", pageSize.ToString())
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    values = new[]
                    {
                        new { id = "1", key = "TEST-1" },
                        new { id = "2", key = "TEST-2" }
                    },
                    total = 5
                })));

        // Page 2
        this._wireMock.Given(Request.Create()
                .WithPath($"/rest/api/3/search*")
                .WithParam("startAt", "2")
                .WithParam("maxResults", pageSize.ToString())
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    values = new[]
                    {
                        new { id = "3", key = "TEST-3" },
                        new { id = "4", key = "TEST-4" }
                    },
                    total = 5
                })));

        // Page 3 (last page, only one item)
        this._wireMock.Given(Request.Create()
                .WithPath($"/rest/api/3/search*")
                .WithParam("startAt", "4")
                .WithParam("maxResults", pageSize.ToString())
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    values = new[]
                    {
                        new { id = "5", key = "TEST-5" }
                    },
                    total = 5
                })));

        var client = this.CreateClient();

        // Act
        var results = await client.GetAllPaginatedAsync<JsonElement>(endpoint, "values", pageSize);

        // Assert
        results.Should().HaveCount(5);
        results.Select(r => r.GetProperty("key").GetString()).Should().ContainInOrder(
            "TEST-1", "TEST-2", "TEST-3", "TEST-4", "TEST-5");
    }

    [Fact]
    public async Task GetAsync_RetriesOn429WithRetryAfter()
    {
        // Arrange
        var endpoint = "/rest/api/3/issue/TEST-1";
        var successResponse = new { id = "1", key = "TEST-1", fields = new { summary = "Test issue" } };

        // First attempt returns 429 with Retry-After header
        this._wireMock.Given(Request.Create()
                .WithPath(endpoint)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(429)
                .WithHeader("Retry-After", "0") // Immediate retry for test speed
                .WithBody("Rate limited"));

        // Second attempt returns 429 with Retry-After header
        this._wireMock.Given(Request.Create()
                .WithPath(endpoint)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(429)
                .WithHeader("Retry-After", "0")
                .WithBody("Rate limited"));

        // Third attempt succeeds
        this._wireMock.Given(Request.Create()
                .WithPath(endpoint)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(successResponse)));

        var client = this.CreateClient();

        // Act
        var result = await client.GetAsync<JsonElement>(endpoint);

        // Assert
        result.Should().NotBeNull();
        result.GetProperty("key").GetString().Should().Be("TEST-1");
    }

    [Fact]
    public async Task GetAsync_401ReturnsClearAuthenticationFailure()
    {
        // Arrange
        var endpoint = "/rest/api/3/issue/TEST-1";

        this._wireMock.Given(Request.Create()
                .WithPath(endpoint)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(401)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    errorMessages = new[] { "Invalid authentication" },
                    errors = new {}
                })));

        var client = this.CreateClient();

        // Act
        Func<Task> act = async () => await client.GetAsync<JsonElement>(endpoint);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.Message.Contains("401") && ex.Message.Contains("Unauthorized"));
    }

    [Fact]
    public async Task GetAsync_MalformedJsonThrowsDescriptiveException()
    {
        // Arrange
        var endpoint = "/rest/api/3/issue/TEST-1";

        this._wireMock.Given(Request.Create()
                .WithPath(endpoint)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{ invalid json : }")); // Intentionally malformed

        var client = this.CreateClient();

        // Act
        Func<Task> act = async () => await client.GetAsync<JsonElement>(endpoint);

        // Assert
        await act.Should().ThrowAsync<JsonException>()
            .WithMessage("*");
    }
}