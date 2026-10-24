// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Net;
using System.Text.Json;
using JiraAnalyticsCli.Configuration;

namespace JiraAnalyticsCli.Diagnostics;

/// <summary>
/// The Jira REST checks of <c>doctor</c>: authentication, board, closed sprint and story-points field.
/// </summary>
public static partial class JiraDoctor
{
    private const string BoardPathFormat = "/rest/agile/1.0/board/{0}";
    private const string ClosedSprintsPathFormat = "/rest/agile/1.0/board/{0}/sprint?state=closed&maxResults={1}";
    private const string FieldsPath = "/rest/api/3/field";
    private const int ClosedSprintsToRequest = 50;
    private const string StoryPointsNameFragment = "story point";
    private const string NumberFieldType = "number";

    /// <summary>
    /// Calls <c>GET /rest/api/3/myself</c> with the Bearer token and reads the display name of the account.
    /// </summary>
    internal static async Task<DoctorCheckResult> CheckAuthenticationAsync(HttpClient http, Uri baseUri, CancellationToken ct)
    {
        using var response = await http.GetAsync(new Uri(baseUri, JiraConfigChecker.MyselfPath), ct);
        var code = StatusText(response);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return DoctorCheckResult.Fail(AuthenticationCheck,
                $"{code}: the API token was rejected",
                "Create a new API token in your Atlassian account settings, then set it with JIRA_API_TOKEN, --jira-token or \"jiraApiToken\" in appsettings.json.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return DoctorCheckResult.Fail(AuthenticationCheck,
                $"{code} from {JiraConfigChecker.MyselfPath}",
                "Jira answered with an unexpected status. Check that BaseUrl is the Jira site itself.");
        }

        var root = await ReadObjectAsync(response, ct);
        if (root is null)
        {
            return DoctorCheckResult.Fail(AuthenticationCheck,
                "the response from /myself was not a JSON object",
                "BaseUrl may point at a login or proxy page instead of Jira. Check the URL in a browser.");
        }

        var name = StringProperty(root.Value, "displayName") ?? "unknown account";
        return DoctorCheckResult.Pass(AuthenticationCheck, $"authenticated as {name}");
    }

    /// <summary>
    /// Calls <c>GET /rest/agile/1.0/board/{id}</c> and reads the board name.
    /// </summary>
    internal static async Task<DoctorCheckResult> CheckBoardAsync(HttpClient http, Uri baseUri, int? boardId, CancellationToken ct)
    {
        if (boardId is not { } id)
        {
            return DoctorCheckResult.Fail(BoardCheck,
                "no board is configured",
                "Set the board ID with --board-id, JIRA_BOARD_ID or \"jiraBoardId\" in appsettings.json. It is the number at the end of the board URL (.../boards/42).");
        }

        using var response = await http.GetAsync(new Uri(baseUri, string.Format(CultureInfo.InvariantCulture, BoardPathFormat, id)), ct);
        var code = StatusText(response);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return DoctorCheckResult.Fail(BoardCheck,
                $"board {id} does not exist or is not visible to this account",
                "Check --board-id against the board URL in Jira.");
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return DoctorCheckResult.Fail(BoardCheck,
                $"no access to board {id} ({code})",
                "The account needs the View permission on this board. Ask a Jira admin, or use a board this account can see.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return DoctorCheckResult.Fail(BoardCheck,
                $"{code} from board {id}",
                "Check that the instance has Jira Software (Agile) enabled and that BaseUrl is correct.");
        }

        var root = await ReadObjectAsync(response, ct);
        var name = root is { } board ? StringProperty(board, "name") : null;
        return DoctorCheckResult.Pass(BoardCheck, $"board {id} \"{name ?? "unnamed"}\"");
    }

    /// <summary>
    /// Calls <c>GET /rest/agile/1.0/board/{id}/sprint?state=closed</c> and checks that at least one sprint is returned.
    /// </summary>
    internal static async Task<DoctorCheckResult> CheckClosedSprintAsync(HttpClient http, Uri baseUri, int boardId, CancellationToken ct)
    {
        var path = string.Format(CultureInfo.InvariantCulture, ClosedSprintsPathFormat, boardId, ClosedSprintsToRequest);
        using var response = await http.GetAsync(new Uri(baseUri, path), ct);

        // Jira answers 400 for boards that have no sprints, which means a Kanban board.
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return DoctorCheckResult.Fail(ClosedSprintCheck,
                $"board {boardId} does not support sprints",
                "Sprints exist only on Scrum boards. The board is probably Kanban, so point --board-id at a Scrum board.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return DoctorCheckResult.Fail(ClosedSprintCheck,
                $"{StatusText(response)} from the sprint list of board {boardId}",
                "Check that this account can view the board's sprints.");
        }

        var root = await ReadObjectAsync(response, ct);
        if (root is not { } body || !body.TryGetProperty("values", out var values) || values.ValueKind != JsonValueKind.Array)
        {
            return DoctorCheckResult.Fail(ClosedSprintCheck,
                "the sprint list was not in the expected format",
                "Check that BaseUrl is the Jira site and that the instance has Jira Software (Agile) enabled.");
        }

        var count = values.GetArrayLength();
        if (count == 0)
        {
            return DoctorCheckResult.Fail(ClosedSprintCheck,
                $"board {boardId} has no closed sprints",
                "Velocity and analytics need at least one completed sprint. Complete a sprint on this board, or use a Scrum board that has one.");
        }

        var countText = count == ClosedSprintsToRequest ? $"{count}+" : count.ToString(CultureInfo.InvariantCulture);
        var example = StringProperty(values.EnumerateArray().First(), "name");
        return DoctorCheckResult.Pass(ClosedSprintCheck,
            $"{countText} closed sprint(s) found" + (example is null ? string.Empty : $", e.g. \"{example}\""));
    }

    /// <summary>
    /// Calls <c>GET /rest/api/3/field</c> and checks that the configured story-points field exists and is numeric.
    /// When it does not, it looks for another field whose name mentions story points and suggests it.
    /// </summary>
    internal static async Task<DoctorCheckResult> CheckStoryPointsFieldAsync(HttpClient http, Uri baseUri, string configuredField, CancellationToken ct)
    {
        using var response = await http.GetAsync(new Uri(baseUri, FieldsPath), ct);
        if (!response.IsSuccessStatusCode)
        {
            return DoctorCheckResult.Fail(StoryPointsCheck,
                $"{StatusText(response)} from the field list",
                "Check that the account can read Jira fields and that BaseUrl is correct.");
        }

        var fields = await ReadArrayAsync(response, ct);
        if (fields is null)
        {
            return DoctorCheckResult.Fail(StoryPointsCheck,
                "the field list was not in the expected format",
                "Check that BaseUrl is the Jira site.");
        }

        var configured = fields.FirstOrDefault(field => StringProperty(field, "id") == configuredField);
        if (configured.ValueKind == JsonValueKind.Object)
        {
            var name = StringProperty(configured, "name") ?? "unnamed";
            var type = SchemaType(configured);
            if (type is null or NumberFieldType)
            {
                return DoctorCheckResult.Pass(StoryPointsCheck, $"{configuredField} \"{name}\"");
            }

            return DoctorCheckResult.Fail(StoryPointsCheck,
                $"{configuredField} \"{name}\" is a {type} field, not a number",
                "Pick the numeric story-points field and set it with --story-points-field, VELOCITY_STORY_POINTS_FIELD or \"velocityStoryPointsField\" in appsettings.json.");
        }

        var candidate = fields.FirstOrDefault(IsStoryPointsCandidate);
        if (candidate.ValueKind == JsonValueKind.Object)
        {
            var candidateId = StringProperty(candidate, "id");
            var candidateName = StringProperty(candidate, "name");
            return DoctorCheckResult.Fail(StoryPointsCheck,
                $"{configuredField} does not exist; {candidateId} is named \"{candidateName}\"",
                $"Set the field with --story-points-field {candidateId}, VELOCITY_STORY_POINTS_FIELD or \"velocityStoryPointsField\" in appsettings.json.");
        }

        return DoctorCheckResult.Fail(StoryPointsCheck,
            $"{configuredField} does not exist and no story-points field was found",
            "Ask a Jira admin which numeric custom field holds story points, then set it with --story-points-field.");
    }

    private static bool IsStoryPointsCandidate(JsonElement field)
    {
        var name = StringProperty(field, "name");
        var custom = field.TryGetProperty("custom", out var isCustom) && isCustom.ValueKind == JsonValueKind.True;
        return custom && name is not null && name.Contains(StoryPointsNameFragment, StringComparison.OrdinalIgnoreCase);
    }

    private static string? SchemaType(JsonElement field) =>
        field.TryGetProperty("schema", out var schema) && schema.ValueKind == JsonValueKind.Object
            ? StringProperty(schema, "type")
            : null;

    private static string? StringProperty(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string StatusText(HttpResponseMessage response) =>
        $"HTTP {(int)response.StatusCode}";

    private static async Task<JsonElement?> ReadObjectAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await ReadJsonAsync(response, ct);
        return json is { ValueKind: JsonValueKind.Object } ? json : null;
    }

    private static async Task<JsonElement[]?> ReadArrayAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await ReadJsonAsync(response, ct);
        return json is { ValueKind: JsonValueKind.Array } array ? array.EnumerateArray().ToArray() : null;
    }

    private static async Task<JsonElement?> ReadJsonAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
