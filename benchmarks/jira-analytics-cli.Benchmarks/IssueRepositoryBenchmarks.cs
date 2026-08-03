[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 500)]
[MemoryDiagnoser]
public class IssueRepositoryBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Method implementation with loops, LINQ, or I/O
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2()
    {
        // Method implementation with input size parameter
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Method implementation
    }
}