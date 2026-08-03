[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 500)]
[MemoryDiagnoser]
public class MarkdownReportServiceBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data here
    }

    [Benchmark]
    public void Benchmark_MarkdownReportGeneration()
    {
        // Test markdown report generation
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_MarkdownReportGeneration_Params()
    {
        // Test markdown report generation with varying input sizes
    }

    [Benchmark]
    public void Benchmark_MarkdownReportGeneration_LargeInput()
    {
        // Test markdown report generation with large input
    }
}