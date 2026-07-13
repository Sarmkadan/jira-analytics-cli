// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Formatting utilities for console output and reports
/// </summary>
public static class FormattingHelpers
{
    /// <summary>
    /// Format number as percentage with specified decimal places
    /// </summary>
    public static string FormatPercentage(double value, int decimalPlaces = 1)
    {
        return value.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture) + "%";
    }

    /// <summary>
    /// Format number with thousand separators
    /// </summary>
    public static string FormatNumber(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format double with specified decimal places
    /// </summary>
    public static string FormatDecimal(double value, int decimalPlaces = 2)
    {
        return value.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format date in standard format
    /// </summary>
    public static string FormatDate(DateTime date, string format = "yyyy-MM-dd")
    {
        return date.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format date and time in standard format
    /// </summary>
    public static string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format bytes as human-readable size
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len.ToString("0.##", CultureInfo.InvariantCulture)} {sizes[order]}";
    }

    /// <summary>
    /// Create table-formatted output from data
    /// </summary>
    public static string CreateTable(string[] headers, List<string[]> rows)
    {
        if (headers == null || headers.Length == 0 || !rows.Any())
            return string.Empty;

        // Calculate column widths
        var columnWidths = new int[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            columnWidths[i] = headers[i].Length;

            foreach (var row in rows)
            {
                if (i < row.Length && row[i].Length > columnWidths[i])
                    columnWidths[i] = row[i].Length;
            }
        }

        var sb = new System.Text.StringBuilder();

        // Header row
        sb.Append("| ");
        for (int i = 0; i < headers.Length; i++)
        {
            sb.Append(headers[i].PadRight(columnWidths[i]));
            sb.Append(" | ");
        }
        sb.AppendLine();

        // Separator
        sb.Append("|");
        for (int i = 0; i < headers.Length; i++)
        {
            sb.Append("-".PadRight(columnWidths[i] + 2, '-'));
            sb.Append("|");
        }
        sb.AppendLine();

        // Data rows
        foreach (var row in rows)
        {
            sb.Append("| ");
            for (int i = 0; i < headers.Length; i++)
            {
                var value = i < row.Length ? row[i] : string.Empty;
                sb.Append(value.PadRight(columnWidths[i]));
                sb.Append(" | ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Color code for console output (ANSI)
    /// </summary>
    public static string ColorText(string text, ConsoleColor color)
    {
        return $"\x1b[{(int)color}m{text}\x1b[0m";
    }

    /// <summary>
    /// Format status indicator with emoji
    /// </summary>
    public static string FormatStatus(string status)
    {
        return status switch
        {
            "Done" or "Closed" => "✅ " + status,
            "In Progress" or "In Review" => "🔄 " + status,
            "Open" => "📋 " + status,
            "Blocked" => "🚫 " + status,
            "On Hold" => "⏸️  " + status,
            _ => "❓ " + status
        };
    }

    /// <summary>
    /// Repeat character for line drawing
    /// </summary>
    public static string RepeatChar(char character, int count)
    {
        return new string(character, count);
    }

    /// <summary>
    /// Create indented text for hierarchical display
    /// </summary>
    public static string Indent(string text, int spaces = 2)
    {
        var indentation = new string(' ', spaces);
        return indentation + text;
    }

    /// <summary>
    /// Pad string symmetrically
    /// </summary>
    public static string CenterText(string text, int width)
    {
        if (text.Length >= width)
            return text;

        var totalPadding = width - text.Length;
        var leftPadding = totalPadding / 2;
        var rightPadding = totalPadding - leftPadding;

        return new string(' ', leftPadding) + text + new string(' ', rightPadding);
    }
}
