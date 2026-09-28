namespace Relay.Core.Normality;

public sealed class LocationRanker : ILocationRanker
{
    private enum RankGroup
    {
        Flagged,
        Normal,
        InsufficientData,
    }

    public IReadOnlyList<LocationHealth> Rank(IReadOnlyList<LocationHealth> locations) =>
        locations
            .OrderBy(location => RankGroupOf(location.Status))
            .ThenByDescending(location => Math.Abs(location.Deviation ?? 0.0))
            .ThenBy(location => location.Status == HealthStatus.Below ? 0 : 1)
            .ThenBy(location => location.Location, StringComparer.Ordinal)
            .ToList();

    private static RankGroup RankGroupOf(HealthStatus status) => status switch
    {
        HealthStatus.Above or HealthStatus.Below => RankGroup.Flagged,
        HealthStatus.Normal => RankGroup.Normal,
        _ => RankGroup.InsufficientData,
    };
}
