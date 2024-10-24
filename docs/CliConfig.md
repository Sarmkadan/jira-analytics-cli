# CliConfig

`CliConfig` holds the runtime configuration parameters for the Jira Analytics CLI tool. It centralizes connection details, caching behavior, logging verbosity, and output preferences so that all application components operate against a single validated configuration object. An instance is typically populated from command-line arguments, environment variables, or a configuration file before being passed to the analytics engine.

## API

### `public string JiraBaseUrl`

The root URL of the target Jira instance (for example, `https://your-domain.atlassian.net`). Must be an absolute URI. The value is required; validation will reject null, empty, or malformed URLs.

### `public string JiraApiToken`

The bearer token or personal access token used to authenticate against the Jira REST API. This field is required and must be non-empty. It is treated as a secret and should never be logged in plain text, even when `EnableDetailedLogging` is active.

### `public string? DefaultProject`

An optional Jira project key (for example, `PROJ`) that scopes queries when no explicit project is supplied at runtime. When set to `null`, commands that require a project will demand one at invocation time.

### `public int CacheExpirationMinutes`

The number of minutes a cached API response remains valid before a fresh request is forced. Must be greater than or equal to zero. A value of zero disables caching entirely, causing every query to hit the live API.

### `public bool EnableDetailedLogging`

When `true`, the application emits verbose diagnostic output including request URLs, timing information, and cache hit/miss events. When `false`, only warnings and errors are written. Does not affect the `ExportFormat` output stream.

### `public int DefaultSprintCount`

The default number of recent sprints to include in velocity and trend reports when no explicit count is provided. Must be a positive integer. Validation rejects zero and negative values.

### `public string ExportFormat`

Specifies the output format for exported reports. Accepted values are determined by the validation logic (typically `json`, `csv`, or `markdown`). The string is case-insensitive during validation but stored as provided.

### `public void Validate()`

Performs a full consistency check on all configuration fields.

- **Parameters:** none.
- **Return value:** void.
- **Throws:** `ArgumentException` when `JiraBaseUrl` is null, empty, or not a well-formed absolute URI. `ArgumentException` when `JiraApiToken` is null or empty. `ArgumentOutOfRangeException` when `CacheExpirationMinutes` is negative. `ArgumentOutOfRangeException` when `DefaultSprintCount` is less than 1. `ArgumentException` when `ExportFormat` is not one of the recognized format identifiers.

Callers must invoke `Validate()` before using the configuration. The method does not perform network connectivity checks; it only validates structural correctness of the fields.

## Usage

### Example 1: Minimal valid configuration

```csharp
var config = new CliConfig
{
    JiraBaseUrl = "https://mycompany.atlassian.net",
    JiraApiToken = Environment.GetEnvironmentVariable("JIRA_TOKEN"),
    DefaultProject = null,
    CacheExpirationMinutes = 15,
    EnableDetailedLogging = false,
    DefaultSprintCount = 3,
    ExportFormat = "json"
};

config.Validate(); // succeeds, project is optional
```

### Example 2: Configuration with explicit project and disabled cache

```csharp
var config = new CliConfig
{
    JiraBaseUrl = "https://mycompany.atlassian.net",
    JiraApiToken = "ATATT3xFfGF0...",
    DefaultProject = "DEV",
    CacheExpirationMinutes = 0,
    EnableDetailedLogging = true,
    DefaultSprintCount = 5,
    ExportFormat = "csv"
};

config.Validate(); // cache disabled, detailed logging on
```

## Notes

- **Thread safety:** `CliConfig` is a plain data object with no internal synchronization. After `Validate()` completes successfully, the instance should be treated as immutable and shared freely across threads. If fields are mutated after validation, the caller must re-validate and ensure no concurrent readers are using stale values.
- **Validation scope:** `Validate()` does not verify that `JiraBaseUrl` is reachable or that `JiraApiToken` is accepted by the server. Runtime authentication failures will surface as exceptions from the HTTP layer, not from configuration validation.
- **`DefaultProject` null handling:** When `DefaultProject` is `null`, every command that requires a project scope must receive an explicit project argument. The configuration itself remains valid regardless.
- **`ExportFormat` casing:** Validation is case-insensitive, but downstream formatters may rely on the exact stored string. Supplying a recognized value in unexpected casing (e.g., `JSON` instead of `json`) will pass validation but could cause mismatches if consumers perform case-sensitive comparisons. Normalizing to lowercase before storage is recommended.
- **`CacheExpirationMinutes` at zero:** A value of zero bypasses the cache layer entirely. This is distinct from a very small positive value, which would still consult the cache and potentially return stale data within that narrow window.
- **`EnableDetailedLogging` and secrets:** Even when detailed logging is enabled, the application must suppress the token value. The configuration object itself does not enforce this; it is the responsibility of the logging pipeline.
