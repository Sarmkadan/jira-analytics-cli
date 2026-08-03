using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraAnalyticsCli.Benchmarks
{
    [MemoryDiagnoser]
    public class MetricsRepositoryBenchmarks
    {
        private MetricsRepository _repository;
        private List<string> _keys;
        private List<int> _values;

        [GlobalSetup]
        public void Setup()
        {
            _repository = new MetricsRepository();
            _keys = Enumerable.Range(0, 1000).Select(i => $"key{i}").ToList();
            _values = Enumerable.Range(0, 1000).Select(i => i).ToList();
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_GetMetrics(int size)
        {
            var keys = _keys.Take(size).ToList();
            _repository.GetMetrics(keys);
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_GetMetric(int size)
        {
            var key = _keys[size - 1];
            _repository.GetMetric(key);
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_SetMetric(int size)
        {
            var key = _keys[size - 1];
            var value = _values[size - 1];
            _repository.SetMetric(key, value);
        }
    }
}
