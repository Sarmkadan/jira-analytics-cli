// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace JiraAnalyticsCli.Configuration;

/// <summary>Outcome of one check run by <c>--check-config</c>.</summary>
public enum ConfigCheckStatus
{
    /// <summary>The check passed.</summary>
    Ok,

    /// <summary>The check failed.</summary>
    Failed,

    /// <summary>The check did not run because a check it depends on failed.</summary>
    Skipped
}

/// <summary>One line of the <c>--check-config</c> report.</summary>
/// <param name="Name">What was checked, e.g. <c>BaseUrl</c> or <c>Jira connection</c>.</param>
/// <param name="Status">Outcome of the check.</param>
/// <param name="Detail">Value that was checked, or the reason for a failure. Never contains the API token.</param>
public sealed record ConfigCheckItem(string Name, ConfigCheckStatus Status, string Detail);

/// <summary>
/// Runs the <c>--check-config</c> checks: one item per Jira setting, then an authenticated
/// <c>GET /rest/api/3/myself</c> against the configured instance.
/// </summary>
/// <remarks>
/// The request goes to the same URL that <c>JiraApiService.VerifyConnectionAsync</c> uses, so a
/// successful check means the regular commands can reach Jira with these settings. It is skipped when a setting is invalid.
/// </remarks>
public static class JiraConfigChecker
{
    /// <summary>Path of the lightweight authenticated endpoint. Same path as <c>JiraApiService</c> uses.</summary>
    public const string MyselfPath = "/rest/api/3/myself";

    /// <summary>
    /// Runs every check.
    /// </summary>
    /// <param name="loaded">Result of <see cref="JiraOptionsLoader.Load"/>, including its problems.</param>
    /// <param name="handler">HTTP handler for the connection check. Null uses the default handler. Tests pass a stub.</param>
    /// <param name="cancellationToken">Cancels the connection check.</param>
    /// <returns>One item per check, in the order they run.</returns>
    public static async Task<IReadOnlyList<ConfigCheckItem>> RunAsync(
        JiraOptionsLoadResult loaded,
        HttpMessageHandler? handler = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loaded);

        var items = new List<ConfigCheckItem>();
        var unclaimed = new List<string>(loaded.Problems);

        items.Add(SettingItem(JiraOptionsLoader.AppSettingsFileName, loaded, unclaimed, _ => "valid, or not present"));
        items.Add(SettingItem(JiraSettingLocations.BaseUrl.Setting, loaded, unclaimed, o => o.BaseUrl));
        items.Add(SettingItem(JiraSettingLocations.ApiToken.Setting, loaded, unclaimed, _ => "set (value hidden)"));
        items.Add(SettingItem(JiraSettingLocations.BoardId.Setting, loaded, unclaimed,
            o => o.BoardId?.ToString(CultureInfo.InvariantCulture) ?? "not set (optional)"));
        items.Add(SettingItem(JiraSettingLocations.TimeoutSeconds.Setting, loaded, unclaimed,
            o => $"{o.TimeoutSeconds.ToString(CultureInfo.InvariantCulture)} s"));

        foreach (var problem in unclaimed)
        {
            items.Add(new ConfigCheckItem("Other settings", ConfigCheckStatus.Failed, problem));
        }

        if (items.Any(item => item.Status == ConfigCheckStatus.Failed))
        {
            items.Add(new ConfigCheckItem("Jira connection", ConfigCheckStatus.Skipped,
                "not attempted until the settings above are fixed"));
            return items;
        }

        items.Add(await CheckConnectionAsync(loaded.Options, handler, cancellationToken));
        return items;
    }

    /// <summary>
    /// Formats the items as one line each, followed by a summary line.
    /// </summary>
    /// <param name="items">Items returned by <see cref="RunAsync"/>.</param>
    /// <returns>Text for stdout.</returns>
    public static string Format(IReadOnlyList<ConfigCheckItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var width = items.Count == 0 ? 0 : items.Max(item => item.Name.Length);
        var lines = items.Select(item =>
            $"[{Label(item.Status)}] {item.Name.PadRight(width)}  {item.Detail}");

        var failures = items.Count(item => item.Status == ConfigCheckStatus.Failed);
        var summary = failures == 0
            ? "All configuration checks passed."
            : $"{failures} configuration check(s) failed.";

        return string.Join(Environment.NewLine, lines.Append(string.Empty).Append(summary));
    }

    private static ConfigCheckItem SettingItem(
        string name,
        JiraOptionsLoadResult loaded,
        List<string> unclaimed,
        Func<JiraOptions, string> describe)
    {
        var matching = unclaimed.Where(problem => problem.StartsWith(name + " ", StringComparison.Ordinal)).ToList();
        foreach (var problem in matching)
        {
            unclaimed.Remove(problem);
        }

        return matching.Count == 0
            ? new ConfigCheckItem(name, ConfigCheckStatus.Ok, describe(loaded.Options))
            : new ConfigCheckItem(name, ConfigCheckStatus.Failed, string.Join(" ", matching));
    }

    private static async Task<ConfigCheckItem> CheckConnectionAsync(
        JiraOptions options,
        HttpMessageHandler? handler,
        CancellationToken cancellationToken)
    {
        const string name = "Jira connection";
        var timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

        using var client = new HttpClient(handler ?? new HttpClientHandler(), disposeHandler: true) { Timeout = timeout };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Matches JiraApiService: a leading slash resolves against the host, so a path prefix in BaseUrl is not kept.
        var url = new Uri(new Uri(options.BaseUrl), MyselfPath);

        try
        {
            using var response = await client.GetAsync(url, cancellationToken);
            var code = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);

            if (response.IsSuccessStatusCode)
            {
                var who = await ReadDisplayNameAsync(response, cancellationToken);
                return new ConfigCheckItem(name, ConfigCheckStatus.Ok,
                    who is null ? $"HTTP {code}, authenticated" : $"authenticated as {who}");
            }

            var reason = response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden
                ? "the API token was rejected"
                : response.ReasonPhrase ?? "unexpected response";
            return new ConfigCheckItem(name, ConfigCheckStatus.Failed, $"HTTP {code} from {url.Host}: {reason}");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ConfigCheckItem(name, ConfigCheckStatus.Failed,
                $"no response from {url.Host} within {options.TimeoutSeconds.ToString(CultureInfo.InvariantCulture)} s");
        }
        catch (HttpRequestException ex)
        {
            return new ConfigCheckItem(name, ConfigCheckStatus.Failed, $"could not reach {url.Host}: {ex.Message}");
        }
    }

    private static async Task<string?> ReadDisplayNameAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return document.RootElement.TryGetProperty("displayName", out var name) && name.ValueKind == JsonValueKind.String
                ? name.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Label(ConfigCheckStatus status) => status switch
    {
        ConfigCheckStatus.Ok => "OK",
        ConfigCheckStatus.Failed => "FAIL",
        _ => "SKIP"
    };
}
