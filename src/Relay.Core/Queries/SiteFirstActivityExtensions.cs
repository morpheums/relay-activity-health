namespace Relay.Core.Queries;

internal static class SiteFirstActivityExtensions
{
    public static DateTime? AccountFirstActivityUtc(this IReadOnlyList<SiteFirstActivity> siteFirstActivities) =>
        siteFirstActivities.Count == 0 ? null : siteFirstActivities.Min(site => site.FirstActivityUtc);
}
