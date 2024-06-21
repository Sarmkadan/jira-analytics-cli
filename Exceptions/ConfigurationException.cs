// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Exceptions;

/// <summary>
/// Exception thrown when configuration is invalid or missing required settings
/// </summary>
public class ConfigurationException : Exception
{
    public string? PropertyName { get; }

    public ConfigurationException(string message)
        : base(message) { }

    public ConfigurationException(string message, string propertyName)
        : base(message)
    {
        PropertyName = propertyName;
    }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }
}
