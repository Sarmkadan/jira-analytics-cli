[Benchmark]
[MemoryDiagnoser]
public class AnalyticsServiceBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // prepare test data
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // test method 1
    }

    [Benchmark]
    [Params(10)]
    public void BenchmarkMethod2(int inputSize)
    {
        // test method 2 with input size
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // test method 3
    }
}