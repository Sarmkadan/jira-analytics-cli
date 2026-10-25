# JqlQueryService

`JqlQueryService` runs custom JQL (Jira Query Language) searches through `IJiraApiService` and returns the results as a `JqlQueryResult`. The result carries pagination data and a success flag, so callers can page through large result sets and report failures without catching exceptions. The class also has static helpers for building common JQL clauses and for printing results as a console table.

The source is in `Services/JqlQueryService.cs`. The companion validation utility, `JqlHelper`, lives in `Services/JqlHelper.cs` and is covered in its own section below.

## Contents

- [Overview](#overview)
- [Dependencies and registration](#dependencies-and-registration)
- [Query execution](#query-execution)
- [Request and result models](#request-and-result-models)
- [Static helpers](#static-helpers)
- [Text formatting](#text-formatting)
- [JqlHelper](#jqlhelper)
- [Error handling and logging](#error-handling-and-logging)
- [Known limitations](#known-limitations)
- [Related components](#related-components)

## Overview

| Member | Kind | Purpose |
|---|---|---|
| `IJqlQueryService` | interface | Contract used by callers and DI |
| `JqlQueryService` | class | Implementation; wraps `IJiraApiService.SearchByJqlAsync` |
| `JqlQueryRequest` | class | Parameters for a structured query (JQL, page size, offset, label) |
| `JqlQueryResult` | class | Issues, total count, pagination state, and success/error information |
| `BuildProjectJql`, `BuildLabelJql`, `BuildDateRangeJql` | static methods | Build JQL clauses as strings |
| `FormatAsText` | static method | Render a `JqlQueryResult` as a fixed-width console table |
| `JqlHelper` | static class | Validate project and issue keys, sprint IDs, and paging values |

The service does not build JQL by itself. It sends the string it receives to Jira unchanged, so the caller owns the query text. The static helpers exist to make common clauses easier to assemble.

## Dependencies and registration

The constructor takes two dependencies and rejects `null` for either one with `ArgumentNullException`:

- `IJiraApiService` — performs the HTTP call. The concrete `JiraApiService` sends the request to `/rest/api/3/search` with the JQL URL-encoded.
- `ILogger<JqlQueryService>` — logs each execution and each failure.

In the application, `ServiceFactory.cs` registers the service as a singleton:

```csharp
services.AddSingleton<IJqlQueryService, JqlQueryService>();
```

Code that needs the service should depend on `IJqlQueryService` rather than on the class, which keeps it easy to mock in tests. `Program.cs` resolves it through `GetRequiredService<IJqlQueryService>()` for the `jql` command.

## Query execution

There are two overloads of `ExecuteQueryAsync`. Both return `Task<JqlQueryResult>`.

### `ExecuteQueryAsync(string jql, int maxResults = 50)`

A shortcut for the common case. It validates the query and then delegates to the request overload with `StartAt = 0`.

- **Parameters**
  - `jql` — the JQL string. Must not be null, empty, or whitespace.
  - `maxResults` — the page size. Defaults to `50`. The service does not validate or clamp this value itself.
- **Throws** `ArgumentException` if `jql` is null, empty, or whitespace.

```csharp
JqlQueryResult result = await jqlQueryService.ExecuteQueryAsync(
    "project = PROJ AND statusCategory != Done ORDER BY priority DESC",
    maxResults: 25);
```

### `ExecuteQueryAsync(JqlQueryRequest request)`

The structured form. Use it when you need `StartAt`, or when you want to attach a label.

- **Parameters**
  - `request` — must not be `null`. Its `Jql` property must not be null, empty, or whitespace.
- **Throws**
  - `ArgumentNullException` if `request` is `null`.
  - `ArgumentException` if `request.Jql` is null, empty, or whitespace.

The method logs the query at `Information` level, calls `IJiraApiService.SearchByJqlAsync(jql, MaxResults, StartAt)`, and copies the response into a new `JqlQueryResult`. It sets `IsSuccess = true` and copies `Jql`, `Total`, `StartAt` (from the API response), `MaxResults` (from the request), and `Issues`.

Two details that are easy to miss:

- **Failures do not throw.** Any exception from the API call — HTTP errors, `JiraApiException`, or argument checks made by the API layer, such as a non-positive `MaxResults` — is caught. The method returns a result with `IsSuccess = false` and `ErrorMessage` set to the exception message. Callers must check `IsSuccess` before reading `Issues`.
- **No cancellation token.** The service does not accept a `CancellationToken`, even though `IJiraApiService.SearchByJqlAsync` does. A cancellation request therefore cannot reach the HTTP call through this service.

### Paging through results

`JqlQueryResult.HasMore` is true while more issues remain. Advance `StartAt` by the number of issues returned:

```csharp
var request = new JqlQueryRequest
{
    Jql        = "project = PROJ ORDER BY created ASC",
    MaxResults = 50,
    StartAt    = 0
};

var all = new List<JiraIssue>();

while (true)
{
    var page = await jqlQueryService.ExecuteQueryAsync(request);
    if (!page.IsSuccess)
    {
        throw new InvalidOperationException(page.ErrorMessage);
    }

    all.AddRange(page.Issues);

    if (!page.HasMore || page.Issues.Count == 0)
    {
        break;
    }

    request.StartAt += page.Issues.Count;
}
```

The `page.Issues.Count == 0` guard stops the loop if Jira returns an empty page before `Total` is reached. Without it, the loop would never end.

## Request and result models

Both models are defined in `Services/IJqlQueryService.cs`. Property names are annotated with `System.Text.Json` attributes, so the `json` output format of the CLI uses the camelCase names shown below.

### `JqlQueryRequest`

| Property | Type | Default | JSON name | Description |
|---|---|---|---|---|
| `Jql` | `string` | `""` | `jql` | The JQL query to run |
| `MaxResults` | `int` | `50` | `maxResults` | Page size |
| `StartAt` | `int` | `0` | `startAt` | Zero-based index of the first issue to return |
| `Label` | `string?` | `null` | `label` | Optional human-readable name for the query, intended for report headings |

The `Label` property is stored and serialized, but the service does not read it. Nothing in the service changes behavior based on it.

### `JqlQueryResult`

| Property | Type | Default | JSON name | Description |
|---|---|---|---|---|
| `Jql` | `string` | `""` | `jql` | The query that was executed |
| `Total` | `int` | `0` | `total` | Number of matching issues reported by Jira |
| `StartAt` | `int` | `0` | `startAt` | Index of the first returned issue |
| `MaxResults` | `int` | `0` | `maxResults` | Page size requested |
| `Issues` | `List<JiraIssue>` | empty | `issues` | Issues in this page |
| `ExecutedAt` | `DateTime` | `DateTime.UtcNow` at creation | `executedAt` | UTC time the result object was created |
| `IsSuccess` | `bool` | `true` | `isSuccess` | `false` when the query failed |
| `ErrorMessage` | `string?` | `null` | `errorMessage` | Failure message when `IsSuccess` is `false` |
| `HasMore` | `bool` | computed | not serialized | `true` when `StartAt + Issues.Count < Total` |

`HasMore` is marked `[JsonIgnore]`, so it does not appear in JSON output.

`JiraIssue` is the same model used across the rest of the CLI. `FormatAsText` reads its `Key`, `IssueType`, `Status`, `Assignee`, `Priority`, `StoryPoints`, and `Summary` properties.

## Static helpers

These methods build JQL fragments as strings. They do not call Jira and do not depend on instance state. Each one has a caveat, described below, that matters if you rely on its output.

### `BuildProjectJql(string projectKey)`

Returns `project = "<key>"`, with embedded double quotes doubled.

```csharp
JqlQueryService.BuildProjectJql("PROJ");   // project = "PROJ"
```

Caveat: the implementation replaces `"` with `""`. JQL does not use that syntax; inside a quoted string it escapes quotes with a backslash (`\"`). A key containing a quote therefore produces a query Jira will likely reject. Keys in Jira are normally uppercase letters and digits, so this rarely comes up. If you need strict validation, run the key through `JqlHelper.EscapeProjectKey` first.

### `BuildLabelJql(string label)`

Returns `label = "<label>"`, with the same quote-doubling as `BuildProjectJql` and the same caveat.

```csharp
JqlQueryService.BuildLabelJql("release-2026");   // label = "release-2026"
```

### `BuildDateRangeJql(string field, DateTime? from, DateTime? to)`

Builds `>=` and `<=` clauses for a date field. Dates are formatted as `yyyy-MM-dd`. The method uses the `DateTime` value as given and does not convert it to UTC.

```csharp
JqlQueryService.BuildDateRangeJql("updated", DateTime.UtcNow.AddDays(-7), null);
// updated >= "2026-10-18"

JqlQueryService.BuildDateRangeJql("created", null, null);
// ""  (empty string)
```

Caveat: when both `from` and `to` are given, the two clauses are joined with a space and no connector:

```csharp
JqlQueryService.BuildDateRangeJql("created", new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));
// created >= "2026-01-01" created <= "2026-01-31"
```

Jira will reject this query, because the two clauses need `AND` between them. Do not pass both bounds to this helper. To use both bounds, build the clauses in a list and join them with `AND`:

```csharp
var clauses = new List<string> { JqlQueryService.BuildProjectJql("PROJ") };

if (from.HasValue)
{
    clauses.Add($"created >= \"{from.Value:yyyy-MM-dd}\"");
}

if (to.HasValue)
{
    clauses.Add($"created <= \"{to.Value:yyyy-MM-dd}\"");
}

var jql = string.Join(" AND ", clauses) + " ORDER BY created DESC";
```

Joining a list also avoids a dangling `AND` when one of the bounds is missing.

### Combining clauses

The helpers return fragments, not complete queries. Join them with `AND` and add `ORDER BY` yourself, as in the README example:

```csharp
var projectJql = JqlQueryService.BuildProjectJql("PROJ");
var dateJql    = JqlQueryService.BuildDateRangeJql("updated", DateTime.UtcNow.AddDays(-7), null);
var jql        = $"{projectJql} AND {dateJql} ORDER BY updated DESC";
```

This works because only one date bound is used. See the caveat above before passing both bounds.

## Text formatting

`FormatAsText(JqlQueryResult result)` returns a multi-line string for console output or a text file. The `jql` command uses it when `--format` is `text`.

Output layout:

1. A header with the JQL, the total count and the number of issues shown, the start index, and the execution time in UTC.
2. A column header and a separator line of 105 `─` characters.
3. One row per issue: key (15 characters), type (12), status (16), assignee (20, `Unassigned` if null), priority (10), story points (6, `-` if null), and summary.
4. If `HasMore` is true, a hint with the next `--start-at` value.

Behavior details:

- If `IsSuccess` is `false`, the method returns `Query failed: <ErrorMessage>` and nothing else.
- If there are no issues, it prints `No issues matched the query.` after the header.
- Summaries longer than 60 characters are cut to 57 characters and suffixed with `...`.
- Each column is truncated to its width, so long values do not shift the columns.

Illustrative output (column spacing is approximate):

```
JQL  : project = PROJ ORDER BY created DESC
Total: 42 matching issues — showing 2 (page start: 0)
Run  : 2026-10-25 09:12:44 UTC

Key             Type         Status           Assignee             Priority   Pts    Summary
─────────────────────────────────────────────────────────────────────────────────────────────────────────
PROJ-101        Bug          In Progress      Alice                High       3      Login fails after timeout
PROJ-102        Story        To Do            Unassigned           Medium     -      Add export to CSV
  ...40 more result(s). Use --start-at 2 to see the next page.
```

The hint refers to `--start-at`, which is a CLI option. Programmatic callers should use `StartAt` on `JqlQueryRequest`.

## JqlHelper

`JqlHelper` (`Services/JqlHelper.cs`) is a static class of validation and clamping utilities for values that go into JQL and paging parameters. It is separate from `JqlQueryService`: the service does not call it, and no other code in the repository calls it at the time of writing. Use it in code that builds JQL, before the string reaches the service.

The "Escape" methods do not escape anything. They validate the input and return it unchanged. Their purpose is to reject values that would produce a malformed or unsafe query, so that bad input fails early with a clear message.

### Constants

| Constant | Value | Used by |
|---|---|---|
| `MaxSprintId` | `int.MaxValue / 2` | `EscapeSprintId` |
| `MaxProjectKeyLength` | `100` | `EscapeProjectKey` |
| `MaxIssueKeyLength` | `100` | `EscapeIssueKey` |

### `EscapeSprintId(int sprintId)`

Returns `sprintId.ToString()` if `sprintId` is between `1` and `MaxSprintId` inclusive.

- **Throws** `ArgumentOutOfRangeException` for values `<= 0` or `> MaxSprintId`.

```csharp
var jql = $"sprint = {JqlHelper.EscapeSprintId(sprintId)} ORDER BY created DESC";
```

### `EscapeProjectKey(string projectKey)`

Returns the key unchanged if it is valid.

- **Rules**: not null or whitespace; at most 100 characters; only ASCII uppercase letters and digits.
- **Throws** `ArgumentException` on any violation.

```csharp
var key = JqlHelper.EscapeProjectKey("PROJ");   // "PROJ"
JqlHelper.EscapeProjectKey("proj");             // throws: uppercase letters and digits only
```

Note that the check is stricter than what Jira accepts in some cases. Jira keys are normally uppercase, so this is usually the desired behavior. If a key has lowercase letters, normalize it first with `ToUpperInvariant()`.

Combined with the service:

```csharp
var key = JqlHelper.EscapeProjectKey(projectKey);
var result = await jqlQueryService.ExecuteQueryAsync(
    $"project = {key} AND statusCategory != Done ORDER BY rank");
```

### `EscapeIssueKey(string issueKey)`

Returns the key unchanged if it has the form `PROJECT-123`.

- **Rules**: not null or whitespace; at most 100 characters; uppercase ASCII letters, then exactly one dash, then digits. The dash cannot be first or last. Both a letter part and a digit part are required.
- **Throws** `ArgumentException` on any violation, with a message that says which rule failed.

```csharp
JqlHelper.EscapeIssueKey("PROJ-42");   // "PROJ-42"
JqlHelper.EscapeIssueKey("PROJ-");     // throws: format PROJECT-123
JqlHelper.EscapeIssueKey("42-PROJ");   // throws: digits must follow the dash
```

### `ClampMaxResults`, `ClampPageSize`, `ClampStartAt`

These take the requested paging value and the limits from an `IJiraApiServiceConfig`, and return a safe value.

| Method | Behavior |
|---|---|
| `ClampMaxResults(maxResults, config)` | Throws `ArgumentOutOfRangeException` if `maxResults <= 0`. Otherwise returns `Math.Min(maxResults, config.JiraApiMaxResultsPerRequest)`. |
| `ClampPageSize(pageSize, config)` | Throws `ArgumentOutOfRangeException` if `pageSize <= 0`. Otherwise returns `Math.Min(pageSize, config.JiraApiMaxPageSize)`. |
| `ClampStartAt(startAt, config)` | Returns `Math.Max(0, Math.Min(startAt, config.JiraApiMaxResultsPerRequest * config.JiraApiMaxPages))`. Negative values become `0`. Does not throw. |

Caveats:

- The methods do not log when they clamp. The code has comments saying the caller should log, but nothing does.
- `IJiraApiServiceConfig` is an interface with three read-only properties: `JiraApiMaxResultsPerRequest`, `JiraApiMaxPages`, and `JiraApiMaxPageSize`. No class in this repository implements it yet, so you must provide an implementation to use these methods.

A minimal implementation for tests or ad hoc code:

```csharp
private sealed record JqlLimits(
    int JiraApiMaxResultsPerRequest,
    int JiraApiMaxPages,
    int JiraApiMaxPageSize) : IJiraApiServiceConfig;

var limits = new JqlLimits(JiraApiMaxResultsPerRequest: 100, JiraApiMaxPages: 20, JiraApiMaxPageSize: 100);

int maxResults = JqlHelper.ClampMaxResults(500, limits);   // 100
int startAt    = JqlHelper.ClampStartAt(5000, limits);     // 2000
```

Because `JqlQueryService` does not clamp `MaxResults`, you can apply `ClampMaxResults` before building the request:

```csharp
var request = new JqlQueryRequest
{
    Jql        = jql,
    MaxResults = JqlHelper.ClampMaxResults(requestedMax, limits),
    StartAt    = JqlHelper.ClampStartAt(requestedStart, limits)
};
```

## Error handling and logging

| Situation | Behavior |
|---|---|
| `null` request | `ArgumentNullException` |
| Null, empty, or whitespace JQL | `ArgumentException` |
| API or HTTP failure, including `JiraApiException` | Caught; returns `IsSuccess = false` with `ErrorMessage` |
| Cancellation | Not supported by this service |
| Successful query | `Information` log with page size, start index, and JQL; a second `Information` log with the returned and total counts |
| Failed query | `Error` log with the exception and the JQL |

Because failures are returned rather than thrown, check `IsSuccess` after every call. Do not assume `Issues` is populated.

Be aware that the JQL text is written to the logs in full. Do not put secrets in JQL.

## Known limitations

These describe the current behavior. Changing them would change behavior, so they are documented here rather than fixed.

1. **Quote escaping in `BuildProjectJql` and `BuildLabelJql`** doubles the quote character (`""`) instead of backslash-escaping it (`\"`). Keys or labels containing quotes produce invalid JQL.
2. **`BuildDateRangeJql` with both bounds** omits the `AND` between clauses, producing invalid JQL. The single-bound case is valid.
3. **No cancellation support** on `JqlQueryService`.
4. **No clamping in the service.** `MaxResults` and `StartAt` are passed to the API as given. Use `JqlHelper` clamp methods if you need limits.
5. **`JqlHelper` is unused** in the current codebase and its clamp methods need a caller-supplied `IJiraApiServiceConfig`.
6. **`JqlQueryRequest.Label` is not used** by the service or by `FormatAsText`.

The unit tests for the static helpers are in `tests/jira-analytics-cli.Tests/Services/JqlQueryServiceTests.cs`. That file is not listed in `jira-analytics-cli.Tests.csproj`, so it is not compiled or run. Its quote-escaping expectation (`PROJ\"KEY`) also does not match the implementation, which produces `PROJ""KEY`. Fixing the test list or the test expectations is a separate change.

## Related components

- `IJqlQueryService` (`Services/IJqlQueryService.cs`) — interface and request/result models.
- `IJiraApiService.SearchByJqlAsync` (`Services/IJiraApiService.cs`) — the underlying search call. It takes `CancellationToken` and validates `maxResults` and `startAt`.
- `Program.cs` `jql` command — exposes the service through `-q/--query`, `-n/--max-results` (default 50), `--start-at`, `-o/--output`, and `-f/--format` (`text` or `json`).
- `README.md` — the `JqlQueryService` section with a short usage example.
- `docs/IJqlQueryService.md` — documentation for the interface.
