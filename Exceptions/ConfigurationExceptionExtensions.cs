// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace JiraAnalyticsCli.Exceptions;

/// <summary>
/// Extension methods for <see cref="ConfigurationException"/> that provide additional functionality
/// for working with configuration validation errors.
/// </summary>
public static class ConfigurationExceptionExtensions
{
    /// <summary>
    /// Creates a new <see cref="ConfigurationException"/> with an additional context property.
    /// </summary>
    /// <param name="exception">The original exception (cannot be null)</param>
    /// <param name="additionalPropertyName">The name of the additional property that failed validation</param>
    /// <param name="additionalPropertyValue">The value of the additional property</param>
    /// <returns>A new <see cref="ConfigurationException"/> with combined context</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null</exception>
    public static ConfigurationException WithContext(
        this ConfigurationException exception,
        string additionalPropertyName,
        [DisallowNull] object? additionalPropertyValue)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(additionalPropertyName);

        string message = exception.PropertyName is null
            ? exception.Message
            : $"{exception.Message} (Additional context: {additionalPropertyName} = {additionalPropertyValue})";

        return new ConfigurationException(message, exception.PropertyName ?? additionalPropertyName)
        {
            Source = exception.Source,
            HelpLink = exception.HelpLink
        };
    }

    /// <summary>
    /// Creates a new <see cref="ConfigurationException"/> with a formatted message that includes the property name
    /// and value for better debugging information.
    /// </summary>
    /// <param name="exception">The original exception (cannot be null)</param>
    /// <param name="propertyValue">The value of the property that caused the exception</param>
    /// <returns>A new <see cref="ConfigurationException"/> with formatted message</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null</exception>
    public static ConfigurationException WithValueContext(
        this ConfigurationException exception,
        [AllowNull] object? propertyValue)
    {
        ArgumentNullException.ThrowIfNull(exception);

        string message = exception.PropertyName is null
            ? exception.Message
            : $"{exception.Message} (Property: {exception.PropertyName}, Value: {FormatPropertyValue(propertyValue)}) ";

        return new ConfigurationException(message, exception);
    }

    /// <summary>
    /// Determines whether this exception represents a missing configuration value
    /// (i.e., the PropertyName is set and the message indicates it's missing).
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if this is a missing configuration exception; otherwise false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null</exception>
    public static bool IsMissingConfiguration(this ConfigurationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.PropertyName is not null
               && exception.Message.Contains("missing", StringComparison.OrdinalIgnoreCase)
               && exception.Message.Contains("required", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether this exception represents an invalid configuration value
    /// (i.e., the PropertyName is set and the message indicates it's invalid).
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if this is an invalid configuration exception; otherwise false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null</exception>
    public static bool IsInvalidConfiguration(this ConfigurationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.PropertyName is not null
               && exception.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Formats a property value for display in error messages.
    /// Handles null values, strings, and other common types appropriately.
    /// </summary>
    private static string FormatPropertyValue(object? value)
    {
        return value switch
        {
            null => "null",
            string str => $"\"{str}\"",
            { } when value is IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            { } => value.ToString() ?? "null"
        };
    }
}