using System.Globalization;
using JiraAnalyticsCli.Models;
using JiraAnalyticsCli.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JiraAnalyticsCli.Tests.Services;

/// <summary>
/// Unit tests for <see cref="CsvExportService"/> verifying CSV export behavior for sprint and team metrics.
/// </summary>
public class CsvExportServiceTests
{
    private readonly Mock<ILogger<CsvExportService>> _loggerMock;
    private readonly CsvExportService _service;
    private readonly string _testPath = "test_output.csv";

    /// <summary>
    /// Initializes a new instance of <see cref="CsvExportServiceTests"/>.
    /// Creates a mock <see cref="ILogger{CsvExportService}"/> and the service under test.
    /// </summary>
    public CsvExportServiceTests()
    {
        _loggerMock = new Mock<ILogger<CsvExportService>>();
        _service = new CsvExportService(_loggerMock.Object);
    }

    /// <summary>
    /// Verifies that exporting an empty collection of sprint metrics writes only the CSV header.
    /// </summary>
    /// <returns>A task that represents the asynchronous test execution.</returns>
    [Fact]
    public async Task ExportSprintMetrics_ShouldWriteHeaderOnly_WhenMetricsIsEmpty()
    {
        const string testName = nameof(ExportSprintMetrics_ShouldWriteHeaderOnly_WhenMetricsIsEmpty);
        _loggerMock.Object.LogInformation("Starting test {TestName}", testName);

        try
        {
            // Act
            await _service.ExportSprintMetrics(Enumerable.Empty<SprintMetric>(), _testPath);

            // If the metrics collection is empty we are in a fallback path that only writes the header.
            _loggerMock.Object.LogWarning("Metrics collection is empty; only header will be written.");

            // Assert
            var content = await File.ReadAllTextAsync(_testPath);
            var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            
            // According to requirement: "empty dataset produces header only"
            Assert.Single(lines);
            Assert.StartsWith("SprintId,SprintName", lines[0]);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in test {TestName}", testName);
            throw;
        }
        finally
        {
            _loggerMock.Object.LogInformation("Finished test {TestName}", testName);
        }
    }

    /// <summary>
    /// Verifies that special characters in sprint names are correctly escaped in the CSV output.
    /// </summary>
    /// <returns>A task that represents the asynchronous test execution.</returns>
    [Fact]
    public async Task ExportSprintMetrics_ShouldEscapeSpecialCharactersInSprintName()
    {
        const string testName = nameof(ExportSprintMetrics_ShouldEscapeSpecialCharactersInSprintName);
        _loggerMock.Object.LogInformation("Starting test {TestName}", testName);

        try
        {
            // Arrange
            var metrics = new List<SprintMetric>
            {
                new() { 
                    SprintId = 1, 
                    SprintName = "Name, with comma", 
                    StartDate = new DateTime(2025, 1, 1), 
                    EndDate = new DateTime(2025, 1, 14) 
                },
                new() { 
                    SprintId = 2, 
                    SprintName = "Name \"with quotes\"", 
                    StartDate = new DateTime(2025, 1, 15), 
                    EndDate = new DateTime(2025, 1, 28) 
                },
                new() { 
                    SprintId = 3, 
                    SprintName = "Name\nwith newline", 
                    StartDate = new DateTime(2025, 2, 1), 
                    EndDate = new DateTime(2025, 2, 14) 
                }
            };

            // Act
            await _service.ExportSprintMetrics(metrics, _testPath);

            // Assert
            var content = await File.ReadAllTextAsync(_testPath);
            
            Assert.Contains("\"Name, with comma\"", content);
            Assert.Contains("\"Name \"\"with quotes\"\"\"", content);
            Assert.Contains("\"Name\nwith newline\"", content);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in test {TestName}", testName);
            throw;
        }
        finally
        {
            _loggerMock.Object.LogInformation("Finished test {TestName}", testName);
        }
    }

    /// <summary>
    /// Ensures that numeric and date values are formatted using the invariant culture (e.g., decimal point '.' and ISO date format).
    /// </summary>
    /// <returns>A task that represents the asynchronous test execution.</returns>
    [Fact]
    public async Task ExportSprintMetrics_ShouldUseInvariantCultureForNumbersAndDates()
    {
        const string testName = nameof(ExportSprintMetrics_ShouldUseInvariantCultureForNumbersAndDates);
        _loggerMock.Object.LogInformation("Starting test {TestName}", testName);

        try
        {
            // Arrange
            var metrics = new List<SprintMetric>
            {
                new() { 
                    SprintId = 1, 
                    SprintName = "Test Sprint", 
                    StartDate = new DateTime(2025, 1, 1), 
                    EndDate = new DateTime(2025, 1, 14),
                    AverageCycleTime = 12.34
                }
            };

            // Act
            await _service.ExportSprintMetrics(metrics, _testPath);

            // Assert
            var lines = await File.ReadAllLinesAsync(_testPath);
            // AverageCycleTime is 12.34. Invariant culture should ensure 12.34, not 12,34
            Assert.Contains("12.34", lines[1]);
            // Start date should be 2025-01-01
            Assert.Contains("2025-01-01", lines[1]);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in test {TestName}", testName);
            throw;
        }
        finally
        {
            _loggerMock.Object.LogInformation("Finished test {TestName}", testName);
        }
    }

    /// <summary>
    /// Verifies that exporting team metrics writes the header and the data rows, including proper escaping for developer names containing commas.
    /// </summary>
    /// <returns>A task that represents the asynchronous test execution.</returns>
    [Fact]
    public async Task ExportTeamMetrics_ShouldWriteHeaderAndData()
    {
        const string testName = nameof(ExportTeamMetrics_ShouldWriteHeaderAndData);
        _loggerMock.Object.LogInformation("Starting test {TestName}", testName);

        try
        {
            // Arrange
            var metrics = new List<KeyValuePair<string, int>>
            {
                new("Dev1", 5),
                new("Dev2, With Comma", 10)
            };

            // Act
            await _service.ExportTeamMetrics(metrics, _testPath);

            // Assert
            var lines = await File.ReadAllLinesAsync(_testPath);
            Assert.Equal(3, lines.Length); // Header + 2 data rows
            Assert.Equal("Developer,AssignedIssues", lines[0]);
            Assert.Contains("\"Dev2, With Comma\"", lines[2]);
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error in test {TestName}", testName);
            throw;
        }
        finally
        {
            _loggerMock.Object.LogInformation("Finished test {TestName}", testName);
        }
    }
}
