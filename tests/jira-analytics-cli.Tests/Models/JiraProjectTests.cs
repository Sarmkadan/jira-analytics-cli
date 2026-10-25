// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Models;
using Xunit;

namespace JiraAnalyticsCli.Tests.Models;

/// <summary>
/// Contains unit tests for the argument guards and validation rules of <see cref="JiraProject"/>.
/// </summary>
public class JiraProjectTests
{
    private static JiraProject CreateValidProject()
    {
        return new JiraProject
        {
            Key = "PROJ",
            Id = "10000",
            Name = "Project",
            CreatedDate = new DateTime(2026, 1, 1)
        };
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.AddSprint"/> rejects a <c>null</c> sprint.
    /// </summary>
    [Fact]
    public void AddSprint_ShouldThrowArgumentNullException_WhenSprintIsNull()
    {
        var project = CreateValidProject();

        Action act = () => project.AddSprint(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("sprint");
        project.Sprints.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.AddSprint"/> validates the sprint and rejects an invalid one.
    /// </summary>
    [Fact]
    public void AddSprint_ShouldThrowArgumentException_WhenSprintIsInvalid()
    {
        var project = CreateValidProject();
        var sprint = new Sprint { Id = 0, Name = "Sprint" };

        Action act = () => project.AddSprint(sprint);

        act.Should().Throw<ArgumentException>();
        project.Sprints.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.AddSprint"/> ignores a sprint whose identifier is already present.
    /// </summary>
    [Fact]
    public void AddSprint_ShouldIgnoreDuplicateId_WhenSprintAlreadyAdded()
    {
        var project = CreateValidProject();
        project.AddSprint(new Sprint { Id = 1, Name = "Sprint 1" });

        project.AddSprint(new Sprint { Id = 1, Name = "Sprint 1 copy" });

        project.Sprints.Should().HaveCount(1);
        project.Sprints[0].Name.Should().Be("Sprint 1");
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.AddTeamMember"/> rejects a <c>null</c> developer.
    /// </summary>
    [Fact]
    public void AddTeamMember_ShouldThrowArgumentNullException_WhenDeveloperIsNull()
    {
        var project = CreateValidProject();

        Action act = () => project.AddTeamMember(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("developer");
        project.TeamMembers.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.AddTeamMember"/> validates the developer and rejects an invalid one.
    /// </summary>
    [Fact]
    public void AddTeamMember_ShouldThrowArgumentException_WhenDeveloperKeyIsEmpty()
    {
        var project = CreateValidProject();
        var developer = new Developer { Key = " ", Name = "Dev" };

        Action act = () => project.AddTeamMember(developer);

        act.Should().Throw<ArgumentException>();
        project.TeamMembers.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.AddTeamMember"/> ignores a developer whose key is already present.
    /// </summary>
    [Fact]
    public void AddTeamMember_ShouldIgnoreDuplicateKey_WhenDeveloperAlreadyAdded()
    {
        var project = CreateValidProject();
        project.AddTeamMember(new Developer { Key = "dev1", Name = "Dev One" });

        project.AddTeamMember(new Developer { Key = "dev1", Name = "Dev One copy" });

        project.TeamMembers.Should().HaveCount(1);
        project.TeamMembers[0].Name.Should().Be("Dev One");
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.Validate"/> accepts a project with key, name and created date set.
    /// </summary>
    [Fact]
    public void Validate_ShouldNotThrow_WhenProjectIsComplete()
    {
        var project = CreateValidProject();

        Action act = () => project.Validate();

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.Validate"/> rejects a blank key.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenKeyIsWhitespace()
    {
        var project = CreateValidProject();
        project.Key = "   ";

        Action act = () => project.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("Project key cannot be empty*");
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.Validate"/> rejects a blank name.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        var project = CreateValidProject();
        project.Name = string.Empty;

        Action act = () => project.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("Project name cannot be empty*");
    }

    /// <summary>
    /// Verifies that <see cref="JiraProject.Validate"/> rejects a project whose created date was never set.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenCreatedDateIsDefault()
    {
        var project = CreateValidProject();
        project.CreatedDate = default;

        Action act = () => project.Validate();

        act.Should().Throw<ArgumentException>().WithMessage("Created date must be set*");
    }

    /// <summary>
    /// Verifies that the sprint and velocity helpers return neutral values for a project with no sprints.
    /// </summary>
    [Fact]
    public void SprintQueries_ShouldReturnNeutralValues_WhenProjectHasNoSprints()
    {
        var project = CreateValidProject();

        project.GetCurrentActiveSprint().Should().BeNull();
        project.GetRecentSprints(5).Should().BeEmpty();
        project.GetAverageVelocity().Should().Be(0);
        project.GetTotalSprintCount().Should().Be(0);
        project.GetCompletedSprintCount().Should().Be(0);
    }

    /// <summary>
    /// Verifies that the health score is zero when the project has no metrics history.
    /// </summary>
    [Fact]
    public void GetProjectHealthScore_ShouldReturnZero_WhenMetricsHistoryIsEmpty()
    {
        var project = CreateValidProject();

        project.GetProjectHealthScore().Should().Be(0);
    }
}
