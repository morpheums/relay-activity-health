using System.Collections.Frozen;
using System.Text.Json;
using Relay.Core.Queries;

namespace Relay.Api.Http;

public static class ActivityTypeNames
{
    private static readonly FrozenDictionary<string, ActivityType> ActivityTypesByName =
        Enum.GetValues<ActivityType>().ToFrozenDictionary(WireName, StringComparer.Ordinal);

    public static IReadOnlyList<string> Names { get; } = [.. Enum.GetValues<ActivityType>().Select(WireName)];

    public static bool TryParse(string name, out ActivityType activityType) =>
        ActivityTypesByName.TryGetValue(name, out activityType);

    private static string WireName(ActivityType activityType) =>
        JsonNamingPolicy.SnakeCaseLower.ConvertName(activityType.ToString());
}
