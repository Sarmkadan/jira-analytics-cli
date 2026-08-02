using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Engines;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;

namespace JiraAnalyticsCli.Benchmarks
{
    [MemoryDiagnoser]
    public class CycleTimeResultBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            // Add test data setup here
        }

        [Benchmark]
        public void Benchmark_CycleTimeResult_GetData()
        {
            // Add benchmark code here
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_CycleTimeResult_GetData_WithParams()
        {
            // Add benchmark code here
        }

        [Benchmark]
        public void Benchmark_CycleTimeResult_GetData_Async()
        {
            // Add benchmark code here
        }
    }
}
