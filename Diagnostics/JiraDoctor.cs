// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Net.Http.Headers;
using JiraAnalyticsCli.Configuration;

namespace JiraAnalyticsCli.Diagnostics;

/// <summary>
/// Runs the <c>doctor</c> health checks in order and formats the results as a table.
/// </summary>
/// <remarks>
/// Each check runs under its own timeout, taken from <see cref="JiraOptions.TimeoutSeconds"/>. A check whose
/// prerequisite failed is reported as Skipped, so one root cause does not produce a list of follow-on failures.
/// The order is: configuration, DNS/TLS reachability, authentication, board, closed sprint, story-points field.
/// </remarks>
public static partial class JiraDoctor
{
    /// <summary>Name of the configuration check.</summary>
    public const string ConfigurationCheck = "Configuration";

    /// <summary>Name of the DNS, TCP and TLS check.</summary>
    public const string ReachabilityCheck = "DNS/TLS reachability";

    /// <summary>Name of the authenticated <c>/myself</c> check.</summary>
    public const string AuthenticationCheck = "Authentication";

    /// <summary>Name of the board lookup check.</summary>
    public const string BoardCheck = "Board";

    /// <summary>Name of the closed-sprint check.</summary>
    public const string ClosedSprintCheck = "Closed sprint";

    /// <summary>Name of the story-points field check.</summary>
    public const string StoryPointsCheck = "Story points field";

    /// <summary>
    /// Runs every check, stopping each branch at the first failed prerequisite.
    /// </summary>
    /// <param name="jira">Result of <see cref="JiraOptionsLoader.Load"/>.</param>
    /// <param name="velocity">Result of <see cref="VelocityOptionsLoader.Load"/>. Its story-points field is the one checked.</param>
    /// <returns>One result per check, in the order they run.</returns>
    public static async Task<IReadOnlyList<DoctorCheckResult>> RunAsync(
        JiraOptionsLoadResult jira,
        VelocityOptionsLoadResult velocity)
    {
        ArgumentNullException.ThrowIfNull(jira);
        ArgumentNullException.ThrowIfNull(velocity);

        var results = new List<DoctorCheckResult>();

        var configuration = CheckConfiguration(jira, velocity);
        results.Add(configuration);
        if (configuration.Status != DoctorStatus.Ok)
        {
            results.AddRange(new[] { AuthenticationCheck, BoardCheck, ClosedSprintCheck, StoryPointsCheck }
                .Select(name => DoctorCheckResult.Skip(name, ConfigurationCheck)));
            return results;
        }

        var options = jira.Options;
        var timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        var baseUri = new Uri(options.BaseUrl);

        // The timeout is applied per check by RunWithTimeoutAsync, so the HttpClient itself has none.
        using var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var reachability = await RunWithTimeoutAsync(ReachabilityCheck, timeout,
            ct => JiraDoctorNetwork.CheckAsync(baseUri, ct));
        results.Add(reachability);
        if (reachability.Status != DoctorStatus.Ok)
        {
            results.AddRange(new[] { AuthenticationCheck, BoardCheck, ClosedSprintCheck, StoryPointsCheck }
                .Select(name => DoctorCheckResult.Skip(name, ReachabilityCheck)));
            return results;
        }

        var authentication = await RunWithTimeoutAsync(AuthenticationCheck, timeout,
            ct => CheckAuthenticationAsync(http, baseUri, ct));
        results.Add(authentication);
        if (authentication.Status != DoctorStatus.Ok)
        {
            results.AddRange(new[] { BoardCheck, ClosedSprintCheck, StoryPointsCheck }
                .Select(name => DoctorCheckResult.Skip(name, AuthenticationCheck)));
            return results;
        }

        var board = await RunWithTimeoutAsync(BoardCheck, timeout,
            ct => CheckBoardAsync(http, baseUri, options.BoardId, ct));
        results.Add(board);

        if (board.Status == DoctorStatus.Ok && options.BoardId is { } boardId)
        {
            results.Add(await RunWithTimeoutAsync(ClosedSprintCheck, timeout,
                ct => CheckClosedSprintAsync(http, baseUri, boardId, ct)));
        }
        else
        {
            results.Add(DoctorCheckResult.Skip(ClosedSprintCheck, BoardCheck));
        }

        // Story points depend only on authentication, so this runs even when the board check failed.
        var storyPointsField = velocity.Options?.StoryPointsField ?? VelocityOptions.DefaultStoryPointsField;
        results.Add(await RunWithTimeoutAsync(StoryPointsCheck, timeout,
            ct => CheckStoryPointsFieldAsync(http, baseUri, storyPointsField, ct)));

        return results;
    }

    /// <summary>
    /// True when at least one check failed. Skipped checks do not count.
    /// </summary>
    /// <param name="results">Results returned by <see cref="RunAsync"/>.</param>
    public static bool HasFailures(IReadOnlyList<DoctorCheckResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.Any(result => result.Status == DoctorStatus.Failed);
    }

    /// <summary>
    /// Formats the results as a table with one row per check, then the remediation hint for each failure.
    /// </summary>
    /// <param name="results">Results returned by <see cref="RunAsync"/>.</param>
    /// <returns>Text for stdout.</returns>
    public static string Format(IReadOnlyList<DoctorCheckResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var rows = results
            .Select(result => (Check: result.Name, Status: Label(result.Status), Detail: result.Detail))
            .ToList();
        var headers = (Check: "Check", Status: "Status", Detail: "Detail");

        var checkWidth = Math.Max(headers.Check.Length, rows.Count == 0 ? 0 : rows.Max(row => row.Check.Length));
        var statusWidth = Math.Max(headers.Status.Length, rows.Count == 0 ? 0 : rows.Max(row => row.Status.Length));

        var lines = new List<string>
        {
            $"{headers.Check.PadRight(checkWidth)}  {headers.Status.PadRight(statusWidth)}  {headers.Detail}",
            $"{new string('-', checkWidth)}  {new string('-', statusWidth)}  {new string('-', headers.Detail.Length)}"
        };
        lines.AddRange(rows.Select(row =>
            $"{row.Check.PadRight(checkWidth)}  {row.Status.PadRight(statusWidth)}  {row.Detail}"));

        var fixes = results.Where(result => result.Status == DoctorStatus.Failed && result.Hint is not null).ToList();
        if (fixes.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("How to fix:");
            lines.AddRange(fixes.Select(fix => $"  - {fix.Name}: {fix.Hint}"));
        }

        var failures = results.Count(result => result.Status == DoctorStatus.Failed);
        lines.Add(string.Empty);
        lines.Add(failures == 0
            ? $"All {results.Count} check(s) passed."
            : $"{failures} of {results.Count} check(s) failed.");

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Runs one check. If it does not finish within <paramref name="timeout"/>, it is cancelled and reported as failed.
    /// </summary>
    private static async Task<DoctorCheckResult> RunWithTimeoutAsync(
        string name,
        TimeSpan timeout,
        Func<CancellationToken, Task<DoctorCheckResult>> check)
    {
        using var cancellation = new CancellationTokenSource();
        var work = RunSafelyAsync(name, check, cancellation.Token);

        // Waiting on the timer as well as the work guarantees the timeout even when a call does not observe its token.
        var finished = await Task.WhenAny(work, Task.Delay(timeout));
        if (finished == work)
            return await work;

        cancellation.Cancel();
        return DoctorCheckResult.Fail(name,
            $"no answer within {timeout.TotalSeconds.ToString("0.#", CultureInfo.InvariantCulture)} s",
            "Check the network path to the Jira host, or raise the limit with --timeout-seconds (JIRA_API_TIMEOUT_SECONDS).");
    }

    /// <summary>
    /// Runs one check and turns an unexpected exception into a failed result, so one check cannot stop the rest.
    /// </summary>
    private static async Task<DoctorCheckResult> RunSafelyAsync(
        string name,
        Func<CancellationToken, Task<DoctorCheckResult>> check,
        CancellationToken cancellationToken)
    {
        try
        {
            return await check(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return DoctorCheckResult.Fail(name,
                $"unexpected error: {ex.Message}",
                "Run the command again. If it keeps failing, check the network path to the Jira host.");
        }
        catch (OperationCanceledException)
        {
            return DoctorCheckResult.Fail(name, "cancelled", "Run the command again.");
        }
    }

    private static DoctorCheckResult CheckConfiguration(JiraOptionsLoadResult jira, VelocityOptionsLoadResult velocity)
    {
        var problems = new List<string>(jira.Problems);
        if (velocity.Problems.Count > 0)
            problems.AddRange(velocity.Problems);

        if (problems.Count == 0)
            return DoctorCheckResult.Pass(ConfigurationCheck, "Jira and velocity settings are valid");

        return DoctorCheckResult.Fail(ConfigurationCheck,
            string.Join(" ", problems),
            "Fix the settings named in the detail. Each message says where the setting can be set.");
    }

    private static string Label(DoctorStatus status) => status switch
    {
        DoctorStatus.Ok => "OK",
        DoctorStatus.Failed => "FAIL",
        _ => "SKIP"
    };
}
