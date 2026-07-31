[Benchmark]
[MemoDiagnoser]
public class TeamComparisonServiceTestsValidationBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // Test data preparation
        var testData = new object[] { /* input data */ };
        // Method call
        var result = TeamComparisonServiceTestsValidation.Method1(testData);
        // Assert the result
    }

    [Benchmark]
    [Params(10)]
    public void Benchmark_Method2()
    {
        // Test data preparation
        var testData = new object[] { /* input data */ };
        // Method call
        var result = TeamComparisonServiceTestsValidation.Method2(testData);
        // Assert the result
    }

    [Benchmark]
    [Params(100)]
    public void Benchmark_Method3()
    {
        // Test data preparation
        var testData = new object[] { /* input data */ };
        // Method call
        var result = TeamComparisonServiceTestsValidation.Method3(testData);
        // Assert the result
    }
}