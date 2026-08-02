using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraAnalyticsCli.Benchmarks
{
    [MemoryDiagnoser]
    public class AnalyticsControllerBenchmarks
    {
        private AnalyticsController _analyticsController;
        private List<string> _projectKeys;
        private List<int> _sprintCounts;

        [GlobalSetup]
        public void Setup()
        {
            _analyticsController = new AnalyticsController();
            _projectKeys = Enumerable.Range(0, 100).Select(i => $"project-{i}").ToList();
            _sprintCounts = Enumerable.Range(0, 100).Select(i => i).ToList();
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_AnalyzeSprints(int inputSize)
        {
            for (int i = 0; i < inputSize; i++)
            {
                _analyticsController.AnalyzeSprints(_projectKeys[i % _projectKeys.Count], _sprintCounts[i % _sprintCounts.Count]);
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_GenerateReport(int inputSize)
        {
            for (int i = 0; i < inputSize; i++)
            {
                var analysisResult = _analyticsController.AnalyzeSprints(_projectKeys[i % _projectKeys.Count], _sprintCounts[i % _sprintCounts.Count]);
                _analyticsController.GenerateReport(analysisResult);
            }
        }

        [Benchmark]
        public void Benchmark_GetSprintMetrics()
        {
            foreach (var projectKey in _projectKeys)
            {
                _analyticsController.GetSprintMetrics(projectKey);
            }
        }
    }
}
