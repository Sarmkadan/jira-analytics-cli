[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class AnalyticsServiceValidationBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // setup test data
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // test code for Benchmark_Method1
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_Method2()
    {
        // test code for Benchmark_Method2
    }

    [Benchmark]
    public void Benchmark_Method3()
    {
        // test code for Benchmark_Method3
    }
}
