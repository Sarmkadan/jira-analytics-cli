using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Benchmarks
{
    [MemoryDiagnoser]
    public class JiraIssueBenchmarks
    {
        [Params(10, 100, 1000)]
        public int IssueCount;

        private JiraIssue _issue;

        [GlobalSetup]
        public void Setup()
        {
            _issue = new JiraIssue();

            // Attempt to set common properties via reflection if they exist.
            var type = typeof(JiraIssue);

            var createdProp = type.GetProperty("CreatedDate");
            createdProp?.SetValue(_issue, DateTime.UtcNow.AddDays(-5));

            var dueProp = type.GetProperty("DueDate");
            dueProp?.SetValue(_issue, DateTime.UtcNow.AddDays(2));

            var priorityProp = type.GetProperty("Priority");
            priorityProp?.SetValue(_issue, "High");

            var statusProp = type.GetProperty("Status");
            statusProp?.SetValue(_issue, "In Progress");

            var updatedProp = type.GetProperty("UpdatedDate");
            updatedProp?.SetValue(_issue, DateTime.UtcNow.AddDays(-1));
        }

        [Benchmark]
        public void IsOverdue()
        {
            for (int i = 0; i < IssueCount; i++)
                _issue.IsOverdue();
        }

        [Benchmark]
        public void IsHighPriority()
        {
            for (int i = 0; i < IssueCount; i++)
                _issue.IsHighPriority();
        }

        [Benchmark]
        public void GetDaysOpenWithoutProgress()
        {
            for (int i = 0; i < IssueCount; i++)
                _issue.GetDaysOpenWithoutProgress();
        }

        [Benchmark]
        public void IsInProgress()
        {
            for (int i = 0; i < IssueCount; i++)
                _issue.IsInProgress();
        }
    }
}
