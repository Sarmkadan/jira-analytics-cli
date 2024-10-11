# CacheBenchmarks

`CacheBenchmarks` is a utility class designed to measure and compare the performance of different caching strategies within the `jira-analytics-cli` project. It provides methods to initialize, populate, retrieve, and check for cached `JiraIssue` objects, enabling benchmarking of cache hit/miss ratios and access times.

## API

### `void Setup()`

Initializes the benchmark cache with a clean state. This method should be called before running any benchmarking tests to ensure a consistent starting point. It clears any existing cached data and prepares the cache for new entries.

- **Parameters**: None
- **Return value**: None
- **Exceptions**: May throw if the underlying cache implementation fails to initialize (e.g., due to resource constraints).

---

### `void CacheSet(JiraIssue issue)`

Stores a `JiraIssue` object in the benchmark cache using its `Key` property as the lookup identifier.

- **Parameters**:
  - `issue` (`JiraIssue`): The issue to cache. Must not be `null`.
- **Return value**: None
- **Exceptions**:
  - Throws `ArgumentNullException` if `issue` is `null`.
  - May throw if the cache is full or the insertion fails for other reasons (e.g., serialization errors).

---

### `JiraIssue? CacheGet(string key)`

Retrieves a cached `JiraIssue` by its lookup key. Returns `null` if the key is not found.

- **Parameters**:
  - `key` (`string`): The lookup identifier for the cached issue. Must not be `null`.
- **Return value**:
  - `JiraIssue?`: The cached issue if found; otherwise, `null`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `key` is `null`.

---

### `bool CacheContains(string key)`

Checks whether a `JiraIssue` with the given key exists in the cache.

- **Parameters**:
  - `key` (`string`): The lookup identifier to check. Must not be `null`.
- **Return value**:
  - `bool`: `true` if the key exists in the cache; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `key` is `null`.

## Usage

### Example 1: Basic Benchmarking Workflow
```csharp
var benchmarks = new CacheBenchmarks();

// Initialize the cache
benchmarks.Setup();

// Cache a sample issue
var issue = new JiraIssue { Key = "PROJ-123", Summary = "Fix critical bug" };
benchmarks.CacheSet(issue);

// Check cache operations
bool contains = benchmarks.CacheContains("PROJ-123"); // true
JiraIssue? retrieved = benchmarks.CacheGet("PROJ-123"); // returns the issue
```

### Example 2: Measuring Cache Misses
```csharp
var benchmarks = new CacheBenchmarks();
benchmarks.Setup();

bool contains = benchmarks.CacheContains("MISSING-456"); // false
JiraIssue? retrieved = benchmarks.CacheGet("MISSING-456"); // null

// Cache a different issue
var issue = new JiraIssue { Key = "PROJ-789", Summary = "New feature" };
benchmarks.CacheSet(issue);
```

## Notes

- **Thread Safety**: The class is not thread-safe by default. Concurrent access from multiple threads may lead to race conditions (e.g., `CacheGet` and `CacheSet` operations interfering with each other). If thread safety is required, external synchronization (e.g., `lock`) or a thread-safe cache implementation should be used.
- **Null Handling**: All methods validate input parameters for `null` and throw `ArgumentNullException`. Ensure keys and issues are non-null before calling these methods.
- **Cache State**: `Setup()` resets the cache entirely. Use this between benchmark iterations to avoid interference from previous runs.
- **Performance Considerations**: `CacheGet` and `CacheContains` are designed for O(1) average-case lookup time (assuming a hash-based cache implementation). Benchmarking results may vary based on the underlying cache size and eviction policies.
