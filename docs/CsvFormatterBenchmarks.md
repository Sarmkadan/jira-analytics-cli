# CsvFormatterBenchmarks

Overview of the benchmark harness used to measure CSV formatting and parsing performance for sprint metric data in the *jira-analytics-cli* project. The class provides a simple API to configure the number of items, prepare test data, produce a CSV string, and parse it back into a list of `SprintMetric` objects.

## API

### ItemCount
`public int ItemCount`

Gets or sets the number of `SprintMetric` instances that will be generated during benchmark preparation.  
- **Purpose:** Controls the workload size for the benchmark.  
- **Parameters:** None.  
- **Return value:** The current count of items.  
- **Exceptions:**  
  - `ArgumentOutOfRangeException` if a negative value is assigned.

### Setup
`public void Setup`

Initializes internal state required for the subsequent `Format` and `Parse` operations. Typically called before each benchmark iteration.  
- **Purpose:** Creates `ItemCount` synthetic `SprintMetric` objects and stores them for formatting/parsing.  
- **Parameters:** None.  
- **Return value:** None.  
- **Exceptions:**  
  - `InvalidOperationException` if `ItemCount` is less than or equal to zero.  
  - `InvalidOperationException` if called after `Format` or `Parse` without resetting the instance.

### Format
`public string Format`

Produces a CSV‑formatted string representing the currently stored sprint metrics.  
- **Purpose:** Provides the output that will be measured by the benchmark.  
- **Parameters:** None.  
- **Return value:** A string containing comma‑separated values with a header row.  
- **Exceptions:**  
  - `InvalidOperationException` if `Setup` has not been invoked prior to calling this method.  
  - `ObjectDisposedException` if the instance has been reset or disposed after use.

### Parse
`public List<SprintMetric> Parse`

Parses the internal CSV string back into a list of `SprintMetric` objects.  
- **Purpose:** Allows benchmarking of the parsing logic alongside formatting.  
- **Parameters:** None.  
- **Return value:** A `List<SprintMetric>` containing the parsed entities.  
- **Exceptions:**  
  - `InvalidOperationException` if `Setup` has not been called.  
  - `FormatException` if the internal CSV data is malformed (e.g., missing columns or invalid numeric values).

## Usage

```csharp
var bench = new CsvFormatterBenchmarks();

// Configure the benchmark to work with 10,000 sprint metrics.
bench.ItemCount = 10_000;

// Prepare the data set.
bench.Setup;

// Obtain the CSV representation for measurement.
string csv = bench.Format;

// Parse the CSV back into objects to measure round‑trip performance.
List<SprintMetric> parsed = bench.Parse;
```

```csharp
// Re‑using the same instance for multiple iterations requires a reset.
for (int i = 0; i < 5; i++)
{
    bench.ItemCount = 1_000 * (i + 1); // Vary the workload.
    bench.Setup;                       // Refresh internal state.
    string output = bench.Format;      // Benchmark formatting step.
    List<SprintMetric> result = bench.Parse; // Benchmark parsing step.
    // … record timings …
}
```

## Notes

- **Thread safety:** The class holds mutable state (`ItemCount` and the internal collection of metrics). Concurrent calls to `Setup`, `Format`, or `Parse` from multiple threads may result in race conditions, corrupted data, or unexpected exceptions. It is intended for single‑threaded benchmark scenarios; if parallel execution is required, each thread should operate on its own instance.
- **State dependence:** `Format` and `Parse` rely on a successful prior call to `Setup`. Invoking them before `Setup` will throw an `InvalidOperationException`. After a call to `Format` or `Parse`, the internal CSV string is considered consumed; calling either method again without invoking `Setup` first will also throw.
- **ItemCount semantics:** A value of zero results in an empty metric list; `Setup` will still succeed but `Format` will produce a CSV containing only the header line. Negative values are not permitted and trigger an exception.
- **Exception contracts:** Only the exceptions listed above are thrown under normal operation. Any other exception indicates an unexpected failure (e.g., out‑of‑memory) and should be treated as fatal.
- **Performance considerations:** The class does not perform any caching of the CSV string across calls; each invocation of `Format` reconstructs the output from the current metric list, and `Parse` reparses the string each time. This reflects the typical workload measured by benchmarking tools.
