// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    public const string ApplicationName = "Jira Analytics CLI";
    public const string ApplicationVersion = "1.0.0";
    public const string Author = "Vladyslav Zaiets";
    public const string AuthorUrl = "https://sarmkadan.com";

    public const int DefaultHttpTimeoutSeconds = 30;
    public const int DefaultCacheExpirationMinutes = 15;
    public const int MaxConcurrentRequests = 5;
    public const int DefaultPageSize = 100;

    // Jira API endpoints (relative to base URL)
    public const string JiraApiV3Prefix = "/rest/api/3";
    public const string JiraProjectsEndpoint = "/projects";
    public const string JiraSprintsEndpoint = "/sprints";
    public const string JiraSearchEndpoint = "/search";
    public const string JiraMyUserEndpoint = "/myself";

    // Issue status constants
    public const string IssueStatusOpen = "Open";
    public const string IssueStatusInProgress = "In Progress";
    public const string IssueStatusInReview = "In Review";
    public const string IssueStatusDone = "Done";
    public const string IssueStatusClosed = "Closed";
    public const string IssueStatusBlocked = "Blocked";

    // Issue priority constants
    public const string PriorityCritical = "Critical";
    public const string PriorityBlocker = "Blocker";
    public const string PriorityHigh = "High";
    public const string PriorityMedium = "Medium";
    public const string PriorityLow = "Low";

    // Issue type constants
    public const string IssueTypeBug = "Bug";
    public const string IssueTypeTask = "Task";
    public const string IssueTypeStory = "Story";
    public const string IssueTypeSubTask = "Sub-task";
    public const string IssueTypeEpic = "Epic";
}

/// <summary>
/// Sprint states enumeration
/// </summary>
public enum SprintState
{
    Open,
    Active,
    Closed
}

/// <summary>
/// Health status levels for sprints
/// </summary>
public enum HealthStatus
{
    Critical,
    AtRisk,
    Healthy,
    Excellent
}

/// <summary>
/// Velocity trend directions
/// </summary>
public enum VelocityTrend
{
    Decreasing,
    Stable,
    Increasing
}

/// <summary>
/// Supported export formats
/// </summary>
public enum ExportFormat
{
    Text,
    Html,
    Json,
    Csv,
    Png,
    Jpg,
    Pdf
}

/// <summary>
/// Report types
/// </summary>
public enum ReportType
{
    Summary,
    Detailed,
    Burndown,
    TeamMetrics,
    VelocityTrend
}
