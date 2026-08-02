[Benchmark]
[MemoryDiagnoser]
public class TeamComparisonServiceBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test TeamComparisonService method 1 with input size 10
    }

    [Benchmark]
    public void BenchmarkMethod2()
    {
        // Test TeamComparisonService method 2 with input size 100
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test TeamComparisonService method 3 with input size 1000
    }
}
