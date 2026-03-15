// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Formatters;

/// <summary>
/// Formats analytics data as XML with proper structure and validation.
/// Supports nested objects and collections with customizable root element names.
/// </summary>
public class XmlFormatter
{
    private readonly ILogger<XmlFormatter> _logger;
    private readonly bool _includeXmlDeclaration;

    public XmlFormatter(ILogger<XmlFormatter> logger, bool includeXmlDeclaration = true)
    {
        _logger = logger;
        _includeXmlDeclaration = includeXmlDeclaration;
    }

    /// <summary>
    /// Formats object as XML with root element.
    /// Uses reflection to build XML structure from object properties.
    /// </summary>
    public string Format(object data, string rootElement = "data")
    {
        try
        {
            var xdoc = BuildXmlDocument(data, rootElement);
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = !_includeXmlDeclaration,
                Encoding = System.Text.Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            xdoc.WriteTo(xmlWriter);
            xmlWriter.Flush();

            var xml = stringWriter.ToString();
            _logger.LogDebug("Formatted data to XML: {XmlLength} bytes", xml.Length);

            return xml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting XML");
            throw new InvalidOperationException("Failed to format XML", ex);
        }
    }

    /// <summary>
    /// Validates XML string against basic well-formedness rules.
    /// Does not validate against schema, just syntax.
    /// </summary>
    public (bool IsValid, string? Error) Validate(string xml)
    {
        try
        {
            XDocument.Parse(xml);
            return (true, null);
        }
        catch (XmlException ex)
        {
            return (false, $"XML parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts XML to pretty-printed format with consistent indentation.
    /// </summary>
    public string Prettify(string xml)
    {
        try
        {
            var xdoc = XDocument.Parse(xml);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = !_includeXmlDeclaration
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            xdoc.WriteTo(xmlWriter);
            xmlWriter.Flush();

            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error prettifying XML");
            throw new InvalidOperationException("Failed to prettify XML", ex);
        }
    }

    /// <summary>
    /// Extracts values from XML by XPath expression.
    /// </summary>
    public List<string> SelectValues(string xml, string xpathExpression)
    {
        try
        {
            var xdoc = XDocument.Parse(xml);
            var values = new List<string>();

            foreach (var element in xdoc.XPathSelectElements(xpathExpression))
            {
                if (element.HasElements)
                {
                    values.Add(element.Value ?? string.Empty);
                }
                else
                {
                    values.Add(element.Value ?? string.Empty);
                }
            }

            return values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting XML values with XPath: {XPath}", xpathExpression);
            return new List<string>();
        }
    }

    private XDocument BuildXmlDocument(object data, string rootElement)
    {
        var root = new XElement(rootElement);

        if (data is System.Collections.IDictionary dict)
        {
            foreach (var key in dict.Keys)
            {
                var value = dict[key];
                var element = CreateXmlElement(key.ToString() ?? "item", value);
                root.Add(element);
            }
        }
        else if (data is System.Collections.IEnumerable enumerable and not string)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                var element = CreateXmlElement("item", item);
                element.SetAttributeValue("index", i++);
                root.Add(element);
            }
        }
        else
        {
            var properties = data?.GetType().GetProperties() ?? Array.Empty<System.Reflection.PropertyInfo>();

            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(data);
                    var element = CreateXmlElement(prop.Name, value);
                    root.Add(element);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not read property {Property}: {Error}", prop.Name, ex.Message);
                }
            }
        }

        return new XDocument(
            _includeXmlDeclaration
                ? new XDeclaration("1.0", "UTF-8", "yes")
                : null,
            root
        );
    }

    private XElement CreateXmlElement(string name, object? value)
    {
        var element = new XElement(SanitizeElementName(name));

        if (value == null)
        {
            element.SetAttributeValue("null", "true");
        }
        else if (value is DateTime dt)
        {
            element.Value = dt.ToString("O");
        }
        else if (value is System.Collections.IDictionary dict)
        {
            foreach (var key in dict.Keys)
            {
                var child = CreateXmlElement(key.ToString() ?? "item", dict[key]);
                element.Add(child);
            }
        }
        else if (value is System.Collections.IEnumerable enumerable and not string)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                var child = CreateXmlElement("item", item);
                child.SetAttributeValue("index", i++);
                element.Add(child);
            }
        }
        else
        {
            element.Value = value.ToString() ?? string.Empty;
        }

        return element;
    }

    private string SanitizeElementName(string name)
    {
        // XML element names must start with letter or underscore, contain only valid chars
        var sanitized = System.Text.RegularExpressions.Regex.Replace(name, "[^a-zA-Z0-9_-]", "_");

        if (string.IsNullOrEmpty(sanitized) || !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = "_" + sanitized;
        }

        return sanitized;
    }
}
