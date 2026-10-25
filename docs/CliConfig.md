# CliConfig

`CliConfig` (namespace `JiraAnalyticsCli.Configuration`) is the runtime settings object for the CLI. It implements `ICliConfig`, which exposes read-only properties. Application code receives it through `ICliConfig`, which `ServiceFactory` registers as a singleton after `AppConfigurationProvider.LoadConfiguration()` builds and validates it.

This page lists every option, the environment variable and `appsettings.json` key that set it, its default, and what `Validate()` enforces. The loading rules are described in more detail in the Configuration section of the [README](../README.md).

## Sources and precedence

Each source overwrites the one before it:

1. Built-in defaults in `CliConfig`.
2. `appsettings.json` in the current working directory. Keys are camelCase. A missing file is skipped silently.
3. Environment variables (see the table below).
4. Jira values from `JiraOptions`, which are resolved from CLI flags, then environment variables, then `appsettings.json`. These always replace the values from steps 2 and 3 for the four settings marked **JiraOptions** below.

Environment variables that are unset or empty are ignored.

## Options

| Property | `appsettings.json` key | Environment variable(s) | Default | Allowed values |
|---|---|---|---|---|
| `JiraBaseUrl` | `jiraBaseUrl` | `JIRA_URL`, `JIRA_BASE_URL` | `https://jira.atlassian.net` | Absolute `http` or `https` URI with a host. **JiraOptions** (`BaseUrl`) overrides it. |
| `JiraApiToken` | `jiraApiToken` | `JIRA_TOKEN`, `JIRA_API_TOKEN` | `""` | Non-empty. Sent as a Bearer token. **JiraOptions** (`ApiToken`) overrides it. |
| `DefaultProject` | `defaultProject` | `JIRA_PROJECT`, `JIRA_DEFAULT_PROJECT` | `null` | Any string or `null`. **JiraOptions** (`ProjectKey`) overrides it. A per-command `-p/--project` still takes precedence. |
| `JiraApiTimeoutSeconds` | `jiraApiTimeoutSeconds` | `JIRA_API_TIMEOUT_SECONDS` | `30` | Integer 1–300 from the environment. Used as the `HttpClient` timeout. **JiraOptions** (`TimeoutSeconds`) overrides it. |
| `CacheExpirationMinutes` | `cacheExpirationMinutes` | `CACHE_EXPIRATION_MINUTES` | `15` | Integer 0–10080 from the environment. JSON values must be ≥ 0. |
| `EnableDetailedLogging` | `enableDetailedLogging` | `DETAILED_LOGGING` | `false` | `true` or `false`, case-insensitive. Other values are ignored. |
| `DefaultSprintCount` | `defaultSprintCount` | `DEFAULT_SPRINT_COUNT` | `5` | Integer 1–100 from the environment. JSON values must be > 0. |
| `ExportFormat` | `exportFormat` | `EXPORT_FORMAT` | `"txt"` | Any string. Not checked by `Validate()`. |
| `JiraApiMaxRetryAttempts` | `jiraApiMaxRetryAttempts` | `JIRA_API_MAX_RETRY_ATTEMPTS` | `5` | Integer 1–10 from the environment. JSON values must be > 0. |
| `JiraApiCircuitBreakerFailureThreshold` | `jiraApiCircuitBreakerFailureThreshold` | `JIRA_API_CIRCUIT_BREAKER_FAILURE_THRESHOLD` | `5` | Integer 1–100 from the environment. JSON values must be > 0. |
| `JiraApiCircuitBreakerDurationSeconds` | `jiraApiCircuitBreakerDurationSeconds` | `JIRA_API_CIRCUIT_BREAKER_DURATION_SECONDS` | `30` | Integer 1–3600 from the environment. JSON values must be > 0. |
| `JiraApiMaxResultsPerRequest` | `jiraApiMaxResultsPerRequest` | `JIRA_API_MAX_RESULTS_PER_REQUEST` | `1000` | Integer 1–1000 from the environment. JSON values must be > 0. |
| `JiraApiMaxPages` | `jiraApiMaxPages` | `JIRA_API_MAX_PAGES` | `100` | Integer 1–1000 from the environment. JSON values must be > 0. |
| `JiraApiMaxPageSize` | `jiraApiMaxPageSize` | `JIRA_API_MAX_PAGE_SIZE` | `200` | Integer 1–1000 from the environment. JSON values must be > 0. |

### Consumers in this tree

Not every option is read by production code yet. Current readers:

- `JiraBaseUrl` and `JiraApiToken` are read by `ServiceFactory` to configure the `jira` `HttpClient` (base address and Bearer header).
- `JiraApiTimeoutSeconds` is read by `ServiceFactory` to set the `HttpClient` timeout.
- `JiraApiMaxResultsPerRequest`, `JiraApiMaxPages` and `JiraApiMaxPageSize` are read by `Services/JqlHelper.cs`.

The following options are loaded and validated, but no code outside `Configuration/` reads them at the time of writing: `DefaultProject`, `CacheExpirationMinutes`, `EnableDetailedLogging`, `DefaultSprintCount`, `ExportFormat`, `JiraApiMaxRetryAttempts`, `JiraApiCircuitBreakerFailureThreshold` and `JiraApiCircuitBreakerDurationSeconds`. Setting them changes the object, but no behavior changes yet. The `-f/--format` option of the export command is separate from `ExportFormat` and is validated by `OutputOptionsValidator`.

## Environment variable rules

- Integer variables must be whole numbers, with optional sign and surrounding whitespace, inside the range shown above. Anything else throws `ConfigurationException` with `PropertyName` set to the variable name. The message never repeats the value, because it may be a credential pasted into the wrong variable.
- `DETAILED_LOGGING` is parsed with `bool.TryParse`. An invalid value is silently ignored and the previous value is kept.
- `EXPORT_FORMAT`, `JIRA_URL`, `JIRA_TOKEN` and `JIRA_PROJECT` accept any non-empty string. Validation of the URL and token happens in `Validate()`.

## `appsettings.json` rules

- Numeric keys must be JSON integers. A value of the wrong type is ignored, and the value from the previous source is kept.
- A string key whose value is not a JSON string stops the rest of the file from being read, so the remaining keys keep their defaults.
- JSON integers are not range-checked by the loader. Only `Validate()` checks them, and it checks lower bounds only.
- If the file is not valid JSON, a warning is logged with the line and byte position, and loading continues with environment variables and defaults. The warning does not quote the file contents.

## Methods

### `public void Validate()`

Checks the whole object and throws `InvalidOperationException` on the first problem. The message names the setting. It does not contact Jira.

Checks, in order:

- `JiraBaseUrl` is non-empty, is an absolute URI with an `http` or `https` scheme, and has a host. The URL is not echoed in the message.
- `JiraApiToken` is non-empty.
- `CacheExpirationMinutes` ≥ 0.
- `DefaultSprintCount`, `JiraApiMaxRetryAttempts`, `JiraApiCircuitBreakerFailureThreshold`, `JiraApiCircuitBreakerDurationSeconds`, `JiraApiTimeoutSeconds`, `JiraApiMaxResultsPerRequest`, `JiraApiMaxPages` and `JiraApiMaxPageSize` are each > 0.

`Validate()` does not check `DefaultProject`, `ExportFormat`, `EnableDetailedLogging`, or upper bounds for JSON-supplied values.

`AppConfigurationProvider.LoadConfiguration()` calls `Validate()`, logs any failure, and rethrows it.

### `public override string ToString()`

Returns a one-line summary for diagnostics: base URL, `JiraApiToken = <redacted>`, default project, timeout, page size and page count. Use it for logging instead of serializing the object.

## Example

Loading the configuration through the provider, which is how the application does it:

```csharp
ICliConfig config = provider.LoadConfiguration(); // applies sources in order, then Validate()

Console.WriteLine(config); // token is redacted
```

Building the object directly, for example in a test:

```csharp
var config = new CliConfig
{
    JiraBaseUrl = "https://mycompany.atlassian.net",
    JiraApiToken = "test-token",
    DefaultProject = "DEV",
    JiraApiTimeoutSeconds = 60,
};

config.Validate(); // throws InvalidOperationException if any rule above fails
```

## Notes

- **Secrets:** `JiraApiToken` must not be logged. `ToString()` redacts it, and the provider never includes it in exception messages. Do not add it to `appsettings.json` in version control; use `JIRA_API_TOKEN` instead.
- **Thread safety:** `CliConfig` is a plain mutable object with no synchronization. Treat it as read-only once `Validate()` has succeeded and share it freely across threads. If you change a property after validation, call `Validate()` again.
- **Units:** timeouts and circuit-breaker durations are in seconds. `CacheExpirationMinutes` is in minutes.
