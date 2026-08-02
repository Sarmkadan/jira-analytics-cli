[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class ReportServiceValidationBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
    }

    [Benchmark]
    public void Benchmark_ReportServiceValidation_10()
    {
        // Test ReportServiceValidation with 10 items
        var reportServiceValidation = new ReportServiceValidation();
        var inputData = new List<string> { "input1", "input2", ... "input10" };
        reportServiceValidation.Validate(inputData);
    }

    [Benchmark]
    public void Benchmark_ReportServiceValidation_100()
    {
        // Test ReportServiceValidation with 100 items
        var reportServiceValidation = new ReportServiceValidation();
        var inputData = new List<string> { "input1", "input2", ... "input100" };
        reportServiceValidation.Validate(inputData);
    }

    [Benchmark]
    public void Benchmark_ReportServiceValidation_1000()
    {
        // Test ReportServiceValidation with 1000 items
        var reportServiceValidation = new ReportServiceValidation();
        var inputData = new List<string> { "input1", "input2", ... "input1000" };
        reportServiceValidation.Validate(inputData);
    }
}