// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.BackgroundTasks;

/// <summary>
/// Manages scheduled background task execution with error handling.
/// Supports recurring tasks and one-time executions with cancellation support.
/// </summary>
public class BackgroundTaskRunner : IDisposable
{
    private readonly ILogger<BackgroundTaskRunner> _logger;
    private readonly Dictionary<string, TaskInfo> _tasks = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public BackgroundTaskRunner(ILogger<BackgroundTaskRunner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a recurring background task that executes on specified interval.
    /// Task will repeat until StopTask is called or cancellation is triggered.
    /// </summary>
    public void ScheduleRecurringTask(string taskName, Func<CancellationToken, Task> action, TimeSpan interval)
    {
        if (string.IsNullOrEmpty(taskName))
            throw new ArgumentException("Task name cannot be empty", nameof(taskName));

        var taskInfo = new TaskInfo
        {
            Name = taskName,
            IsRecurring = true,
            Interval = interval,
            ExecuteAt = DateTime.UtcNow.Add(interval)
        };

        _tasks[taskName] = taskInfo;

        var task = Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    if (now >= taskInfo.ExecuteAt)
                    {
                        _logger.LogInformation("Executing recurring task: {TaskName}", taskName);
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        await action(_cancellationTokenSource.Token);

                        stopwatch.Stop();
                        taskInfo.LastExecutedAt = now;
                        taskInfo.ExecutionCount++;
                        taskInfo.LastExecutionDurationMs = stopwatch.ElapsedMilliseconds;
                        taskInfo.ExecuteAt = now.Add(interval);

                        _logger.LogInformation(
                            "Task completed: {TaskName} (elapsed: {DurationMs}ms)",
                            taskName,
                            stopwatch.ElapsedMilliseconds);
                    }

                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Task cancelled: {TaskName}", taskName);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing task: {TaskName}", taskName);
                    taskInfo.LastException = ex;
                }
            }
        }, _cancellationTokenSource.Token);

        taskInfo.Task = task;
    }

    /// <summary>
    /// Schedules a one-time task to execute after specified delay.
    /// </summary>
    public void ScheduleOneTimeTask(string taskName, Func<CancellationToken, Task> action, TimeSpan delay)
    {
        if (string.IsNullOrEmpty(taskName))
            throw new ArgumentException("Task name cannot be empty", nameof(taskName));

        var taskInfo = new TaskInfo
        {
            Name = taskName,
            IsRecurring = false,
            ExecuteAt = DateTime.UtcNow.Add(delay)
        };

        _tasks[taskName] = taskInfo;

        var task = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("Waiting to execute one-time task: {TaskName} (delay: {DelaySeconds}s)",
                    taskName, delay.TotalSeconds);

                await Task.Delay(delay, _cancellationTokenSource.Token);

                _logger.LogInformation("Executing one-time task: {TaskName}", taskName);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                await action(_cancellationTokenSource.Token);

                stopwatch.Stop();
                taskInfo.LastExecutedAt = DateTime.UtcNow;
                taskInfo.ExecutionCount++;
                taskInfo.LastExecutionDurationMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("One-time task completed: {TaskName} (elapsed: {DurationMs}ms)",
                    taskName, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("One-time task cancelled: {TaskName}", taskName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing one-time task: {TaskName}", taskName);
                taskInfo.LastException = ex;
            }
        }, _cancellationTokenSource.Token);

        taskInfo.Task = task;
    }

    /// <summary>
    /// Stops specific task by name.
    /// </summary>
    public async Task StopTaskAsync(string taskName)
    {
        if (_tasks.TryGetValue(taskName, out var taskInfo))
        {
            _logger.LogInformation("Stopping task: {TaskName}", taskName);

            if (taskInfo.Task != null && !taskInfo.Task.IsCompleted)
            {
                try
                {
                    await Task.WaitAsync(taskInfo.Task, TimeSpan.FromSeconds(30));
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Task did not complete within timeout: {TaskName}", taskName);
                }
            }

            _tasks.Remove(taskName);
        }
    }

    /// <summary>
    /// Gets information and status of task by name.
    /// </summary>
    public TaskInfo? GetTaskInfo(string taskName)
    {
        return _tasks.TryGetValue(taskName, out var info) ? info : null;
    }

    /// <summary>
    /// Gets all registered tasks and their status.
    /// </summary>
    public IEnumerable<TaskInfo> GetAllTasks()
    {
        return _tasks.Values;
    }

    /// <summary>
    /// Stops all tasks and waits for completion.
    /// </summary>
    public async Task StopAllAsync()
    {
        _logger.LogInformation("Stopping all background tasks ({Count})", _tasks.Count);
        _cancellationTokenSource.Cancel();

        var tasks = _tasks.Values.Select(t => t.Task).Where(t => t != null).Cast<Task>();
        await Task.WhenAll(tasks);

        _tasks.Clear();
        _logger.LogInformation("All background tasks stopped");
    }

    public void Dispose()
    {
        try
        {
            StopAllAsync().GetAwaiter().GetResult();
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
        }
    }

    public class TaskInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTime ExecuteAt { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public int ExecutionCount { get; set; }
        public long LastExecutionDurationMs { get; set; }
        public Exception? LastException { get; set; }
        public Task? Task { get; set; }
    }
}
