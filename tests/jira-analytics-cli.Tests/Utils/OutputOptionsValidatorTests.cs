// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using JiraAnalyticsCli.Utils;
using Xunit;

namespace JiraAnalyticsCli.Tests.Utils;

/// <summary>
/// Unit tests for <see cref="OutputOptionsValidator"/>: format names, sprint counts, date ranges and output paths.
/// Path tests use a fresh temporary root per test and check that nothing is created.
/// </summary>
public class OutputOptionsValidatorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "jac-output-validator-" + Guid.NewGuid().ToString("N"));

    public OutputOptionsValidatorTests() => Directory.CreateDirectory(this._root);

    public void Dispose()
    {
        if (Directory.Exists(this._root))
        {
            Directory.Delete(this._root, recursive: true);
        }
    }

    // -------------------------------------------------------------------------
    // Format
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("png")]
    [InlineData("JSON")]
    [InlineData("Md")]
    public void CheckFormat_SupportedValue_ReturnsNull(string format)
    {
        OutputOptionsValidator.CheckFormat(format, OutputOptionsValidator.ExportFormats).Should().BeNull();
    }

    [Fact]
    public void CheckFormat_UnknownValue_ListsSupportedFormats()
    {
        var problem = OutputOptionsValidator.CheckFormat("pdf", OutputOptionsValidator.ExportFormats);

        problem.Should().Contain("'pdf'");
        problem.Should().Contain("png, jpg, svg, json, csv, md");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void CheckFormat_MissingValue_AsksForOne(string? format)
    {
        var problem = OutputOptionsValidator.CheckFormat(format, OutputOptionsValidator.TextOrJsonFormats, "--format");

        problem.Should().StartWith("--format requires a value");
        problem.Should().Contain("text, json");
    }

    [Fact]
    public void CheckFormat_NullSupportedList_Throws()
    {
        var act = () => OutputOptionsValidator.CheckFormat("json", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // Sprint count
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(50)]
    public void CheckSprintCount_InRange_ReturnsNull(int count)
    {
        OutputOptionsValidator.CheckSprintCount(count).Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(51)]
    public void CheckSprintCount_OutOfRange_ReturnsProblemWithBounds(int count)
    {
        var problem = OutputOptionsValidator.CheckSprintCount(count);

        problem.Should().Contain("between 1 and 50");
        problem.Should().Contain($"got {count}");
    }

    // -------------------------------------------------------------------------
    // Date range
    // -------------------------------------------------------------------------

    [Fact]
    public void CheckDateRange_StartBeforeEnd_IsValid()
    {
        OutputOptionsValidator.CheckDateRange("2026-01-01", "2026-01-31").Should().BeNull();
    }

    [Fact]
    public void CheckDateRange_StartEqualsEnd_IsValid()
    {
        OutputOptionsValidator.CheckDateRange("2026-03-15", "2026-03-15").Should().BeNull();
    }

    [Fact]
    public void CheckDateRange_OpenEnded_IsValid()
    {
        OutputOptionsValidator.CheckDateRange("2026-01-01", null).Should().BeNull();
        OutputOptionsValidator.CheckDateRange(null, "2026-01-31").Should().BeNull();
        OutputOptionsValidator.CheckDateRange(null, null).Should().BeNull();
    }

    [Fact]
    public void CheckDateRange_StartAfterEnd_IsRejected()
    {
        var problem = OutputOptionsValidator.CheckDateRange("2026-02-01", "2026-01-31");

        problem.Should().Contain("must not be after");
    }

    [Theory]
    [InlineData("01/02/2026")]
    [InlineData("2026-13-01")]
    [InlineData("yesterday")]
    public void CheckDateRange_UnparseableStart_IsRejected(string from)
    {
        OutputOptionsValidator.CheckDateRange(from, "2026-12-31").Should().Contain("--from must be a date in yyyy-MM-dd format");
    }

    [Fact]
    public void CheckDateRange_UnparseableEnd_IsRejected()
    {
        OutputOptionsValidator.CheckDateRange("2026-01-01", "soon").Should().Contain("--to must be a date in yyyy-MM-dd format");
    }

    // -------------------------------------------------------------------------
    // Output file
    // -------------------------------------------------------------------------

    [Fact]
    public void CheckOutputFile_NotGiven_IsValid()
    {
        OutputOptionsValidator.CheckOutputFile(null).Should().BeNull();
        OutputOptionsValidator.CheckOutputFile("").Should().BeNull();
    }

    [Fact]
    public void CheckOutputFile_ExistingDirectory_IsRejected()
    {
        var problem = OutputOptionsValidator.CheckOutputFile(this._root);

        problem.Should().Contain("is a directory");
    }

    [Fact]
    public void CheckOutputFile_MissingDirectoryThatCanBeCreated_IsValidAndNotCreated()
    {
        var target = Path.Combine(this._root, "reports", "2026", "sprint.html");

        OutputOptionsValidator.CheckOutputFile(target).Should().BeNull();
        Directory.Exists(Path.Combine(this._root, "reports")).Should().BeFalse();
    }

    [Fact]
    public void CheckOutputFile_ParentIsAFile_IsRejected()
    {
        var blocker = Path.Combine(this._root, "blocker.txt");
        File.WriteAllText(blocker, "x");

        var problem = OutputOptionsValidator.CheckOutputFile(Path.Combine(blocker, "nested", "out.csv"));

        problem.Should().Contain("is a file, not a directory");
    }

    [Fact]
    public void CheckOutputFile_BareFileName_IsValid()
    {
        OutputOptionsValidator.CheckOutputFile("report.txt").Should().BeNull();
    }

    [Fact]
    public void CheckOutputFile_PathWithoutFileName_IsRejected()
    {
        var problem = OutputOptionsValidator.CheckOutputFile(Path.Combine(this._root, "missing") + Path.DirectorySeparatorChar);

        problem.Should().Contain("does not name a file");
    }

    // -------------------------------------------------------------------------
    // Output directory
    // -------------------------------------------------------------------------

    [Fact]
    public void CheckOutputDirectory_ExistingDirectory_IsValid()
    {
        OutputOptionsValidator.CheckOutputDirectory(this._root).Should().BeNull();
    }

    [Fact]
    public void CheckOutputDirectory_NestedMissingDirectory_IsValidAndNotCreated()
    {
        var target = Path.Combine(this._root, "a", "b", "c");

        OutputOptionsValidator.CheckOutputDirectory(target).Should().BeNull();
        Directory.Exists(Path.Combine(this._root, "a")).Should().BeFalse();
    }

    [Fact]
    public void CheckOutputDirectory_PointsAtFile_IsRejected()
    {
        var file = Path.Combine(this._root, "file.txt");
        File.WriteAllText(file, "x");

        OutputOptionsValidator.CheckOutputDirectory(file).Should().Contain("is a file, not a directory");
    }

    [Fact]
    public void CheckOutputDirectory_NotGiven_IsValid()
    {
        OutputOptionsValidator.CheckOutputDirectory(null).Should().BeNull();
    }

    [Fact]
    public void CheckOutputDirectory_UsesOptionNameInMessage()
    {
        var file = Path.Combine(this._root, "file.txt");
        File.WriteAllText(file, "x");

        OutputOptionsValidator.CheckOutputDirectory(file, "--custom").Should().StartWith("--custom ");
    }

    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    [Fact]
    public void SupportedFormats_MatchWhatTheCommandsAccept()
    {
        OutputOptionsValidator.ExportFormats.Should().BeEquivalentTo(new[] { "png", "jpg", "svg", "json", "csv", "md" });
        OutputOptionsValidator.TextOrJsonFormats.Should().BeEquivalentTo(new[] { "text", "json" });
    }

    [Fact]
    public void SprintBounds_AreOneToFifty()
    {
        OutputOptionsValidator.MinSprints.Should().Be(1);
        OutputOptionsValidator.MaxSprints.Should().Be(50);
    }
}
