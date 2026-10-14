// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Diagnostics;

/// <summary>Outcome of one <c>doctor</c> check.</summary>
public enum DoctorStatus
{
    /// <summary>The check passed.</summary>
    Ok,

    /// <summary>The check failed. The result carries a remediation hint.</summary>
    Failed,

    /// <summary>The check did not run because a check it depends on failed.</summary>
    Skipped
}

/// <summary>One row of the <c>doctor</c> table.</summary>
/// <param name="Name">What was checked, e.g. <c>Authentication</c>.</param>
/// <param name="Status">Outcome of the check.</param>
/// <param name="Detail">What was found, or why the check failed or was skipped. Never contains the API token.</param>
/// <param name="Hint">How to fix a failed check. Null for passed and skipped checks.</param>
public sealed record DoctorCheckResult(string Name, DoctorStatus Status, string Detail, string? Hint = null)
{
    /// <summary>Creates a passed check.</summary>
    public static DoctorCheckResult Pass(string name, string detail) =>
        new(name, DoctorStatus.Ok, detail);

    /// <summary>Creates a failed check with the step that the user should take next.</summary>
    public static DoctorCheckResult Fail(string name, string detail, string hint) =>
        new(name, DoctorStatus.Failed, detail, hint);

    /// <summary>Creates a check that did not run because <paramref name="dependency"/> failed.</summary>
    public static DoctorCheckResult Skip(string name, string dependency) =>
        new(name, DoctorStatus.Skipped, $"not run: {dependency} did not pass");
}
