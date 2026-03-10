// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace JiraAnalyticsCli.Formatters;

/// <summary>
/// Formats analytics data as CSV (Comma-Separated Values).
/// Handles complex types, proper escaping, and header generation.
/// </summary>
public class CsvFormatter
{
    private readonly ILogger<CsvFormatter> _logger;
    private const char Delimiter = ',';
    private const char QuoteChar = '"';

    // PropertyInfo arrays are cached per type so the cost of reflection is paid once,
    // not on every Format / Parse call.
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    // Pooled StringBuilders eliminate per-call heap allocations for the output buffer.
    // DefaultObjectPool uses StringBuilderPooledObjectPolicy, which resets Length on return.
    private static readonly ObjectPool<StringBuilder> _builderPool =
        ObjectPool.Create<StringBuilder>();

    public CsvFormatter(ILogger<CsvFormatter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Formats collection of objects as CSV with headers.
    /// Each property becomes a column; properties are derived via reflection.
    /// </summary>
    public string Format<T>(IEnumerable<T> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var items = data.ToList();
        if (!items.Any())
        {
            _logger.LogWarning("No data to format as CSV");
            return string.Empty;
        }

        var properties = _propertyCache.GetOrAdd(typeof(T),
            static t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        var headers = Array.ConvertAll(properties, static p => p.Name);

        var csv = _builderPool.Get();
        try
        {
            csv.Clear();
            csv.AppendLine(EscapeCsvLine(headers));

            foreach (var item in items)
            {
                var values = Array.ConvertAll(properties, p => FormatValue(p.GetValue(item)));
                csv.AppendLine(EscapeCsvLine(values));
            }

            _logger.LogDebug("Formatted {ItemCount} items to CSV", items.Count);
            return csv.ToString();
        }
        finally
        {
            _builderPool.Return(csv);
        }
    }

    /// <summary>
    /// Formats collection with custom column mapping.
    /// Allows renaming columns and filtering which properties to include.
    /// </summary>
    public string FormatWithMapping<T>(IEnumerable<T> data, Dictionary<string, Func<T, string>> columnMapping)
    {
        if (data == null || columnMapping == null)
            throw new ArgumentNullException(data == null ? nameof(data) : nameof(columnMapping));

        var items = data.ToList();
        var headers = columnMapping.Keys.ToArray();

        var csv = _builderPool.Get();
        try
        {
            csv.Clear();
            csv.AppendLine(EscapeCsvLine(headers));

            foreach (var item in items)
            {
                var values = columnMapping.Values.Select(mapper => mapper(item)).ToArray();
                csv.AppendLine(EscapeCsvLine(values));
            }

            _logger.LogDebug("Formatted {ItemCount} items with mapping to CSV", items.Count);
            return csv.ToString();
        }
        finally
        {
            _builderPool.Return(csv);
        }
    }

    /// <summary>
    /// Converts CSV string back to objects of specified type.
    /// Handles quoted fields and proper parsing.
    /// </summary>
    public List<T> Parse<T>(string csv) where T : class, new()
    {
        if (string.IsNullOrEmpty(csv))
            return new List<T>();

        var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<T>();

        if (lines.Length < 2)
            return result;

        var headers = ParseCsvLine(lines[0]);

        // Use cached PropertyInfo and build a header-to-property map to avoid an O(n)
        // FirstOrDefault scan inside the inner loop — one dictionary lookup per cell instead.
        var allProperties = _propertyCache.GetOrAdd(typeof(T),
            static t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        var propertyMap = new Dictionary<string, PropertyInfo>(headers.Length, StringComparer.OrdinalIgnoreCase);
        foreach (var prop in allProperties)
        {
            if (headers.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                propertyMap[prop.Name] = prop;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            var item = new T();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                if (!propertyMap.TryGetValue(headers[j], out var property))
                    continue;

                try
                {
                    var convertedValue = Convert.ChangeType(values[j], property.PropertyType);
                    property.SetValue(item, convertedValue);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not convert value '{Value}' to type {Type}: {Error}",
                        values[j], property.PropertyType.Name, ex.Message);
                }
            }

            result.Add(item);
        }

        _logger.LogDebug("Parsed {ItemCount} items from CSV", result.Count);
        return result;
    }

    private static string EscapeCsvLine(string[] fields)
    {
        var escapedFields = Array.ConvertAll(fields, EscapeCsvField);
        return string.Join(Delimiter, escapedFields);
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(Delimiter) || field.Contains(QuoteChar) || field.Contains('\n'))
        {
            var escaped = field.Replace("\"", "\"\"");
            return $"{QuoteChar}{escaped}{QuoteChar}";
        }

        return field;
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == QuoteChar)
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == QuoteChar)
                {
                    currentField.Append(QuoteChar);
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == Delimiter && !inQuotes)
            {
                fields.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(ch);
            }
        }

        fields.Add(currentField.ToString().Trim());
        return fields.ToArray();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
