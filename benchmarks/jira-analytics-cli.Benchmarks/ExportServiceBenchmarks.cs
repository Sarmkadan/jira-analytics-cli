[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class ExportServiceBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test ExportService public method 1
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // Test ExportService public method 2 with input size
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test ExportService public method 3
    }
}
