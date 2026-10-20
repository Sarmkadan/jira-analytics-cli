// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Formatters;

/// <summary>
/// Formats analytics data as Markdown for human-readable reports.
/// Supports tables, lists, headers, code blocks, and emphasis.
/// </summary>
public class MarkdownFormatter
{
    private const int MinHeaderLevel = 1;
    private const int MaxHeaderLevel = 6;
    private const string BulletPrefix = "- ";
    private const string BlockQuotePrefix = "> ";
    private const string CodeFence = "```";
    private const string BoldMarker = "**";
    private const string ItalicMarker = "*";
    private const string HorizontalRuleMarker = "---";
    private const string NullValuePlaceholder = "-";
    private const string TableColumnSeparator = " | ";
    private const string TableBorderChar = "-";
    private const string GeneratedDateFormat = "yyyy-MM-dd HH:mm:ss UTC";

    private readonly ILogger<MarkdownFormatter> _logger;

    public MarkdownFormatter(ILogger<MarkdownFormatter> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Formats header with markdown heading syntax.
    /// Level 1-6 supported for different header sizes.
    /// </summary>
    public string Header(string text, int level = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        if (level < MinHeaderLevel || level > MaxHeaderLevel)
            level = MinHeaderLevel;

        return new string('#', level) + " " + text;
    }

    /// <summary>
    /// Formats collection as markdown table.
    /// Each object property becomes a column.
    /// </summary>
    public string Table<T>(IEnumerable<T> data)
    {
        if (data == null || !data.Any())
            return string.Empty;

        var properties = typeof(T).GetProperties();
        var headers = properties.Select(p => p.Name).ToArray();
        var rows = new List<string[]>();

        foreach (var item in data)
        {
            var values = properties
                .Select(p => EscapeMarkdown(p.GetValue(item)?.ToString() ?? NullValuePlaceholder))
                .ToArray();
            rows.Add(values);
        }

        return BuildMarkdownTable(headers, rows.ToArray());
    }

    /// <summary>
    /// Formats key-value pairs as definition list.
    /// Useful for metrics and summaries.
    /// </summary>
    public string DefinitionList(Dictionary<string, string> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var sb = new StringBuilder();

        foreach (var item in items)
        {
            sb.AppendLine($"{BoldMarker}{EscapeMarkdown(item.Key)}{BoldMarker}: {EscapeMarkdown(item.Value)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats collection as bulleted list.
    /// </summary>
    public string BulletList(IEnumerable<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var sb = new StringBuilder();

        foreach (var item in items)
        {
            sb.AppendLine($"{BulletPrefix}{EscapeMarkdown(item)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats collection as numbered list.
    /// </summary>
    public string NumberedList(IEnumerable<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var sb = new StringBuilder();
        var index = 1;

        foreach (var item in items)
        {
            sb.AppendLine($"{index}. {EscapeMarkdown(item)}");
            index++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats text as code block with optional language syntax highlighting.
    /// </summary>
    public string CodeBlock(string code, string language = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(code);
        var sb = new StringBuilder();
        sb.AppendLine($"{CodeFence}{language}");
        sb.AppendLine(code);
        sb.AppendLine(CodeFence);

        return sb.ToString();
    }

    /// <summary>
    /// Formats text as blockquote (indented quote).
    /// </summary>
    public string BlockQuote(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        return string.Join("\n", lines.Select(l => BlockQuotePrefix + l));
    }

    /// <summary>
    /// Formats text as bold.
    /// </summary>
    public string Bold(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        return $"{BoldMarker}{text}{BoldMarker}";
    }

    /// <summary>
    /// Formats text as italic.
    /// </summary>
    public string Italic(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        return $"{ItalicMarker}{text}{ItalicMarker}";
    }

    /// <summary>
    /// Creates horizontal rule separator.
    /// </summary>
    public string HorizontalRule()
    {
        return HorizontalRuleMarker;
    }

    /// <summary>
    /// Formats hyperlink.
    /// </summary>
    public string Link(string text, string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentException.ThrowIfNullOrEmpty(url);
        return $"[{EscapeMarkdown(text)}]({url})";
    }

    /// <summary>
    /// Formats complete markdown document with title and sections.
    /// </summary>
    public string Document(string title, params (string SectionTitle, string Content)[] sections)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentNullException.ThrowIfNull(sections);
        var sb = new StringBuilder();

        sb.AppendLine($"# {title}");
        sb.AppendLine();
        sb.AppendLine($"{ItalicMarker}Generated: {DateTime.UtcNow.ToString(GeneratedDateFormat)}{ItalicMarker}");
        sb.AppendLine();

        foreach (var (sectionTitle, content) in sections)
        {
            sb.AppendLine(Header(sectionTitle, 2));
            sb.AppendLine();
            sb.AppendLine(content);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string BuildMarkdownTable(string[] headers, string[][] rows)
    {
        var sb = new StringBuilder();

        // Calculate column widths
        var colWidths = headers.Select(h => h.Length).ToArray();
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length && i < colWidths.Length; i++)
            {
                colWidths[i] = Math.Max(colWidths[i], row[i].Length);
            }
        }

        // Header row
        sb.AppendLine("| " + string.Join(TableColumnSeparator, headers.Select((h, i) => h.PadRight(colWidths[i]))) + " |");

        // Separator
        sb.AppendLine("|" + string.Join("|", colWidths.Select(w => new string(TableBorderChar[0], w + 2))) + "|");

        // Data rows
        foreach (var row in rows)
        {
            sb.AppendLine("| " + string.Join(TableColumnSeparator, row.Select((v, i) => v.PadRight(colWidths[i]))) + " |");
        }

        return sb.ToString();
    }

    private string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Escape special markdown characters
        var escaped = text
            .Replace("\\", "\\\\")
            .Replace("*", "\\*")
            .Replace("_", "\\_")
            .Replace("`", "\\`")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("|", "\\|");

        return escaped;
    }
}
