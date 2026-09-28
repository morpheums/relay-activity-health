using System.Text.Json;

namespace Relay.Api.Tests.Fixtures;

public sealed class HealthReportJson(JsonElement root)
{
    public JsonElement Root { get; } = root;

    public SeriesJson Summary => SeriesJson.From(Root.GetProperty("summary"));

    public IReadOnlyList<SeriesJson> Locations => [.. Root.GetProperty("locations").EnumerateArray().Select(SeriesJson.From)];

    public IReadOnlyList<string?> LocationNames => [.. Locations.Select(location => location.Location)];

    public string? EventType => Root.GetProperty("eventType").GetString();

    public string? WeekStart => Root.GetProperty("week").GetProperty("start").GetString();

    public string? WeekEnd => Root.GetProperty("week").GetProperty("end").GetString();

    public string? LatestCompleteWeek => Root.GetProperty("latestCompleteWeek").GetString();

    public string? EarliestWeek => Root.GetProperty("earliestWeek").GetString();

    public JsonElement DataAsOf => Root.GetProperty("dataAsOf");

    public SeriesJson Location(string locationName) => Locations.Single(location => location.Location == locationName);
}
