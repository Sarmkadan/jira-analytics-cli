using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Benchmarks
{
    [MemoryDiagnoser]
    public class DeveloperBenchmarks
    {
        private Developer _developer;
        private List<JiraIssue> _issues;

        [GlobalSetup]
        public void Setup()
        {
            _issues = new List<JiraIssue>();
            for (int i = 0; i < 1000; i++)
            {
                _issues.Add(new JiraIssue
                {
                    Key = $"ISSUE-{i}",
                    Status = i % 2 == 0 ? "Done" : "In Progress",
                    StoryPoints = i % 2 == 0 ? 3 : 5
                });
            }

            _developer = new Developer
            {
                Key = "DEV-1",
                Name = "John Doe",
                AssignedIssues = _issues
            };
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void GetCompletedIssues_Benchmark(int issueCount)
        {
            var issues = _issues.Take(issueCount).ToList();
            var developer = new Developer { AssignedIssues = issues };
            developer.GetCompletedIssues();
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void GetTotalStoryPoints_Benchmark(int issueCount)
        {
            var issues = _issues.Take(issueCount).ToList();
            var developer = new Developer { AssignedIssues = issues };
            developer.GetTotalStoryPoints();
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void GetLoadFactor_Benchmark(int issueCount)
        {
            var issues = _issues.Take(issueCount).ToList();
            var developer = new Developer { AssignedIssues = issues };
            developer.GetLoadFactor(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void GetAverageStoryPointsPerIssue_Benchmark(int issueCount)
        {
            var issues = _issues.Take(issueCount).ToList();
            var developer = new Developer { AssignedIssues = issues };
            developer.GetAverageStoryPointsPerIssue();
        }
    }
}
