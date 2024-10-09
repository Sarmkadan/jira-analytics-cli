# ConfigurationException

Represents an error that occurs during the loading or validation of application configuration settings in the `jira-analytics-cli` project. This exception is designed to provide context regarding which configuration property caused the failure, allowing for clearer error reporting.

## API

### PropertyName
`public string? PropertyName { get; set; }`

Gets or sets the name of the configuration property associated with this exception. Returns `null` if the exception is not specific to a single property.

### Constructors

The `ConfigurationException` class provides the following constructors:

- `public ConfigurationException()`
- `public ConfigurationException(string message)`
- `public ConfigurationException(string message, string propertyName)`

These constructors allow for the initialization of the exception with optional error messages and association with a specific configuration property.

## Usage

### Basic Usage

```csharp
if (string.IsNullOrEmpty(config.JiraUrl))
{
    throw new ConfigurationException("Jira URL is required.");
}
```

### Using PropertyName for Context

```csharp
if (config.RetryLimit < 0)
{
    throw new ConfigurationException("Retry limit cannot be negative.", nameof(config.RetryLimit));
}
```

## Notes

- **Thread-Safety:** This exception class is thread-safe, as it does not maintain internal state that is modified after instantiation.
- **Edge Cases:** Ensure that `PropertyName` is explicitly handled in logging or UI display logic to provide helpful feedback to the user when it is not null.
