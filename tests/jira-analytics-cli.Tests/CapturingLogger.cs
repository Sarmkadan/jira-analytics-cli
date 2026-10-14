// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Tests;

/// <summary>
/// One formatted log entry captured by <see cref="CapturingLogger{T}"/>.
/// </summary>
internal sealed record CapturedLogLine(LogLevel Level, string Message);

/// <summary>
/// An <see cref="ILogger{T}"/> that records every formatted message, so tests can assert on log content.
/// </summary>
internal sealed class CapturingLogger<T> : ILogger<T>
{
    /// <summary>Every entry written, in order.</summary>
    public List<CapturedLogLine> Messages { get; } = new();

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Messages.Add(new CapturedLogLine(logLevel, formatter(state, exception)));
}
