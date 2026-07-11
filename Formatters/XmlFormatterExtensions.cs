// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace JiraAnalyticsCli.Formatters;

/// <summary>
/// Extension methods for XmlFormatter providing additional XML formatting and manipulation capabilities.
/// </summary>
public static class XmlFormatterExtensions
{
    /// <summary>
    /// Formats multiple objects as a single XML document with a root element containing all items.
    /// </summary>
    /// <param name="formatter">The XmlFormatter instance</param>
    /// <param name="items">Collection of objects to format</param>
    /// <param name="rootElement">Root element name (default: "items")</param>
    /// <returns>Formatted XML string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="items"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="rootElement"/> is null or empty.</exception>
    public static string FormatCollection(this XmlFormatter formatter, IEnumerable<object> items, string rootElement = "items")
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrEmpty(rootElement);

        var root = new XElement(rootElement);
        var index = 0;

        foreach (var item in items)
        {
            var element = formatter.Format(item);
            var xdoc = XDocument.Parse(element);
            var itemElement = xdoc.Root ?? new XElement("item");
            itemElement.SetAttributeValue("index", index++);
            root.Add(itemElement);
        }

        var resultDoc = new XDocument(root);
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = " ",
            OmitXmlDeclaration = true,
            Encoding = System.Text.Encoding.UTF8
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        resultDoc.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return stringWriter.ToString();
    }

    /// <summary>
    /// Validates XML and returns detailed validation information including line numbers and error positions.
    /// </summary>
    /// <param name="formatter">The XmlFormatter instance</param>
    /// <param name="xml">XML string to validate</param>
    /// <returns>Tuple with validation result, error message, and line number if available</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="xml"/> is null.</exception>
    public static (bool IsValid, string? Error, int? LineNumber) ValidateWithDetails(this XmlFormatter formatter, string xml)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(xml);

        try
        {
            XDocument.Parse(xml);
            return (true, null, null);
        }
        catch (XmlException ex)
        {
            return (false, ex.Message, ex.LineNumber);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    /// <summary>
    /// Extracts values from XML using XPath and returns them as a dictionary with element names as keys.
    /// </summary>
    /// <param name="formatter">The XmlFormatter instance</param>
    /// <param name="xml">XML string to query</param>
    /// <param name="xpathExpression">XPath expression to select elements</param>
    /// <returns>Dictionary mapping element names to their values</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/>, <paramref name="xml"/>, or <paramref name="xpathExpression"/> is null.</exception>
    public static Dictionary<string, List<string>> SelectValuesByName(this XmlFormatter formatter, string xml, string xpathExpression)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(xml);
        ArgumentNullException.ThrowIfNull(xpathExpression);

        var result = new Dictionary<string, List<string>>();

        try
        {
            var xdoc = XDocument.Parse(xml);
            var elements = xdoc.XPathSelectElements(xpathExpression);

            foreach (var element in elements)
            {
                var elementName = element.Name.LocalName;
                if (!result.TryGetValue(elementName, out var values))
                {
                    values = new List<string>();
                    result[elementName] = values;
                }

                values.Add(element.Value ?? string.Empty);
            }
        }
        catch (XmlException ex)
        {
            throw new ArgumentException("Invalid XML or XPath expression", nameof(xml), ex);
        }
        catch (XPathException ex)
        {
            throw new ArgumentException("Invalid XPath expression", nameof(xpathExpression), ex);
        }

        return result;
    }

    /// <summary>
    /// Converts XML to a compact format with minimal whitespace and indentation.
    /// Useful for reducing XML size for storage or transmission.
    /// </summary>
    /// <param name="formatter">The XmlFormatter instance</param>
    /// <param name="xml">XML string to compact</param>
    /// <returns>Compact XML string with minimal whitespace</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="xml"/> is null.</exception>
    public static string Compact(this XmlFormatter formatter, string xml)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(xml);

        try
        {
            var xdoc = XDocument.Parse(xml);
            var settings = new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true,
                Encoding = System.Text.Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            xdoc.WriteTo(xmlWriter);
            xmlWriter.Flush();

            return stringWriter.ToString();
        }
        catch (XmlException ex)
        {
            throw new ArgumentException("Invalid XML content", nameof(xml), ex);
        }
    }
}
