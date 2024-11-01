# MarkdownFormatter

The `MarkdownFormatter` class provides a set of methods for generating Markdown-formatted strings. It is designed for use in the Jira analytics CLI to produce structured reports, such as tables, lists, code blocks, and document headers. Each method returns a raw Markdown string that can be composed into larger documents or written directly to output.

## API

### `public MarkdownFormatter()`

Initializes a new instance of the `MarkdownFormatter` class. The constructor takes no parameters and performs no allocation of mutable state.

### `public string Header(string text, int level = 1)`

Generates a Markdown heading.

- **Parameters**  
  `text` – The heading text.  
  `level` – The heading level (1–6). Defaults to 1.
- **Returns**  
  A string containing the Markdown heading, e.g., `# text`.
- **Throws**  
  `ArgumentNullException` if `text` is `null`.  
  `ArgumentOutOfRangeException` if `level` is less than 1 or greater than 6.

### `public string Table<T>(IEnumerable<T> rows)`

Generates a Markdown table from a collection of objects. The method uses reflection to read the public readable properties of `T` as columns. Each property name becomes a column header, and each row is formatted accordingly.

- **Type parameters**  
  `T` – The type of objects in the collection.
- **Parameters**  
  `rows` – A sequence of objects to render as table rows.
- **Returns**  
  A string containing the Markdown table, including header separator and rows.
- **Throws**  
  `ArgumentNullException` if `rows` is `null`.  
  `InvalidOperationException` if `T` has no public readable properties.

### `public string DefinitionList(IEnumerable<KeyValuePair<string, string>> items)`

Generates a Markdown definition list from a sequence of term–definition pairs.

- **Parameters**  
  `items` – A sequence of key-value pairs where the key is the term and the value is the definition.
- **Returns**  
  A string containing the definition list in the format:  
  `Term`  
  `:   Definition`
- **Throws**  
  `ArgumentNullException` if `items` is `null`.

### `public string BulletList(IEnumerable<string> items)`

Generates an unordered (bullet) list.

- **Parameters**  
  `items` – The list items.
- **Returns**  
  A string with each item prefixed by `- `.
- **Throws**  
  `ArgumentNullException` if `items` is `null`.

### `public string NumberedList(IEnumerable<string> items)`

Generates an ordered (numbered) list.

- **Parameters**  
  `items` – The list items.
- **Returns**  
  A string with each item prefixed by `1. `, `2. `, etc.
- **Throws**  
  `ArgumentNullException` if `items` is `null`.

### `public string CodeBlock(string code, string language = null)`

Generates a fenced code block.

- **Parameters**  
  `code` – The code content.  
  `language` – An optional language identifier for syntax highlighting (e.g., `"csharp"`).
- **Returns**  
  A string wrapped in triple backticks.
- **Throws**  
  `ArgumentNullException` if `code` is `null`.

### `public string BlockQuote(string text)`

Generates a blockquote.

- **Parameters**  
  `text` – The quoted text.
- **Returns**  
  A string with each line prefixed by `> `.
- **Throws**  
  `ArgumentNullException` if `text` is `null`.

### `public string Bold(string text)`

Wraps text in double asterisks for bold formatting.

- **Parameters**  
  `text` – The text to embolden.
- **Returns**  
  A string in the form `**text**`.
- **Throws**  
  `ArgumentNullException` if `text` is `null`.

### `public string Italic(string text)`

Wraps text in single asterisks for italic formatting.

- **Parameters**  
  `text` – The text to italicise.
- **Returns**  
  A string in the form `*text*`.
- **Throws**  
  `ArgumentNullException` if `text` is `null`.

### `public string HorizontalRule()`

Generates a horizontal rule (`---`).

- **Parameters**  
  None.
- **Returns**  
  The string `---`.

### `public string Link(string text, string url)`

Generates a Markdown inline link.

- **Parameters**  
  `text` – The visible link text.  
  `url` – The target URL.
- **Returns**  
  A string in the form `[text](url)`.
- **Throws**  
  `ArgumentNullException` if `text` or `url` is `null`.

### `public string Document(string title, params string[] sections)`

Composes a complete Markdown document by combining a title heading with one or more content sections.

- **Parameters**  
  `title` – The document title (rendered as a level‑1 heading).  
  `sections` – Zero or more Markdown strings to append after the title, each separated by a blank line.
- **Returns**  
  A string containing the title heading followed by the sections.
- **Throws**  
  `ArgumentNullException` if `title` is `null`.

## Usage

### Example 1: Generating a report with a table and list

```csharp
var formatter = new MarkdownFormatter();

var issues = new[]
{
    new { Key = "PROJ-1", Status = "Open", Priority = "High" },
    new { Key = "PROJ-2", Status = "In Progress", Priority = "Medium" }
};

string report = formatter.Document(
    "Open Issues",
    formatter.Table(issues),
    formatter.BulletList(new[] { "Review blockers", "Update estimates" })
);

Console.WriteLine(report);
```

Output (simplified):

```
# Open Issues

| Key    | Status       | Priority |
|--------|--------------|----------|
| PROJ-1 | Open         | High     |
| PROJ-2 | In Progress  | Medium   |

- Review blockers
- Update estimates
```

### Example 2: Using inline formatting and a definition list

```csharp
var formatter = new MarkdownFormatter();

string summary = formatter.Document(
    "Sprint Retrospective",
    formatter.Bold("Action Items"),
    formatter.DefinitionList(new[]
    {
        new KeyValuePair<string, string>("Improve testing", "Add integration tests for the analytics pipeline"),
        new KeyValuePair<string, string>("Documentation", "Update the CLI usage guide")
    }),
    formatter.Italic("Generated on " + DateTime.Now.ToShortDateString())
);
```

Output (simplified):

```
# Sprint Retrospective

**Action Items**

Improve testing
:   Add integration tests for the analytics pipeline

Documentation
:   Update the CLI usage guide

*Generated on 4/12/2025*
```

## Notes

- **Null handling** – All methods that accept string parameters throw `ArgumentNullException` when a required argument is `null`. Collection parameters (`IEnumerable<T>`) also throw on `null`. Empty collections are permitted and produce an empty Markdown block (e.g., an empty table with only headers, or an empty list).
- **Special characters** – The formatter does not escape Markdown special characters (e.g., `*`, `_`, `[`, `` ` ``) inside text parameters. Callers should sanitise or escape content if it may contain characters that would alter the intended formatting.
- **Thread safety** – The `MarkdownFormatter` class is immutable and stateless. All methods are pure functions that depend only on their parameters. Instances can be safely shared across threads without synchronization.
- **Table reflection** – The `Table<T>` method uses reflection to discover properties at runtime. For performance‑sensitive scenarios, consider caching property metadata or using a predefined column mapping. The method throws `InvalidOperationException` if the type `T` exposes no public readable properties.
- **Document composition** – The `Document` method concatenates sections with a blank line separator. It does not add a trailing newline. Callers may append additional content or a final newline as needed.
