using BenchmarkDotNet.Attributes;
using JiraAnalyticsCli.Models;

namespace JiraAnalyticsCli.Benchmarks;

[MemoryDiagnoser]
public class SprintBenchmarks
{
    [Params(10, 100, 1000)]
    public int IssueCount { get; set; }

    private Sprint? _sprint;

    [GlobalSetup]
    public void Setup()
    {
        _sprint = new Sprint
        {
            Id = 1,
            Name = "Benchmark Sprint",
            State = "Active",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10)
        };

        for (int i = 0; i < IssueCount; i++)
        {
            var issue = new JiraIssue
            {
                Key = $"TEST-{i}",
                Summary = $"Test Issue {i}",
                IssueType = i % 3 == 0 ? "Sub-task" : "Story",
                StoryPoints = (i % 5) + 1,
                Status = i % 5 switch
                {
                    0 => "Done",
                    1 => "Closed",
                    2 => "In Progress",
                    3 => "Blocked",
                    _ => "To Do"
                }
            };
            _sprint.AddIssue(issue);
        }
    }

    [Benchmark]
    public int GetPlannedStoryPoints() => _sprint!.GetPlannedStoryPoints();

    [Benchmark]
    public int GetCompletedStoryPoints() => _sprint!.GetCompletedStoryPoints();

    [Benchmark]
    public double GetVelocity() => _sprint!.GetVelocity();

    [Benchmark]
    public List<JiraIssue> GetOverdueIssues() => _sprint!.GetOverdueIssues();

    [Benchmark]
    public List<JiraIssue> GetInProgressIssues() => _sprint!.GetInProgressIssues();
}
