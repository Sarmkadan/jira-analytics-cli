[MemoryDiagnoser]
public class CacheManagerBenchmarks
{
    [Benchmark]
    public void Benchmark_Cache_Get()
    {
        // Test data setup
        var cache = new CacheManager();
        var key = "test_key";
        var value = "test_value";
        cache.Set(key, value);
        // Benchmark
        for (int i = 0; i < 100; i++)
        {
            cache.Get(key);
        }
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_Cache_Set()
    {
        // Test data setup
        var cache = new CacheManager();
        var key = "test_key";
        var value = "test_value";
        // Benchmark
        for (int i = 0; i < 1000; i++)
        {
            cache.Set(key, value);
        }
    }

    [Benchmark]
    public void Benchmark_Cache_Remove()
    {
        // Test data setup
        var cache = new CacheManager();
        var key = "test_key";
        // Benchmark
        for (int i = 0; i < 100; i++)
        {
            cache.Remove(key);
        }
    }
}