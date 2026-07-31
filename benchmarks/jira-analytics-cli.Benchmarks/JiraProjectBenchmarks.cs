using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using JiraAnalyticsCli.Models;

namespace jira_analytics_cli.Benchmarks
{
    [MemoryDiagnoser]
    public class JiraProjectBenchmarks
    {
        private JiraProject _jiraProject;
        private List<Sprint> _sprints;
        private List<Developer> _developers;

        [GlobalSetup]
        public void Setup()
        {
            _jiraProject = new JiraProject
            {
                Key = "KEY",
                Id = "ID",
                Name = "NAME",
                CreatedDate = DateTime.Now
            };

            _sprints = new List<Sprint>();
            _developers = new List<Developer>();

            for (int i = 0; i < 100; i++)
            {
                _sprints.Add(new Sprint
                {
                    Id = i.ToString(),
                    Name = $"Sprint {i}",
                    StartDate = DateTime.Now.AddDays(-i),
                    EndDate = DateTime.Now.AddDays(-i + 7)
                });

                _developers.Add(new Developer
                {
                    Key = i.ToString(),
                    Name = $"Developer {i}",
                    Active = true
                });
            }
        }

        [Params(10, 100, 1000)]
        public int InputSize { get; set; }

        [Benchmark]
        public void AddSprints_Benchmark()
        {
            var sprintsToAdd = _sprints.Take(InputSize).ToList();
            foreach (var sprint in sprintsToAdd)
            {
                _jiraProject.AddSprint(sprint);
            }
        }

        [Benchmark]
        public void GetRecentSprints_Benchmark()
        {
            _jiraProject.Sprints = _sprints.Take(InputSize).ToList();
            _jiraProject.GetRecentSprints(10);
        }

        [Benchmark]
        public void GetTopPerformers_Benchmark()
        {
            _jiraProject.TeamMembers = _developers.Take(InputSize).ToList();
            _jiraProject.GetTopPerformers(10);
        }

        [Benchmark]
        public void GetProjectHealthScore_Benchmark()
        {
            var metricsHistory = new List<SprintMetric>();
            for (int i = 0; i < InputSize; i++)
            {
                metricsHistory.Add(new SprintMetric
                {
                    EndDate = DateTime.Now.AddDays(-i),
                    CompletionRate = 0.5,
                    QualityScore = 0.8,
                    RiskScore = 0.2
                });
            }
            _jiraProject.MetricsHistory = metricsHistory;
            _jiraProject.GetProjectHealthScore();
        }
    }
}
