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
    private readonly ILogger<MarkdownFormatter> _logger;

    public MarkdownFormatter(ILogger<MarkdownFormatter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Formats header with markdown heading syntax.
    /// Level 1-6 supported for different header sizes.
    /// </summary>
    public string Header(string text, int level = 1)
    {
        if (level < 1 || level > 6)
            level = 1;

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
                .Select(p => EscapeMarkdown(p.GetValue(item)?.ToString() ?? "-"))
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
        var sb = new StringBuilder();

        foreach (var item in items)
        {
            sb.AppendLine($"**{EscapeMarkdown(item.Key)}**: {EscapeMarkdown(item.Value)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats collection as bulleted list.
    /// </summary>
    public string BulletList(IEnumerable<string> items)
    {
        var sb = new StringBuilder();

        foreach (var item in items)
        {
            sb.AppendLine($"- {EscapeMarkdown(item)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats collection as numbered list.
    /// </summary>
    public string NumberedList(IEnumerable<string> items)
    {
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
        var sb = new StringBuilder();
        sb.AppendLine($"```{language}");
        sb.AppendLine(code);
        sb.AppendLine("```");

        return sb.ToString();
    }

    /// <summary>
    /// Formats text as blockquote (indented quote).
    /// </summary>
    public string BlockQuote(string text)
    {
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        return string.Join("\n", lines.Select(l => "> " + l));
    }

    /// <summary>
    /// Formats text as bold.
    /// </summary>
    public string Bold(string text)
    {
        return $"**{text}**";
    }

    /// <summary>
    /// Formats text as italic.
    /// </summary>
    public string Italic(string text)
    {
        return $"*{text}*";
    }

    /// <summary>
    /// Creates horizontal rule separator.
    /// </summary>
    public string HorizontalRule()
    {
        return "---";
    }

    /// <summary>
    /// Formats hyperlink.
    /// </summary>
    public string Link(string text, string url)
    {
        return $"[{EscapeMarkdown(text)}]({url})";
    }

    /// <summary>
    /// Formats complete markdown document with title and sections.
    /// </summary>
    public string Document(string title, params (string SectionTitle, string Content)[] sections)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# {title}");
        sb.AppendLine();
        sb.AppendLine($"*Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}*");
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
        sb.AppendLine("| " + string.Join(" | ", headers.Select((h, i) => h.PadRight(colWidths[i]))) + " |");

        // Separator
        sb.AppendLine("|" + string.Join("|", colWidths.Select(w => new string('-', w + 2))) + "|");

        // Data rows
        foreach (var row in rows)
        {
            sb.AppendLine("| " + string.Join(" | ", row.Select((v, i) => v.PadRight(colWidths[i]))) + " |");
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
