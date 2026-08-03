using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Repositories;

namespace JiraAnalyticsCli.Benchmarks
{
    [MemoryDiagnoser]
    public class SprintRepositoryBenchmarks
    {
        private SprintRepository _repository = null!;
        private List<Sprint> _sprints = null!;

        // Vary the amount of data stored in the repository
        [Params(10, 100, 1000)]
        public int Size;

        // Runs once per benchmark execution (once per Params value)
        [GlobalSetup]
        public void GlobalSetup()
        {
            // Use a null logger – we don't want logging overhead in the benchmarks
            var logger = NullLogger<SprintRepository>.Instance;
            _repository = new SprintRepository(logger);

            // Create a realistic collection of Sprint objects
            _sprints = new List<Sprint>(Size);
            var now = DateTime.UtcNow;

            for (int i = 0; i < Size; i++)
            {
                // Simple heuristic:
                // - Every 3rd sprint is active (EndDate in the future)
                // - Every 5th sprint is closed (CompleteDate in the past)
                // - The rest are future sprints
                var isActive = i % 3 == 0;
                var isClosed = i % 5 == 0;

                var sprint = new Sprint
                {
                    Id = i,
                    Name = $"Sprint {i}",
                    ProjectKey = $"PROJ{(i % 4)}",
                    StartDate = now.AddDays(-i - 10),
                    EndDate = isActive ? now.AddDays(10) : now.AddDays(-i),
                    CompleteDate = isClosed ? now.AddDays(-i - 1) : (DateTime?)null
                };

                _sprints.Add(sprint);
            }

            // Populate the repository with the generated data
            _repository.SaveRangeAsync(_sprints).GetAwaiter().GetResult();
        }

        // -----------------------------------------------------------------
        // Benchmarks for the most frequently used public methods
        // -----------------------------------------------------------------

        [Benchmark]
        public async Task GetByIdAsync()
        {
            // Pick a middle id to avoid edge‑case cache hits
            int id = Size / 2;
            await _repository.GetByIdAsync(id);
        }

        [Benchmark]
        public async Task GetByProjectAsync()
        {
            // Use a project key that is guaranteed to exist in the generated data
            await _repository.GetByProjectAsync("PROJ1");
        }

        [Benchmark]
        public async Task GetActiveSprintsAsync()
        {
            await _repository.GetActiveSprints();
        }

        [Benchmark]
        public async Task GetRecentClosedSprintsAsync()
        {
            // Retrieve the 10 most recent closed sprints (or fewer if not enough)
            await _repository.GetRecentClosedSprints(10);
        }

        [Benchmark]
        public async Task SaveRangeAsync()
        {
            // Save a fresh copy of the data to measure bulk insert performance.
            // We clone the list to avoid mutating the repository state used by other benchmarks.
            var copy = new List<Sprint>(_sprints);
            await _repository.SaveRangeAsync(copy);
        }
    }
}
