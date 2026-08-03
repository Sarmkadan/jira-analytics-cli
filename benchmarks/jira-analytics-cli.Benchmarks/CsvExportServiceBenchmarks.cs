[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class CsvExportServiceBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // Test method 1 with input size [Params(10)]
    }

    [Benchmark]
    public void Benchmark_Method2()
    {
        // Test method 2 with input size [Params(100)]
    }

    [Benchmark]
    public void Benchmark_Method3()
    {
        // Test method 3 with input size [Params(1000)]
    }
}