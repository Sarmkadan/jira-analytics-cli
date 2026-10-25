using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Contains unit tests for the <see cref="JiraAnalyticsCli.Models.JiraProjectExtensions"/> class.
/// </summary>
public class JiraProjectExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.JiraProjectExtensions.FindSprintById"/>
    /// returns the sprint whose identifier matches the requested value.
    /// </summary>
    [Fact]
    public void FindSprintById_ShouldReturnMatchingSprint_WhenSprintExists()
    {
        // Arrange
        var project = new JiraProject
        {
            Sprints = new List<Sprint>
            {
                new Sprint { Id = 1, Name = "Sprint 1" },
                new Sprint { Id = 2, Name = "Sprint 2" },
                new Sprint { Id = 3, Name = "Sprint 3" }
            }
        };

        // Act
        var result = project.FindSprintById(2);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Sprint 2");
    }

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.JiraProjectExtensions.FindSprintById"/>
    /// returns <c>null</c> when no sprint has the requested identifier.
    /// </summary>
    [Fact]
    public void FindSprintById_ShouldReturnNull_WhenSprintDoesNotExist()
    {
        // Arrange
        var project = new JiraProject
        {
            Sprints = new List<Sprint> { new Sprint { Id = 1, Name = "Sprint 1" } }
        };

        // Act
        var result = project.FindSprintById(99);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.JiraProjectExtensions.FindSprintById"/>
    /// returns <c>null</c> when the project has no sprints at all.
    /// </summary>
    [Fact]
    public void FindSprintById_ShouldReturnNull_WhenProjectHasNoSprints()
    {
        // Arrange
        var project = new JiraProject();

        // Act
        var result = project.FindSprintById(1);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.JiraProjectExtensions.FindSprintById"/>
    /// returns the first sprint in project order when several share the same identifier.
    /// </summary>
    [Fact]
    public void FindSprintById_ShouldReturnFirstMatch_WhenDuplicateIdsExist()
    {
        // Arrange
        var project = new JiraProject
        {
            Sprints = new List<Sprint>
            {
                new Sprint { Id = 5, Name = "First" },
                new Sprint { Id = 5, Name = "Second" }
            }
        };

        // Act
        var result = project.FindSprintById(5);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("First");
    }

    /// <summary>
    /// Verifies that <see cref="JiraAnalyticsCli.Models.JiraProjectExtensions.FindSprintById"/>
    /// throws an <see cref="ArgumentNullException"/> when the project is null.
    /// </summary>
    [Fact]
    public void FindSprintById_ShouldThrowArgumentNullException_WhenProjectIsNull()
    {
        // Arrange
        JiraProject? project = null;

        // Act
        Action act = () => project!.FindSprintById(1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
