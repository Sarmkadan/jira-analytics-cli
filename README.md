// ... rest of README content ...

## ValidationHelpers

The `ValidationHelpers` class provides a set of utility methods for validating various data types and formats. It includes methods for checking Jira issue keys, project keys, URLs, email addresses, sprint IDs, story points, date ranges, and percentage values. Additionally, it offers methods for truncating strings with ellipsis, sanitizing strings for CSV/file output, and converting percentage values to visual progress bars.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Validate Jira issue key
var isValidIssueKey = ValidationHelpers.IsValidJiraIssueKey("PROJ-123");
Console.WriteLine(isValidIssueKey); // Output: true

// Validate URL
var isValidUrl = ValidationHelpers.IsValidUrl("https://example.com");
Console.WriteLine(isValidUrl); // Output: true

// Truncate string with ellipsis
var truncatedString = ValidationHelpers.TruncateWithEllipsis("This is a very long string", 20);
Console.WriteLine(truncatedString); // Output: "This is a very long..."

// Convert percentage to visual progress bar
var percentage = 75;
var progressBar = ValidationHelpers.ToProgressBar(percentage);
Console.WriteLine(progressBar); // Output: [█████████░░░░░]
```

## DateTimeExtensions

The `DateTimeExtensions` class provides utility extension methods for date and time calculations, formatting, and business logic operations. It includes methods for calculating business days, checking business hours, determining week numbers, validating date states, formatting durations, and finding the last business day of a month.


### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Current date and time
var now = DateTime.UtcNow;
var yesterday = now.AddDays(-1);
var tomorrow = now.AddDays(1);
var lastMonth = new DateTime(now.Year, now.Month - 1, 15);

// Check if date is in business hours
var isBusinessHour = now.IsBusinessHour();
Console.WriteLine($"Is business hour: {isBusinessHour}");

// Calculate business days between two dates
var businessDays = yesterday.GetBusinessDaysBetween(now);
Console.WriteLine($"Business days between yesterday and now: {businessDays}");

// Get week number (ISO 8601)
var weekNumber = now.GetWeekNumber();
Console.WriteLine($"Current week number: {weekNumber}");

// Check if date is in past or future
Console.WriteLine($"Yesterday is past: {yesterday.IsPast()}");
Console.WriteLine($"Tomorrow is future: {tomorrow.IsFuture()}");

// Format duration in human-readable format
var duration = TimeSpan.FromHours(2.5);
Console.WriteLine($"Duration: {duration.ToHumanReadableDuration()}");

// Get last business day of month
var lastBusinessDay = now.GetLastBusinessDayOfMonth();
Console.WriteLine($"Last business day of month: {lastBusinessDay:yyyy-MM-dd}");
```

## FormattingHelpers

The `FormattingHelpers` class provides utility methods for formatting various data types for console output and reports. It includes methods for formatting numbers as percentages, formatting integers with thousand separators, formatting decimals, dates, bytes, creating tables, applying ANSI color codes, formatting status indicators with emojis, and text alignment utilities.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Numeric formatting
var percentage = FormattingHelpers.FormatPercentage(85.678, 2);
Console.WriteLine(percentage); // Output: "85.68%"

var formattedNumber = FormattingHelpers.FormatNumber(12500);
Console.WriteLine(formattedNumber); // Output: "12,500"

var formattedDecimal = FormattingHelpers.FormatDecimal(3.14159, 3);
Console.WriteLine(formattedDecimal); // Output: "3.142"

// Date formatting
var now = DateTime.UtcNow;
var formattedDate = FormattingHelpers.FormatDate(now);
Console.WriteLine(formattedDate); // Output: "2026-07-16"

var formattedDateTime = FormattingHelpers.FormatDateTime(now);
Console.WriteLine(formattedDateTime); // Output: "2026-07-16 14:30:45"

// Byte formatting
var fileSize = FormattingHelpers.FormatBytes(1572864);
Console.WriteLine(fileSize); // Output: "1.5 MB"

// Table creation
var headers = new[] { "ID", "Name", "Status", "Created" };
var rows = new List<string[]>
{
    new[] { "1", "Feature X", "In Progress", "2026-07-15" },
    new[] { "2", "Bug Y", "Done", "2026-07-14" },
    new[] { "3", "Task Z", "Open", "2026-07-13" }
};
var table = FormattingHelpers.CreateTable(headers, rows);
Console.WriteLine(table);

// Color formatting
var coloredText = FormattingHelpers.ColorText("Important Message", ConsoleColor.Red);
Console.WriteLine(coloredText);

// Status formatting
var status = FormattingHelpers.FormatStatus("Done");
Console.WriteLine(status); // Output: "✅ Done"

// Text alignment
var centered = FormattingHelpers.CenterText("Centered Title", 50);
Console.WriteLine(centered);

var indented = FormattingHelpers.Indent("Nested content", 4);
Console.WriteLine(indented);

// Character repetition
var separator = FormattingHelpers.RepeatChar('-', 50);
Console.WriteLine(separator);
```

## XmlFormatter

The `XmlFormatter` class provides utilities for converting objects and collections to well-formed XML with customizable formatting options. It supports automatic XML structure generation from object properties, validation, pretty-printing, and XPath-based value extraction.

### Usage Example

```csharp
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging;

// Create formatter with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var formatter = new XmlFormatter(loggerFactory.CreateLogger<XmlFormatter>(), includeXmlDeclaration: true);

// Format an object to XML
var projectData = new
{
    Name = "Analytics Dashboard",
    Version = "1.0.0",
    Issues = new List<object>
    {
        new { Key = "PROJ-123", Summary = "Implement login page", Status = "In Progress", Priority = "High" },
        new { Key = "PROJ-124", Summary = "Fix authentication bug", Status = "Done", Priority = "Critical" },
        new { Key = "PROJ-125", Summary = "Update documentation", Status = "Open", Priority = "Medium" }
    },
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

var xml = formatter.Format(projectData, "project");
Console.WriteLine(xml);

// Validate XML
var (isValid, error) = formatter.Validate(xml);
Console.WriteLine(isValid ? "Valid XML" : $"Invalid: {error}");

// Prettify existing XML
var prettyXml = formatter.Prettify(xml);
Console.WriteLine(prettyXml);

// Select values using XPath
var issueKeys = formatter.SelectValues(xml, "//item[starts-with(@Key, 'PROJ-')]/@Key");
foreach (var key in issueKeys)
{
    Console.WriteLine(key);
}
```

## JsonFormatter

The `JsonFormatter` class provides utilities for converting objects and collections to JSON format with support for prettification, metadata wrapping, field filtering, and validation. It handles serialization with configurable indentation, circular reference detection, and safe error handling.

### Usage Example

```csharp
using JiraAnalyticsCli.Formatters;
using Microsoft.Extensions.Logging;

// Create formatter with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var formatter = new JsonFormatter(loggerFactory.CreateLogger<JsonFormatter>(), prettyPrint: true);

// Format an object to JSON
var projectData = new
{
    Name = "Analytics Dashboard",
    Version = "1.0.0",
    Issues = new List<object>
    {
        new { Key = "PROJ-123", Summary = "Implement login page", Status = "In Progress", Priority = "High" },
        new { Key = "PROJ-124", Summary = "Fix authentication bug", Status = "Done", Priority = "Critical" },
        new { Key = "PROJ-125", Summary = "Update documentation", Status = "Open", Priority = "Medium" }
    },
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

var json = formatter.Format(projectData);
Console.WriteLine(json);

// Format with metadata wrapper
var jsonWithMetadata = formatter.FormatWithMetadata(projectData, "Project Analytics Report", "2.1.0");
Console.WriteLine(jsonWithMetadata);

// Format filtered fields only
var filteredJson = formatter.FormatFiltered(projectData, new[] { "Name", "Version", "CreatedAt" });
Console.WriteLine(filteredJson);

// Validate JSON
var (isValid, errors) = formatter.Validate(json);
Console.WriteLine(isValid ? "Valid JSON" : $"Invalid: {string.Join(", ", errors)}");

// Prettify existing JSON
var minifiedJson = "{\\"Name\\":\\"Test\\",\\"Count\\":42}";
var prettyJson = formatter.Prettify(minifiedJson);
Console.WriteLine(prettyJson);
```

// Create header
var header = formatter.Header("Project Status Report", 1);
Console.WriteLine(header);
// Output: # Project Status Report

// Create table from collection
var issues = new List<Issue>
{
    new Issue { Id = "PROJ-1", Title = "Implement login page", Status = "In Progress", Priority = "High" },
    new Issue { Id = "PROJ-2", Title = "Fix authentication bug", Status = "Done", Priority = "Critical" },
    new Issue { Id = "PROJ-3", Title = "Update documentation", Status = "Open", Priority = "Medium" }
};
var table = formatter.Table(issues);
Console.WriteLine(table);

// Create definition list
var metrics = new Dictionary<string, string>
{
    { "Total Issues", "42" },
    { "Completed", "35 (83%)" },
    { "In Progress", "5" },
    { "Blocked", "2" }
};
var definitionList = formatter.DefinitionList(metrics);
Console.WriteLine(definitionList);

// Create bulleted list
var features = new List<string> { "User authentication", "Dashboard analytics", "API integration", "Reporting module" };
var bulletList = formatter.BulletList(features);
Console.WriteLine(bulletList);

// Create numbered list
var steps = new List<string> { "Setup project structure", "Implement core features", "Add unit tests", "Deploy to staging" };
var numberedList = formatter.NumberedList(steps);
Console.WriteLine(numberedList);

// Create code block
var codeSample = "public class Program { public static void Main() { Console.WriteLine(\"Hello World!\"); } }";
var codeBlock = formatter.CodeBlock(codeSample, "csharp");
Console.WriteLine(codeBlock);

// Create blockquote
var quote = formatter.BlockQuote("This is a critical finding that requires immediate attention.\nThe issue affects user authentication and should be prioritized.");
Console.WriteLine(quote);

// Format emphasis
var boldText = formatter.Bold("Important");
var italicText = formatter.Italic("emphasis");
Console.WriteLine($"{boldText} and {italicText}");

// Create horizontal rule
var hr = formatter.HorizontalRule();
Console.WriteLine(hr);

// Create hyperlink
var link = formatter.Link("View on GitHub", "https://github.com/sarmkadan/jira-analytics-cli");
Console.WriteLine(link);

// Create complete document
var document = formatter.Document(
    "Jira Analytics Report",
    ("Summary", formatter.DefinitionList(new Dictionary<string, string>
    {
        { "Generated", DateTime.UtcNow.ToString("yyyy-MM-dd") },
        { "Total Issues", "42" },
        { "Completion Rate", "83%" }
    })),
    ("Issues by Status", table)
);
Console.WriteLine(document);

// Supporting class for example
public class Issue
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
}
```

## StringExtensions

The `StringExtensions` class provides a set of extension methods for string manipulation and formatting. It offers utilities for truncating strings, removing whitespace, converting to slug format, parsing boolean values, repeating strings, matching patterns, finding common prefixes, and escaping special characters for safe SQL use.

### Usage Example

```csharp
using JiraAnalyticsCli.Utils;

// Sample data
var longString = "This is a very long string that needs to be truncated.";
var whitespaceString = "   This string has leading and trailing whitespace.   ";
var slugString = "This string will be converted to slug format.";
var boolString = "true";

// Truncate string with ellipsis
var truncated = longString.TruncateWithEllipsis(20);
Console.WriteLine(truncated); // Output: "This is a very long..."

// Remove whitespace from string
var cleaned = whitespaceString.RemoveWhitespace();
Console.WriteLine(cleaned); // Output: "This string has leading and trailing whitespace."

// Convert string to slug format
var slug = slugString.ToSlug();
Console.WriteLine(slug); // Output: "this-string-will-be-converted-to-slug-format"

// Parse boolean value from string
if (boolString.TryParseBool(out bool result))
{
    Console.WriteLine(result); // Output: true
}

// Repeat string
var repeated = "Hello".Repeat(3);
Console.WriteLine(repeated); // Output: "HelloHelloHello"

// Match pattern
var pattern = "Hello*";
if ("HelloWorld".MatchesPattern(pattern))
{
    Console.WriteLine("Match found!"); // Output: Match found!
}

// Find common prefix
var prefix = "Hello".GetCommonPrefix("HelloWorld");
Console.WriteLine(prefix); // Output: "Hello"

// Escape special characters for SQL
var sqlString = "Hello'World";
var escaped = sqlString.EscapeForSql();
Console.WriteLine(escaped); // Output: "Hello''World"
```

## TeamComparisonServiceTestsValidation

`TeamComparisonServiceTestsValidation` provides a collection of fluent validation helpers used in unit tests for the `TeamComparisonService`.  
The static extension methods assert that a `TeamComparisonReport` contains the expected teams, correctly identifies the fastest, highest‑quality, and most‑consistent teams, and that individual `TeamProjectSnapshot` objects have the expected velocity and defect‑rate metrics. Additional helpers verify that generated text reports contain the required project keys and performance labels.

### Usage Example

```csharp
using System.Collections.Generic;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Tests.Services;

// Create a sample report
var report = new TeamComparisonReport
{
    Teams = new List<TeamProjectSnapshot>
    {
        new TeamProjectSnapshot { ProjectKey = "PROJ1", AverageVelocity = 20.5, DefectRate = 0.02 },
        new TeamProjectSnapshot { ProjectKey = "PROJ2", AverageVelocity = 18.0, DefectRate = 0.01 }
    },
    FastestTeam = "PROJ1",
    HighestQualityTeam = "PROJ2",
    MostConsistentTeam = "PROJ1"
};

var expectedKeys = new[] { "PROJ1", "PROJ2" };

// Validate the report contents
report.ShouldContainTeams(expectedKeys);
report.ShouldIdentifyFastestTeam("PROJ1");
report.ShouldIdentifyHighestQualityTeam("PROJ2");
report.ShouldIdentifyMostConsistentTeam("PROJ1");

// Validate a specific team snapshot's metrics
var snapshot = report.Teams[0];
snapshot.ShouldHaveMetrics(expectedVelocity: 20.5, expectedDefectRate: 0.02);

// Validate a generated text report
var textReport = @"
Fastest team: PROJ1
Highest quality: PROJ2
Most consistent: PROJ1
";
textReport.ShouldContainProjectKeys(expectedKeys);
textReport.ShouldContainPerformanceLabels();
```
