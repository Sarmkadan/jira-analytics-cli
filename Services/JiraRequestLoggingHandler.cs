// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Services;

/// <summary>
/// Logs every Jira HTTP request at Debug level: method, URL, status code and duration.
/// Sits in the "jira" HttpClient pipeline, so every Jira call is covered. Only the scheme, host, path and
/// query are logged. Headers (the Bearer token) and URI user-info are never written.
/// </summary>
public sealed class JiraRequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger<JiraRequestLoggingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="JiraRequestLoggingHandler"/>.
    /// </summary>
    /// <param name="logger">Logger that receives the request lines.</param>
    public JiraRequestLoggingHandler(ILogger<JiraRequestLoggingHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this._logger = logger;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = RedactUrl(request.RequestUri);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            this._logger.LogDebug("Jira request {Method} {Url} returned {StatusCode} in {ElapsedMs} ms",
                request.Method, url, (int)response.StatusCode, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            this._logger.LogDebug("Jira request {Method} {Url} failed after {ElapsedMs} ms: {ErrorMessage}",
                request.Method, url, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Returns the URL without user-info (<c>user:password@</c>), fragment or anything else that could carry credentials.
    /// </summary>
    /// <param name="uri">Request URI; may be null or relative.</param>
    /// <returns>The scheme, host, path and query, or a placeholder when there is no absolute URI.</returns>
    internal static string RedactUrl(Uri? uri)
    {
        if (uri is null)
        {
            return "<no url>";
        }

        return uri.IsAbsoluteUri
            ? uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path | UriComponents.Query, UriFormat.UriEscaped)
            : "<relative url>";
    }
}
