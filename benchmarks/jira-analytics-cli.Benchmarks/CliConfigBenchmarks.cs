[Benchmark]
[MemoryDiagnoser]
public class CliConfigBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test data preparation
        var testData = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            testData.Add("testData" + i);
        }

        // Method call with test data
        var result = CliConfig.Method1(testData);
        // Assert result
        Assert.IsTrue(result);
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int inputSize)
    {
        // Test data preparation
        var testData = new List<string>();
        for (int i = 0; i < inputSize; i++)
        {
            testData.Add("testData" + i);
        }

        // Method call with test data
        var result = CliConfig.Method2(testData);
        // Assert result
        Assert.IsTrue(result);
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test data preparation
        var testData = new Dictionary<string, string>();
        testData.Add("key1", "value1");
        testData.Add("key2", "value2");

        // Method call with test data
        var result = CliConfig.Method3(testData);
        // Assert result
        Assert.IsTrue(result);
    }
}