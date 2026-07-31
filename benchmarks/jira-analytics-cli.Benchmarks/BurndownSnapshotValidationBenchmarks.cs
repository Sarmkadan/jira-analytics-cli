using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Benchmarks
{
    [MemoryDiagnoser]
    public class BurndownSnapshotValidationBenchmarks
    {
        // Size of the test data set
        [Params(10, 100, 1000)]
        public int Size;

        private List<BurndownSnapshot> _snapshots;
        private BurndownSnapshot _singleSnapshot;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Create a single snapshot for single-item benchmarks
            _singleSnapshot = GenerateSnapshot(0);

            // Create a list of snapshots for bulk benchmarks
            _snapshots = new List<BurndownSnapshot>(Size);
            for (int i = 0; i < Size; i++)
            {
                _snapshots.Add(GenerateSnapshot(i));
            }
        }

        // Helper to generate a valid BurndownSnapshot
        private BurndownSnapshot GenerateSnapshot(int index)
        {
            return new BurndownSnapshot
            {
                SprintId = index + 1,
                Timestamp = DateTime.UtcNow.AddMinutes(-index),
                RemainingStoryPoints = 10,
                CompletedStoryPoints = 5,
                TotalStoryPoints = 15,
                RemainingIssueCount = 3,
                CompletedIssueCount = 2,
                TotalIssueCount = 5,
                ScopeChanges = 0
            };
        }

        [Benchmark]
        public void ValidateSingle()
        {
            // Validate a single snapshot
            _singleSnapshot.Validate();
        }

        [Benchmark]
        public void ValidateMultiple()
        {
            // Validate each snapshot in the list
            foreach (var snapshot in _snapshots)
            {
                snapshot.Validate();
            }
        }

        [Benchmark]
        public void IsValidSingle()
        {
            // Check validity of a single snapshot
            _singleSnapshot.IsValid();
        }

        [Benchmark]
        public void IsValidMultiple()
        {
            // Check validity of each snapshot in the list
            foreach (var snapshot in _snapshots)
            {
                snapshot.IsValid();
            }
        }

        [Benchmark]
        public void GetValidationErrorsSingle()
        {
            // Retrieve validation errors for a single snapshot
            _singleSnapshot.GetValidationErrors();
        }

        [Benchmark]
        public void GetValidationErrorsMultiple()
        {
            // Retrieve validation errors for each snapshot in the list
            foreach (var snapshot in _snapshots)
            {
                snapshot.GetValidationErrors();
            }
        }

        [Benchmark]
        public void EnsureValidSingle()
        {
            // Ensure a single snapshot is valid (throws if not)
            _singleSnapshot.EnsureValid();
        }

        [Benchmark]
        public void EnsureValidMultiple()
        {
            // Ensure each snapshot in the list is valid
            foreach (var snapshot in _snapshots)
            {
                snapshot.EnsureValid();
            }
        }
    }
}
