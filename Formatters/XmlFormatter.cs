using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Formatters;

/// <summary>
/// Formats analytics data as XML with proper structure and validation.
/// Supports nested objects and collections with customizable root element names.
/// </summary>
public class XmlFormatter
{
    private const string DefaultRootElementName = "data";
    private const string ItemElementName = "item";
    private const string IndexAttributeName = "index";
    private const string NullAttributeName = "null";
    private const string NullAttributeValue = "true";
    private const string IndentCharacters = "  ";
    private const string XmlVersion = "1.0";
    private const string XmlEncodingName = "UTF-8";
    private const string XmlStandaloneValue = "yes";
    private const string RoundTripDateTimeFormat = "O";
    private const string InvalidElementNamePattern = "[^a-zA-Z0-9_-]";
    private const string InvalidElementNameReplacement = "_";
    private const string ElementNameFallbackPrefix = "_";
    private const string XmlParsingErrorPrefix = "XML parsing error: ";
    private const string UnexpectedErrorPrefix = "Unexpected error: ";
    private const string FormatFailedMessage = "Failed to format XML";
    private const string PrettifyFailedMessage = "Failed to prettify XML";
    private const string FormattedLogMessage = "Formatted data to XML: {XmlLength} bytes";
    private const string FormatErrorLogMessage = "Error formatting XML";
    private const string PrettifyErrorLogMessage = "Error prettifying XML";
    private const string XPathErrorLogMessage = "Error selecting XML values with XPath: {XPath}";
    private const string PropertyReadWarningMessage = "Could not read property {Property}: {Error}";

    private readonly ILogger<XmlFormatter> _logger;
    private readonly bool _includeXmlDeclaration;

    public XmlFormatter(ILogger<XmlFormatter> logger, bool includeXmlDeclaration = true)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this._logger = logger;
        this._includeXmlDeclaration = includeXmlDeclaration;
    }

    /// <summary>
    /// Formats object as XML with root element.
    /// Uses reflection to build XML structure from object properties.
    /// </summary>
    public string Format(object data, string rootElement = DefaultRootElementName)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(rootElement);

        try
        {
            var xdoc = this.BuildXmlDocument(data, rootElement);
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = IndentCharacters,
                OmitXmlDeclaration = !this._includeXmlDeclaration,
                Encoding = System.Text.Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            xdoc.WriteTo(xmlWriter);
            xmlWriter.Flush();

            var xml = stringWriter.ToString();
            this._logger.LogDebug(FormattedLogMessage, xml.Length);

            return xml;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, FormatErrorLogMessage);
            throw new InvalidOperationException(FormatFailedMessage, ex);
        }
    }

    /// <summary>
    /// Validates XML string against basic well-formedness rules.
    /// Does not validate against schema, just syntax.
    /// </summary>
    public (bool IsValid, string? Error) Validate(string xml)
    {
        ArgumentException.ThrowIfNullOrEmpty(xml);

        try
        {
            XDocument.Parse(xml);
            return (true, null);
        }
        catch (XmlException ex)
        {
            return (false, $"{XmlParsingErrorPrefix}{ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"{UnexpectedErrorPrefix}{ex.Message}");
        }
    }

    /// <summary>
    /// Converts XML to pretty-printed format with consistent indentation.
    /// </summary>
    public string Prettify(string xml)
    {
        ArgumentException.ThrowIfNullOrEmpty(xml);

        try
        {
            var xdoc = XDocument.Parse(xml);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = IndentCharacters,
                OmitXmlDeclaration = !this._includeXmlDeclaration
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            xdoc.WriteTo(xmlWriter);
            xmlWriter.Flush();

            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, PrettifyErrorLogMessage);
            throw new InvalidOperationException(PrettifyFailedMessage, ex);
        }
    }

    /// <summary>
    /// Extracts values from XML by XPath expression.
    /// </summary>
    public List<string> SelectValues(string xml, string xpathExpression)
    {
        ArgumentException.ThrowIfNullOrEmpty(xml);
        ArgumentException.ThrowIfNullOrEmpty(xpathExpression);

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
            this._logger.LogError(ex, XPathErrorLogMessage, xpathExpression);
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
                var element = this.CreateXmlElement(key.ToString() ?? ItemElementName, value);
                root.Add(element);
            }
        }
        else if (data is System.Collections.IEnumerable enumerable and not string)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                var element = this.CreateXmlElement(ItemElementName, item);
                element.SetAttributeValue(IndexAttributeName, i++);
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
                    var element = this.CreateXmlElement(prop.Name, value);
                    root.Add(element);
                }
                catch (Exception ex)
                {
                    this._logger.LogWarning(PropertyReadWarningMessage, prop.Name, ex.Message);
                }
            }
        }

        return new XDocument(
            this._includeXmlDeclaration
                ? new XDeclaration(XmlVersion, XmlEncodingName, XmlStandaloneValue)
                : null,
            root
        );
    }

    private XElement CreateXmlElement(string name, object? value)
    {
        var element = new XElement(this.SanitizeElementName(name));

        if (value == null)
        {
            element.SetAttributeValue(NullAttributeName, NullAttributeValue);
        }
        else if (value is DateTime dt)
        {
            element.Value = dt.ToString(RoundTripDateTimeFormat);
        }
        else if (value is System.Collections.IDictionary dict)
        {
            foreach (var key in dict.Keys)
            {
                var child = this.CreateXmlElement(key.ToString() ?? ItemElementName, dict[key]);
                element.Add(child);
            }
        }
        else if (value is System.Collections.IEnumerable enumerable and not string)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                var child = this.CreateXmlElement(ItemElementName, item);
                child.SetAttributeValue(IndexAttributeName, i++);
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
        var sanitized = System.Text.RegularExpressions.Regex.Replace(name, InvalidElementNamePattern, InvalidElementNameReplacement);

        if (string.IsNullOrEmpty(sanitized) || !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = ElementNameFallbackPrefix + sanitized;
        }

        return sanitized;
    }
}
