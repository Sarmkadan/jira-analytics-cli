[Benchmark]
[MemoryDiagnoser]
public class SprintMetricBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test data preparation
        var testData = new object[] { /* test data */ };
        // Method call with test data
        var result = SprintMetric.Method1(testData);
        // Assert the result
        Assert.AreEqual(expectedResult, result);
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int inputSize)
    {
        // Test data preparation
        var testData = new object[] { /* test data */ };
        // Method call with test data
        var result = SprintMetric.Method2(testData, inputSize);
        // Assert the result
        Assert.AreEqual(expectedResult, result);
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test data preparation
        var testData = new object[] { /* test data */ };
        // Method call with test data
        var result = SprintMetric.Method3(testData);
        // Assert the result
        Assert.AreEqual(expectedResult, result);
    }
}