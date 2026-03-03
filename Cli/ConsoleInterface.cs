// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Cli;

/// <summary>
/// Manages console output with colored formatting, tables, and progress indicators.
/// Centralizes all CLI UI output for consistent theming and styling.
/// </summary>
public class ConsoleInterface
{
    private readonly ILogger<ConsoleInterface> _logger;
    private readonly bool _useColors;
    private readonly TextWriter _output;
    private readonly TextWriter _errorOutput;

    public ConsoleInterface(ILogger<ConsoleInterface> logger, bool useColors = true)
    {
        _logger = logger;
        _useColors = useColors && Console.IsOutputRedirected == false;
        _output = Console.Out;
        _errorOutput = Console.Error;
    }

    /// <summary>
    /// Displays a formatted header with equal signs border.
    /// Used for section separation and visual clarity.
    /// </summary>
    public void PrintHeader(string title)
    {
        var border = new string('=', title.Length + 4);
        WriteLine($"\n{border}");
        WriteLine($"  {title}");
        WriteLine($"{border}\n");
    }

    /// <summary>
    /// Prints a formatted table with headers and rows. Handles column alignment.
    /// Calculates optimal column widths based on content.
    /// </summary>
    public void PrintTable(string[] headers, string[][] rows)
    {
        var columnWidths = CalculateColumnWidths(headers, rows);

        PrintTableRow(headers, columnWidths, isHeader: true);
        PrintSeparator(columnWidths);

        foreach (var row in rows)
        {
            PrintTableRow(row, columnWidths, isHeader: false);
        }

        WriteLine();
    }

    /// <summary>
    /// Displays a progress indicator during long-running operations.
    /// Supports percentage completion and spinner animation.
    /// </summary>
    public void PrintProgress(string message, double percentage, bool clear = false)
    {
        if (clear)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        var barLength = 30;
        var filledLength = (int)(barLength * percentage / 100);
        var bar = new string('█', filledLength) + new string('░', barLength - filledLength);

        var progressText = $"{message} [{bar}] {percentage:F1}%";
        _output.WriteLine(progressText);
    }

    /// <summary>
    /// Prints colored error message to error stream.
    /// Logs critical errors for audit trail.
    /// </summary>
    public void PrintError(string message)
    {
        if (_useColors)
        {
            _errorOutput.Write($"[91m");
        }

        _errorOutput.WriteLine($"✗ Error: {message}");

        if (_useColors)
        {
            _errorOutput.Write($"[0m");
        }

        _logger.LogError("Console error displayed: {Message}", message);
    }

    /// <summary>
    /// Prints colored success message to standard output.
    /// Provides visual confirmation of successful operations.
    /// </summary>
    public void PrintSuccess(string message)
    {
        if (_useColors)
        {
            _output.Write($"[92m");
        }

        _output.WriteLine($"✓ Success: {message}");

        if (_useColors)
        {
            _output.Write($"[0m");
        }

        _logger.LogInformation("Console success displayed: {Message}", message);
    }

    /// <summary>
    /// Prints colored warning message.
    /// Used for non-critical issues requiring user attention.
    /// </summary>
    public void PrintWarning(string message)
    {
        if (_useColors)
        {
            _output.Write($"[93m");
        }

        _output.WriteLine($"⚠ Warning: {message}");

        if (_useColors)
        {
            _output.Write($"[0m");
        }

        _logger.LogWarning("Console warning displayed: {Message}", message);
    }

    /// <summary>
    /// Prints colored informational message.
    /// </summary>
    public void PrintInfo(string message)
    {
        if (_useColors)
        {
            _output.Write($"[94m");
        }

        _output.WriteLine($"ℹ {message}");

        if (_useColors)
        {
            _output.Write($"[0m");
        }
    }

    /// <summary>
    /// Prints metrics card with key-value pairs in a formatted box.
    /// Used for displaying KPIs and statistics.
    /// </summary>
    public void PrintMetricsCard(string title, Dictionary<string, string> metrics)
    {
        PrintHeader(title);

        var maxKeyLength = metrics.Keys.Max(k => k.Length);

        foreach (var (key, value) in metrics)
        {
            var paddedKey = key.PadRight(maxKeyLength);
            _output.WriteLine($"  {paddedKey}  :  {value}");
        }

        WriteLine();
    }

    private void PrintTableRow(string[] cells, int[] columnWidths, bool isHeader)
    {
        var row = new StringBuilder("| ");

        for (int i = 0; i < cells.Length; i++)
        {
            var cell = cells[i] ?? string.Empty;
            row.Append(cell.Length > columnWidths[i]
                ? cell.Substring(0, columnWidths[i] - 1) + "…"
                : cell.PadRight(columnWidths[i]));
            row.Append(" | ");
        }

        if (isHeader && _useColors)
        {
            _output.Write($"[1m");
        }

        _output.WriteLine(row.ToString());

        if (isHeader && _useColors)
        {
            _output.Write($"[0m");
        }
    }

    private void PrintSeparator(int[] columnWidths)
    {
        var separator = "+" + string.Join("+", columnWidths.Select(w => new string('-', w + 1))) + "+";
        WriteLine(separator);
    }

    private int[] CalculateColumnWidths(string[] headers, string[][] rows)
    {
        var widths = headers.Select(h => h.Length + 2).ToArray();

        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length && i < widths.Length; i++)
            {
                widths[i] = Math.Max(widths[i], (row[i]?.Length ?? 0) + 2);
            }
        }

        return widths;
    }

    private void WriteLine(string? text = null)
    {
        if (text == null)
            _output.WriteLine();
        else
            _output.WriteLine(text);
    }
}
