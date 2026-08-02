[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 500)]
[MemoryDiagnoser]
public class JiraIssueBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
    }

    [Benchmark]
    public void Benchmark_CreateIssue()
    {
        // Test creating an issue
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_CreateIssues()
    {
        // Test creating multiple issues
    }

    [Benchmark]
    public void Benchmark_UpdateIssue()
    {
        // Test updating an issue
    }
